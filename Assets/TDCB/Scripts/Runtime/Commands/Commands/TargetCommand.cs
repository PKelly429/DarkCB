using AudioSystem;
using UnityEngine;

namespace TDCB
{
    [CreateAssetMenu(menuName = "Command/Position Command")]
    public class TargetCommand : Command<Vector3>, ITargetCommand
    {
        public override void Execute()
        {
            OnBeforeExecute();
            SceneReferences.Instance.inputHandler.SetCommand(this);
            if(soundClip != null) SoundManager.Instance.CreateSoundBuilder().Play(soundClip);
        }
        
        public virtual void Execute(ISelectable target)
        {
            Execute(target.Position);
        }

        public virtual void OnBeforeExecute()
        {
            
        }

        public virtual void OnAfterExecuteOrCancel()
        {
            
        }
    }
}