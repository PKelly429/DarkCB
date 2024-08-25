using System;
using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using Pathfinding;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TDCB
{
    public class MoveableEntity : MonoBehaviour, IControllableUnit
    {
        [Required, SerializeField] private GameObject aiObject;
        [SerializeField, SuffixLabel("Higher Better")] private int priority;
        [SerializeField] private SoundData moveSoundData;
        
        private IAstarAI ai;

        private void Awake()
        {
            ai = aiObject.GetComponent<IAstarAI>();
        }

        public int Priority => priority;
        public SoundData MoveClip => moveSoundData;
        public Vector3 CurrentPosition => ai.position;

        public void Move(Vector3 position)
        {
            ai.isStopped = false;
            ai.destination = position;
            ai.SearchPath();
        }

        public void AttackMove(Vector3 position)
        {
        }

        public void Stop()
        {
            ai.destination = ai.position;
            ai.isStopped = true;
        }

        public void HoldPosition()
        {
        }
    }
}
