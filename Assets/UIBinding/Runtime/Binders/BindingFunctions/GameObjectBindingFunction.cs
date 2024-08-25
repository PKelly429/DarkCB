using UnityEngine;

namespace DataBinding
{
    public abstract class GameObjectBindingFunction : ScriptableObject
    {
        public abstract void Bind(GameObject gameObject, object bindingObj);
        public abstract void Unbind(GameObject gameObject , object previousObj);
    }
}