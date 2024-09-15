using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Sirenix.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using @ReadOnly = global::Unity.Collections.ReadOnlyAttribute;

namespace TDCB
{
    public class GridManagerJobs : MonoBehaviour
    {
        public const int WorldSize = 1024;
        public const int GridSize = 4;
        public const int PathfindingGridSize = 2;
        
        public const int GridBounds = 1024 / GridSize; // WorldSize / GridSize
        public const int HalfGridBounds = GridBounds / 2;

        public const int MaxUnits = 100;
        
        [SerializeField] private AstarPath pathfinding;
        [SerializeField] private Material gridMaterial;
        [SerializeField] private Texture2D gridTexture;
        
        private static readonly int MousePos = Shader.PropertyToID("_MousePos");
        private static readonly int BuildingPos = Shader.PropertyToID("_BuildingPosition");
        private static readonly int BuildingRange = Shader.PropertyToID("_BuildingRange");
        private LocalKeyword _showGridKeyword;
        
        private readonly HashSet<FogClearingObject> additions = new HashSet<FogClearingObject>();
        private readonly List<UnitVision> removals = new List<UnitVision>();
        private readonly HashSet<FogClearingObject> fogClearingObjects = new HashSet<FogClearingObject>();
        
        private NativeArray<UnitVision> unitsToProcess;
        private NativeArray<uint> unitsWithVisionInCell;
        private NativeArray<uint> unitsWithVisionInCellBuffer; // separate array for passing into the job
        
        private NativeArray<ResourceType> resourceInCell;
        private NativeArray<bool> resourceAvailable;
        private NativeArray<bool> resourceAvailableBuffer; // separate array for passing into the job
        
        private NativeArray<bool> blockedCells;
        private NativeArray<bool> blockedCellsBuffer; // separate array for passing into the job
        private NativeArray<Color32> gridTextureData;
        
        private JobHandle _updateLitCellsJobHandle;
        private JobHandle _updateTextureJobHandle;
        private bool _litCalculationJobRunning;
        private bool _updateTextureJobRunning;

        private bool _blockedCellsUpdated;
        
        #region Grid Visibility
        private bool _showGrid;
        public bool ShowGrid
        {
            get => _showGrid;
            set
            {
                _showGrid = value;
                gridMaterial.SetKeyword(_showGridKeyword, value);
            }
        }
        private bool _showWood;
        private static readonly int ShowWood = Shader.PropertyToID("_ShowTrees");

        public bool ShowWoodResource
        {
            get => _showWood;
            set
            {
                _showWood = value;
                gridMaterial.SetFloat(ShowWood, _showWood ? 1 : 0);
            }
        }
        
        private bool _showStone;
        private static readonly int ShowStone = Shader.PropertyToID("_ShowStone");

        public bool ShowStoneResource
        {
            get => _showStone;
            set
            {
                _showStone = value;
                gridMaterial.SetFloat(ShowStone, _showStone ? 1 : 0);
            }
        }

        private Vector2 _buildingPos;
        private float _buildingRange;
        public void SetBuildingPlacementPosition(Vector3 pos, float range)
        {
            _buildingPos = new Vector2(pos.x, pos.z);
            _buildingRange = range;
            gridMaterial.SetVector(BuildingPos, _buildingPos);
            gridMaterial.SetFloat(BuildingRange, _buildingRange);
        }
        
        private void SetMouseGridPos()
        {
            if (!_showGrid) return;
            
            Vector3 worldPos = SceneReferences.Instance.inputHandler.MousePosition;
            Vector2 mousePos = new Vector2(worldPos.x, worldPos.z);
            gridMaterial.SetVector(MousePos, mousePos);
        }
        #endregion
        
        #region Public Methods
        public Vector3 GetWorldPositionFromCell(GridCell cell)
        {
            float halfGrid = GridSize / 2f;
            float worldX = ((cell.x-HalfGridBounds) * GridSize)+halfGrid;
            float worldZ = ((cell.y-HalfGridBounds) * GridSize)+halfGrid;
            //float worldY = SampleTerrainHeight(new Vector3(worldX, 0, worldZ));
            float worldY = 0;
            return new Vector3(worldX, worldY, worldZ);
        }
        
