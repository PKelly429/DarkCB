using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TDCB
{
    public class UIReferences : MonoBehaviour
    {
        public CommandButtonGrid commandButtonGrid;
        public TooltipManager tooltipManager;
        
        #region Singleton
        public static UIReferences Instance {get; private set;}

        private void Awake()
        {
            Instance = this;
        }
        #endregion
    }
}
