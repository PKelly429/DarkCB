using System;
using System.Collections;
using System.Collections.Generic;
using DataBinding;
using UnityEngine;

namespace TDCB
{
    [Bindable]
    public class ResourceProducer : MonoBehaviour, IBuildingPlacementFunctions
    {
        [SerializeField] private ResourceType resource;
        [SerializeField] private int defaultProductionRate;
        public BindableInt ProductionRate { get; private set; } = new BindableInt(0);
        public int TargetProductionRate { get; private set; } 

        public delegate void RateChanged(ResourceType resource, int difference);
        public event RateChanged OnProductionRateChanged;

        private bool hasWorkerAssignmentModule;
        private WorkerAssignment workerAssignmentModule;

        private bool _isRegistered;

        public void SetResource(ResourceType resourceType)
        {
            resource = resourceType;
        }

        public void SetProductionRate(int value)
        {
            TargetProductionRate = value;

            int target = hasWorkerAssignmentModule ? Mathf.RoundToInt(value*workerAssignmentModule.WorkRate) : value;
            
            int difference = target-ProductionRate;
            ProductionRate.SetValue(target);
            OnProductionRateChanged?.Invoke(resource, difference);
        }
        
        private void WorkerAssignmentModuleOnOnWorkRateChanged(float workrate)
        {
            SetProductionRate(TargetProductionRate);
        }

        private void OnDisable()
        {
            if (!_isRegistered) return;
            if (hasWorkerAssignmentModule)
            {
                workerAssignmentModule.OnWorkRateChanged -= WorkerAssignmentModuleOnOnWorkRateChanged;
            }
            
            OnProductionRateChanged?.Invoke(resource, -ProductionRate);
            SceneReferences.Instance.resourceManager.Unregister(this);
            _isRegistered = false;
        }

        public void OnBeginPlacement()
        {
        }

        public void OnCancelPlacement()
        {
        }

        public void OnFinishPlacement()
        {
            _isRegistered = true;
            
            workerAssignmentModule = GetComponent<WorkerAssignment>();
            hasWorkerAssignmentModule = workerAssignmentModule != null;

            if (hasWorkerAssignmentModule)
            {
                workerAssignmentModule.OnWorkRateChanged += WorkerAssignmentModuleOnOnWorkRateChanged;
            }
            
            SceneReferences.Instance.resourceManager.Register(this);
            SetProductionRate(defaultProductionRate);
        }
    }
}
