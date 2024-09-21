using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace TDCB
{
    public class Unit : SelectableObject
    {
        [SerializeField] private UnitData data;
        [SerializeField] private float size;
        [SerializeField] private Collider selectionCollider;
        [SerializeField] private bool isWorker;
        
        public override int Priority => data.priority;
        public override SelectableType selectableType => SelectableType.Unit;
        public override Unit unit => this;
        public override Building building => null;
        public override Sprite Icon => data.icon;
        public override bool HasCommands => true;
        public override CommandTemplate Commands => data.commands;
        public override float Size => size;
        public override SoundData SelectionClip => data.selectSound;
        public SoundData MoveClip => data.moveSound;
        public SoundData WorkClip => data.workSound;
        public override Collider Collider => selectionCollider;

        public bool IsWorker => isWorker;

        protected override void OnRegister()
        {
            SceneReferences.Instance.resourceManager.UpdateResourceValue(ResourceType.Population, data.population);
        }
        
        protected override void OnDeregister()
        {
            SceneReferences.Instance.resourceManager.UpdateResourceValue(ResourceType.Population, -data.population);
        }
    }
}
