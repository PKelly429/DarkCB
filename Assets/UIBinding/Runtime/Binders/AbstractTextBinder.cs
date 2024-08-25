using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataBinding
{
    public abstract class AbstractTextBinder : AbstractBinder
    {
        [SerializeField] private BindingField target;
        [SerializeField] protected string stringFormat;

        protected AbstractBindableVariable bindableVariable;

        public sealed override void Bind(object obj)
        {
            Unbind();

            bindableVariable = target.GetBindingVariable(obj);
            if (bindableVariable == null) return;

            bindableVariable.onValueChanged += OnValueChanged;
            OnValueChanged();
        }

        public sealed override void Unbind()
        {
            if (bindableVariable == null) return;
            bindableVariable.onValueChanged -= OnValueChanged;
            bindableVariable = null;
        }

        protected abstract void OnValueChanged();

        protected virtual string GetBoundText()
        {
            if (string.IsNullOrEmpty(stringFormat))
            {
                return bindableVariable.stringValue;
            }
            
            return string.Format(stringFormat, bindableVariable.stringValue);
        }
        
#if UNITY_EDITOR
        public override void DebugBinder()
        {
            target.PrintMessageIfInvalid(gameObject);
        }
#endif
    }
}
