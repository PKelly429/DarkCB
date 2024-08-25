using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TDCB
{
    public class PositionCommandListener : MonoBehaviour, ICommandRegister, ICommandListener<Vector3>
    {
        [SerializeField] Command<Vector3> command;
        [SerializeField] UnityEvent<Vector3> unityEvent;
        
        public void Register()
        {
            command.Register(this);
        }

        public void Deregister()
        {
            command.Deregister(this);
        }

        public void Raise(Vector3 value)
        {
            unityEvent?.Invoke(value);
        }
    }
}
