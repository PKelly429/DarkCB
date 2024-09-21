using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using UnityEngine;

namespace TDCB
{
    public class StopCommand : BaseCommand
    {
        public override void Execute()
        {
            var observers = SceneReferences.Instance.unitManager.allControllableUnits;
            
            foreach (var observer in observers)
            {
                observer.Stop();
            }
            
            if(soundClip != null) SoundManager.Instance.CreateSoundBuilder().Play(soundClip);
        }
    }
}
