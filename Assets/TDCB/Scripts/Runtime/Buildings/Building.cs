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

        //TODO: Move to separate component and remove when built
        [SerializeField] private Renderer renderer;
        [SerializeField] private Material valid;
        [SerializeField] private Material inValid;
        private bool _validBuildingPosition;
        public bool ValidBuildingPosition
        {
            get => _validBuildingPosition;
            set
            {
                _validBuildingPosition = value;

                if (_validBuildingPosition)
                {
                    renderer.material = valid;
                }
                else
                {
                    renderer.material = inValid;
                }
                
            }
        }
        
        protected override void OnEnable()
        {
            if (autoRegister) Build();
        }

        public void Build()
        {
            RegisterObject();
            
            SceneReferences.Instance.gridManager.SetFlags(Collider.bounds, GridState.Blocked);
        }
    }
}
