using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TDCB
{
    public class AttackComponent : MonoBehaviour
    {
        [SerializeField] private bool enemy;
        [SerializeField, Required] private SelectableObject selectableObject;
        [SerializeField, Required] private AttackEffect attackEffect;
        
        private IMoveToAttackTarget _moveComponent;
        private bool _hasMoveComponent;
        private UnitStats _stats;

        private float attackCooldown;
        private float attackDelay;
        private float range;

        private bool AbleToAttack => !_hasMoveComponent || _moveComponent.IsAbleToAttack; // prevent attacking if ordering to move

        private bool IsValidUnit()
        {
            if (enemy)
            {
                return selectableObject.HashGridIndex < SceneReferences.Instance.enemyUnitHash.closestEnemy.Count;   
            }
            
            return selectableObject.HashGridIndex < SceneReferences.Instance.playerUnitHash.closestEnemy.Count;
        }
        
        private NearbyUnit GetClosestUnit()
        {
            if (enemy)
            {
                return SceneReferences.Instance.enemyUnitHash.closestEnemy[selectableObject.HashGridIndex];    
            }
            return SceneReferences.Instance.playerUnitHash.closestEnemy[selectableObject.HashGridIndex];
        }
        
        private Transform GetClosestUnitTransform(int id)
        {
            if (enemy)
            {
                if (!SceneReferences.Instance.playerUnitHash.IsValidUnit(id)) return null;
                return SceneReferences.Instance.playerUnitHash.GetUnit(id).Transform;  
            }

            if (!SceneReferences.Instance.enemyUnitHash.IsValidUnit(id)) return null;
            return SceneReferences.Instance.enemyUnitHash.GetUnit(id).Transform;
        }
        
        private NearbyUnit _closestEnemy;
        private SelectableObject _target;
        
        public void Awake()
        {
            _stats = selectableObject.stats;
            _moveComponent = GetComponent<IMoveToAttackTarget>();
            _hasMoveComponent = _moveComponent != null;

            range = (_stats.range * _stats.range) + selectableObject.Size;
            attackDelay = _stats.attackDelay;
        }

        private void Update()
        {
            if (attackCooldown > 0)
            {
                attackCooldown -= Time.deltaTime;
            }

            if (!AbleToAttack) return;
            
            if (!IsValidUnit())
            {
                return;
            }

            _closestEnemy = GetClosestUnit();
            if (_closestEnemy.hasNearbyUnit)
            {
                if (!SetTarget(_closestEnemy.id)) return;

                if (attackCooldown > 0) return;
                if (_stats.damage < 1) return;
                if (_target.health != null && _closestEnemy.sqDistance < range + (_target.Size*_target.Size))
                {
                    attackDelay -= Time.deltaTime;
                    if (attackDelay > 0) return;
                    
                    attackCooldown = _stats.attackSpeed;
                    attackEffect.OnAttack(_target, () => { ApplyDamage(_target); });
                    attackDelay = _stats.attackDelay;
                }
                else
                {
                    attackDelay = _stats.attackDelay;
                }
            }
            else
            {
                _moveComponent.SetTarget(null);
                _moveComponent.SetDesiredDistance(MoveableEntity.MeleeDistance);
                attackDelay = _stats.attackDelay;
            }
        }

        private void ApplyDamage(SelectableObject target)
        {
            if (target.IsAlive)
            {
                target.health.ApplyDamage(CalculateDamage(_stats.damage, _stats.damageType, _target.stats.armor));
            }
        }
        
        private bool SetTarget(int target)
        {
            Transform enemyTransform = GetClosestUnitTransform(target);
            _moveComponent.SetTarget(enemyTransform);
            _moveComponent.SetDesiredDistance(_stats.range);
            if (enemyTransform == null)
            {
                _target = null;
                return false;
            }
            _target = enemyTransform.GetComponent<SelectableObject>();
            return true;
        }

        private int CalculateDamage(int damage, DamageTypes damageType, Armor armor)
        {
            switch (damageType)
            {
                case DamageTypes.Melee:
                    damage -= armor.melee;
                    break;
                case DamageTypes.Piercing:
                    damage -= armor.piercing;
                    break;
            }

            return Mathf.Max(damage, 1);
        }
    }
    
    public interface IMoveToAttackTarget
    {
        public bool IsAbleToAttack { get; }
        public void SetTarget(Transform transform);
        public void SetDesiredDistance(float distance);
    }
}
