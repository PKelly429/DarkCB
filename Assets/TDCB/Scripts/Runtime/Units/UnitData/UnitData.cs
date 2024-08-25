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
        public SoundData selectSound;
        public SoundData moveSound;
        public SoundData stopSound;
        public SoundData holdSound;
        public SoundData attackSound;
        
        [BoxGroup("Tooltip"), HideLabel] public Tooltip tooltip;
    }
}
