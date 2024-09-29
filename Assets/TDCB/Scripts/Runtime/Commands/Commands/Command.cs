using System;
using System.Collections.Generic;
using AudioSystem;
using DataBinding;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TDCB
{
    [Serializable, Bindable]
    public abstract class BaseCommand : ScriptableObject, ICommand
    {
        [PreviewField] public Sprite image;
        [PreviewField] public Sprite imageHover;
        public BindableString hotkey;
        public SoundData soundClip;

        [BoxGroup("Tooltip"), HideLabel] public Tooltip tooltip;
        public abstract void Execute();

        public virtual Tooltip GetTooltip()
        {
            return tooltip;
        }
    }

    [CreateAssetMenu(menuName="Command/Command")]
    public class Command : BaseCommand
    {
        protected readonly HashSet<ICommandListener> observers = new HashSet<ICommandListener>();

        public override void Execute()
        {
            foreach (var observer in observers)
            {
                observer.Raise();
            }
            
            if(soundClip != null) SoundManager.Instance.CreateSoundBuilder().Play(soundClip);
        }
            
        public void Register(ICommandListener observer) => observers.Add(observer);
        public void Deregister(ICommandListener observer) => observers.Remove(observer);
    }
    
    public abstract class Command<T> : BaseCommand
    {
        protected readonly HashSet<ICommandListener<T>> observers = new HashSet<ICommandListener<T>>();

        /// <summary>
        /// Commands that require a value must implement the base Execute() and use it to call Execute(value)
        /// </summary>
        /// <param name="value"></param>
        public virtual void Execute(T value)
        {
            foreach (var observer in observers)
            {
                observer.Raise(value);
            }
            
            if(soundClip != null) SoundManager.Instance.CreateSoundBuilder().Play(soundClip);
        }
            
        public void Register(ICommandListener<T> observer) => observers.Add(observer);
        public void Deregister(ICommandListener<T> observer) => observers.Remove(observer);

    }
}