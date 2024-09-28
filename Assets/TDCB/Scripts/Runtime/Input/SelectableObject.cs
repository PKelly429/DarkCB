using System;
using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using Shapes;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = System.Random;


namespace TDCB
{
    public abstract class SelectableObject : MonoBehaviour, ISelectable, ISpatialHashable
    {
        public static readonly HashSet<SelectableObject> AllSelectableObjects = new HashSet<SelectableObject>();

        public int HashGridIndex { get; set; } = -1;
        public bool IsAlive { get; private set; }
        public Transform Transform => transform;

        public abstract int Priority { get; }
        public abstract SelectableType selectableType { get; }
        public abstract UnitStats stats { get; }
        public abstract Unit unit { get; }
        public abstract Building building { get; }
        public abstract HealthComponent health { get; }
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
        private bool _isRegisteredToManagers;

        protected virtual void OnEnable()
        {
            RegisterObject();
        }
        
        protected virtual void OnDisable()
        {
            if (IsAlive)
            {
                DeregisterObject();
            }
        }

        protected void RegisterObject()
        {
            _isRegisteredToManagers = true;
            IsAlive = true;

            if (selectableType == SelectableType.Enemy)
            {
                SceneReferences.Instance.enemyUnitHash.RegisterUnit(this);
                commandListeners = Array.Empty<ICommandRegister>();
            }
            else
            {
                SceneReferences.Instance.playerUnitHash.RegisterUnit(this);   
                AllSelectableObjects.Add(this);
                _highlight = SceneReferences.Instance.highlightManager.RegisterUnit(this);
                ControllableUnit = GetComponent<IControllableUnit>();
                IsControllable = ControllableUnit != null;
            }
            OnRegister();
        }
        
        protected void DeregisterObject()
        {
            IsAlive = false;
            
            if (!_isRegisteredToManagers) return;
            _isRegisteredToManagers = false;
            
            AllSelectableObjects.Remove(this);
            SceneReferences.Instance.unitManager.RemoveUnitFromSelectionAndControlGroups(this);
            SceneReferences.Instance.highlightManager.DeregisterUnit(this);

            if (selectableType == SelectableType.Enemy)
            {
                SceneReferences.Instance.enemyUnitHash.DeregisterUnit(this);
            }
            else
            {
                SceneReferences.Instance.playerUnitHash.DeregisterUnit(this);
            }

            DeregisterCommands();
            OnDeregister();
        }

        protected virtual void OnRegister()
        {
            
        }
        
        protected virtual void OnDeregister()
        {
            
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
        
        public virtual void OnHoverBegin()
        {
            if (selectableType == SelectableType.Enemy) return; //TODO: Make enemies selectable
            
            _highlight.Hovered = true;
            SetLayer();
        }

        public virtual void OnHoverEnd()
        {
            if (selectableType == SelectableType.Enemy) return; //TODO: Make enemies selectable
            
            _highlight.Hovered = false;
            SetLayer();
        }

        public virtual void OnSelect()
        {
            if (selectableType == SelectableType.Enemy) return; //TODO: Make enemies selectable
            
            _highlight.Selected = true;
            SetLayer();
            RegisterCommands();
        }

        public virtual void OnDeSelect()
        {
            if (selectableType == SelectableType.Enemy) return; //TODO: Make enemies selectable
            
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
            commandListeners = GetComponents<ICommandRegister>();
            OnAwake();
        }

        protected virtual void OnAwake()
        {
            
        }
    }
}
