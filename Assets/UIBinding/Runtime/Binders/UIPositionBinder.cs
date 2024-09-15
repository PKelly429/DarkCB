using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataBinding
{
    [RequireComponent(typeof(RectTransform))]
    public class UIPositionBinder : AbstractBinder<Transform>
    {
        private RectTransform _rectTransform;
        private RectTransform _canvasTransform;
        
        private Transform targetTransform;
        private Camera mainCamera;

        private void Awake()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            _rectTransform = GetComponent<RectTransform>();
            _canvasTransform = _rectTransform.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            mainCamera = Camera.main;
        }

        public void Update()
        {
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (targetTransform == null) return;
            
            Vector2 screenPoint = mainCamera.WorldToScreenPoint(targetTransform.position + Vector3.up);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasTransform, screenPoint, null, out var pos);
            _rectTransform.anchoredPosition = pos;
        }


        [BindingType(typeof(Transform))] public BindingField target;
        protected override BindingField BindingField => target;
        protected override void OnBindingValueChanged()
        {
            targetTransform = bindableVariable.GetValue();
            UpdatePosition();
        }
    }
}
