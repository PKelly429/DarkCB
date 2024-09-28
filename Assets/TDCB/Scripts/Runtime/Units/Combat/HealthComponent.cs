using System.Collections;
using System.Collections.Generic;
using DataBinding;
using UnityEngine;

namespace TDCB
{
    [Bindable]
    public class HealthComponent : MonoBehaviour
    {
        public BindableFloat CurrentHealth;
        public BindableFloat MaxHealth;

        public delegate void Killed();
        public event Killed OnKilled;

        public void ApplyDamage(float value)
        {
            ApplyHealthChange(-value);
        }

        public void ApplyHealthChange(float value)
        {
            CurrentHealth.SetValue(Mathf.Min(CurrentHealth + value, MaxHealth));
            if (CurrentHealth <= 0)
            {
                OnKilled?.Invoke();
            }
        }
    }
}
