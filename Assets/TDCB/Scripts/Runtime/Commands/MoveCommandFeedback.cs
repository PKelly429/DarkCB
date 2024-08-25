using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using Shapes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TDCB
{
    public class MoveCommandFeedback : MonoBehaviour
    {
        [SerializeField] private DiscAnimator[] _animators;
        
        private void OnEnable()
        {
            foreach (var animator in _animators)
            {
                animator.Animate();
            }
        }
    }

    [Serializable]
    public struct DiscAnimator
    {
        [SerializeField] private Disc disc;
        [SerializeField] private float startPos;
        [SerializeField] private float endPos;
        [SerializeField] private float duration;

        public void Animate()
        {
            disc.enabled = true;
            Tween.Custom(startPos, endPos, duration/2f, updateValue, Ease.InSine).Chain(Tween.Custom(endPos, startPos, duration, updateValue, Ease.InSine)).OnComplete(finish);
        }
        
        private void updateValue(float value)
        {
            disc.Radius = value;
        }

        private void finish()
        {
            disc.enabled = false;
        }
        
    }
}
