using System;
using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TDCB
{
    public class BuildingPlacement : MonoBehaviour
    {
        [SerializeField] private Texture2D objectPlacementMap;
        [SerializeField] private SoundData placeBuildingSFX;
        [SerializeField] private SoundData failToPlaceBuildingSFX;
        
        public bool InPlacementMode { get; private set; }
        private GameObject _currentPlacement;
        private Building _currentPlacementBuilding;
        private ISelectable _currentBuilder;
        private ResourceHarvester _resourceHarvester;
        private bool _hasTextureChanges;

        private void Start()
        {
            ClearMap();
        }


        public void StartPlacement(BuildingData building, ISelectable builder)
        {
            #if DEBUG
            if (InPlacementMode)
            {
                Debug.LogError("Already in placement mode");
                return;
            }
            #endif
            
            _currentPlacement = Instantiate(building.buildingPrefab);
            _currentBuilder = builder;
            _currentPlacementBuilding = _currentPlacement.GetComponent<Building>();
            _resourceHarvester = _currentPlacement.GetComponent<ResourceHarvester>();

            var getBuilderComponents = _currentPlacement.GetComponents<IGetBuilder>();
            foreach (var buildingModule in getBuilderComponents)
            {
                buildingModule.GetBuilder(builder);
            }
            
            _currentPlacementBuilding.BeginPlacement();
            
            SetPlacementMode(true);
        }
        
        public (bool, ISelectable) TryCompletePlacement()
        {
            if (!_currentPlacementBuilding.ValidBuildingPosition)
            {
                //TODO: Display Error Message
                SoundManager.Instance.CreateSoundBuilder().Play(failToPlaceBuildingSFX);
                return (false, null);
            }

            var placedBuilding = _currentPlacementBuilding;
            SoundManager.Instance.CreateSoundBuilder().Play(placeBuildingSFX);
            _currentPlacementBuilding.Build();
            //_currentPlacement = null;
            //SetPlacementMode(false);

            InPlacementMode = false;
            StartPlacement(placedBuilding.BuildingData, _currentBuilder);
            
            return (true, placedBuilding);
        }
        
        public void CancelPlacement()
        {
            if (_currentPlacement != null)
            {
                SoundManager.Instance.CreateSoundBuilder().Play(failToPlaceBuildingSFX);
            }
            SetPlacementMode(false);
        }

        private void Update()
        {
            if (!InPlacementMode) return;

            Vector3 mousePos = SceneReferences.Instance.inputHandler.MousePosition;
            Bounds bounds = _currentPlacementBuilding.Collider.bounds;
            Vector3 position = SceneReferences.Instance.gridJobs.GetCenterPosition(mousePos, bounds);
            _currentPlacement.transform.position = position;
            
            _currentPlacementBuilding.ValidBuildingPosition = _currentPlacementBuilding.IsPlacementValid();
            
            if (_resourceHarvester != null)
            {
                if (_currentPlacementBuilding.ValidBuildingPosition)
                {
                    SetResourceMode(_resourceHarvester.resource);
                    SceneReferences.Instance.gridJobs.SetBuildingPlacementPosition(position, _resourceHarvester.range);   
                }
                else
                {
                    SetResourceMode(ResourceType.None);
                }
            }

            RemovePreviousTextureChanges();
            ApplyObjectTextureChanges();
        }


        private void SetPlacementMode(bool active)
        {
            if (InPlacementMode == active) return;

            if (active)
            {
                RemovePreviousTextureChanges();
            }
            else
            {
                if (_currentPlacement != null && _currentPlacement.IsAlive())
                {
                    Destroy(_currentPlacement);
                    _currentPlacement = null;
                }   
                _currentPlacementBuilding = null;
            }
            
            InPlacementMode = active;
            SceneReferences.Instance.gridJobs.ShowGrid = InPlacementMode;
            SetResourceMode(ResourceType.None);
        }

        private void SetResourceMode(ResourceType resourceType)
        {
            SceneReferences.Instance.gridJobs.ShowWoodResource = resourceType == ResourceType.Wood;
            SceneReferences.Instance.gridJobs.ShowStoneResource = resourceType == ResourceType.Stone;
        }

        private GridCell previousMin;
        private GridCell previousMax;
        
        private void ApplyObjectTextureChanges()
        {
            if (_hasTextureChanges)
            {
                SetTexture(previousMin, previousMax, Color.black);
            }

            var bounds = _currentPlacementBuilding.Collider.bounds;
            GridCell min = GridCell.FromWorldPos(bounds.min);
            GridCell max = GridCell.FromWorldPos(bounds.max);
            SetTexture(min, max, Color.red);
            previousMin = min;
            previousMax = max;
            objectPlacementMap.Apply();
            _hasTextureChanges = true;
        }

        public void RemovePreviousTextureChanges()
        {
            if (_hasTextureChanges)
            {
                SetTexture(previousMin, previousMax, Color.black);
                objectPlacementMap.Apply();
                _hasTextureChanges = false;
            }
        }

        private void SetTexture(GridCell min, GridCell max, Color color)
        {
            int width = max.x - min.x;
            int height = max.y - min.y;

            Color[] colors = new Color[width * height];

            if (color != Color.black)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        colors[y * width + x] = color;
                    }
                }
            }

            objectPlacementMap.SetPixels(min.x, min.y, width, height,colors);
        }
        
        [Button]
        private void ClearMap()
        {
            objectPlacementMap.SetPixels(new Color[objectPlacementMap.width*objectPlacementMap.height]);
            objectPlacementMap.Apply();
        }
    }
}