        public Vector3 GetCenterPosition(Vector3 position, Bounds bounds)
        {
            int sizeX = Mathf.CeilToInt(bounds.size.x / GridSize);
            int sizeY = Mathf.CeilToInt(bounds.size.z / GridSize);
            
            int halfGridSize = (int)(GridSize / 2); 
            Vector3 worldPos = GetWorldPositionFromCell(GridCell.FromWorldPos(position));
            
            bool xEven = sizeX % 2 == 0;
            bool yEven = sizeY % 2 == 0;
            if (xEven)
            {
                if (worldPos.x > position.x)
                {
                    worldPos.x -= halfGridSize;
                }
                else
                {
                    worldPos.x += halfGridSize;
                }
            }

            if (yEven)
            {
                if (worldPos.z > position.z)
                {
                    worldPos.z -= halfGridSize;
                }
                else
                {
                    worldPos.z += halfGridSize;
                }
            }

            //float worldY = SampleTerrainHeight(new Vector3(worldX, 0, worldZ));
            float worldY = 0;
            return new Vector3(worldPos.x, worldY, worldPos.z);
        }
        
        public bool IsPositionValid(Bounds bounds)
        {
            GridCell min = GridCell.FromWorldPos(bounds.min);
            GridCell max = GridCell.FromWorldPos(bounds.max);

            for (int x = min.x; x < max.x; x++)
            {
                for (int y = min.y; y < max.y; y++)
                {
                    if (blockedCells[y * GridBounds + x]) return false;
                    if (unitsWithVisionInCell[y * GridBounds + x] < 1) return false;
                }
            }

            return true;
        }
        
        public void SetBoundsBlocked(Bounds bounds, bool blocked)
        {
            if (_disposed) return;
            
            _blockedCellsUpdated = true;
            
            GridCell min = GridCell.FromWorldPos(bounds.min);
            GridCell max = GridCell.FromWorldPos(bounds.max);

            for (int x = min.x; x < max.x; x++)
            {
                for (int y = min.y; y < max.y; y++)
                {
                    blockedCells[y * GridBounds + x] = blocked;
                }
            }

            SetWalkable(min, max, !blocked);
        }
        
        #endregion
        
        #region Unit Registration
        public void RegisterFogClearingObj(FogClearingObject obj)
        {
            fogClearingObjects.Add(obj);
            additions.Add(obj);
        }
        
        public void DeregisterFogClearingObj(FogClearingObject obj)
        {
            fogClearingObjects.Remove(obj);
            removals.Add(new UnitVision()
            {
                lastGridCell = obj.GridPosition,
                newGridCell = obj.GridPosition,
                radius = obj.Radius,
                onlyRemove = true
            });
        }
        #endregion
        
        #region Update Pathfinding
        private void SetWalkable(GridCell min, GridCell max, bool walkable)
        {
            int gridDifference = GridSize / PathfindingGridSize;
            int halfGrid = gridDifference / 2;
            
            int minX = min.x * gridDifference;
            int minY = min.y * gridDifference;
            int maxX = (max.x * gridDifference)-halfGrid;
            int maxY = (max.y * gridDifference)-halfGrid;
            int width = (maxX - minX)+1;
            int height = (maxY - minY)+1;

            pathfinding.AddWorkItem(new AstarWorkItem(() => 
            {
                if (pathfinding.graphs[0] is not GridGraph gridGraph) return;

                bool[] walkableArray = new bool[width * height];
                for (int i = 0; i < walkableArray.Length; i++)
                {
                    walkableArray[i] = walkable;
                }
                gridGraph.SetWalkability(walkableArray, new IntRect(minX, minY, maxX, maxY));
            }));
        }
        #endregion
        
