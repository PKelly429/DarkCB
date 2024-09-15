using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace TDCB
{
    [CreateAssetMenu (menuName="TDCB/Building")]
    public class BuildingData : ScriptableObject
    {
        [SuffixLabel("Higher Better")] public int priority;
        public Sprite icon;
        public GameObject buildingPrefab;

        [Title("Commands")] 
        public CommandTemplate commands;
        
        [Title("Voice Clips")]
        public SoundData selectSound;
        
        [BoxGroup("Tooltip"), HideLabel] public Tooltip tooltip;
    }
}
