using System.Collections;
using System.Collections.Generic;
using DataBinding;
using UnityEngine;

namespace TDCB
{
    [Bindable]
    public class WorkerAssignment : MonoBehaviour, IWorkerAssignment
    {
        public BindableInt MaxWorkers;
        public BindableInt CurrentWorkers { get; private set; } = new BindableInt(0);

        public float WorkRate => MaxWorkers == 0 ? 1 : Mathf.Clamp01(CurrentWorkers.GetValue() / (float)MaxWorkers.GetValue());

        public Vector3 Position => transform.position;
        
        
        public delegate void WorkRateChanged(float workRate);
        public event WorkRateChanged OnWorkRateChanged;

        private HashSet<Unit> workers = new HashSet<Unit>();
        
        public AssignmentArgs Assign(Unit unit)
        {
            if (CurrentWorkers.GetValue() >= MaxWorkers.GetValue()) return new AssignmentArgs(){WasAssigned = false};
            bool added = workers.Add(unit);
            if(added) UpdateWorkRate();
            return new AssignmentArgs()
            {
                WasAssigned = true,
                AssignedTo = this
            };
        }

        public void Unassign(Unit unit)
        {
            if(workers.Remove(unit)) UpdateWorkRate();
        }
        
        private void UpdateWorkRate()
        {
            if (CurrentWorkers.GetValue() != workers.Count)
            {
                CurrentWorkers.SetValue(workers.Count);
                OnWorkRateChanged?.Invoke(WorkRate);
            }
        }
    }
}
