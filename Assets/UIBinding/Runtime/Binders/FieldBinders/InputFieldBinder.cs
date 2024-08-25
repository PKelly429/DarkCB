using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DataBinding
{
    public class InputFieldBinder : AbstractBinder<string>
    {
        public TMP_InputField inputField;
        [BindingType(typeof(string))] public BindingField target;
        
        protected override BindingField BindingField => target;

        protected override void OnBind(object obj)
        {
            inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
        }

        protected override void OnUnbind()
        {
            inputField.onValueChanged.RemoveListener(OnInputFieldValueChanged);
        }
    
        protected override void OnBindingValueChanged()
        {
            inputField.SetTextWithoutNotify(bindableVariable.stringValue);
        }

        private void OnInputFieldValueChanged(string newValue)
        {
            bindableVariable.SetValue(newValue);
        }
        
        private void Awake()
        {
            if (inputField == null)
            {
                inputField = GetComponent<TMP_InputField>();
            }
        }
        
#if UNITY_EDITOR
        public override void Reset()
        {
            base.Reset();
            inputField = GetComponent<TMP_InputField>();
        }
#endif
    }
}
