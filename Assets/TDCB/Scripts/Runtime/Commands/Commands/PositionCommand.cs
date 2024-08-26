using UnityEngine;

namespace TDCB
{
    [CreateAssetMenu(menuName = "Command/Position Command")]
    public class PositionCommand : Command<Vector3>, IPositionCommand
    {
        public override void Execute()
        {
            OnBeforeExecute();
            SceneReferences.Instance.inputHandler.SetCommand(this);
        }

        public virtual void OnBeforeExecute()
        {
            
        }
        
        public virtual void OnAfterExecuteOrCancel()
        {
            
        }
    }
}