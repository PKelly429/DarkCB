using System;
using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using Shapes;
using Sirenix.OdinInspector;
using UnityEngine;


namespace TDCB
{
    public class SelectableObject : MonoBehaviour, ISelectable
    {
        [SerializeField, SuffixLabel("Higher Better")] private int priority = 1;
        [SerializeField] private SelectableType unitType;
        [SerializeField] private float size;
        [SerializeField] private CommandTemplate commands;
        [SerializeField] private Collider collider;
        [SerializeField] private SoundData selectSoundData;

        public static readonly HashSet<SelectableObject> AllSelectableObjects = new HashSet<SelectableObject>();

        public int Priority => priority;
        public SelectableType selectableType => unitType;
        public float Size => size;
        
        public SoundData SelectionClip => selectSoundData;
        
        public bool IsControllable { get; private set; }
        public IControllableUnit ControllableUnit { get; private set; }

        public Vector3 Position => transform.position;
        public Collider Collider => collider;

        private ICommandRegister[] commandListeners;
        private IHighlight _highlight;

        private void OnEnable()
        {
            AllSelectableObjects.Add(this);
            _highlight = SceneReferences.Instance.highlightManager.RegisterUnit(this);
            ControllableUnit = GetComponent<IControllableUnit>();
            IsControllable = ControllableUnit != null;
        }
        
        private void OnDisable()
        {
            AllSelectableObjects.Remove(this);
            SceneReferences.Instance.highlightManager.DeregisterUnit(this);
        }

        private void Start()
        {
            commandListeners = GetComponents<ICommandRegister>();
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
            UIReferences.Instance.commandButtonGrid.Bind(commands);
            
            _highlight.Selected = true;
            SetLayer();
            RegisterCommands();
        }

        public void OnDeSelect()
        {
            UIReferences.Instance.commandButtonGrid.Unbind();
            
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
