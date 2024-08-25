using System.Collections;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.Events;

namespace TDCB
{
    public interface ICommandRegister
    {
        public void Register();
        public void Deregister();
    }
    
    public interface ICommandListener
    {
        public void Raise();
    }
    
    public interface ICommandListener<T>
    {
        public void Raise(T value);
    }
    
    public class CommandListener : MonoBehaviour, ICommandRegister, ICommandListener
    {
        [SerializeField] Command command;
        [SerializeField] UnityEvent unityEvent;
        
        public void Register()
        {
            command.Register(this);
        }

        public void Deregister()
        {
            command.Deregister(this);
        }

        public void Raise()
        {
            unityEvent?.Invoke();
        }
    }
    
}
