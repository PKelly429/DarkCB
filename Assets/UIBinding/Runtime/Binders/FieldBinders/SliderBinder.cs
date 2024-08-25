using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DataBinding
{
    public class SliderBinder : AbstractBinder<float>
    {
        public Slider sliderField;
        [BindingType(typeof(float))] public BindingField target;
        [SerializeField] private bool twoWayBinding = true;
        
        protected override BindingField BindingField => target;
        
        protected override void OnBind(object obj)
        {
            if(!twoWayBinding) return;
            sliderField.onValueChanged.AddListener(OnSliderFieldValueChanged);
        }

        protected override void OnUnbind()
        {
            if(!twoWayBinding) return;
            sliderField.onValueChanged.RemoveListener(OnSliderFieldValueChanged);
        }
    
        protected override void OnBindingValueChanged()
        {
            sliderField.SetValueWithoutNotify(bindableVariable.GetValue());
        }

        private void OnSliderFieldValueChanged(float newValue)
        {
            bindableVariable.SetValue(newValue);
        }
        
        private void Awake()
        {
            if (sliderField == null)
            {
                sliderField = GetComponent<Slider>();
            }
        }
        
#if UNITY_EDITOR
        public override void Reset()
        {
            base.Reset();
            sliderField = GetComponent<Slider>();
        }
#endif
    }
}
