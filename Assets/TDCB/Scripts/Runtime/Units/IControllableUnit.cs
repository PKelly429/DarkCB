using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using UnityEngine;

namespace TDCB
{
    public interface IControllableUnit
    {
        public SoundData MoveClip { get; }
        
        public Vector3 CurrentPosition { get; }
        
        public void Move(Vector3 position);
        public void AttackMove(Vector3 position);
        public void Stop();
        public void HoldPosition();
    }
}
