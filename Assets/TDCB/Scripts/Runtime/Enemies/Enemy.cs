using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using Pathfinding;
using UnityEngine;

namespace TDCB
{
    public class Enemy : MonoBehaviour, ISpatialHashable
    {
        [SerializeField] private AIDestinationSetter unitTarget;
        private IAstarAI ai;
        
        
        public int HashGridIndex { get; set; }
        public Transform Transform => transform;

        private void Awake()
        {
            ai = GetComponent<IAstarAI>();
        }
        
        protected void OnEnable()
        {
            SceneReferences.Instance.enemyUnitHash.RegisterUnit(this);
        }
        
        protected void OnDisable()
        {
            SceneReferences.Instance.enemyUnitHash.DeregisterUnit(this);
        }
        
        private void Update()
        {
            if (HashGridIndex >= SceneReferences.Instance.playerUnitHash.closestEnemy.Count)
            {
                return;
            }
            int closestUnit = SceneReferences.Instance.enemyUnitHash.closestEnemy[HashGridIndex];
            if (closestUnit >= 0)
            {
                SetTarget(closestUnit);
            }
        }
        
        private void SetTarget(int target)
        {
            if (SceneReferences.Instance.playerUnitHash.IsValidUnit(target))
            {
                unitTarget.target = SceneReferences.Instance.playerUnitHash.GetUnit(target).Transform;
            }
        }
    }
}
