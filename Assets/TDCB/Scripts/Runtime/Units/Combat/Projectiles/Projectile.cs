using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

namespace TDCB
{
    public class Projectile : MonoBehaviour
    {
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public void Shoot(Vector3 startPos, Transform target, float speed, Action onHit)
        {
            Vector3 targetPos = target.position;
            float distance = Vector3.Distance(startPos, targetPos);
            float timeToHit = distance / speed;

            transform.forward = targetPos - startPos;
            Tween.Position(transform, startPos, targetPos, timeToHit).OnComplete(onHit);
        }
    }
}
