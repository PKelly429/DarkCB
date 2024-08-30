using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Profiling;
using VolumetricFogAndMist2;

namespace TDCB
{
    public class FogOfWarManager : MonoBehaviour
    {
        [SerializeField] private VolumetricFog fogOfWar;
        [SerializeField] private VolumetricFog fog;
        
        private static readonly HashSet<FogClearingObject> staticFogClearingObjects = new HashSet<FogClearingObject>();
        private static readonly HashSet<FogClearingObject> dynamicFogClearingObjects = new HashSet<FogClearingObject>();

        // could use a count to avoid recalculating
        [SerializeField] private HashSet<GridCell> _staticallyLitCells = new HashSet<GridCell>();
        [SerializeField] private HashSet<GridCell> _dynamicallyLitCells = new HashSet<GridCell>();

        private bool _recalculateStaticObjs;

        public void RegisterFogClearingObj(FogClearingObject obj)
        {
            if (obj.IsStatic)
            {
                staticFogClearingObjects.Add(obj);
                AddLitStateForObject(obj);
            }
            // else
            // {
            //     dynamicFogClearingObjects.Add(obj);
            // }
        }
        
        public void DeregisterFogClearingObj(FogClearingObject obj)
        {
            if (obj.IsStatic)
            {
                staticFogClearingObjects.Remove(obj);
                _recalculateStaticObjs = true;
            }
            // else
            // {
            //     dynamicFogClearingObjects.Remove(obj);
            // }
        }
        
        private void Start()
        {
            //fogOfWar.ResetFogOfWar(1);
            //fog.ResetFogOfWar(1);
        }
        
        #if UNITY_EDITOR
        private void OnDestroy()
        {
            if (Application.isPlaying)
            {
                //fogOfWar.ResetFogOfWar(0);
                //fog.ResetFogOfWar(0);
            }
        }
        #endif 

        private void Update()
        {
            // foreach (var obj in dynamicFogClearingObjects)
            // {
            //     SetFogOfWarAlpha(obj.Position, obj.Radius, 0);
            // }
            
            Profiler.BeginSample("Update Lit GridCells");
            if (_recalculateStaticObjs)
            {
                _recalculateStaticObjs = false;
                foreach (var cell in _staticallyLitCells)
                {
                    SceneReferences.Instance.gridManager.ClearGridCellState(cell, GridState.Lit);
                    SceneReferences.Instance.gridManager.ApplyGridCellColor(cell);
                }
                _staticallyLitCells.Clear();
                
                foreach (var obj in staticFogClearingObjects)
                {
                    AddLitStateForObject(obj);
                }
            }
            
            // Profiler.BeginSample("Clear Previous Cells");
            // foreach (var cell in _dynamicallyLitCells)
            // {
            //     SceneReferences.Instance.gridManager.ClearGridCellState(cell, GridState.Lit);
            //     SceneReferences.Instance.gridManager.ApplyGridCellColor(cell);
            // }
            // _dynamicallyLitCells.Clear();
            // Profiler.EndSample();
            //
            // Profiler.BeginSample("Apply dynamic objects");
            // foreach (var obj in dynamicFogClearingObjects)
            // {
            //     AddLitStateForObject(obj);
            // }
            //
            // Profiler.EndSample();
        }

        
        private void AddLitStateForObject(FogClearingObject obj)
        {
            Profiler.BeginSample("Get Bounds");
            obj.GridPosition = GridCell.FromWorldPos(obj.Position);
            Bounds bounds = new Bounds(obj.Position, new Vector3(obj.Radius * 2, 1, obj.Radius * 2));
            GridCell min = GridCell.FromWorldPos(bounds.min);
            GridCell max = GridCell.FromWorldPos(bounds.max);
            Profiler.EndSample();
            
            Profiler.BeginSample("Test Cells and Set States");
            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    GridCell cell = new GridCell(x, y);
                    if(_staticallyLitCells.Contains(cell)) continue;
                    if(!obj.IsStatic && _dynamicallyLitCells.Contains(cell)) continue;
                    
                    if(!cell.Overlaps(obj.Position, obj.Radius)) continue;
                    
                    Profiler.BeginSample("Set Cell State");
                    if (obj.IsStatic)
                    {
                        if (_staticallyLitCells.Add(cell))
                        {
                            _dynamicallyLitCells.Remove(cell);
                            SceneReferences.Instance.gridManager.SetGridCellState(cell, GridState.Lit);
                        }
                    }
                    else
                    {
                        if (_dynamicallyLitCells.Add(cell))
                        {
                            SceneReferences.Instance.gridManager.SetGridCellState(cell, GridState.Lit);
                        }
                    }
                    Profiler.EndSample();
                }
            }
            Profiler.EndSample();
            
            SceneReferences.Instance.gridManager.ApplyGridCellColors(min, max);
        }

        public void SetFogOfWarAlpha(Vector3 position, float radius, float alpha)
        {
            fogOfWar.SetFogOfWarAlpha(position, radius+10, alpha, true, 0.5f, 1f, 1f, 0.5f);
            
            //fog.SetFogOfWarAlpha(position, radius, alpha, true, 2f, 1f, 1f, 3f);
             fog.SetFogOfWarAlpha(position, radius-5, alpha, true, 1f, 1f, 1f, 1f);
            if (alpha < 0.5f)
            {
                //fogOfWar.SetFogOfWarAlpha(position, radius+12, 0.5f, true, 0.5f, 1f, 1f, 0.5f);
                fog.SetFogOfWarAlpha(position, radius, 0.3f, true, 1f, 1f, 1f, 1f);
            }
        }
    }
}
