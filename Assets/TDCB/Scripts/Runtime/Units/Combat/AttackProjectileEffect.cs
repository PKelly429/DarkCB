using System;
using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TDCB
{
    public class AttackProjectileEffect : AttackEffect
    {
        [SerializeField, Required] private SelectableObject _selectableObject;
        public SoundData fireSfx;
        public SoundData hitSfx;
        
        public override void OnAttack(SelectableObject target, Action onHit)
        {
            SoundManager.Instance.CreateSoundBuilder().WithPosition(_selectableObject.Position).Play(fireSfx);
            onHit += () => { PlayHitSFX(target); };
            SceneReferences.Instance.projectileManager.arrowPool.ShootProjectile(_selectableObject.Position + Vector3.up, target.transform, 20f, onHit);
        }

        private void PlayHitSFX(SelectableObject target)
        {
            if (target.IsAlive)
            {
                SoundManager.Instance.CreateSoundBuilder().WithPosition(target.Position).Play(hitSfx);
            }
        }
    }
}
