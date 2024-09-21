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
        [Required, SerializeField] private Unit unit;
        [Required, SerializeField] private GameObject aiObject;
        [Required, SerializeField] private SelectableObject selectableObject;
        
        private IAstarAI ai;
        [SerializeField] private AIDestinationSetter unitTarget;

        public SoundData MoveClip => unit.MoveClip;
        public Vector3 CurrentPosition => ai.position;
        
        private bool _hasMoveCommand;
        private bool _assignedToWorkplace;
        private WorkerAssignment _assignedTo;

        private void Awake()
        {
            ai = aiObject.GetComponent<IAstarAI>();
        }

        public void Move(ISelectable target)
        {
            if(unit.IsWorker) RemoveFromWorkplace();
            
            if (target.selectableType == SelectableType.Building && unit.IsWorker)
            {
                
                //TODO: Add a worker component
                var workerAssignment = target.building.GetComponent<WorkerAssignment>();
                if (workerAssignment != null)
                {
                    _assignedToWorkplace = workerAssignment.Assign(unit);
                    _assignedTo = workerAssignment;

                    SoundManager.Instance.CreateSoundBuilder().Play(unit.WorkClip);
                }
            }
            
            Move_Internal(target.Position);
        }

        public void Move(Vector3 position)
        {
            if(unit.IsWorker) RemoveFromWorkplace();

            Move_Internal(position);
        }

        private void Move_Internal(Vector3 position)
        {
            _hasMoveCommand = true;
            ai.isStopped = false;
            ai.destination = position;
            ai.SearchPath();

            unitTarget.enabled = false;
        }

        private void RemoveFromWorkplace()
        {
            if (_assignedToWorkplace)
            {
                _assignedTo.Unassign(unit);
                _assignedToWorkplace = false;
            }
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
