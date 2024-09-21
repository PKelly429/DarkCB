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

        [Title("Resource Costs")] 
        public ResourceValue[] costs;
        
        [Title("Resources Given")]
        public ResourceValue[] produced;
        
        [PropertySpace]
        [BoxGroup("Tooltip"), HideLabel] public Tooltip tooltip;
    }
}