        #region Resource Buildings
        private HashSet<GridCell> _nearbyGridCells = new HashSet<GridCell>();
        public int GetAvailableResources(ResourceType resourceType, Vector3 pos, float distance)
        {
            GetAvailableResourceCells(_nearbyGridCells, resourceType, pos, distance);

            return _nearbyGridCells.Count;
        }
        public void ClaimResources(HashSet<GridCell> output, ResourceType resourceType, Vector3 pos, float distance)
        {
            GetAvailableResourceCells(output, resourceType, pos, distance);

            foreach (var cell in output)
            {
                resourceAvailable[cell.y * GridBounds + cell.x] = false;
            }
        }

        public void FreeResources(HashSet<GridCell> claimed)
        {
            if (_disposed) return;
            
            foreach (var cell in claimed)
            {
                resourceAvailable[cell.y * GridBounds + cell.x] = true;
            }
        }

        public void GetAvailableResourceCells(HashSet<GridCell> gridCells, ResourceType resourceType, Vector3 pos, float distance)
        {
            gridCells.Clear();
            
            float xPos = (pos.x / GridSize) + HalfGridBounds;
            float yPos = (pos.z / GridSize) + HalfGridBounds;
            
            int radius = Mathf.FloorToInt(distance / GridSize);
            int radiusSquared = radius*radius;
            for (int x = Mathf.FloorToInt(xPos - radius); x <= Mathf.CeilToInt(xPos + radius); x++)
            {
                if (x is < 0 or >= GridBounds) continue;

                float xOffset = x - xPos;
                int height = (int)Math.Sqrt(radiusSquared - (xOffset * xOffset));

                for (int y = Mathf.FloorToInt(yPos - height); y <= Mathf.CeilToInt(yPos + height); y++)
                {
                    if (y is < 0 or >= GridBounds) continue;
                    if(resourceInCell[y*GridBounds+x] != resourceType) continue;
                    if(!resourceAvailable[y*GridBounds+x]) continue;
                    gridCells.Add(new GridCell(x, y));
                }
            }
        }

        #endregion

        private void Awake()
        {
            unitsToProcess = new NativeArray<UnitVision>(MaxUnits, Allocator.Persistent);
            unitsWithVisionInCell = new NativeArray<uint>(GridBounds * GridBounds, Allocator.Persistent);
            unitsWithVisionInCellBuffer = new NativeArray<uint>(GridBounds * GridBounds, Allocator.Persistent);
            resourceInCell = new NativeArray<ResourceType>(GridBounds * GridBounds, Allocator.Persistent);
            resourceAvailable = new NativeArray<bool>(GridBounds * GridBounds, Allocator.Persistent);
            resourceAvailableBuffer = new NativeArray<bool>(GridBounds * GridBounds, Allocator.Persistent);
            blockedCells = new NativeArray<bool>(GridBounds * GridBounds, Allocator.Persistent);
            blockedCellsBuffer = new NativeArray<bool>(GridBounds * GridBounds, Allocator.Persistent);
            
            InitialiseGridWithTerrain();
            gridTextureData = gridTexture.GetRawTextureData<Color32>();

            for (int x = 0; x < GridBounds; x++)
            {
                for (int y = 0; y < GridBounds; y++)
                {
                    byte sample = gridTextureData[y * GridBounds + x].g;
                    if (sample > 0)
                    {
                        if (sample == ResourceType.Wood.GetResourceTexMapColour())
                        {
                            resourceInCell[y * GridBounds + x] = ResourceType.Wood;
                            resourceAvailable[y * GridBounds + x] = true;
                        }
                        if (sample == ResourceType.Stone.GetResourceTexMapColour())
                        {
                            resourceInCell[y * GridBounds + x] = ResourceType.Stone;
                            resourceAvailable[y * GridBounds + x] = true;
                        }
                    }
                }
            }
            
            _showGridKeyword = new LocalKeyword(gridMaterial.shader, "_SHOWGRID_ON");
        }
        
        private void InitialiseGridWithTerrain()
        {
            for (int x = 0; x < GridBounds; x++)
            {
                for (int y = 0; y < GridBounds; y++)
                {
                    if (gridTexture.GetPixel(x, y).r > 0.5f)
                    {
                        blockedCells[y * GridBounds + x] = true;
                        blockedCellsBuffer[y * GridBounds + x] = true;
                    }
                }
            }
        }

