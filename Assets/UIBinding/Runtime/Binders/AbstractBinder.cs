using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataBinding
{
    [ExecuteInEditMode]
    public abstract class AbstractBinder : MonoBehaviour
    {
        public virtual void OnDisable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            Unbind();
        }

        public abstract void Bind(object obj);
        public abstract void Unbind();
        
#if UNITY_EDITOR
        public abstract void DebugBinder();
        
        public virtual void Reset()
        {
            BinderGroup binderGroup = FindComponentInParent<BinderGroup>(transform.parent);
            if (binderGroup != null)
            {
                binderGroup.Add(this);
            }
        }
        
        private void OnDestroy()
        {
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                BinderGroup binderGroup = FindComponentInParent<BinderGroup>(transform.parent);
                if (binderGroup != null)
                {
                    binderGroup.Remove(this);
                }
            }
        }

        private T FindComponentInParent<T>(Transform obj) where T : MonoBehaviour
        {
            if (obj == null) return null;
            
            T component = obj.GetComponent<T>();
            if (component != null)
            {
                return component;
            }

            if (obj.parent == null)
            {
                return null;
            }

            return FindComponentInParent<T>(obj.parent);
        }
#endif
    }

    public abstract class AbstractBinder<T> : AbstractBinder
    {
        protected BindableVariable<T> bindableVariable;

        protected abstract BindingField BindingField { get; }
        
        public sealed override void Bind(object obj)
        {
            Unbind();
            if (obj == null) return;

            try
            {
                bindableVariable = BindingField.GetBindingVariable(obj) as BindableVariable<T>;
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not bind {name}");
            }

            if (bindableVariable == null) return;
        
            bindableVariable.onValueChanged += OnBindingValueChanged;
            OnBindingValueChanged();
            OnBind(obj);
        }

        public sealed override void Unbind()
        {
            if (bindableVariable == null) return;
            bindableVariable.onValueChanged -= OnBindingValueChanged;
            bindableVariable = null;
            OnUnbind();
        }

        protected virtual void OnBind(object obj) { }
        protected virtual void OnUnbind() { }
        protected abstract void OnBindingValueChanged();
        
#if UNITY_EDITOR
        public override void DebugBinder()
        {
            BindingField.PrintMessageIfInvalid(gameObject);
        }
#endif
    }
}