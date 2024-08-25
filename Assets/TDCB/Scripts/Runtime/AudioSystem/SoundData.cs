using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
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
        public bool fullyRandom;

        [InfoBox("Prevent repeating any clips with matching voice until time has passed")] public bool uniqueClip;
        [ShowIf("uniqueClip")] public SharedVoice sharedVoice;
        
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

        public bool CanPlay()
        {
            if (clips.Length == 0) return false;
            if (uniqueClip)
            {
                return sharedVoice.CanPlay();
            }

            return true;
        }
        
        [NonSerialized] private List<AudioClip> _next = new List<AudioClip>();
        public AudioClip GetClip
        {
            get
            {
                sharedVoice.SetTime();
                
                if (fullyRandom)
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