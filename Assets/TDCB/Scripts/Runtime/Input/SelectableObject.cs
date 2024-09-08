using System;
using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using Shapes;
using Sirenix.OdinInspector;
using UnityEngine;


namespace TDCB
{
    public abstract class SelectableObject : MonoBehaviour, ISelectable, ISpatialHashable
    {
        public static readonly HashSet<SelectableObject> AllSelectableObjects = new HashSet<SelectableObject>();
        
        public int HashGridIndex { get; set; }
        public Transform Transform => transform;

        public abstract int Priority { get; }
        public abstract SelectableType selectableType { get; }
        public abstract Sprite Icon { get; }
        public abstract float Size { get; }
        public abstract Collider Collider { get; }
        
        public abstract SoundData SelectionClip { get; }
        
        public bool IsControllable { get; private set; }
        public IControllableUnit ControllableUnit { get; private set; }
        
        public abstract bool HasCommands { get; }
        public abstract CommandTemplate Commands { get; }


        public Vector3 Position => transform.position;

        private ICommandRegister[] commandListeners;
        private IHighlight _highlight;

        protected virtual void OnEnable()
        {
            RegisterObject();
        }
        
        protected virtual void OnDisable()
        {
            DeregisterObject();
        }

        protected virtual void Start()
        {
            commandListeners = GetComponents<ICommandRegister>();
        }

        protected void RegisterObject()
        {
            AllSelectableObjects.Add(this);
            _highlight = SceneReferences.Instance.highlightManager.RegisterUnit(this);
            ControllableUnit = GetComponent<IControllableUnit>();
            IsControllable = ControllableUnit != null;

            SceneReferences.Instance.playerUnitHash.RegisterUnit(this);
        }
        
        protected void DeregisterObject()
        {
            AllSelectableObjects.Remove(this);
            SceneReferences.Instance.highlightManager.DeregisterUnit(this);
            
            SceneReferences.Instance.playerUnitHash.DeregisterUnit(this);
        }

        private void RegisterCommands()
        {
            foreach (var commandListener in commandListeners)
            {
                commandListener.Register();
            }
        }
        
        private void DeregisterCommands()
        {
            foreach (var commandListener in commandListeners)
            {
                commandListener.Deregister();
            }
        }
        
        public void OnHoverBegin()
        {
            _highlight.Hovered = true;
            SetLayer();
        }

        public void OnHoverEnd()
        {
            _highlight.Hovered = false;
            SetLayer();
        }

        public void OnSelect()
        {
            _highlight.Selected = true;
            SetLayer();
            RegisterCommands();
        }

        public void OnDeSelect()
        {
            _highlight.Selected = false;
            SetLayer();
            DeregisterCommands();
        }

        private void SetLayer()
        {
            // if (_isHovered || _isSelected)
            // {
            //     renderer.layer = _highlightLayer;
            // }
            // else
            // {
            //     renderer.layer = _defaultLayer;
            // }
        }

        private void Awake()
        {
            OnAwake();
        }

        protected virtual void OnAwake()
        {
            
        }
        
    }
}
