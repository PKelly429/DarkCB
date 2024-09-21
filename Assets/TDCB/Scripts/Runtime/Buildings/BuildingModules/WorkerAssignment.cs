using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public class WorkerAssignment : MonoBehaviour
    {
        public int MaxWorkers;
        public int CurrentWorkers { get; private set; }

        public float WorkRate => MaxWorkers == 0 ? 1 : Mathf.Clamp01(CurrentWorkers / (float)MaxWorkers);
        
        
        public delegate void WorkRateChanged(float workRate);
        public event WorkRateChanged OnWorkRateChanged;

        private HashSet<Unit> workers = new HashSet<Unit>();
        
        public bool Assign(Unit unit)
        {
            if (CurrentWorkers >= MaxWorkers) return false;
            bool added = workers.Add(unit);
            if(added) UpdateWorkRate();
            return added;
        }

        public void Unassign(Unit unit)
        {
            if(workers.Remove(unit)) UpdateWorkRate();
        }
        
        private void UpdateWorkRate()
        {
            if (CurrentWorkers != workers.Count)
            {
                CurrentWorkers = workers.Count;
                OnWorkRateChanged?.Invoke(WorkRate);
            }
        }
    }
}
