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
        
        [SerializeField, InfoBox("Check AutoRegister if placed in Scene")] private bool autoRegister;
        
        public override int Priority => data.priority;
        public override SelectableType selectableType => SelectableType.Building;
        public override Sprite Icon => data.icon;
        public override bool HasCommands => true;
        public override CommandTemplate Commands => data.commands;
        public override float Size => size;
        public override SoundData SelectionClip => data.selectSound;
        public override Collider Collider => selectionCollider;
        
        // Building Components
        private IBuildingPlacementFunctions[] _buildingPlacementListeners;
        private IBuildingDestroyFunction[] _buildingDestroyedListeners;
        private IBuildingSelectionFunctions[] _buildingSelectionListeners;

        //TODO: Move to separate component and remove when built
        [SerializeField] private Renderer renderer;
        [SerializeField] private Material buildingPlacementMaterial;
        private bool _validBuildingPosition;
        private bool _isPlaced;
        private Bounds _bounds; // can't get the bounds of a collider when it gets disabled (when obj is destroyed)
        private static readonly int Valid = Shader.PropertyToID("_Valid");
        private Material[] _defaultMaterials;

        public bool ValidBuildingPosition
        {
            get => _validBuildingPosition;
            set
            {
                _validBuildingPosition = value;

                if (_validBuildingPosition)
                {
                    buildingPlacementMaterial.SetFloat(Valid, 1);
                }
                else
                {
                    buildingPlacementMaterial.SetFloat(Valid, 0);
                }
                
            }
        }
        
        protected override void OnEnable()
        {
            _buildingPlacementListeners = GetComponents<IBuildingPlacementFunctions>();
            _buildingDestroyedListeners = GetComponents<IBuildingDestroyFunction>();
            _buildingSelectionListeners = GetComponents<IBuildingSelectionFunctions>();
            
            _defaultMaterials = renderer.materials;

            var swapMats = renderer.materials;
            for(int i=0; i<swapMats.Length; i++)
            {
                swapMats[i] = buildingPlacementMaterial;
            }

            renderer.materials = swapMats;
            if (autoRegister)
            {
                Build();
            }
            else
            {
                foreach (var module in _buildingPlacementListeners)
                {
                    module.OnBeginPlacement();
                }
            }
        }

        protected override void OnDisable()
        {
            if (_isPlaced)
            {
                SceneReferences.Instance.gridJobs.SetBoundsBlocked(_bounds, false);
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

        public void Build()
        {
            _isPlaced = true;
            _bounds = Collider.bounds;
            RegisterObject();
            
            renderer.materials = _defaultMaterials;
            SceneReferences.Instance.gridJobs.SetBoundsBlocked(_bounds, true);
            
            foreach (var module in _buildingPlacementListeners)
            {
                module.OnFinishPlacement();
            }
        }

        public bool IsPlacementValid()
        {
            bool validPosition = SceneReferences.Instance.gridJobs.IsPositionValid(Collider.bounds);
            if (!validPosition) return false;
            foreach (var module in _buildingPlacementListeners)
            {
                if (!module.IsValid()) return false;
            }

            return true;
        }
    }
}
