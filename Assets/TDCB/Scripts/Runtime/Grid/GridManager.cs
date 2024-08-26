using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.Rendering;

namespace TDCB
{
    public class GridManager : MonoBehaviour
    {
        public const int WorldSize = 1024;
        public const int GridBounds = 512; // WorldSize / GridSize
        public const int HalfGridBounds = 256;
        public const int GridSize = 2;
        
        private static readonly int MousePos = Shader.PropertyToID("_MousePos");
        private LocalKeyword _showGraph;
        
        [SerializeField] private AstarPath pathfinding;
        [SerializeField] private Material gridMaterial;
        [SerializeField] private Texture2D gridTexture;
        
        private Dictionary<GridCell, GridState> _gridCells = new Dictionary<GridCell, GridState>();
        private bool _needToApplyTexture = false;

        public bool IsPositionValid(Bounds bounds)
        {
            GridCell min = GetGridCellFromWorldPos(bounds.min);
            GridCell max = GetGridCellFromWorldPos(bounds.max);

            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    if (GetGridCellState(x, y).HasFlag(GridState.BlockedByTerrain) || GetGridCellState(x, y).HasFlag(GridState.BlockedByBuilding))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        
        public void SetFlags(Bounds bounds, GridState toSet)
        {
            GridCell min = GetGridCellFromWorldPos(bounds.min);
            GridCell max = GetGridCellFromWorldPos(bounds.max);

            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    SetGridCellState(x, y, toSet);
                }
            }

            _needToApplyTexture = true;

            if (toSet.HasFlag(GridState.BlockedByBuilding))
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
    
                    gridGraph.SetWalkability(new bool[width*height], new IntRect(minX, minY, maxX, maxY));
                }));
            }
        }

        public void SetFlags(Vector3 position, float radius, GridState toSet)
        {
            Bounds bounds = new Bounds(position, new Vector3(radius * 2, 1, radius * 2));
            GridCell min = GetGridCellFromWorldPos(bounds.min);
            GridCell max = GetGridCellFromWorldPos(bounds.max);
            
            GridCell center = GetGridCellFromWorldPos(position);
            Vector2 centerPos = new Vector2(center.x, center.y);
            float distance = Mathf.FloorToInt(radius / GridSize);
            
            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    if(Vector2.Distance(centerPos, new Vector2(x, y)) > distance) continue;
                    SetGridCellState(x, y, toSet);
                }
            }
            
            _needToApplyTexture = true;
        }

        public GridCell GetGridCellFromWorldPos(Vector3 pos)
        {
            int x = Mathf.FloorToInt(Mathf.Clamp((pos.x / GridSize)+HalfGridBounds, 0, GridBounds));
            int y = Mathf.FloorToInt(Mathf.Clamp((pos.z / GridSize)+HalfGridBounds, 0, GridBounds));
            return new GridCell(x, y);
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
                        _gridCells.TryAdd(new GridCell(x, y), GridState.BlockedByTerrain);
                    }
                }
            }
        }

        private void SetGridCellState(int x, int y, GridState state)
        {
            SetGridCellState(new GridCell(x, y), state);
        }
        
        private void SetGridCellState(GridCell cell, GridState state)
        {
            if (!_gridCells.TryAdd(cell, state))
            {
                _gridCells[cell] |= state;
            }

            var curState = _gridCells[cell];
            bool invalid = curState.HasFlag(GridState.BlockedByTerrain) || curState.HasFlag(GridState.BlockedByBuilding);
            bool lit = curState.HasFlag(GridState.Lit);
            gridTexture.SetPixel(cell.x, cell.y, new Color(invalid ? 1 : 0, 0, lit ? 1 : 0, 1));
        }
        
        private void ClearGridCellState(GridCell cell, GridState state)
        {
            if (_gridCells.ContainsKey(cell))
            {
                _gridCells[cell] &= ~state;
            }
        }

        private GridState GetGridCellState(int x, int y)
        {
            return GetGridCellState(new GridCell(x, y));
        }
        private GridState GetGridCellState(GridCell cell)
        {
            _gridCells.TryAdd(cell, new GridState());
            return _gridCells[cell];
        }
        
        #region Unity Functions
        private void Start()
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
    public enum GridState
    {
        None = 0,
        Lit = 1 << 0,
        BlockedByTerrain = 1 << 1,
        BlockedByBuilding = 1 << 2,
        
        Blocked = BlockedByTerrain | BlockedByBuilding
    }
}
