using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DataBinding
{
    public abstract class GraphicBindingFunction : ScriptableObject
    {
        public abstract void Bind(Graphic graphic, object obj);
        public abstract void Unbind(Graphic graphic, object obj);
    }
}
