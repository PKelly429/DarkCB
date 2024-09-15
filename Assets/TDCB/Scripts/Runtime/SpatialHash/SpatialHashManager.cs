using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine.Jobs;
using UnityEngine.Profiling;
using Vector3 = UnityEngine.Vector3;

namespace TDCB
{
    public class SpatialHashManager : MonoBehaviour, IOnUpdateEarly, IOnUpdateLate
    {
        public const int WorldSize = 1024;
        public const int CellSize = 32;
        public const int CellCount = WorldSize/CellSize;

        public const int Capacity = 1000;

        [SerializeField] private bool _debug;
        [SerializeField] private SpatialHashManager otherFaction;
        
        // Output
        public List<int> closestEnemy = new List<int>();
        
        // Hash grid data
        private List<ISpatialHashable> managedUnits = new List<ISpatialHashable>();
        private TransformAccessArray unitTransforms;
        private NativeList<SpatialHashItem> allUnits;
        private NativeParallelMultiHashMap<SpatialHashCell, int> hashGrid;
        
        // Processing queues
        private List<ISpatialHashable> unitsToRegister = new List<ISpatialHashable>();
        private List<ISpatialHashable> unitsToDeregister = new List<ISpatialHashable>();
        
        private NativeQueue<SpatialHashItem> movedUnits;
        //private NativeList<SpatialHashItem> hashGridRemovals;
        private NativeList<SpatialHashItem> hashGridAdditions;
        
        // Job Handles
        private JobHandle updatePositionsJobHandle;
        private JobHandle hashGridRemovalsJobHandle;
        private JobHandle hashGridAdditionsJobHandle;

        public bool IsValidUnit(int index)
        {
            return index < managedUnits.Count;
        }

        public ISpatialHashable GetUnit(int index)
        {
            return managedUnits[index];
        }

        public NativeParallelMultiHashMap<SpatialHashCell,int>.Enumerator GetUnitIndexsAtCell(int x, int y)
        {
            return hashGrid.GetValuesForKey(new SpatialHashCell()
            {
                X = x,
                Y = y
            });
        }

        public void RegisterUnit(ISpatialHashable unit)
        {
            unitsToRegister.Add(unit);
        }

        public void DeregisterUnit(ISpatialHashable unit)
        {
            unitsToDeregister.Add(unit);
        }

        private void ProcessRegister(ISpatialHashable unit)
        {
            int index = allUnits.Length;
            unit.HashGridIndex = index;
            Vector3 pos = unit.Transform.position;
            SpatialHashItem spatialHashItem = new SpatialHashItem()
            {
                index = unit.HashGridIndex,
                cell = SpatialHashCell.GetCell(pos)
            };
            
            hashGridAdditions.Add(spatialHashItem);
            
            managedUnits.Add(unit);
            allUnits.Add(spatialHashItem);
            unitTransforms.Add(unit.Transform);
            
            closestEnemy.Add(-1);
        }
        
        private void ProcessDeregister(ISpatialHashable unit)
        {
            int id = unit.HashGridIndex;
            if (id < 0) return; // trying to remove unit that had not been registered
            
            //hashGridRemovals.Add(allUnits[^1]);
            
            hashGrid.Remove(allUnits[id].cell, allUnits[id].index);
            
            managedUnits[^1].HashGridIndex = id;
            if (id < allUnits.Length-1) // need to update the unit that gets swapped when deleting
            {
                // movedUnits.Enqueue(new SpatialHashItem()
                // {
                //     index = id,
                //     cell = allUnits[^1].cell
                // });
                hashGridAdditions.Add(new SpatialHashItem()
                {
                    index = id,
                    cell = allUnits[^1].cell
                });
            }

            managedUnits.RemoveAtSwapBack(id);
            allUnits.RemoveAtSwapBack(id);
            unitTransforms.RemoveAtSwapBack(id);
            
            closestEnemy.RemoveAtSwapBack(id);
        }
        
