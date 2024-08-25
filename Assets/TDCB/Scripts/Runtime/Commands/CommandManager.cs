using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TDCB
{
    public class CommandManager : MonoBehaviour, InputControls.ICommandsActions
    {
         [SerializeField] private MoveCommand moveCommand;
         [SerializeField] private Command stopCommand;

         public MoveCommand MoveCommand => moveCommand;
         public Command StopCommand => stopCommand;
         
         
         #region Input Handling
         private InputControls _inputControls;
         
         private void Start()
         {
             _inputControls = SceneReferences.Instance.inputHandler.InputControls;
            
             _inputControls.Commands.Enable();
            
             _inputControls.Commands.Move.performed += OnMove;
             _inputControls.Commands.Stop.performed += OnStop;
         }
         
         public void OnMove(InputAction.CallbackContext context)
         {
             moveCommand.Execute();
         }

         public void OnStop(InputAction.CallbackContext context)
         {
             stopCommand.Execute();
         }
         #endregion
    }
}
