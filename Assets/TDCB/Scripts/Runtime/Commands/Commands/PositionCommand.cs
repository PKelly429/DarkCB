using UnityEngine;

namespace TDCB
{
    [CreateAssetMenu(menuName = "Command/Position Command")]
    public class PositionCommand : Command<Vector3>, IPositionCommand
    {
        public override void Execute()
        {
            SceneReferences.Instance.inputHandler.SetCommand(this);
        }
    }
}