using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DataBinding
{
    public class GraphicFunctionBinder : AbstractBinder
    {
        [SerializeField] private Graphic graphic;
        [SerializeField] private GraphicBindingFunction bindingFunction;
        private object boundObj;
        
        public override void Bind(object obj)
        {
            if (bindingFunction == null) return;
            
            Unbind();

            boundObj = obj;
            bindingFunction.Bind(graphic, boundObj);
        }

        public override void Unbind()
        {
            if (bindingFunction == null) return;
            
            bindingFunction.Unbind(graphic, boundObj);
            boundObj = null;
        }
        
#if UNITY_EDITOR
        public override void DebugBinder()
        {
            if (bindingFunction == null)
            {
                Debug.Log($"GraphicFunction Binder {name} has no function", gameObject);
            }
        }
#endif
    }
}
