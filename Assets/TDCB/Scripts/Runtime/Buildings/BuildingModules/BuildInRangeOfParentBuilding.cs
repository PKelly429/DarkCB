using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public class BuildInRangeOfParentBuilding : MonoBehaviour, IBuildingPlacementFunctions, IGetBuilder, IBuildingPlacementValidFunction, IBuildingSelectionFunctions, IBuildingDestroyFunction
    {
        [SerializeField] private Building _building;
        
        private ParentBuilding _parentBuilding;

        private void OnEnable()
        {
            if (_building == null)
            {
                _building = GetComponent<Building>();
            }
        }

        public bool IsValid()
        {
            return Vector3.Distance(_parentBuilding.transform.position, transform.position) < _parentBuilding.BuildingRange;
        }

        public void UpdateBuildingPlacementValid(bool valid)
        {
        }

        public void GetBuilder(ISelectable builder)
        {
            var parentBuilding = (Building)builder;
            if (parentBuilding == null)
            {
                #if DEBUG
                Debug.LogError("BuildInRange component did not get reference to a Building");
                #endif
                enabled = false;
            } 
            _parentBuilding = parentBuilding.GetComponent<ParentBuilding>();
            if (_parentBuilding == null)
            {
#if DEBUG
                Debug.LogError("BuildInRange component did not get reference to a Building");
#endif
                enabled = false;
            } 
        }

        public void OnBeginPlacement()
        {
            if (!enabled) return;
            
            _parentBuilding.BeginPlacingChild();
        }

        public void OnCancelPlacement()
        {
            if (!enabled) return;
            
            _parentBuilding.FinishPlacingChild(null);
        }

        public void OnFinishPlacement()
        {
            if (!enabled) return;
            
            _parentBuilding.FinishPlacingChild(_building);
        }

        public void OnHoverBegin()
        {
            _parentBuilding.BeginHover(_building);
        }

        public void OnHoverEnd()
        {
            _parentBuilding.EndHover(_building);
        }

        public void OnSelect()
        {
        }

        public void OnDeselect()
        {
        }

        public void OnBuildingDestroyed()
        {
            _parentBuilding.RemoveChildBuilding(_building);
        }
    }
}
