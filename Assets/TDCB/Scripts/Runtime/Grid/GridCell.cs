using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        
        public override string ToString()
        {
            return $"[{x},{y}]";
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
