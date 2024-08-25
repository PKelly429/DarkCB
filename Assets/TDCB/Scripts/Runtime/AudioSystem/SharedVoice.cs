using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AudioSystem
{
    [Serializable]
    [CreateAssetMenu(menuName="SFX/Shared Voice")]
    public class SharedVoice : ScriptableObject
    {
        public bool backoff;
        public bool randomise;
        public float repeatTime;
        [ShowIf("backoff")] public float backoffTime;
        [ShowIf("randomise")] public float randomiseTime;
        
        [NonSerialized] private float _lastPlayTime = float.MinValue;

        private float _nextTimeDelay;

        private void OnEnable()
        {
            _nextTimeDelay = repeatTime;
            if (randomise)
            {
                _nextTimeDelay = Random.Range(_nextTimeDelay - randomiseTime, _nextTimeDelay + randomiseTime);
            }
        }

        public bool CanPlay()
        {
            return Time.time >= _lastPlayTime + _nextTimeDelay;
        }

        public void SetTime()
        {
            if (backoff)
            {
                bool shouldBackOff = Time.time < _lastPlayTime + (_nextTimeDelay + repeatTime);
                _nextTimeDelay = shouldBackOff ? backoffTime : repeatTime;
                if (randomise)
                {
                    _nextTimeDelay = Random.Range(_nextTimeDelay - randomiseTime, _nextTimeDelay + randomiseTime);
                }
            }
            _lastPlayTime = Time.time;
        }
    }
}