        private bool _disposed;
        private void OnDestroy()
        {
            _disposed = true;
            
            if (!_updateLitCellsJobHandle.IsCompleted)
            {
                _updateLitCellsJobHandle.Complete();
            }
            if (!_updateTextureJobHandle.IsCompleted)
            {
                _updateLitCellsJobHandle.Complete();
            }
            unitsToProcess.Dispose();
            unitsWithVisionInCell.Dispose();
            unitsWithVisionInCellBuffer.Dispose();
            resourceInCell.Dispose();
            resourceAvailable.Dispose();
            resourceAvailableBuffer.Dispose();
            blockedCells.Dispose();
            blockedCellsBuffer.Dispose();
            gridTextureData.Dispose();
        }

        private void Update()
        {
            SetMouseGridPos();
            
            int processQueueIndex = 0;
            
            foreach (var removal in removals)
            {
                unitsToProcess[processQueueIndex] = removal;
                processQueueIndex++;
            }
            
            foreach (var obj in fogClearingObjects)
            {
                // skip if hasn't moved
                GridCell newPos = GridCell.FromWorldPos(obj.Position);
                if(obj.GridPosition.Equals(newPos)) continue;
                
                unitsToProcess[processQueueIndex] = new UnitVision()
                {
                    newGridCell = newPos,
                    lastGridCell = obj.GridPosition,
                    radius = obj.Radius,
                    onlyAdd = additions.Contains(obj)
                };
                
                obj.GridPosition = newPos;
                processQueueIndex++;
                if(processQueueIndex >= MaxUnits) break;
            }

            additions.Clear();
            removals.Clear();

            if (processQueueIndex > 0)
            {
                var gridLitJob = new UpdateGridLitJob()
                {
                    units = unitsToProcess,
                    unitsWithVisionInCell = unitsWithVisionInCellBuffer,
                    UnitCount = processQueueIndex,
                    GridBounds = GridBounds
                };
                
                _updateLitCellsJobHandle = gridLitJob.Schedule();
                _litCalculationJobRunning = true;
                
                var updateTextureJob = new UpdateGridTextureJob()
                {
                    blockedCells = blockedCellsBuffer,
                    resourceInCell = resourceInCell,
                    resourceAvailable = resourceAvailableBuffer,
                    unitsWithVisionInCell = unitsWithVisionInCellBuffer,
                    texture = gridTextureData
                };
                _updateTextureJobHandle = updateTextureJob.Schedule(GridBounds*GridBounds, 8, _updateLitCellsJobHandle);
                _updateTextureJobRunning = true;

            }
            else if (_blockedCellsUpdated) // just update texture
            {
                var updateTextureJob = new UpdateGridTextureJob()
                {
                    blockedCells = blockedCellsBuffer,
                    resourceInCell = resourceInCell,
                    resourceAvailable = resourceAvailableBuffer,
                    unitsWithVisionInCell = unitsWithVisionInCellBuffer,
                    texture = gridTextureData
                };
                _updateTextureJobHandle = updateTextureJob.Schedule(GridBounds*GridBounds, 8);
                _updateTextureJobRunning = true;
            }
        }

        private void LateUpdate()
        {
            if (!_litCalculationJobRunning && !_updateTextureJobRunning) return;
            
            Profiler.BeginSample("Complete Job");
            if(_litCalculationJobRunning) _updateLitCellsJobHandle.Complete();
            _litCalculationJobRunning = false;
            if(_updateTextureJobRunning) _updateTextureJobHandle.Complete();
            _updateTextureJobRunning = false;
            Profiler.EndSample();
            
            Profiler.BeginSample("Apply Texture");
            gridTexture.Apply();
            Profiler.EndSample();

            Profiler.BeginSample("Copy blocked cell buffer");
            blockedCells.CopyTo(blockedCellsBuffer);
            resourceAvailable.CopyTo(resourceAvailableBuffer);
            unitsWithVisionInCellBuffer.CopyTo(unitsWithVisionInCell);
            Profiler.EndSample();
        }

