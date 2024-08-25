using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace DataBinding
{
    public class GraphicColourBinder : AbstractBinder<Color>
    {
        [SerializeField] private Graphic graphic;
        [BindingType(typeof(Color))] public BindingField target;

        [SerializeField] private bool smoothValue;
        [SerializeField] private float smoothTime;

        protected override BindingField BindingField => target;

        protected override void OnBindingValueChanged()
        {
            if (smoothValue)
            {
                Tween.Color(graphic, bindableVariable.GetValue(), smoothTime);
            }
            else
            {
                graphic.color = bindableVariable.GetValue();
            }
        }
        
        private void Awake()
        {
            if (graphic == null)
            {
                graphic = GetComponent<Graphic>();
            }
            
            if (smoothTime <= 0) smoothValue = false;
        }
        
#if UNITY_EDITOR
        public override void Reset()
        {
            base.Reset();
            graphic = GetComponent<Graphic>();
        }
#endif
    }
}
