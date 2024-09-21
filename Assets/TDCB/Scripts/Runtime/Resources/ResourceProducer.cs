using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public class ResourceProducer : MonoBehaviour
    {
        [SerializeField] private ResourceType resource;
        [SerializeField] private int defaultProductionRate;
        public int ProductionRate { get; private set; }
        public int TargetProductionRate { get; private set; }

        public delegate void RateChanged(ResourceType resource, int difference);
        public event RateChanged OnProductionRateChanged;

        private bool hasWorkerAssignmentModule;
        private WorkerAssignment workerAssignmentModule;

        public void SetResource(ResourceType resourceType)
        {
            resource = resourceType;
        }

        public void SetProductionRate(int value)
        {
            TargetProductionRate = value;

            int target = hasWorkerAssignmentModule ? Mathf.RoundToInt(value*workerAssignmentModule.WorkRate) : value;
            
            int difference = target-ProductionRate;
            ProductionRate = target;
            OnProductionRateChanged?.Invoke(resource, difference);
        }
        
        private void WorkerAssignmentModuleOnOnWorkRateChanged(float workrate)
        {
            SetProductionRate(TargetProductionRate);
        }

        private void OnEnable()
        {
            workerAssignmentModule = GetComponent<WorkerAssignment>();
            hasWorkerAssignmentModule = workerAssignmentModule != null;

            if (hasWorkerAssignmentModule)
            {
                workerAssignmentModule.OnWorkRateChanged += WorkerAssignmentModuleOnOnWorkRateChanged;
            }
            
            SceneReferences.Instance.resourceManager.Register(this);
            SetProductionRate(defaultProductionRate);
        }

        private void OnDisable()
        {
            if (hasWorkerAssignmentModule)
            {
                workerAssignmentModule.OnWorkRateChanged -= WorkerAssignmentModuleOnOnWorkRateChanged;
            }
            
            OnProductionRateChanged?.Invoke(resource, -ProductionRate);
            SceneReferences.Instance.resourceManager.Unregister(this);
        }
    }
}
