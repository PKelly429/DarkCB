using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Shapes;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace TDCB
{
    public class InputHandler : MonoBehaviour, InputControls.IPlayerInputActions
    {
        private const int MaxHits = 20;
        private const float MaxDistance = 500;
        
        private const float SingleUnitSelectTime = 0.3f;
        
        [SerializeField] private Camera _mainCamera;

        [SerializeField] private LayerMask groundLayers;
        [SerializeField] private LayerMask clickableLayers;
        [SerializeField] private LayerMask uiLayer;

        public Action OnSelectionDragStart;
        public Action OnSelectionDragFinish;

        private InputControls inputControls;
        public InputControls InputControls => inputControls ??= new InputControls();
        
        public bool PointerOverUI { get; private set; }
        public Vector2 MouseScreenPosition { get; private set; }
        public Vector3 MousePosition { get; private set; }
        
        public bool LeftMouseDown { get; private set; }
        public bool LeftMouseDownPressedThisFrame { get; private set; }
        public float LeftMouseHeldTime { get; private set; }
        
        public bool ControlDown { get; private set; }
        public bool ShiftDown { get; private set; }
        
        // Commands
        public bool HasTargetCommand { get; private set; }
        private ITargetCommand currentTargetCommand;
        
        // Selectable Objects
        private readonly RaycastHit[] _hits = new RaycastHit[MaxHits];
        private int _hitCount;
        private readonly HashSet<ISelectable> _hoveredSelectables = new HashSet<ISelectable>();
        
        private DragState _dragState;

        private DragState dragState
        {
            get => _dragState;
            set
            {
                if (_dragState == value) return;
                
                if (value == DragState.Idle)
                {
                    SceneReferences.Instance.unitManager.SetUnitSelection(_hoveredSelectables, !ShiftDown);
                    ClearHover();
                    OnSelectionDragFinish?.Invoke();
                }
                    
                _dragState = value;

                if (dragState == DragState.SingleUnitSelection)
                {
                    _dragStartPosition = MouseScreenPosition;
                }
                else if (_dragState == DragState.MultiUnitSelection)
                {
                    OnSelectionDragStart?.Invoke();
                }
            }
        }
        
        private Vector2 _dragStartPosition;
        private Vector2 _dragCurrentPosition;

        private enum DragState
        {
            Idle,
            SingleUnitSelection,
            MultiUnitSelection
        }
        

        private void Start()
        {
            inputControls ??= new InputControls();

            inputControls.PlayerInput.Enable();

            inputControls.PlayerInput.LeftMousePress.performed += OnLeftMousePress;
            inputControls.PlayerInput.LeftMouseRelease.performed += OnLeftMouseRelease;
            inputControls.PlayerInput.Select.performed += OnSelect;
            inputControls.PlayerInput.Cancel.performed += OnCancel;
            inputControls.PlayerInput.RightMousePress.performed += OnRightMousePress;
            inputControls.PlayerInput.RightMouseRelease.performed += OnRightMouseRelease;
            inputControls.PlayerInput.Pause.performed += OnPause;
        }

        private void OnDestroy()
        {
            inputControls.PlayerInput.LeftMousePress.performed -= OnLeftMousePress;
            inputControls.PlayerInput.LeftMouseRelease.performed -= OnLeftMouseRelease;
            inputControls.PlayerInput.Select.performed -= OnSelect;
            inputControls.PlayerInput.Cancel.performed -= OnCancel;
            inputControls.PlayerInput.RightMousePress.performed -= OnRightMousePress;
            inputControls.PlayerInput.RightMouseRelease.performed -= OnRightMouseRelease;
            inputControls.PlayerInput.Pause.performed -= OnPause;
        }

        public void Update()
        {
            PointerOverUI = EventSystem.current.IsPointerOverGameObject();
            
            LeftMouseDown = InputControls.PlayerInput.LeftMousePress.IsPressed();
            LeftMouseDownPressedThisFrame = InputControls.PlayerInput.LeftMousePress.WasPressedThisFrame();

            ControlDown = inputControls.PlayerInput.Control.IsPressed();
            ShiftDown = inputControls.PlayerInput.Shift.IsPressed();

            if (LeftMouseDownPressedThisFrame)
            {
                LeftMouseHeldTime = 0f;
            }
            else if (LeftMouseDown)
            {
                LeftMouseHeldTime += Time.unscaledDeltaTime;
            }
            
            MouseScreenPosition = inputControls.PlayerInput.MousePosition.ReadValue<Vector2>();
            
            // Vector3 worldMousePos = _mainCamera.ScreenToWorldPoint(MouseScreenPosition);
            // MousePosition = new Vector3(worldMousePos.x, 0, worldMousePos.z);
            
            bool processCommand = LeftMouseDownPressedThisFrame && HasTargetCommand;
            processCommand |= InputControls.PlayerInput.RightMousePress.WasPressedThisFrame() && !HasTargetCommand;

            Ray ray = _mainCamera.ScreenPointToRay(MouseScreenPosition);
            
            _hitCount = Physics.RaycastNonAlloc(ray.origin, ray.direction, _hits, MaxDistance, groundLayers);
            if (_hitCount > 0)
            {
                MousePosition = _hits[0].point;
            }

            if (processCommand)
            {
                HandleMoveCommand(ray);
            }
            else
            {
                HandleSelectionDrag(ray);
            }
        }

        public void SetCommand(ITargetCommand command)
        {
            HasTargetCommand = true;
            currentTargetCommand = command;
        }

        private void HandleMoveCommand(Ray ray)
        {
            if (PointerOverUI) return;
            
            HoverFirstSelectableUnit();
            ISelectable target = _hoveredSelectables.Count > 0 ? _hoveredSelectables.First() : null;
            
            if (_hitCount > 0 || target != null)
            {
                bool hasCommand = HasTargetCommand;
                ITargetCommand toExecute = currentTargetCommand;
                
                // Clearing first in case a command wants to chain a second command
                ClearCurrentCommand();
                
                if (target == null)
                {
                    if (hasCommand)
                    {
                        toExecute.Execute(_hits[0].point);
                    }
                    else
                    {
                        SceneReferences.Instance.commandManager.MoveCommand.Execute(_hits[0].point);   
                    }
                }
                else
                {
                    if (hasCommand)
                    {
                        toExecute.Execute(target);
                    }
                    else
                    {
                        SceneReferences.Instance.commandManager.MoveCommand.Execute(target); 
                    }
                }
            }
        }

        private void ClearCurrentCommand()
        {
            if (!HasTargetCommand) return;
            
            HasTargetCommand = false;
            currentTargetCommand = null;
        }

        private void HandleSelectionDrag(Ray ray)
        {
            if (dragState == DragState.Idle && PointerOverUI)
            {
                ClearHover();
                return;
            }
            
            if (dragState == DragState.Idle && LeftMouseDownPressedThisFrame)
            {
                dragState = DragState.SingleUnitSelection;
            }

            if (dragState is DragState.Idle or DragState.SingleUnitSelection)
            {
                HoverFirstSelectableUnit();
            }

            if (dragState == DragState.Idle) return;
            
            if (!LeftMouseDown)
            {
                dragState = DragState.Idle;
                return;
            }
            
            _dragCurrentPosition = MouseScreenPosition;

            if (dragState == DragState.SingleUnitSelection)
            {
                if (IsMultiUnitSelect())
                {
                    dragState = DragState.MultiUnitSelection;
                }
            }
            else
            {
                Vector2 startPos = new Vector2(Mathf.Min(_dragStartPosition.x, _dragCurrentPosition.x), Mathf.Min(_dragStartPosition.y, _dragCurrentPosition.y));
                Rect rect = new Rect(startPos.x, startPos.y, Mathf.Abs(_dragStartPosition.x - _dragCurrentPosition.x), Mathf.Abs(_dragStartPosition.y - _dragCurrentPosition.y));
                
                ClearHover();
                foreach (var obj in SelectableObject.AllSelectableObjects)
                {
                    if (IsUnitInSelectionBox(rect, obj))
                    {
                        AddHover(obj);
                    }
                }
            }
        }

        private List<ISelectable> selectablesAtMousePos = new List<ISelectable>();
        private void HoverFirstSelectableUnit()
        {
            Vector3 cameraPos = _mainCamera.transform.position;
            selectablesAtMousePos.Clear();
            foreach (var obj in SelectableObject.AllSelectableObjects)
            {
                if (IsUnitInSelection(MouseScreenPosition, obj))
                {
                    selectablesAtMousePos.Add(obj);
                }
            }
            
            selectablesAtMousePos.Sort((x, y) => Vector3.Distance(y.Position, cameraPos).CompareTo(Vector3.Distance(x.Position, cameraPos)));

            if (selectablesAtMousePos.Count > 0)
            {
                ClearHover(selectablesAtMousePos[0]);
                AddHover(selectablesAtMousePos[0]);
            }
            else
            {
                ClearHover();
            }
        }

        private void SortHits()
        {
            for (int i = 0; i < _hitCount - 1; i++) 
            { 
                int minIndex = i; 
                for (int j = i + 1; j < _hitCount; j++) 
                    if (_hits[j].distance < _hits[minIndex].distance) 
                        minIndex = j; 
                
                (_hits[minIndex], _hits[i]) = (_hits[i], _hits[minIndex]);
            } 
        }


        private bool IsMultiUnitSelect()
        {
            if (LeftMouseHeldTime > SingleUnitSelectTime) return true;
            
            return Vector2.Distance(_dragStartPosition, _dragCurrentPosition) > 30f;
        }

        private void ClearHover(ISelectable exclude)
        {
            bool isInSelection = false;
            foreach (var hovered in _hoveredSelectables)
            {
                if (hovered == exclude)
                {
                    isInSelection = true;
                    continue;
                }
                
                if (hovered != null && hovered.IsAlive())
                {
                    hovered.OnHoverEnd();
                }
            }
            _hoveredSelectables.Clear();
            if (isInSelection) _hoveredSelectables.Add(exclude);
        }

        private void ClearHover()
        {
            foreach (var hovered in _hoveredSelectables)
            {
                if (hovered != null && hovered.IsAlive())
                {
                    hovered.OnHoverEnd();
                }
            }
            _hoveredSelectables.Clear();
        }

        private void AddHover(ISelectable selectableObject)
        {
            if (_hoveredSelectables.Contains(selectableObject)) return;
            
            if (selectableObject != null && selectableObject.IsAlive())
            {
                selectableObject.OnHoverBegin();
                _hoveredSelectables.Add(selectableObject);
            }
        }

        private bool IsUnitInSelectionBox(Rect rect, SelectableObject obj)
        {
            Vector2 min = _mainCamera.WorldToScreenPoint(obj.Collider.bounds.min);
            Vector2 max = _mainCamera.WorldToScreenPoint(obj.Collider.bounds.max);
            
            Rect objRect = new Rect(Mathf.Min(min.x, max.x), Mathf.Min(min.y, max.y), Mathf.Abs(min.x - max.x), Mathf.Abs(min.y - max.y));
            
            return rect.Overlaps(objRect);
        }
        
        private bool IsUnitInSelection(Vector3 pos, SelectableObject obj)
        {
            Vector2 min = _mainCamera.WorldToScreenPoint(obj.Collider.bounds.min);
            Vector2 max = _mainCamera.WorldToScreenPoint(obj.Collider.bounds.max);
            
            Rect objRect = new Rect(Mathf.Min(min.x, max.x), Mathf.Min(min.y, max.y), Mathf.Abs(min.x - max.x), Mathf.Abs(min.y - max.y));
            
            return objRect.Contains(pos);
        }

        public void OnSelect(InputAction.CallbackContext context)
        {
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            if (HasTargetCommand)
            {
                currentTargetCommand.OnCancel();
                ClearCurrentCommand();
            }
            else
            {
                UIReferences.Instance.commandButtonGrid.BindToSelectedUnits();
            }
        }

        public void OnMouseDelta(InputAction.CallbackContext context)
        {
        }

        public void OnLeftMousePress(InputAction.CallbackContext context)
        {
        }

        public void OnLeftMouseRelease(InputAction.CallbackContext context)
        {
        }

        public void OnRightMousePress(InputAction.CallbackContext context)
        {
        }

        public void OnRightMouseRelease(InputAction.CallbackContext context)
        {
        }
        
        public void OnScrollDelta(InputAction.CallbackContext context)
        {
        }

        public void OnMousePosition(InputAction.CallbackContext context)
        {
        }
        
        public void OnControl(InputAction.CallbackContext context)
        {
        }

        public void OnShift(InputAction.CallbackContext context)
        {
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            UIReferences.Instance.pauseMenu.TogglePause();
        }
    }
}
