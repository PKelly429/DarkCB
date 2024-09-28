using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataBinding
{
    public interface IBindingModifer<T> where T : AbstractBinder
    {
        public void OnBindingChanged(T binder);
    }
}
