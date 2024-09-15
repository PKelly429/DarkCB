using System;
using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using Pathfinding;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TDCB
{
    public class MoveableEntity : MonoBehaviour, IControllableUnit
    {
        [Required, SerializeField] private GameObject aiObject;
        [Required, SerializeField] private SelectableObject selectableObject;
        [SerializeField] private SoundData moveSoundData;
        
        private IAstarAI ai;
        [SerializeField] private AIDestinationSetter unitTarget;

        private bool _hasMoveCommand;

        public SoundData MoveClip => moveSoundData;
        public Vector3 CurrentPosition => ai.position;

        private void Awake()
        {
            ai = aiObject.GetComponent<IAstarAI>();
        }

        public void Move(Vector3 position)
        {
            _hasMoveCommand = true;
            ai.isStopped = false;
            ai.destination = position;
            ai.SearchPath();

            unitTarget.enabled = false;
        }

        public void AttackMove(Vector3 position)
        {
        }

        public void Stop()
        {
            _hasMoveCommand = false;
            unitTarget.enabled = true;
            
            ai.destination = ai.position;
            ai.isStopped = true;
        }

        public void HoldPosition()
        {
        }
        
        private void Update()
        {
            if (_hasMoveCommand)
            {
                if (ai.reachedDestination)
                {
                    _hasMoveCommand = false;
                    unitTarget.enabled = true;
                }
                return;
            }
            if (selectableObject.HashGridIndex >= SceneReferences.Instance.playerUnitHash.closestEnemy.Count)
            {
                return;
            }
            int closestUnit = SceneReferences.Instance.playerUnitHash.closestEnemy[selectableObject.HashGridIndex];
            if (closestUnit >= 0)
            {
                SetTarget(closestUnit);
            }
            else
            {
                unitTarget.target = null;
            }
        }
        
        private void SetTarget(int target)
        {
            if (SceneReferences.Instance.enemyUnitHash.IsValidUnit(target))
            {
                unitTarget.target = SceneReferences.Instance.enemyUnitHash.GetUnit(target).Transform;
            }
        }
    }
}
