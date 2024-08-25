using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using Shapes;
using UnityEngine;

namespace TDCB
{
    public interface ISelectable
    {
        public int Priority { get; }
        public SelectableType selectableType { get; }
        public bool IsControllable { get; }
        public IControllableUnit ControllableUnit { get; }
        
        public Vector3 Position { get; }
        public float Size { get; }
        
        // SFX
        public SoundData SelectionClip { get; }
        
        
        
        public void OnHoverBegin();
        public void OnHoverEnd();
        
        public void OnSelect();
        public void OnDeSelect();
    }

    public enum SelectableType
    {
        Unit,
        Building
    }

    public interface IHoverable
    {
        public void OnHoverBegin();
        public void OnHoverEnd();
    }

    public interface IClickable
    {
        public void OnClick();
        public void OnRightClick();
    }
}
