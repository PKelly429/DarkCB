using System;
using System.Collections;
using System.Collections.Generic;
using DataBinding;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace TDCB
{
    [Bindable]
    public class ResourceHarvester : MonoBehaviour, IBuildingPlacementFunctions, IBuildingSelectionFunctions, IBuildingPlacementValidFunction
    {
        [SerializeField] private Building building;
        [SerializeField, Required] private ResourceProducer producer;
        [SerializeField, Required] private WorkerAssignment workerAssignment;
        
        public ResourceType resource;
        public float range;
        public int buildingsMaxWorkers = 4;
        public float productionRate = 1f;

        public BindableTransform iconPosition;
        public BindableSprite resourceIcon;
        public BindableInt currentWorkers;
        public BindableInt resourceProduction;

        private static Dictionary<ResourceType, HashSet<ResourceHarvester>> resourceHarvesters = new Dictionary<ResourceType, HashSet<ResourceHarvester>>();
        
        private bool _inPlacementMode;
        private Vector3 _placementPosition;
        private HashSet<GridCell> resourcesClaimed = new HashSet<GridCell>(40);

        private AbstractBinder uiIcon;

        private bool _showIconPlacement;
        private bool _showIconHover;

        private bool _iconVisible;

        private void OnEnable()
        {
            if (resourceIcon.GetValue() == null)
            {
                resourceIcon.SetValue(SceneReferences.Instance.resourceManager.GetResourceIcon(resource));
            }
            producer.SetResource(resource);

            if (!resourceHarvesters.ContainsKey(resource))
            {
                resourceHarvesters[resource] = new HashSet<ResourceHarvester>();
            }
            resourceHarvesters[resource].Add(this);
        }

        private void OnDisable()
        {
            if (_inPlacementMode) return;
            
            resourceHarvesters[resource].Remove(this);
            SceneReferences.Instance.gridJobs.FreeResources(resourcesClaimed);
            resourcesClaimed.Clear();

            Vector3 pos = transform.position;
            float distance = range * 2f;
            foreach (var other in resourceHarvesters[resource])
            {
                if(other == null) continue;
                if (Vector3.Distance(pos, other.transform.position) < distance)
                {
                    other.TryClaimMoreResources();
                }
            }
            
            producer.OnProductionRateChanged -= ProducerOnOnProductionRateChanged;
            workerAssignment.OnWorkRateChanged -= WorkerAssignmentOnOnWorkRateChanged;
        }

        private void Update()
        {
            if (!_inPlacementMode) return;
            
            _showIconPlacement = building.ValidBuildingPosition;
            UpdateIconVisibility();
            
            if (transform.position == _placementPosition) return;
            _placementPosition = transform.position;
            resourceProduction.SetValue(GetResourceProduction(SceneReferences.Instance.gridJobs.GetAvailableResources(resource, _placementPosition, range)));
            currentWorkers.SetValue(Mathf.Min(resourceProduction, buildingsMaxWorkers));
        }

        private void UpdateIconVisibility()
        {
            bool visible = _showIconPlacement || _showIconHover;
            if (_iconVisible == visible) return;
            _iconVisible = visible;
            if (_iconVisible)
            {
                uiIcon = UIReferences.Instance.resourceHarvesterIconPool.GetIcon();
                uiIcon.Bind(this);
            }
            else
            {
                if (uiIcon != null)
                {
                    UIReferences.Instance.resourceHarvesterIconPool.ReleaseIcon(uiIcon);
                }
            }
        }

        public void OnBeginPlacement()
        {
            _inPlacementMode = true;
            _showIconPlacement = false;
            UpdateIconVisibility();
        }

        public void OnCancelPlacement()
        {
            _showIconPlacement = false;
            UpdateIconVisibility();
        }

        public void OnFinishPlacement()
        {
            _inPlacementMode = false;
            SceneReferences.Instance.gridJobs.ClaimResources(resourcesClaimed, resource, transform.position, range);
            UpdateResourceCount();
            
            _showIconPlacement = false;
            UpdateIconVisibility();
            
            producer.OnProductionRateChanged += ProducerOnOnProductionRateChanged;
            workerAssignment.OnWorkRateChanged += WorkerAssignmentOnOnWorkRateChanged;
        }

        public void TryClaimMoreResources()
        {
            HashSet<GridCell> extra = new HashSet<GridCell>();
            SceneReferences.Instance.gridJobs.ClaimResources(extra, resource, transform.position, range);
            
            resourcesClaimed.AddRange(extra);
            UpdateResourceCount();
        }

        private int GetResourceProduction(int claimed)
        {
            return Mathf.CeilToInt(claimed * productionRate);
        }

        private void UpdateResourceCount()
        {
            producer.SetProductionRate(GetResourceProduction(resourcesClaimed.Count));
            int maxWorkers = Mathf.Min(resourcesClaimed.Count, buildingsMaxWorkers);
            workerAssignment.MaxWorkers.SetValue(maxWorkers);
            
            if (_inPlacementMode)
            {
                resourceProduction.SetValue(resourcesClaimed.Count);
                currentWorkers.SetValue(maxWorkers);
            }
            else
            {
                resourceProduction.SetValue(producer.ProductionRate);
                currentWorkers.SetValue(workerAssignment.CurrentWorkers);
            }
        }
        
        private void ProducerOnOnProductionRateChanged(ResourceType resourceType, int difference)
        {
            if (_inPlacementMode) return;
            
            resourceProduction.SetValue(producer.ProductionRate);
            currentWorkers.SetValue(workerAssignment.CurrentWorkers);
        }

        private void WorkerAssignmentOnOnWorkRateChanged(float workrate)
        {
            if (_inPlacementMode) return;
            
            resourceProduction.SetValue(producer.ProductionRate);
            currentWorkers.SetValue(workerAssignment.CurrentWorkers);
        }

        public bool IsValid()
        {
            return resourceProduction > 0;
        }

        public void UpdateBuildingPlacementValid(bool valid)
        {
        }

        public void OnHoverBegin()
        {
            if (_inPlacementMode) return;
            _showIconHover = true;
            UpdateIconVisibility();
        }

        public void OnHoverEnd()
        {
            _showIconHover = false;
            UpdateIconVisibility();
        }

        public void OnSelect()
        {
        }

        public void OnDeselect()
        {
        }
    }
}
