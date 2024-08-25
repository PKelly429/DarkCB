using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public interface IHighlight
    {
        public void SetActive(bool active);
        public bool Selected { get; set; }
        public bool Hovered { get; set; }

        public void SetSize(float size);
        public void SetPosition(Vector3 position);
        public void UpdatePosition(Vector3 position, float deltaDashOffset);
    }
}
