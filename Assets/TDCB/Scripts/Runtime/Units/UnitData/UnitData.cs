using System;
using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TDCB
{
    [Serializable]
    [CreateAssetMenu(menuName="TDCB/Unit")]
    public class UnitData : ScriptableObject, IToolTipResourceCost
    {
        [SuffixLabel("Higher Better")] public int priority;
        public Sprite icon;
        public GameObject unitPrefab;

        [Title("Stats")] 
        public UnitStats stats;

        [Title("Commands")] 
        public CommandTemplate commands;
        
        [Title("Voice Clips")]
        public SoundData trainSound;
        public SoundData selectSound;
        public SoundData moveSound;
        public SoundData stopSound;
        public SoundData holdSound;
        public SoundData attackSound;
        public SoundData workSound;

        [Title("Resource Costs")] 
        public float TrainingTime = 30f;
        public ResourceValue[] costs;
        
        [Title("Resources Consumed")]
        public ResourceValue[] consumed;
        
        [Title("Resources Produced")]
        public ResourceProductionValue[] produced;
        
        [BoxGroup("Tooltip"), HideLabel] public Tooltip tooltip;
        public ResourceValue[] Costs => costs;
        public ResourceProductionValue[] Provides => produced;
        public ResourceValue[] Consumed => consumed;
    }
    
    [Serializable]
    public struct UnitStats
    {
        public int health;
        public Armor armor;
        public int damage;
        public float attackDelay;
        public float attackSpeed;
        public DamageTypes damageType;
        public float range;
    }

    [Serializable]
    public struct Armor
    {
        public int melee;
        public int piercing;
    }

    public enum DamageTypes
    {
        Melee,
        Piercing
    }
}
