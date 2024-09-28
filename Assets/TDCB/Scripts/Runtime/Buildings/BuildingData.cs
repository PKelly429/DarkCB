using System;
using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace TDCB
{
    [CreateAssetMenu (menuName="TDCB/Building")]
    public class BuildingData : ScriptableObject, IToolTipResourceCost
    {
        [SuffixLabel("Higher Better")] public int priority;
        public Sprite icon;
        public GameObject buildingPrefab;

        [Header("Stats")] 
        public UnitStats stats;

        [Title("Commands")] 
        public CommandTemplate commands;
        
        [Title("Voice Clips")]
        public SoundData selectSound;

        [Title("Resource Costs")] 
        public ResourceValue[] costs;
        
        [Title("Resources Consumed")]
        public ResourceValue[] consumed;
        
        [Title("Resources Produced")]
        public ResourceProductionValue[] produced;
        
        [PropertySpace]
        [BoxGroup("Tooltip"), HideLabel] public Tooltip tooltip;

        public Tooltip GetFullTooltip()
        {
            return new Tooltip()
            {
                header = tooltip.header,
                body = tooltip.body,
                position = FixedPosition.Command,
                ResourceCosts = this,
                type = TooltipType.ResourceCost
            };
        }
        
        public ResourceValue[] Costs => costs;
        public ResourceProductionValue[] Provides => produced;
        public ResourceValue[] Consumed => consumed;
    }
}
