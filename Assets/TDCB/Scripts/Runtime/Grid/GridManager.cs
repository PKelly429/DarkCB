using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace TDCB
{
    public class GridManager : MonoBehaviour
    {
        public const int WorldSize = 1024;
        public const int GridBounds = 256; // WorldSize / GridSize
        public const int HalfGridBounds = 128;
        public const int GridSize = 4;
        
        public const int MaxColorBatchSize = 45;
        
        private static readonly int MousePos = Shader.PropertyToID("_MousePos");
        private LocalKeyword _showGraph;
        
        [SerializeField] private AstarPath pathfinding;
        [SerializeField] private Material gridMaterial;
        [SerializeField] private Texture2D gridTexture;

        private Color[] toApply = new Color[MaxColorBatchSize*MaxColorBatchSize];
        private GridState[] gridStates = new GridState[GridBounds * GridBounds];
        private bool _needToApplyTexture = false;

        public bool IsPositionValid(Bounds bounds)
        {
            GridCell min = GridCell.FromWorldPos(bounds.min);
            GridCell max = GridCell.FromWorldPos(bounds.max);

            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    //if ((GetGridCellState(x, y) & GridState.Blocked) != 0)
                    if (GetGridCellState(x, y) != GridState.Lit)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        
        public void SetFlags(Bounds bounds, GridState toSet)
        {
            GridCell min = GridCell.FromWorldPos(bounds.min);
            GridCell max = GridCell.FromWorldPos(bounds.max);

            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    SetGridCellState(x, y, toSet);
                }
            }
            
            ApplyGridCellColors(min, max);

            if (toSet.HasFlag(GridState.Blocked))
            {
                SetWalkable(min, max, false);
            }
        }
        
        public void ClearFlags(Bounds bounds, GridState toSet)
        {
            GridCell min = GridCell.FromWorldPos(bounds.min);
            GridCell max = GridCell.FromWorldPos(bounds.max);

            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    ClearGridCellState(x, y, toSet);
                }
            }

            ApplyGridCellColors(min, max);

            if ((toSet & GridState.Blocked) != 0)
            {
                SetWalkable(min, max, false);
            }
        }

        private void SetWalkable(GridCell min, GridCell max, bool walkable)
        {
            int minX = min.x * 2;
            int minY = min.y * 2;
            int maxX = (max.x * 2)+1;
            int maxY = (max.y * 2)+1;
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

        public Vector3 GetWorldPositionFromCell(GridCell cell)
        {
            float halfGrid = GridSize / 2f;
            float worldX = ((cell.x-HalfGridBounds) * GridSize)+halfGrid;
            float worldZ = ((cell.y-HalfGridBounds) * GridSize)+halfGrid;
            //float worldY = SampleTerrainHeight(new Vector3(worldX, 0, worldZ));
            float worldY = 0;
            return new Vector3(worldX, worldY, worldZ);
        }

        private void InitialiseGridWithTerrain()
        {
            for (int x = 0; x < GridBounds; x++)
            {
                for (int y = 0; y < GridBounds; y++)
                {
                    if (gridTexture.GetPixel(x, y).r > 0.5f)
                    {
                        gridStates[y * GridBounds + x] = GridState.Blocked;
                    }
                }
            }
        }

        public void AddGridCellState(GridCell min, GridCell max, GridState state, Func<GridCell, bool> applyCell)
        {
            Profiler.BeginSample("AddGridCellState Batch"); 
            int width = (max.x - min.x)+1;
            int height = (max.y - min.y)+1;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    GridCell next = new GridCell(min.x+x, min.y+y);
                    if (!applyCell(next)) continue;
                    SetGridCellState(next, state);
                    var curState = GetGridCellState(next); 
                    bool invalid = (curState & GridState.Blocked) != 0;
                    bool lit = (curState & GridState.Lit) != 0; 
                    toApply[y * width + x] = new Color(invalid ? 1 : 0, 0, lit ? 1 : 0, 1);
                } 
            }
            
            gridTexture.SetPixels(min.x, min.y, width, height, toApply);
            _needToApplyTexture = true;
            Profiler.EndSample();
        }

        public void ApplyGridCellColors(GridCell min, GridCell max)
        {
            Profiler.BeginSample("UpdateGridCellColor Batch"); 
            int width = (max.x - min.x)+1;
            int height = (max.y - min.y)+1;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var curState = GetGridCellState(new GridCell(min.x+x, min.y+y)); 
                    bool invalid = (curState & GridState.Blocked) != 0;
                    bool lit = (curState & GridState.Lit) != 0; 
                    toApply[y * width + x] = new Color(invalid ? 1 : 0, 0, lit ? 1 : 0, 1);
                } 
            }
            
            gridTexture.SetPixels(min.x, min.y, width, height, toApply);
            _needToApplyTexture = true;
            Profiler.EndSample();
        }

        public void ApplyGridCellColor(GridCell cell)
        {
            _needToApplyTexture = true;
            
            Profiler.BeginSample("UpdateGridCellColor"); 
            var curState = gridStates[cell.y*GridBounds + cell.x]; 
            bool invalid = (curState & GridState.Blocked) != 0;
            bool lit = (curState & GridState.Lit) != 0; 
            gridTexture.SetPixel(cell.x, cell.y, new Color(invalid ? 1 : 0, 0, lit ? 1 : 0, 1));
            Profiler.EndSample();
        }

        public void SetGridCellState(int x, int y, GridState state)
        {
            SetGridCellState(new GridCell(x, y), state);
        }
        
        public void SetGridCellState(GridCell cell, GridState state)
        {
            gridStates[cell.y * GridBounds + cell.x] |= state;
        }
        
        public void ClearGridCellState(int x, int y, GridState state)
        {
            ClearGridCellState(new GridCell(x, y), state);
        }
        
        public void ClearGridCellState(GridCell cell, GridState state)
        {
            gridStates[cell.y * GridBounds + cell.x] &= ~state;
        }

        private GridState GetGridCellState(int x, int y)
        {
            return GetGridCellState(new GridCell(x, y));
        }
        private GridState GetGridCellState(GridCell cell)
        {
            return gridStates[cell.y * GridBounds + cell.x];
        }
        

        #region Unity Functions
        private void Awake()
        {
            InitialiseGridWithTerrain();
            _showGraph = new LocalKeyword(gridMaterial.shader, "_SHOWGRID_ON");
        }

        public void Update()
        {
            SetMouseGridPos();

            if (_needToApplyTexture)
            {
                _needToApplyTexture = false;
                gridTexture.Apply();
            }
        }
        #endregion
        
        #region Grid Visibility
        private bool _showGrid;
        public bool ShowGrid
        {
            get => _showGrid;
            set
            {
                _showGrid = value;
                gridMaterial.SetKeyword(_showGraph, value);
            }
        }
        private void SetMouseGridPos()
        {
            if (!_showGrid) return;
            
            Vector3 worldPos = SceneReferences.Instance.inputHandler.MousePosition;
            Vector2 mousePos = new Vector2(worldPos.x, worldPos.z);
            gridMaterial.SetVector(MousePos, mousePos);
        }
        #endregion
    }

    [Serializable]
    [Flags]
    public enum GridState : byte
    {
        None = 0,
        Lit = 1 << 0,
        Blocked = 1 << 1,
    }
}
