using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace TDCB
{
    [Serializable]
    public struct GridCell : IEquatable<GridCell>
    {
        public int x;
        public int y;

        public GridCell(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        
        public bool Overlaps(GridCell pos, float radius)
        {
            Profiler.BeginSample("Overlaps");
            Vector2 center = new Vector2(pos.x, pos.y);
            radius /= GridManager.GridSize;
            
            float closestX = Math.Max(x, Math.Min(center.x, x+GridManager.GridSize));
            float closestY = Math.Max(y, Math.Min(center.y, y+GridManager.GridSize));
            
            float distanceX = center.x - closestX;
            float distanceY = center.y - closestY;
            
            float distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);
            Profiler.EndSample();
            return distanceSquared <= (radius * radius);
        }

        public bool Overlaps(Vector3 pos, float radius)
        {
            Vector2 center = new Vector2((pos.x / GridManager.GridSize)+GridManager.HalfGridBounds, (pos.z / GridManager.GridSize)+GridManager.HalfGridBounds);
            radius /= GridManager.GridSize;
            
            float closestX = Math.Max(x, Math.Min(center.x, x+GridManager.GridSize));
            float closestY = Math.Max(y, Math.Min(center.y, y+GridManager.GridSize));
            
            float distanceX = center.x - closestX;
            float distanceY = center.y - closestY;
            
            float distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);
            return distanceSquared <= (radius * radius);
        }
        
        public override string ToString()
        {
            return $"[{x},{y}]";
        }

        public static GridCell FromWorldPos(Vector3 worldPos)
        {
            int x = Mathf.RoundToInt(Mathf.Clamp((worldPos.x / GridManager.GridSize)+GridManager.HalfGridBounds, 0, GridManager.GridBounds));
            int y = Mathf.RoundToInt(Mathf.Clamp((worldPos.z / GridManager.GridSize)+GridManager.HalfGridBounds, 0, GridManager.GridBounds));
            return new GridCell(x, y);
        }
        
        public static GridCell FromWorldPos(Vector2 pos)
        {
            int x = Mathf.RoundToInt(Mathf.Clamp((pos.x / GridManager.GridSize)+GridManager.HalfGridBounds, 0, GridManager.GridBounds));
            int y = Mathf.RoundToInt(Mathf.Clamp((pos.y / GridManager.GridSize)+GridManager.HalfGridBounds, 0, GridManager.GridBounds));
            return new GridCell(x, y);
        }
    
        public static GridCell operator +(GridCell a, GridCell b) => new (a.x + b.x, a.y + b.y);
        public static GridCell operator -(GridCell a, GridCell b) => new (a.x - b.x, a.y - b.y);
    
        #region Equality
        public bool Equals(GridCell other)
        {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj)
        {
            return obj is GridCell other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }
        #endregion
    }
}
