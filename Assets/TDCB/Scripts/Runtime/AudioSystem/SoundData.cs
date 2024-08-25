using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace AudioSystem 
{
    [Serializable]
    [CreateAssetMenu(menuName="SFX/Sound Data")]
    public class SoundData : ScriptableObject
    {
        public AudioClip[] clips;
        public AudioMixerGroup mixerGroup;
        public bool loop;
        public bool playOnAwake;
        public bool frequentSound;
        public bool allowRepeat;
        
        public bool mute;
        public bool bypassEffects;
        public bool bypassListenerEffects;
        public bool bypassReverbZones;
        
        public int priority = 128;
        public float volume = 1f;
        public float pitch = 1f;
        public float panStereo;
        public float spatialBlend;
        public float reverbZoneMix = 1f;
        public float dopplerLevel = 1f;
        public float spread;
        
        public float minDistance = 1f;
        public float maxDistance = 500f;
        
        public bool ignoreListenerVolume;
        public bool ignoreListenerPause;
        
        public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

        [NonSerialized] private List<AudioClip> _next = new List<AudioClip>();
        public AudioClip GetClip
        {
            get
            {
                if (allowRepeat)
                {
                    return clips[Random.Range(0, clips.Length)];
                }
                
                if (clips.Length == 1) return clips[0];

                if (_next.Count < 1)
                {
                    _next.AddRange(clips);
                }

                int random = Random.Range(0, _next.Count);
                AudioClip result = _next[random];
                _next.RemoveAt(random);
                return result;
            }
        }
    }
}