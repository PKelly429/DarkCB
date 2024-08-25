using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        
        // Commands
        public bool HasTargetCommand { get; private set; }
        private IPositionCommand currentTargetCommand;
        
        // Selectable Objects
        private Camera _mainCamera;
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
                
                if (_dragState == DragState.MultiUnitSelection)
                {
                    SceneReferences.Instance.unitManager.SetUnitSelection(_hoveredSelectables);
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
            _mainCamera = Camera.main;

            inputControls ??= new InputControls();

            inputControls.PlayerInput.Enable();

            inputControls.PlayerInput.LeftMousePress.performed += OnLeftMousePress;
            inputControls.PlayerInput.LeftMouseRelease.performed += OnLeftMouseRelease;
            inputControls.PlayerInput.Select.performed += OnSelect;
            inputControls.PlayerInput.Cancel.performed += OnCancel;
            inputControls.PlayerInput.RightMousePress.performed += OnRightMousePress;
            inputControls.PlayerInput.RightMouseRelease.performed += OnRightMouseRelease;
        }
        
        public void Update()
        {
            PointerOverUI = EventSystem.current.IsPointerOverGameObject();
            
            LeftMouseDown = InputControls.PlayerInput.LeftMousePress.IsPressed();
            LeftMouseDownPressedThisFrame = InputControls.PlayerInput.LeftMousePress.WasPressedThisFrame();

            ControlDown = inputControls.PlayerInput.Control.IsPressed();

            if (LeftMouseDownPressedThisFrame)
            {
                LeftMouseHeldTime = 0f;
            }
            else if (LeftMouseDown)
            {
                LeftMouseHeldTime += Time.unscaledDeltaTime;
            }
            
            MouseScreenPosition = inputControls.PlayerInput.MousePosition.ReadValue<Vector2>();
            
            Vector3 worldMousePos = _mainCamera.ScreenToWorldPoint(MouseScreenPosition);
            MousePosition = new Vector3(worldMousePos.x, worldMousePos.y, 0);
            
            bool rightMousePressed = InputControls.PlayerInput.RightMousePress.WasPressedThisFrame();
            bool processCommand = rightMousePressed || (LeftMouseDownPressedThisFrame && HasTargetCommand);

            Ray ray = _mainCamera.ScreenPointToRay(MouseScreenPosition);
            

            if (processCommand)
            {
                HandleMoveCommand(ray);
            }
            else
            {
                HandleSelectionDrag(ray);
            }
        }

        public void SetCommand(IPositionCommand command)
        {
            HasTargetCommand = true;
            currentTargetCommand = command;
        }

        private void HandleMoveCommand(Ray ray)
        {
            if (PointerOverUI) return;
            
            _hitCount = Physics.RaycastNonAlloc(ray.origin, ray.direction, _hits, MaxDistance, groundLayers);
            
            if (_hitCount > 0)
            {
                if (HasTargetCommand)
                {
                    currentTargetCommand.Execute(_hits[0].point);
                }
                else
                {
                    SceneReferences.Instance.commandManager.MoveCommand.Execute(_hits[0].point);   
                }
            }
            
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
                RaycastSelectableUnitsAndHoverFirst(ray);
            }

            if (dragState == DragState.Idle) return;
            
            if (!LeftMouseDown)
            {
                if (dragState == DragState.SingleUnitSelection)
                {
                    TrySelectFirstHoveredUnit();
                }

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

        private void RaycastSelectableUnitsAndHoverFirst(Ray ray)
        {
            _hitCount = Physics.RaycastNonAlloc(ray.origin, ray.direction, _hits, MaxDistance, clickableLayers);

            SortHits();
            
            ClearHover();

            for (int i = 0; i < _hitCount; i++)
            {
                ISelectable selectable = _hits[i].transform.GetComponent<ISelectable>();
                if(selectable == null) continue;
                AddHover(selectable);
                return;
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

        private void TrySelectFirstHoveredUnit()
        {
            for (int i = 0; i < _hitCount; i++)
            {
                ISelectable selectable = _hits[i].transform.GetComponent<ISelectable>();
                if(selectable == null) continue;
                SceneReferences.Instance.unitManager.SetUnitSelection(selectable);
                return;
            }
            
            SceneReferences.Instance.unitManager.ClearSelection();
        }

        private bool IsMultiUnitSelect()
        {
            if (LeftMouseHeldTime > SingleUnitSelectTime) return true;
            
            return Vector2.Distance(_dragStartPosition, _dragCurrentPosition) > 30f;
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

        private void TrySelectHoveredObject(Ray ray)
        {

        }

        public void OnSelect(InputAction.CallbackContext context)
        {
        }

        public void OnCancel(InputAction.CallbackContext context)
        {

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
    }
}