        public void OnEarlyFrameUpdate()
        {
            Profiler.BeginSample("SpatialHash Process Additions and Removals");
            foreach (var unit in unitsToDeregister)
            {
                ProcessDeregister(unit);
            }
            unitsToDeregister.Clear();
            foreach (var unit in unitsToRegister)
            {
                ProcessRegister(unit);
            }
            unitsToRegister.Clear();
            Profiler.EndSample();
            
            Profiler.BeginSample("SpatialHash OnEarlyUpdate");
            var updatePositionsJob = new HashGridCopyPositionsJob()
            {
                items = allUnits,
                movedUnits = movedUnits.AsParallelWriter()
            };
            updatePositionsJobHandle = updatePositionsJob.Schedule(unitTransforms);
            
            var removalJob = new HashGridRemovalsJob()
            {
                //removals = hashGridRemovals,
                items = allUnits,
                movedUnits = movedUnits,
                additions = hashGridAdditions,
                hashGrid = hashGrid
            };

            hashGridRemovalsJobHandle = removalJob.Schedule(updatePositionsJobHandle);
            
            var additionJob = new HashGridAdditionJob()
            {
                additions = hashGridAdditions,
                hashGrid = hashGrid.AsParallelWriter()
            };
            
            hashGridRemovalsJobHandle.Complete();

            hashGridAdditionsJobHandle = additionJob.Schedule(hashGridAdditions.Length, 8, hashGridRemovalsJobHandle);
            Profiler.EndSample();
        }

        public void OnLateFrameUpdate()
        {
            Profiler.BeginSample("SpatialHash OnLateUpdate");
            updatePositionsJobHandle.Complete();
            hashGridAdditionsJobHandle.Complete();
            
            //hashGridRemovals.Clear();
            hashGridAdditions.Clear();
            
            Profiler.EndSample();
        }

        public void LateUpdate()
        {
            float aggroRadius = 30*30f;
            Profiler.BeginSample("SpatialHash Distance Checks");
            foreach (var unit in managedUnits)
            {
                int index = unit.HashGridIndex;
                var cell = allUnits[index].cell;
        
                Vector3 ourPosition = unit.Transform.position;
        
                int best = -1;
                float bestDistance = float.MaxValue;
                for (int x = cell.X - 1; x <= cell.X + 1; x++)
                {
                    for (int y = cell.Y - 1; y <= cell.Y + 1; y++)
                    {
                        var values = otherFaction.GetUnitIndexsAtCell(x, y);
                        foreach (var value in values)
                        {
                            if (!otherFaction.IsValidUnit(value))
                            {
                                Debug.Log($"INVALID UNIT IN CELL: [{x},{y}] : {value}");
                                continue;
                            }
                            
                            var otherUnit = otherFaction.GetUnit(value);
                            float distance = (otherUnit.Transform.position - ourPosition).sqrMagnitude;
                            if (distance > aggroRadius)
                            {
                                #if DEBUG
                                if(_debug) Debug.DrawLine(ourPosition + Vector3.up, otherUnit.Transform.position + Vector3.up, Color.red);
                                #endif
                                continue;
                            }
                            #if DEBUG
                            if(_debug) Debug.DrawLine(ourPosition + Vector3.up, otherUnit.Transform.position + Vector3.up, Color.blue);
                            #endif
                            if (distance < bestDistance)
                            {
                                bestDistance = distance;
                                best = value;
                            }
                        }
                    }
                }

                if (best >= 0)
                {
                    if (!otherFaction.IsValidUnit(best)) continue;
                    #if DEBUG
                    if(_debug) Debug.DrawLine(ourPosition + Vector3.up, otherFaction.GetUnit(best).Transform.position + Vector3.up, Color.yellow);
                    #endif
                }

                closestEnemy[index] = best;
            }
            Profiler.EndSample();
        }

        private void OnEnable()
        {
            SceneReferences.Instance.frameTimings.Register(this);
        }

        private void OnDisable()
        {
            SceneReferences.Instance.frameTimings.Deregister(this);
        }

        private void Awake()
        {
            allUnits = new NativeList<SpatialHashItem>(Capacity, Allocator.Persistent);
            unitTransforms = new TransformAccessArray(Capacity);
            hashGrid = new NativeParallelMultiHashMap<SpatialHashCell, int>(Capacity, Allocator.Persistent);

            movedUnits = new NativeQueue<SpatialHashItem>(Allocator.Persistent);
            //hashGridRemovals = new NativeList<SpatialHashItem>(100, Allocator.Persistent);
            hashGridAdditions = new NativeList<SpatialHashItem>(100, Allocator.Persistent);
        }

        private void OnDestroy()
        {
            hashGridAdditionsJobHandle.Complete();
            
            allUnits.Dispose();
            unitTransforms.Dispose();
            hashGrid.Dispose();
            movedUnits.Dispose();
            //hashGridRemovals.Dispose();
            hashGridAdditions.Dispose();
        }

