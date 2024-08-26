using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public class BuildingPlacement : MonoBehaviour
    {
        public bool InPlacementMode { get; private set; }
        private GameObject _currentPlacement;
        private Building _currentPlacementBuilding;
        
        
        public void StartPlacement(BuildingData building)
        {
            SetPlacementMode(true);

            _currentPlacement = Instantiate(building.buildingPrefab);
            _currentPlacementBuilding = _currentPlacement.GetComponent<Building>();
        }
        
        public void TryCompletePlacement()
        {
            _currentPlacementBuilding.Build();
            _currentPlacement = null;
            SetPlacementMode(false);
        }
        
        public void CancelPlacement()
        {
            SetPlacementMode(false);
        }

        private void Update()
        {
            if (!InPlacementMode) return;

            GridCell mousePos = GridCell.FromWorldPos(SceneReferences.Instance.inputHandler.MousePosition);
            _currentPlacement.transform.position = SceneReferences.Instance.gridManager.GetWorldPositionFromCell(mousePos);
            
            _currentPlacementBuilding.ValidBuildingPosition = SceneReferences.Instance.gridManager.IsPositionValid(_currentPlacementBuilding.Collider.bounds);
        }


        private void SetPlacementMode(bool active)
        {
            if (InPlacementMode == active) return;

            if (_currentPlacement != null && _currentPlacement.IsAlive())
            {
                Destroy(_currentPlacement);
                _currentPlacement = null;
            }

            _currentPlacementBuilding = null;
            InPlacementMode = active;
            SceneReferences.Instance.gridManager.ShowGrid = InPlacementMode;
        }
    }
}
