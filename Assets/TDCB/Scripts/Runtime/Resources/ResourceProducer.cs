using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public class ResourceProducer : MonoBehaviour
    {
        public ResourceType resource;
        public int ProductionRate { get; private set; }

        public delegate void RateChanged(ResourceType resource, int difference);

        public event RateChanged OnProductionRateChanged;

        public void SetProductionRate(int value)
        {
            int difference = value-ProductionRate;
            ProductionRate = value;
            OnProductionRateChanged?.Invoke(resource, difference);
        }

        private void OnEnable()
        {
            SceneReferences.Instance.resourceManager.Register(this);
            OnProductionRateChanged?.Invoke(resource, ProductionRate);
        }

        private void OnDisable()
        {
            OnProductionRateChanged?.Invoke(resource, -ProductionRate);
            SceneReferences.Instance.resourceManager.Unregister(this);
        }
    }
}
