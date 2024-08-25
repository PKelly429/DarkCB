using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DataBinding
{
    public class TMPDropdownBinder : AbstractBinder
    {
        [SerializeField] private TMP_Dropdown dropdown;
        protected AbstractBindableVariable bindableVariable;
        [BindingType(typeof(Enum))] public BindingField target;

        public sealed override void Bind(object obj)
        {
            Unbind();

            try
            {
                bindableVariable = target.GetBindingVariable(obj);
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not bind {name}");
            }

            if (bindableVariable == null) return;
        
            bindableVariable.onValueChanged += OnBindingValueChanged;
            OnBindingValueChanged();
            
            List<string> options = new List<string>();
            foreach (var enumValue in Enum.GetValues(bindableVariable.Type))
            {
                options.Add(bindableVariable.GetLocalisedEnumText(enumValue));   
            }
            dropdown.AddOptions(options);
            
            dropdown.onValueChanged.AddListener(OnInputFieldValueChanged);
        }
        
        public sealed override void Unbind()
        {
            dropdown.ClearOptions();
            dropdown.onValueChanged.RemoveListener(OnInputFieldValueChanged);
            
            if (bindableVariable == null) return;
            bindableVariable.onValueChanged -= OnBindingValueChanged;
            bindableVariable = null;
        }


        protected void OnBindingValueChanged()
        {
            dropdown.SetValueWithoutNotify(Convert.ToInt32(bindableVariable.value));
        }
        
        private void OnInputFieldValueChanged(int newValue)
        {
            bindableVariable.value = newValue;
        }
        
        
        private void Awake()
        {
            if (dropdown == null)
            {
                dropdown = GetComponent<TMP_Dropdown>();
            }
        }
        
#if UNITY_EDITOR
        public override void Reset()
        {
            base.Reset();
            dropdown = GetComponent<TMP_Dropdown>();
        }
        
        public override void DebugBinder()
        {
            target.PrintMessageIfInvalid(gameObject);
        }
#endif

    }
}
