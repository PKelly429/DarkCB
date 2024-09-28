using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public abstract class AttackEffect : MonoBehaviour
    {
        public abstract void OnAttack(SelectableObject target, Action onHit);
    }
}
