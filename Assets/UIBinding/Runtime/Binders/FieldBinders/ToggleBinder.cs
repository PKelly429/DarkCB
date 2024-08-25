using System;
using UnityEngine;
using UnityEngine.UI;

namespace DataBinding
{
    public class ToggleBinder : AbstractBinder<bool>
    {
        [SerializeField] private Toggle toggleField;
        [BindingType(typeof(bool))] public BindingField target;
        [SerializeField] private bool twoWayBinding = true;

        protected override BindingField BindingField => target;

        protected override void OnBind(object obj)
        {
            if(!twoWayBinding) return;
            toggleField.onValueChanged.AddListener(OnToggleFieldValueChanged);
        }

        protected override void OnUnbind()
        {
            if(!twoWayBinding) return;
            toggleField.onValueChanged.RemoveListener(OnToggleFieldValueChanged);
        }
    
        protected override void OnBindingValueChanged()
        {
            toggleField.SetIsOnWithoutNotify(bindableVariable.GetValue());
        }

        private void OnToggleFieldValueChanged(bool newValue)
        {
            bindableVariable.SetValue(newValue);
        }
        
        private void Awake()
        {
            if (toggleField == null)
            {
                toggleField = GetComponent<Toggle>();
            }
        }
        
#if UNITY_EDITOR
        public override void Reset()
        {
            base.Reset();
            toggleField = GetComponent<Toggle>();
        }
#endif
    }
}