        private void OnDrawGizmos()
        {
            if (!_debug) return;

            hashGridAdditionsJobHandle.Complete();

            int cellRange = CellCount / 2;
            int halfWorldSize = WorldSize / 2;
            int halfCellSize = CellSize / 2;
            Vector3 cellSize = new Vector3(CellSize-2, CellSize-2, CellSize-2);

            Gizmos.color = Application.isPlaying ? new Color(0.1f, 0.3f, 0.5f, 0.5f) : new Color(0.5f, 0.1f, 0.1f, 0.5f);

            for (int x = -cellRange; x < cellRange; x++)
            {
                for (int y = -cellRange; y < cellRange; y++)
                {
                    int xPos = (x * CellSize);
                    int yPos = (y * CellSize);
                    Vector3 pos = new Vector3(xPos, 0, yPos);
                    Gizmos.DrawCube(pos, cellSize);

                    if (Application.isPlaying)
                    {
                        var cell = new SpatialHashCell()
                        {
                            X = x, Y = y
                        };
                        int count = hashGrid.CountValuesForKey(cell);
                        
                        Handles.Label(pos,$"{count}");
                    }
                }
            }
            Gizmos.color = Color.white;
        }
    }

    public interface ISpatialHashable
    {
        public int HashGridIndex { get; set; }
        public Transform Transform { get; }
    }
    
    public struct SpatialHashCell : IEquatable<SpatialHashCell>
    {
        public int X;
        public int Y;
        
        public static SpatialHashCell GetCell(Vector3 position)
        {
            return new SpatialHashCell()
            {
                X = Mathf.RoundToInt(position.x / SpatialHashManager.CellSize),
                Y = Mathf.RoundToInt(position.z / SpatialHashManager.CellSize),
            };
        }

        public bool Equals(SpatialHashCell other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is SpatialHashCell other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Y*SpatialHashManager.CellCount)+X;
        }
    
        public static bool operator == (SpatialHashCell obj1, SpatialHashCell obj2)
        {
            return obj1.Equals(obj2);
        }
    
        public static bool operator!= (SpatialHashCell obj1, SpatialHashCell obj2)
        {
            return !obj1.Equals(obj2);
        }
    }

    public struct SpatialHashItem
    {
        public int index;
        public SpatialHashCell cell;
    }
    
    [BurstCompile]
    public struct HashGridCopyPositionsJob : IJobParallelForTransform
    {
        [ReadOnly] public NativeList<SpatialHashItem> items;
        [WriteOnly] public NativeQueue<SpatialHashItem>.ParallelWriter movedUnits;
        
        public void Execute(int index, TransformAccess transform)
        {
            SpatialHashCell cell = SpatialHashCell.GetCell(transform.position);

            if (items[index].cell != cell)
            {
                movedUnits.Enqueue(new SpatialHashItem()
                {
                    cell = cell,
                    index = index
                });
            }
        }
    }

    [BurstCompile]
    public struct HashGridRemovalsJob : IJob
    {
        //[ReadOnly] public NativeList<SpatialHashItem> removals;
        public NativeList<SpatialHashItem> items;
        public NativeQueue<SpatialHashItem> movedUnits;
        [WriteOnly] public NativeList<SpatialHashItem> additions;
        [WriteOnly] public NativeParallelMultiHashMap<SpatialHashCell, int> hashGrid;
        
        
        public void Execute()
        {
            // for (int i = 0; i < removals.Length; i++)
            // {
            //     hashGrid.Remove(removals[i].cell, removals[i].index);
            // }

            while (!movedUnits.IsEmpty())
            {
                SpatialHashItem item = movedUnits.Dequeue();
                hashGrid.Remove(items[item.index].cell, item.index);
                items[item.index] = item;
                additions.Add(item);
            }
        }
    }
    
    [BurstCompile]
    public struct HashGridAdditionJob : IJobParallelFor
    {
        [ReadOnly] public NativeList<SpatialHashItem> additions;
        [WriteOnly] public NativeParallelMultiHashMap<SpatialHashCell, int>.ParallelWriter hashGrid;
        
        
        public void Execute(int index)
        {
            hashGrid.Add(additions[index].cell, additions[index].index);
        }
    }
}
