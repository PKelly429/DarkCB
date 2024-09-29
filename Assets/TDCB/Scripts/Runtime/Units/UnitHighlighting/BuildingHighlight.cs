using System.Collections;
using System.Collections.Generic;
using Shapes;
using UnityEngine;

namespace TDCB
{
    public class BuildingHighlight : MonoBehaviour, IHighlight
    {
        [SerializeField] private Rectangle selectedShape;
        [SerializeField] private Rectangle hoverShape;

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
            size += 0.5f;
            selectedShape.Width = size;
            selectedShape.Height = size;
            hoverShape.Width = size + 0.5f;
            hoverShape.Height = size + 0.5f;
        }
        
        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void UpdatePosition(Vector3 position, float deltaDashOffset)
        {
            if (_hovered) hoverShape.DashOffset += deltaDashOffset;
        }
    }
}
