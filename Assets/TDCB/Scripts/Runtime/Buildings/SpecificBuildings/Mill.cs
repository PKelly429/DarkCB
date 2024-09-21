using System;
using System.Collections;
using System.Collections.Generic;
using DataBinding;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TDCB
{
    [Bindable]
    public class Mill : MonoBehaviour
    {
        [SerializeField, Required] private ParentBuilding _buildingParent;

        private HashSet<WorkerAssignment> _workerAssignments = new HashSet<WorkerAssignment>();
        private HashSet<ResourceProducer> _resourceProducers = new HashSet<ResourceProducer>();
        
        public BindableTransform iconPosition;
        public BindableSprite resourceIcon;
        public BindableInt currentWorkers;
        public BindableInt maxWorkers;
        public BindableInt resourceProduction;
        
        private AbstractBinder uiIcon;
        private bool _iconVisible;
        
        private void OnEnable()
        {
            _buildingParent.OnChildAdded += BuildingParentOnChildAdded;
            _buildingParent.OnChildRemoved += BuildingParentOnChildRemoved;
            _buildingParent.OnChildHovered += BuildingParentOnChildHovered;
            
            resourceIcon.SetValue(SceneReferences.Instance.resourceManager.GetResourceIcon(ResourceType.Food));
        }

        private void OnDisable()
        {
            _buildingParent.OnChildAdded -= BuildingParentOnChildAdded;
            _buildingParent.OnChildRemoved -= BuildingParentOnChildRemoved;
            _buildingParent.OnChildHovered -= BuildingParentOnChildHovered;
        }
        
        private void BuildingParentOnChildAdded(Building child)
        {
            var resourceProducer = child.GetComponent<ResourceProducer>();
            var workerAssignment = child.GetComponent<WorkerAssignment>();

            #if DEBUG
            if (resourceProducer == null || workerAssignment == null)
            {
                Debug.LogError("Child building is not setup correctly, missing producer or worker module");
            }
            #endif

            
            _resourceProducers.Add(resourceProducer);
            _workerAssignments.Add(workerAssignment);
            
            resourceProducer.OnProductionRateChanged += OnChildProductionRateChanged;
            UpdateProductionRates();
        }

        private void BuildingParentOnChildRemoved(Building child)
        {
            var resourceProducer = child.GetComponent<ResourceProducer>();
            var workerAssignment = child.GetComponent<WorkerAssignment>();

#if DEBUG
            if (resourceProducer == null || workerAssignment == null)
            {
                Debug.LogError("Child building is not setup correctly, missing producer or worker module");
            }
#endif

            
            _resourceProducers.Remove(resourceProducer);
            _workerAssignments.Remove(workerAssignment);
            
            resourceProducer.OnProductionRateChanged -= OnChildProductionRateChanged;
            UpdateProductionRates();
        }

        private void BuildingParentOnChildHovered(bool hovered)
        {
            if (_iconVisible != hovered)
            {
                _iconVisible = hovered;
                
                if (_iconVisible)
                {
                    uiIcon = UIReferences.Instance.millIconPool.GetIcon();
                    uiIcon.Bind(this);
                }
                else
                {
                    if (uiIcon != null)
                    {
                        UIReferences.Instance.millIconPool.ReleaseIcon(uiIcon);
                    }
                }
            }
        }
        
        
        private void OnChildProductionRateChanged(ResourceType resource, int difference)
        {
            UpdateProductionRates();
        }

        private void UpdateProductionRates()
        {
            int production = 0;
            int workers = 0;
            int workersMax = 0;

            foreach (var resourceProducer in _resourceProducers)
            {
                production += resourceProducer.ProductionRate;
            }
            
            foreach (var workerAssignment in _workerAssignments)
            {
                workers += workerAssignment.CurrentWorkers;
                workersMax += workerAssignment.MaxWorkers;
            }

            resourceProduction.SetValue(production);
            currentWorkers.SetValue(workers);
            maxWorkers.SetValue(workersMax);
        }
    }
}
