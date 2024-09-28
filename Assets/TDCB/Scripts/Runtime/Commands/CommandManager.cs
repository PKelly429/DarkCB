using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TDCB
{
    public class CommandManager : MonoBehaviour, InputControls.ICommandsActions
    {
         [SerializeField] private MoveCommand moveCommand;
         [SerializeField] private StopCommand stopCommand;
         [SerializeField] private MoveCommand attackMoveCommand;
         
         [SerializeField] private BaseCommand buildCommand;

         public MoveCommand MoveCommand => moveCommand;
         public StopCommand StopCommand => stopCommand;
         public MoveCommand AttackMoveCommand => attackMoveCommand;
         
         
         #region Input Handling
         private InputControls _inputControls;
         
         private void Start()
         {
             _inputControls = SceneReferences.Instance.inputHandler.InputControls;
            
             _inputControls.Commands.Enable();
            
             _inputControls.Commands.Move.performed += OnMove;
             _inputControls.Commands.Stop.performed += OnStop;
             _inputControls.Commands.AttackMove.performed += OnAttackMove;
             _inputControls.Commands.Build.performed += OnBuild;
         }
         
         public void OnMove(InputAction.CallbackContext context)
         {
             moveCommand.Execute();
         }

         public void OnStop(InputAction.CallbackContext context)
         {
             stopCommand.Execute();
         }
         
         public void OnAttackMove(InputAction.CallbackContext context)
         {
             attackMoveCommand.Execute();
         }

         public void OnBuild(InputAction.CallbackContext context)
         {
             if (SceneReferences.Instance.unitManager.SelectedUnitCount <= 0) return;
             
             foreach (var unit in SceneReferences.Instance.unitManager.OrderedUnits)
             {
                 if (unit.selectableType != SelectableType.Unit) continue;
                 if (unit.unit.IsWorker)
                 {
                     buildCommand.Execute();
                     return;
                 }
             }
         }

         #endregion
    }
}
