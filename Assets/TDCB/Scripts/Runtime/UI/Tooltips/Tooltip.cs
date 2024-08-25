using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TDCB
{
    [Serializable]
    public struct Tooltip
    {
        public TooltipType type;
        public FixedPosition position;
        
        [ShowIf("type", TooltipType.HeaderAndBody)]
        public string header;
        public string body;

        public bool ShowHeader()
        {
            return type == TooltipType.HeaderAndBody;
        }
        
        public bool ShowBody()
        {
            return true;
        }
    }
    
    [Serializable]
    public enum TooltipType
    {
        HeaderAndBody,
        OnlyBody,
    }
    
    [Serializable]
    public enum FixedPosition
    {
        None,
        Command
    }
}
