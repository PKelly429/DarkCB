using System;
using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TDCB
{
    public class Building : SelectableObject
    {
        [SerializeField] private BuildingData data;
        [SerializeField] private float size;
        [SerializeField] private Collider selectionCollider;
        [SerializeField] private bool walkable;
        
        [SerializeField, InfoBox("Check AutoRegister if placed in Scene")] private bool autoRegister;

        public BuildingData BuildingData => data;
        
        public override int Priority => data.priority;
        public override SelectableType selectableType => SelectableType.Building;
        public override Unit unit => null;
        public override Building building => this;
        public override Sprite Icon => data.icon;
        public override bool HasCommands => true;
        public override CommandTemplate Commands => data.commands;
        public override float Size => size;
        public override SoundData SelectionClip => data.selectSound;
        public override Collider Collider => selectionCollider;
        
        // Building Components
        private IBuildingPlacementFunctions[] _buildingPlacementListeners;
        private IBuildingPlacementValidFunction[] _buildingPlacementValidListeners;
        private IBuildingDestroyFunction[] _buildingDestroyedListeners;
        private IBuildingSelectionFunctions[] _buildingSelectionListeners;
        
        private bool _validBuildingPosition;
        private bool _isPlaced;
        private Bounds _bounds; // can't get the bounds of a collider when it gets disabled (when obj is destroyed)
        private static readonly int Valid = Shader.PropertyToID("_Valid");

        public bool ValidBuildingPosition
        {
            get => _validBuildingPosition;
            set
            {
                _validBuildingPosition = value;

                foreach (var buildingModule in _buildingPlacementValidListeners)
                {
                    buildingModule.UpdateBuildingPlacementValid(_validBuildingPosition);
                }
            }
        }
        
        protected override void OnEnable()
        {
            _buildingPlacementListeners = GetComponents<IBuildingPlacementFunctions>();
            _buildingPlacementValidListeners = GetComponents<IBuildingPlacementValidFunction>();
            _buildingDestroyedListeners = GetComponents<IBuildingDestroyFunction>();
            _buildingSelectionListeners = GetComponents<IBuildingSelectionFunctions>();
            

            if (autoRegister)
            {
                Build();
            }
        }

        protected override void OnDisable()
        {
            if (_isPlaced)
            {
                SceneReferences.Instance.gridJobs.SetBoundsBlocked(_bounds, false, walkable);
            }
            else
            {
                foreach (var module in _buildingPlacementListeners)
                {
                    module.OnCancelPlacement();
                }
            }
            
            foreach (var module in _buildingDestroyedListeners)
            {
                module.OnBuildingDestroyed();
            }
            
            base.OnDisable();
        }
        
        public override void OnHoverBegin()
        {
            base.OnHoverBegin();
            
            foreach (var module in _buildingSelectionListeners)
            {
                module.OnHoverBegin();
            }
        }

        public override void OnHoverEnd()
        {
            base.OnHoverEnd();
            
            foreach (var module in _buildingSelectionListeners)
            {
                module.OnHoverEnd();
            }
        }

        public override void OnSelect()
        {
            base.OnSelect();
            
            foreach (var module in _buildingSelectionListeners)
            {
                module.OnSelect();
            }
        }

        public override void OnDeSelect()
        {
            base.OnDeSelect();
            
            foreach (var module in _buildingSelectionListeners)
            {
                module.OnDeselect();
            }
        }

        public void BeginPlacement()
        {
            foreach (var module in _buildingPlacementListeners)
            {
                module.OnBeginPlacement();
            }
        }

        public void Build()
        {
            _isPlaced = true;
            _bounds = Collider.bounds;
            RegisterObject();
            
            SceneReferences.Instance.gridJobs.SetBoundsBlocked(_bounds, true, walkable);
            
            foreach (var module in _buildingPlacementListeners)
            {
                module.OnFinishPlacement();
            }
            
            for (int i = 0; i < data.costs.Length; i++)
            {
                SceneReferences.Instance.resourceManager.PayResourceCost(data.costs[i]);
            }
        }

        public bool IsPlacementValid()
        {
            bool validPosition = SceneReferences.Instance.gridJobs.IsPositionValid(Collider.bounds);
            if (!validPosition) return false;

            for (int i = 0; i < data.costs.Length; i++)
            {
                if (!SceneReferences.Instance.resourceManager.CanAffordCost(data.costs[i])) return false;
            }
            
            foreach (var module in _buildingPlacementValidListeners)
            {
                if (!module.IsValid()) return false;
            }

            return true;
        }
    }
}
