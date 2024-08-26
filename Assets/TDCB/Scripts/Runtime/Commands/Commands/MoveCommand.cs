using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using UnityEngine;

namespace TDCB
{
    [CreateAssetMenu(menuName = "Command/Move Command")]
    public class MoveCommand : PositionCommand
    {
        public override void OnBeforeExecute()
        {
            UIReferences.Instance.commandButtonGrid.SetMoveCanvasVisible(true);
        }
        
        public override void OnAfterExecuteOrCancel()
        {
            UIReferences.Instance.commandButtonGrid.SetMoveCanvasVisible(false);
        }
        
        public override void Execute(Vector3 position)
        {
            var unitObservers = SceneReferences.Instance.unitManager.allControllableUnits;
            
            if (unitObservers.Count > 0)
            {
                ISelectable unit = SceneReferences.Instance.unitManager.HighestPrioritySelectedUnit;

                if (unit.IsControllable)
                {
                    IControllableUnit controllableUnit = unit.ControllableUnit;
                    if (unit.IsAlive())
                    {
                        SoundManager.Instance.CreateSoundBuilder().WithPosition(controllableUnit.CurrentPosition).Play(controllableUnit.MoveClip);
                    }
                }
            }
            
            foreach (var observer in unitObservers)
            {
                observer.Move(position);
            }
            
            base.Execute(position);
        }
    }
}
