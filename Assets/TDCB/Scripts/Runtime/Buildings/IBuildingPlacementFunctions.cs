using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public interface IBuildingPlacementFunctions
    {
        public void OnBeginPlacement();
        public void OnCancelPlacement();
        public void OnFinishPlacement();
        public bool IsValid();
    }
}
