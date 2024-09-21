using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using UnityEngine;

namespace TDCB
{
    [CreateAssetMenu(menuName = "Command/Nested Commands")]
    public class ShowNestedCommand : BaseCommand
    {
        [SerializeField] private CommandTemplate nestedCommands;
        
        public override void Execute()
        {
            UIReferences.Instance.commandButtonGrid.Bind(nestedCommands);
            if(soundClip != null) SoundManager.Instance.CreateSoundBuilder().Play(soundClip);
        }
    }
}
