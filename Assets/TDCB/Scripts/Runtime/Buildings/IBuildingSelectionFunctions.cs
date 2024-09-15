using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public interface IBuildingSelectionFunctions
    {
        public void OnHoverBegin();
        public void OnHoverEnd();
        public void OnSelect();
        public void OnDeselect();
    }
}
