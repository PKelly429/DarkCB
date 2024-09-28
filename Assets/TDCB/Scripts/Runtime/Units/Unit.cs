using System;
using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using Sirenix.OdinInspector;
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
        [SerializeField, InfoBox("Check if placed in scene")] private bool applyPopulationCostOnEnable;
        
        public UnitData unitData => data;
        
        public override int Priority => data.priority;
        public override SelectableType selectableType => SelectableType.Unit;
        public override UnitStats stats => unitData.stats;
        public override Unit unit => this;
        public override Building building => null;
        public override HealthComponent health => _healthComponent;
        public override Sprite Icon => data.icon;
        public override bool HasCommands => true;
        public override CommandTemplate Commands => data.commands;
        public override float Size => size;
        public override SoundData SelectionClip => data.selectSound;
        public SoundData MoveClip => data.moveSound;
        public SoundData WorkClip => data.workSound;
        public override Collider Collider => selectionCollider;

        public bool IsWorker => isWorker;
        
        [SerializeField] private HealthComponent _healthComponent;

        protected override void OnRegister()
        {
            if (!applyPopulationCostOnEnable) return;
            
            _healthComponent.OnKilled += Kill;

            health.CurrentHealth.SetValue(stats.health);
            health.MaxHealth.SetValue(stats.health);
            
            foreach (var cost in data.costs)
            {
                if (cost.resourceType == ResourceType.Population)
                {
                    SceneReferences.Instance.resourceManager.UpdateResourceValue(ResourceType.Population, cost.value);
                }
            }
        }
        
        protected override void OnDeregister()
        {
            foreach (var cost in data.costs)
            {
                if (cost.resourceType == ResourceType.Population)
                {
                    SceneReferences.Instance.resourceManager.UpdateResourceValue(ResourceType.Population, -cost.value);
                }
            }
        }

        private void Kill()
        {
            _healthComponent.OnKilled -= Kill;
            if(!IsAlive) return;
            DeregisterObject();
            Destroy(gameObject);
        }

        protected WaitForSeconds _recoveryRate = new WaitForSeconds(3f);
        protected IEnumerator Start()
        {
            while (true)
            {
                yield return _recoveryRate;
                
                _healthComponent.ApplyHealthChange(1);
            }
        }
    }
}
