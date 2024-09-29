using System;
using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using Pathfinding;
using UnityEngine;

namespace TDCB
{
    public class Enemy : SelectableObject, IMoveToAttackTarget
    {
        [SerializeField] private UnitStats _stats;
        [SerializeField] private AIDestinationSetter unitTarget;
        private FollowerEntity ai;
        
        public override int Priority { get; }
        public override SelectableType selectableType => SelectableType.Enemy;
        public override Unit unit { get; }
        public override Building building { get; }
        public override HealthComponent health => _healthComponent;
        public override Sprite Icon { get; }
        public override bool HasCommands { get; }
        public override CommandTemplate Commands { get; }
        public override float Size { get; }
        public override SoundData SelectionClip { get; }
        public override Collider Collider { get; }
        public override UnitStats stats => _stats;

        private void Awake()
        {
            ai = GetComponent<FollowerEntity>();
        }
        
        [SerializeField] private HealthComponent _healthComponent;

        protected override void OnRegister()
        {
            health.CurrentHealth.SetValue(stats.health);
            health.MaxHealth.SetValue(stats.health);
            
            _healthComponent.OnKilled += Kill;
        }
        
        private void Kill()
        {
            _healthComponent.OnKilled -= Kill;
            if(!IsAlive) return;
            
            DeregisterObject();
            Destroy(gameObject);
        }

        public void MoveToCenterOfMap()
        {
            SetTarget(SceneReferences.Instance.mainBuildingTransform);
        }
        
        public bool IsAbleToAttack => true;
        public void SetTarget(Transform target)
        {
            if(target == null) return;
            
            unitTarget.target = target;
            transform.LookAt(target);
        }
        
        public void SetDesiredDistance(float distance)
        {
            ai.stopDistance = distance;
        }
    }
}
