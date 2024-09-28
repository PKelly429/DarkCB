using System;
using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.VFX;

namespace TDCB
{
    public class MeleeAttackEffect : AttackEffect
    {
        [SerializeField, Required] private SelectableObject _selectableObject;
        public SoundData sfx;
        public VisualEffect vfx;
        public override void OnAttack(SelectableObject target, Action onHit)
        {
            onHit?.Invoke();
            SoundManager.Instance.CreateSoundBuilder().WithPosition(_selectableObject.Position).Play(sfx);

            if (vfx != null)
            {
                vfx.Play();
            }
        }
    }
}
