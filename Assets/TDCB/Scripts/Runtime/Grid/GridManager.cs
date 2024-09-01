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
    }
}
