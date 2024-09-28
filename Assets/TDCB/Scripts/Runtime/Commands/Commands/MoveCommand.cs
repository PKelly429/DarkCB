using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using UnityEngine;

namespace TDCB
{
    [CreateAssetMenu(menuName = "Command/Move Command")]
    public class MoveCommand : TargetCommand
    {
        public bool attack;
        
        public override void OnBeforeExecute()
        {
            UIReferences.Instance.commandButtonGrid.SetMoveCanvasVisible(true);
        }
        
        public override void OnCancel()
        {
            UIReferences.Instance.commandButtonGrid.SetMoveCanvasVisible(false);
        }

        public override void Execute(ISelectable target)
        {
            foreach (var observer in SceneReferences.Instance.unitManager.allControllableUnits)
            {
                observer.Move(target);
            }
            
            PlayCommandFeedback();
            UIReferences.Instance.commandButtonGrid.SetMoveCanvasVisible(false);
        }

        public override void Execute(Vector3 position)
        {
            UIReferences.Instance.commandButtonGrid.SetMoveCanvasVisible(false);
            
            if (attack)
            {
                foreach (var observer in SceneReferences.Instance.unitManager.allControllableUnits)
                {
                    observer.AttackMove(position);
                }
            }
            else
            {
                foreach (var observer in SceneReferences.Instance.unitManager.allControllableUnits)
                {
                    observer.Move(position);
                }
            }
            
            PlayCommandFeedback();
            
            base.Execute(position);
        }

        private void PlayCommandFeedback()
        {
            if (SceneReferences.Instance.unitManager.SelectedUnitCount > 0)
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
        }
    }
}
