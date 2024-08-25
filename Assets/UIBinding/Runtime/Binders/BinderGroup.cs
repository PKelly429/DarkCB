using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DataBinding
{
    public class BinderGroup : AbstractBinder
    {
        [SerializeField] private List<AbstractBinder> binders;
        public override void Bind(object obj)
        {
            foreach (var binder in binders)
            {
                binder.Bind(obj);
            }
        }

        public override void Unbind()
        {
        }

#if UNITY_EDITOR
        [ContextMenu("Add Children")]
        public void AddChildren()
        {
            for (int i = binders.Count - 1; i >= 0; i--)
            {
                if (binders[i] == null || binders[i] == this)
                {
                    binders.RemoveAt(i);
                }
            }
            
            for (int i = 0; i < transform.childCount; i++)
            {
                AddChildrenRecursive(transform.GetChild(i));
            }
            
            EditorUtility.SetDirty(this);
        }

        private void AddChildrenRecursive(Transform obj)
        {
            AbstractBinder[] objBinders = obj.GetComponents<AbstractBinder>();


            bool hasGroup = false;
            if (objBinders != null)
            {
                foreach (var binder in objBinders)
                {
                    if (!binders.Contains(binder))
                    {
                        binders.Add(binder);
                    }
                    
                    hasGroup |= binder.GetType() == typeof(BinderGroup);
                }
            }

            // Stop searching if we find another group
            if (hasGroup) return;

            for (int i = 0; i < obj.childCount; i++)
            {
                AddChildrenRecursive(obj.GetChild(i));
            }
        }

        public void Add(AbstractBinder binder)
        {
            if (binder == this) return;
            if (binders.Contains(binder)) return;
            
            binders.Add(binder);
            EditorUtility.SetDirty(this);
        }

        public void Remove(AbstractBinder binder)
        {
            binders.Remove(binder);
            EditorUtility.SetDirty(this);
        }

        public override void DebugBinder()
        {
        }
#endif
    }
}
