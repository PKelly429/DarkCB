using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace TDCB
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Camera camera;
        
        [Header ("Scroll")]
        [SerializeField] private float cameraScrollSpeed = 60f;
        [SerializeField] private float cameraScrollModMulti = 2f;
        private float cameraScrollDampTime = 0.3f;
        private float cameraDragDampTime = 0.1f;
        
        private Transform _cachedTransform;
        private Vector3 _targetPosition;
        private Vector3 _cameraVelocity;

        [Header("Zoom")] 
        [SerializeField] private Transform minPosition;
        [SerializeField] private Transform maxPosition;
        [SerializeField] private AnimationCurve tiltCurve;
        [SerializeField] private float zoomSpeed = 0.05f;
        private float cameraZoomDampTime = 0.1f;

        private float zoom;
        
        private float _targetZoom;
        private float _zoomVelocity;
        private Vector3 _minCameraForward;
        private Vector3 _maxCameraForward;
        private float _minZoomDistance;
        private float _maxZoomDistance;

        [Header("Rotation")] 
        [SerializeField] private float rotationSpeed = 5f;

        private bool _rotatingCamera;
        private Vector2 _rotateMousePosition;

        // Input Controls
        private InputControls _inputControls;
        
        private Plane _cameraDragPlane = new Plane(Vector3.up, Vector3.zero);

        private bool CameraSpeedModifierHeld { get; set; }
        private Vector2 PanCameraInput { get; set; }
        private float CameraZoomInput { get; set; }

        private bool DraggingCamera { get; set; }
        private Vector3 StartCameraDragPosition { get; set; }
        private Vector3 CurrentCameraDragPosition { get; set; }
        private bool RotatingCamera { get; set; }

        private bool _cameraDragDown;

        private void Start()
        {
            _inputControls = SceneReferences.Instance.inputHandler.InputControls;
            _inputControls.CameraControls.Enable();
            
            _cachedTransform = GetComponent<Transform>();

            _minZoomDistance = minPosition.localPosition.magnitude;
            _maxZoomDistance = maxPosition.localPosition.magnitude;
            
            _targetZoom = Mathf.InverseLerp(_minZoomDistance, _maxZoomDistance, cameraTransform.localPosition.magnitude);
            zoom = _targetZoom;
            
            _minCameraForward = minPosition.localRotation * Vector3.forward;
            _maxCameraForward = maxPosition.localRotation * Vector3.forward;
            
            Destroy(minPosition.gameObject);
            Destroy(maxPosition.gameObject);
        }

        public void LateUpdate()
        {
            CameraSpeedModifierHeld = _inputControls.CameraControls.CameraSpeedModifier.IsPressed();
            PanCameraInput = _inputControls.CameraControls.CameraMovement.ReadValue<Vector2>();
            CameraZoomInput = _inputControls.CameraControls.CameraZoom.ReadValue<float>();
            
            Ray ray = camera.ScreenPointToRay(SceneReferences.Instance.inputHandler.MouseScreenPosition);

            HandleCameraDragging(ray);
            
            HandleScroll();
            HandleZoom();
            //HandleRotation();
        }
        
        private void HandleCameraDragging(Ray ray)
        {
            RotatingCamera = _inputControls.CameraControls.RotateCamera.IsPressed();
            
            _cameraDragDown = _inputControls.CameraControls.PanCamera.IsPressed();
            if (_cameraDragDown)
            {
                Vector3 cameraDragPosition;
                if (DraggingCamera)
                {
                    if (SetCameraDragPosition(ray, out cameraDragPosition))
                    {
                        CurrentCameraDragPosition = cameraDragPosition;
                    }  
                }
                else if (SetCameraDragPosition(ray, out cameraDragPosition))
                {
                    StartCameraDragPosition = cameraDragPosition;
                    CurrentCameraDragPosition = cameraDragPosition;
                    DraggingCamera = true;
                }  
            }
            else
            {
                DraggingCamera = false;
            }
        }
        
        private bool SetCameraDragPosition(Ray ray, out Vector3 position)
        {
            float entry = 0;
            if (_cameraDragPlane.Raycast(ray, out entry))
            {
                position = ray.GetPoint(entry);
                return true;
            }

            position = Vector3.zero;
            return false;
        }

        private void HandleScroll()
        {
            if (DraggingCamera)
            {
                _targetPosition = _cachedTransform.position + StartCameraDragPosition - CurrentCameraDragPosition;
                _cachedTransform.position = Vector3.SmoothDamp(_cachedTransform.position, _targetPosition, ref _cameraVelocity, cameraDragDampTime);
            }
            else
            {
                Vector2 cameraMoveDelta = PanCameraInput;
                float speed = cameraScrollSpeed * Time.deltaTime;
                speed *= Mathf.Lerp(0.5f, 1, zoom); // scroll at half speed when zoomed in
                if (CameraSpeedModifierHeld) speed *= cameraScrollModMulti;
                Vector3 moveDelta = (_cachedTransform.rotation*new Vector3(cameraMoveDelta.x, 0, cameraMoveDelta.y)) * speed;
                _targetPosition += moveDelta;
                
                _cachedTransform.position = Vector3.SmoothDamp(_cachedTransform.position, _targetPosition, ref _cameraVelocity, cameraScrollDampTime);
            }
        }

        private void HandleZoom()
        {
            float zoomInput = CameraZoomInput * Time.deltaTime;

            _targetZoom = Mathf.Clamp01(_targetZoom - (zoomInput * zoomSpeed));
            zoom = Mathf.SmoothDamp(zoom, _targetZoom, ref _zoomVelocity, cameraZoomDampTime);
            
            float distance = Mathf.Lerp(_minZoomDistance, _maxZoomDistance, zoom);
            Vector3 forward = Vector3.Lerp(_minCameraForward, _maxCameraForward, tiltCurve.Evaluate(zoom));

            cameraTransform.localPosition = forward * -distance;
            cameraTransform.localRotation = Quaternion.LookRotation(forward);
        }

        private void HandleRotation()
        {
            if (_rotatingCamera != RotatingCamera)
            {
                _rotatingCamera = RotatingCamera;
                _rotateMousePosition = SceneReferences.Instance.inputHandler.MouseScreenPosition;
            }

            if (!_rotatingCamera) return;
            Vector2 difference = _rotateMousePosition - SceneReferences.Instance.inputHandler.MouseScreenPosition;
            transform.rotation *= Quaternion.Euler(Vector3.up *(-difference.x/rotationSpeed));
            _rotateMousePosition = SceneReferences.Instance.inputHandler.MouseScreenPosition;
        }
    }
}