        // private void OnDrawGizmos()
        // {
        //     if (_jobRunning)
        //     {
        //         _jobHandle.Complete();
        //     }
        //     
        //     Profiler.BeginSample("Gizmos");
        //     float size = 0.25f;
        //     for (int i = 0; i < unitsWithVisionInCell.Length; i++)
        //     {
        //         if(unitsWithVisionInCell[i] == 0) continue;
        //         
        //         int x = i % GridBounds;
        //         int y = i/GridBounds;
        //
        //         Color color = unitsWithVisionInCell[i] == 1 ? Color.red : Color.yellow;
        //         if(unitsWithVisionInCell[i] > 2) color = Color.white;
        //         Gizmos.color = color;
        //         Gizmos.DrawSphere(SceneReferences.Instance.gridManager.GetWorldPositionFromCell(new GridCell(x, y)), size*unitsWithVisionInCell[i]);
        //     }
        //     Gizmos.color = Color.white;
        //     Profiler.EndSample();
        // }
    }

    #region Update Lit Objects Job
    public struct UnitVision
    {
        public GridCell lastGridCell;
        public GridCell newGridCell;
        public int radius;

        public bool onlyRemove;
        public bool onlyAdd;
    }

    [BurstCompile(FloatMode = FloatMode.Fast)]
    public struct UpdateGridLitJob : IJob
    {
        [@ReadOnly] public NativeArray<UnitVision> units;
        public NativeArray<uint> unitsWithVisionInCell;

        public int UnitCount;
        public int GridBounds;
        
        public void Execute()
        {
            for (int i = 0; i < UnitCount; i++)
            {
                UnitVision unit = units[i];
                
                int radius = unit.radius;
                int radiusSquared = radius * radius;
                if (unit.onlyAdd)
                {
                    IterateAllCells(unit.newGridCell.x, unit.newGridCell.y, radius, radiusSquared, true);
                    continue;
                }
                if (unit.onlyRemove)
                {
                    IterateAllCells(unit.newGridCell.x, unit.newGridCell.y, radius, radiusSquared, false);
                    continue;
                }
                
                IterateAllCells(unit.lastGridCell.x, unit.lastGridCell.y, radius, radiusSquared, false);
                IterateAllCells(unit.newGridCell.x, unit.newGridCell.y, radius, radiusSquared, true);
            }
        }

        private void IterateAllCells(int xPos, int yPos, int radius, int radiusSquared, bool increment)
        {
            for (int x = xPos - radius; x < xPos + radius; x++)
            {
                if (x < 0 || x >= GridBounds) continue;

                int xOffset = x - xPos;
                int height = (int)Math.Sqrt(radiusSquared - (xOffset * xOffset));
                if (height >= radius) height = radius-1; // make the circle slightly elliptical

                for (int y = yPos - height; y < yPos + height; y++)
                {
                    if (y < 0 || y >= GridBounds) continue;

                    if (increment)
                    {
                        unitsWithVisionInCell[y * GridBounds + x]++;
                    }
                    else
                    {
                        unitsWithVisionInCell[y * GridBounds + x]--;   
                    }
                }
            }
        }
    }
    #endregion
    
    #region Update Texture Job
    [BurstCompile]
    public struct UpdateGridTextureJob : IJobParallelFor
    {
        [@ReadOnly] public NativeArray<bool> blockedCells;
        [@ReadOnly] public NativeArray<ResourceType> resourceInCell;
        [@ReadOnly] public NativeArray<bool> resourceAvailable;
        [@ReadOnly] public NativeArray<uint> unitsWithVisionInCell;
        public NativeArray<Color32> texture;
        public void Execute(int index)
        {
            byte r = blockedCells[index] ? byte.MaxValue : byte.MinValue;
            byte g = resourceInCell[index].GetResourceTexMapColour();
            byte b = resourceAvailable[index] ? byte.MaxValue : byte.MinValue;
            byte a = unitsWithVisionInCell[index] > 0 ? byte.MaxValue : byte.MinValue;
            texture[index] = new Color32(r, g, b, a);
        }
    }
    #endregion
}
