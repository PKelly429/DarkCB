using System;
using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using UnityEngine;

namespace TDCB
{
    public class BuildingPlacement : MonoBehaviour
    {
        [SerializeField] private SoundData failToPlaceBuildingSFX;
        
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
            if (!_currentPlacementBuilding.ValidBuildingPosition)
            {
                //TODO: Display Error Message
                SoundManager.Instance.CreateSoundBuilder().Play(failToPlaceBuildingSFX);
                return;
            }
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

            Vector3 mousePos = SceneReferences.Instance.inputHandler.MousePosition;
            Bounds bounds = _currentPlacementBuilding.Collider.bounds;
            _currentPlacement.transform.position = SceneReferences.Instance.gridJobs.GetCenterPosition(mousePos, bounds);
            
            _currentPlacementBuilding.ValidBuildingPosition = SceneReferences.Instance.gridJobs.IsPositionValid(bounds);
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
            SceneReferences.Instance.gridJobs.ShowGrid = InPlacementMode;
        }
    }
}
