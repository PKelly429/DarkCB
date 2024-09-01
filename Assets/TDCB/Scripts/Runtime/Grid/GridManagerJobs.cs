using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using @ReadOnly = global::Unity.Collections.ReadOnlyAttribute;

namespace TDCB
{
    public class GridManagerJobs : MonoBehaviour
    {
        public const int WorldSize = 1024;
        public const int GridSize = 4;
        
        public const int GridBounds = 1024 / GridSize; // WorldSize / GridSize
        public const int HalfGridBounds = GridBounds / 2;

        public const int MaxUnits = 100;
        
        //private readonly HashSet<FogClearingObject> additions = new HashSet<FogClearingObject>();
        //private readonly HashSet<FogClearingObject> removeals = new HashSet<FogClearingObject>();
        private readonly HashSet<FogClearingObject> fogClearingObjects = new HashSet<FogClearingObject>();
        
        private NativeArray<UnitVision> unitsToProcess;
        private NativeArray<uint> unitsWithVisionInCell;
        //private NativeArray<bool> blockedCells;
        
        private JobHandle _jobHandle;
        private bool _jobRunning;
        
        public void RegisterFogClearingObj(FogClearingObject obj)
        {
            fogClearingObjects.Add(obj);
        }
        
        public void DeregisterFogClearingObj(FogClearingObject obj)
        {
            fogClearingObjects.Remove(obj);
        }

        private void Start()
        {
            unitsToProcess = new NativeArray<UnitVision>(MaxUnits, Allocator.Persistent);
            unitsWithVisionInCell = new NativeArray<uint>(GridBounds * GridBounds, Allocator.Persistent);
        }
        
        private void OnDestroy()
        {
            if (!_jobHandle.IsCompleted)
            {
                _jobHandle.Complete();
            }
            unitsToProcess.Dispose();
            unitsWithVisionInCell.Dispose();
        }

        private void Update()
        {
            int processQueueIndex = 0;
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
                    onlyAdd = !obj.HasBeenAdded
                };

                obj.HasBeenAdded = true;
                obj.GridPosition = newPos;
                processQueueIndex++;
                if(processQueueIndex >= MaxUnits) break;
            }

            if (processQueueIndex > 0)
            {
                var job = new UpdateGridLitJob()
                {
                    units = unitsToProcess,
                    unitsWithVisionInCell = unitsWithVisionInCell,
                    UnitCount = processQueueIndex,
                    GridBounds = GridBounds
                };
                
                _jobHandle = job.Schedule();
                _jobRunning = true;
            }
        }

        private void LateUpdate()
        {
            if (!_jobRunning) return;
            
            Profiler.BeginSample("Complete Job");
            _jobHandle.Complete();
            _jobRunning = false;
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
}
