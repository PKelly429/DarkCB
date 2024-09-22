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
    public class UnitData : ScriptableObject
    {
        [SuffixLabel("Higher Better")] public int priority;
        public Sprite icon;
        public GameObject unitPrefab;

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
        
        [BoxGroup("Tooltip"), HideLabel] public Tooltip tooltip;
    }
}
