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
        
        [ShowIf("@this.type == TooltipType.HeaderAndBody || this.type == TooltipType.ResourceCost")]
        public string header;
        public string body;

        public ResourceValue[] ResourceCosts;

        public bool ShowHeader()
        {
            return type is TooltipType.HeaderAndBody or TooltipType.ResourceCost;
        }
        
        public bool ShowBody()
        {
            return true;
        }
        
        public bool ShowResourceCosts()
        {
            return type == TooltipType.ResourceCost;
        }
    }
    
    [Serializable]
    public enum TooltipType
    {
        HeaderAndBody,
        ResourceCost,
        OnlyBody,
    }
    
    [Serializable]
    public enum FixedPosition
    {
        None,
        Command
    }
}
