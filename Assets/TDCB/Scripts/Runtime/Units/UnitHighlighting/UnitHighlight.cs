using System;
using System.Collections;
using System.Collections.Generic;
using Shapes;
using UnityEngine;

namespace TDCB
{
    public class UnitHighlight : MonoBehaviour, IHighlight
    {
        [SerializeField] private Disc selectedShape;
        [SerializeField] private Disc hoverShape;

        private Transform _cachedTransform;

        private void Awake()
        {
            _cachedTransform = transform;
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
        
        private bool _selected;
        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                if(selectedShape == null) return;
                selectedShape.enabled = _selected;
            } 
        }
        
        private bool _hovered;

        public bool Hovered
        {
            get => _hovered;
            set
            {
                _hovered = value;
                hoverShape.enabled = _hovered;
            } 
        }

        public void SetSize(float size)
        {
            selectedShape.Radius = size;
            hoverShape.Radius = size + selectedShape.Thickness;
        }
        
        public void SetPosition(Vector3 position)
        {
            _cachedTransform.position = position;
        }

        public void UpdatePosition(Vector3 position, float deltaDashOffset)
        {
            _cachedTransform.position = position;
            if (_hovered) hoverShape.DashOffset += deltaDashOffset;
        }
    }
}
