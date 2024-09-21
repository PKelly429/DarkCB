using System;
using System.Collections;
using System.Collections.Generic;
using Shapes;
using UnityEngine;

namespace TDCB
{
    public class ParentBuilding : MonoBehaviour
    {
        [SerializeField] private Disc[] _rangeIndicators;
        [SerializeField] private float _buildingRange;
        
        private HashSet<Building> _childBuildings = new HashSet<Building>();
        private HashSet<Building> _hovered = new HashSet<Building>();
        
        public delegate void ChildAdded(Building child);
        public event ChildAdded OnChildAdded;
        
        public delegate void ChildRemoved(Building child);
        public event ChildRemoved OnChildRemoved;

        public delegate void ChildHover(bool hovered);
        public event ChildHover OnChildHovered;

        public float BuildingRange => _buildingRange;

        public HashSet<Building> GetChildren => _childBuildings;

        public void BeginPlacingChild()
        {
            foreach (var rangeIndicator in _rangeIndicators)
            {
                rangeIndicator.enabled = true;
                rangeIndicator.Radius = _buildingRange;
            }
        }
        
        public void FinishPlacingChild(Building childBuilding)
        {
            foreach (var rangeIndicator in _rangeIndicators)
            {
                rangeIndicator.enabled = false;
            }
            if (childBuilding == null) return;

            if (_childBuildings.Add(childBuilding))
            {
                OnChildAdded?.Invoke(childBuilding);
            }
        }
        
        public void RemoveChildBuilding(Building childBuilding)
        {
            if (_childBuildings.Remove(childBuilding))
            {
                OnChildRemoved?.Invoke(childBuilding);
            }

            _hovered.Remove(childBuilding);
            OnChildHovered?.Invoke(_hovered.Count > 0);
        }

        public void BeginHover(Building child)
        {
            if (!_childBuildings.Contains(child)) return;
            _hovered.Add(child);
            
            OnChildHovered?.Invoke(_hovered.Count > 0);
        }
        
        public void EndHover(Building child)
        {
            _hovered.Remove(child);
            
            OnChildHovered?.Invoke(_hovered.Count > 0);
        }
    }
}
