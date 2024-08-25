using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataBinding
{
    public class ActivateGameObjectBinder : AbstractBinder
    {
        public enum BindingMode
        {
            BindingObjectType,
            BindToBool,
            Function
        }
        
        public BindingMode bindingMode;
        
        //BindingObjectType
        public BindingType bindingType;
        
        //BindToBool
        [SerializeField] private BindingField target;
        private BindableVariable<bool> bindableVariable;
        
        //Function
        [SerializeField] private GameObjectBindingFunction bindingFunction;
        private object boundObj;
        
        
        public override void Bind(object obj)
        {
            Unbind();
            if (obj == null) return;
            
            switch (bindingMode)
            {
                case BindingMode.BindingObjectType:
                    gameObject.SetActive(bindingType.Type == obj.GetType());
                    break;
                case BindingMode.BindToBool:
                    try
                    {
                        bindableVariable = target.GetBindingVariable(obj) as BindableVariable<bool>;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Could not bind {name}");
                    }

                    if (bindableVariable == null) return;
        
                    bindableVariable.onValueChanged += OnBindingValueChanged;
                    OnBindingValueChanged();
                    break;
                case BindingMode.Function:
                    if (bindingFunction == null) return;
                    boundObj = obj;
                    bindingFunction.Bind(gameObject, boundObj);
                    break;
            }
        }

        private void OnBindingValueChanged()
        {
            gameObject.SetActive(bindableVariable.GetValue());
        }

        public override void Unbind()
        {
            switch (bindingMode)
            {
                case BindingMode.BindingObjectType:
                    gameObject.SetActive(false);
                    break;
                case BindingMode.BindToBool:
                    if (bindableVariable != null)
                    {
                        bindableVariable.onValueChanged -= OnBindingValueChanged;
                    }

                    gameObject.SetActive(false);
                    break;
                case BindingMode.Function:
                    if (bindingFunction == null)
                    {
                        gameObject.SetActive(false);
                    }
                    else
                    {
                        bindingFunction.Unbind(gameObject, boundObj);
                        boundObj = null;
                    }
                    break;
            }
        }
        
        public override void OnDisable()
        {
            // don't automatically unbind on disable
        }
        
#if UNITY_EDITOR
        public override void DebugBinder()
        {
            switch (bindingMode)
            {
                case BindingMode.BindingObjectType:
                    break;
                case BindingMode.BindToBool:
                    target.PrintMessageIfInvalid(gameObject);
                    break;
                case BindingMode.Function:
                    if (bindingFunction == null)
                    {
                        Debug.Log($"GameObject Binder {name} has no function", gameObject);
                    }
                    break;
            }
        }
#endif
    }
}
