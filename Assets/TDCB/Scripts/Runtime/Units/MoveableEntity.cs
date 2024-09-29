using System;
using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using Pathfinding;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TDCB
{
    public class MoveableEntity : MonoBehaviour, IControllableUnit, IMoveToAttackTarget
    {
        [Required, SerializeField] private Unit unit;
        [Required, SerializeField] private GameObject aiObject;
        [Required, SerializeField] private SelectableObject selectableObject;
        
        public const float MeleeDistance = 3f;
        
        private FollowerEntity ai;
        [SerializeField] private AIDestinationSetter unitTarget;

        public SoundData MoveClip => unit.MoveClip;
        public Vector3 CurrentPosition => ai.position;

        public bool IsAssignedToWorkplace => _assignedToWorkplace;
        
        private bool _hasMoveCommand;
        private bool _assignedToWorkplace;
        private IWorkerAssignment _assignedTo;

        private NearbyUnit _closestEnemy;
        private HealthComponent _enemyHealthComponent;

        private void Awake()
        {
            ai = aiObject.GetComponent<FollowerEntity>();
        }

        public void Move(ISelectable target)
        {
            if(unit.IsWorker) RemoveFromWorkplace();

            if (target.selectableType == SelectableType.Enemy)
            {
                Debug.Log($"Attack: {target}");
            }
            
            if (target.selectableType == SelectableType.Building && unit.IsWorker)
            {
                
                //TODO: Add a worker component
                var workerAssignment = target.building.GetComponent<IWorkerAssignment>();
                if (workerAssignment != null)
                {
                    var assigned = workerAssignment.Assign(unit);
                    _assignedToWorkplace = assigned.WasAssigned;
                    _assignedTo = assigned.AssignedTo;
                    
                    if (assigned.WasAssigned)
                    {
                        SoundManager.Instance.CreateSoundBuilder().Play(unit.WorkClip);
                        Move_Internal(assigned.AssignedTo.Position);
                        return;
                    }
                }
            }
            
            Move_Internal(target.Position);
        }

        public void Move(Vector3 position)
        {
            if(unit.IsWorker) RemoveFromWorkplace();

            IsAbleToAttack = false;
            ai.stopDistance = MeleeDistance;
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
            IsAbleToAttack = true;
            ai.stopDistance = MeleeDistance;
            Move_Internal(position);
        }

        public void Stop()
        {
            _hasMoveCommand = false;
            unitTarget.enabled = true;
            
            ai.destination = ai.position;
            ai.isStopped = true;

            IsAbleToAttack = true;
        }

        public void HoldPosition()
        {
            IsAbleToAttack = true;
        }

        private void Update()
        {
            if (_hasMoveCommand)
            {
                if (ai.reachedDestination)
                {
                    _hasMoveCommand = false;
                    unitTarget.enabled = true;
                    IsAbleToAttack = true;
                }
            }
        }


        public bool IsAbleToAttack { get; private set; } = true;
        public void SetTarget(Transform target)
        {
            unitTarget.target = target;
            transform.LookAt(target);
        }

        public void SetDesiredDistance(float distance)
        {
            if (!IsAbleToAttack) return;
            ai.stopDistance = distance;
        }
    }
}
