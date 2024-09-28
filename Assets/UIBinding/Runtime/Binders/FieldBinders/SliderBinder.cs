using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DataBinding
{
    public class SliderBinder : AbstractBinder
    {
        public Slider sliderField;
        
        private BindableVariable<float> current;
        private BindableVariable<float> max;

        [Header("Current"),BindingType(typeof(float))] public BindingField currentTarget;
        [Header("Max"),BindingType(typeof(float))] public BindingField maxTarget;

        private bool _hasModifiers;
        private IBindingModifer<SliderBinder>[] _modifiers;

        public float Current => current.GetValue();
        public float Max => max.GetValue();
        
        public sealed override void Bind(object obj)
        {
            Unbind();
            if (obj == null) return;

            try
            {
                current = currentTarget.GetBindingVariable(obj) as BindableVariable<float>;
                max = maxTarget.GetBindingVariable(obj) as BindableVariable<float>;
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not bind {name}");
            }

            if (current == null || max == null) return;
        
            current.onValueChanged += OnBindingValueChanged;
            max.onValueChanged += OnBindingValueChanged;
            OnBindingValueChanged();
        }

        public sealed override void Unbind()
        {
            if (current == null || max == null) return;
            current.onValueChanged -= OnBindingValueChanged;
            max.onValueChanged -= OnBindingValueChanged;
            current = null;
            max = null;
        }

        public override void DebugBinder()
        {
        }


        protected void OnBindingValueChanged()
        {
            sliderField.maxValue = max.GetValue();
            sliderField.SetValueWithoutNotify(current.GetValue());

            if (!_hasModifiers) return;
            
            for (int i = 0; i < _modifiers.Length; i++)
            {
                _modifiers[i].OnBindingChanged(this);
            }
        }
        
        private void Awake()
        {
            if (sliderField == null)
            {
                sliderField = GetComponent<Slider>();
            }

            _modifiers = GetComponentsInChildren<IBindingModifer<SliderBinder>>();
            _hasModifiers = _modifiers != null;
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
