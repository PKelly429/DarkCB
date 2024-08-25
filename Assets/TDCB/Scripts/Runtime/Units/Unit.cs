using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace TDCB
{
    public class Unit : SelectableObject
    {
        [SerializeField] private UnitData data;
        [SerializeField] private float size;
        [SerializeField] private Collider selectionCollider;


        public override int Priority => data.priority;
        public override SelectableType selectableType => SelectableType.Unit;
        public override Sprite Icon => data.icon;
        public override bool HasCommands => true;
        public override CommandTemplate Commands => data.commands;
        public override float Size => size;
        public override SoundData SelectionClip => data.selectSound;
        public override Collider Collider => selectionCollider;
    }
}
