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
    public class ResourceHarvester : MonoBehaviour, IBuildingPlacementFunctions, IBuildingSelectionFunctions
    {
        [SerializeField] private Building building;
        [SerializeField, Required] private ResourceProducer producer;
        
        public ResourceType resource;
        public float range;
        public int buildingsMaxWorkers = 4;

        public BindableTransform iconPosition;
        public BindableSprite resourceIcon;
        public BindableInt currentMaxWorkers;
        public BindableInt resourcesInRange;

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
            producer.resource = resource;

            if (!resourceHarvesters.ContainsKey(resource))
            {
                resourceHarvesters[resource] = new HashSet<ResourceHarvester>();
            }
            resourceHarvesters[resource].Add(this);
        }

        private void OnDisable()
        {
            resourceHarvesters[resource].Remove(this);
            SceneReferences.Instance.gridJobs.FreeResources(resourcesClaimed);
            resourcesClaimed.Clear();

            Vector3 pos = transform.position;
            float distance = range * 2f;
            foreach (var other in resourceHarvesters[resource])
            {
                if (Vector3.Distance(pos, other.transform.position) < distance)
                {
                    other.TryClaimMoreResources();
                }
            }
        }

        private void Update()
        {
            if (!_inPlacementMode) return;
            
            _showIconPlacement = building.ValidBuildingPosition;
            UpdateIconVisibility();
            
            if (transform.position == _placementPosition) return;
            _placementPosition = transform.position;
            resourcesInRange.SetValue(SceneReferences.Instance.gridJobs.GetAvailableResources(resource, _placementPosition, range));
            currentMaxWorkers.SetValue(Mathf.Min(resourcesInRange, buildingsMaxWorkers));
        }

        private void UpdateIconVisibility()
        {
            bool visible = _showIconPlacement || _showIconHover;
            if (_iconVisible == visible) return;
            _iconVisible = visible;
            if (_iconVisible)
            {
                uiIcon = UIReferences.Instance.resourceHarvesterIconPool.GetResourceHarvesterIcon();
                uiIcon.Bind(this);
            }
            else
            {
                if (uiIcon != null)
                {
                    UIReferences.Instance.resourceHarvesterIconPool.ReleaseResourceHarvesterIcon(uiIcon);
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
        }

        public void TryClaimMoreResources()
        {
            HashSet<GridCell> extra = new HashSet<GridCell>();
            SceneReferences.Instance.gridJobs.ClaimResources(extra, resource, transform.position, range);
            
            resourcesClaimed.AddRange(extra);
            UpdateResourceCount();
        }

        private void UpdateResourceCount()
        {
            resourcesInRange.SetValue(resourcesClaimed.Count);
            currentMaxWorkers.SetValue(Mathf.Min(resourcesInRange, buildingsMaxWorkers));

            producer.SetProductionRate(resourcesInRange);
        }

        public bool IsValid()
        {
            return resourcesInRange > 0;
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
