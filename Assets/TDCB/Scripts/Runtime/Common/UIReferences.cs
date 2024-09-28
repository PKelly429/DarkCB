using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TDCB
{
    public class UIReferences : MonoBehaviour
    {
        public UIColors uiColors;
        public CommandButtonGrid commandButtonGrid;
        public TooltipManager tooltipManager;
        public BindableIconUI resourceHarvesterIconPool;
        public BindableIconUI millIconPool;
        
        #region Singleton
        public static UIReferences Instance {get; private set;}

        private void Awake()
        {
            Instance = this;
        }
        #endregion
    }

    [Serializable]
    public struct UIColors
    {
        public Color DefaultText;
        public Color PositiveText;
        public Color NegativeText;
    }
}
