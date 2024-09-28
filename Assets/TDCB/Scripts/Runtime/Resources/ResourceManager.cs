using System;
using System.Collections;
using System.Collections.Generic;
using DataBinding;
using UnityEngine;

namespace TDCB
{
    [Bindable]
    public class ResourceManager : MonoBehaviour
    {
        [SerializeField] private ResourceDefinitions _resourceDefinitions;
        [SerializeField] private float _tickTime;

        public float TickTime => _tickTime;
        [NonSerialized] public BindableFloat TickTimer = new BindableFloat(0);

        private readonly Dictionary<ResourceType, Resource> _resources = new Dictionary<ResourceType, Resource>();

        private HashSet<ResourceProducer> _producers = new HashSet<ResourceProducer>();
        private HashSet<ResourceConsumer> _consumers = new HashSet<ResourceConsumer>();

        private IEnumerator _resourceUpdater;
        
        public Resource GetResource(ResourceType resource)
        {
            return _resources[resource];
        }
        public Sprite GetResourceIcon(ResourceType resource)
        {
            return _resourceDefinitions.GetResourceDefinition(resource).icon;
        }

        public void Register(ResourceProducer producer)
        {
            _producers.Add(producer);
            producer.OnProductionRateChanged += UpdateProductionRate;
        }
        public void Unregister(ResourceProducer producer)
        {
            if (!_producers.Contains(producer)) return;
            
            _producers.Remove(producer);
            producer.OnProductionRateChanged -= UpdateProductionRate;
        }

        public void UpdateResourceMaximum(ResourceType resource, int amount)
        {
            if (resource == ResourceType.None) return;
            
            _resources[resource].Max.SetValue(_resources[resource].Max + amount);
        }
        
        public void PayResourceCost(ResourceValue cost)
        {
            if (cost.resourceType == ResourceType.Population)
            {
                UpdateResourceValue(cost.resourceType, cost.value);
                return;
            }
            
            UpdateResourceValue(cost.resourceType, -cost.value);
        }
        
        public void RefundResourceCost(ResourceValue cost)
        {
            if (cost.resourceType == ResourceType.Population)
            {
                UpdateResourceValue(cost.resourceType, -cost.value);
                return;
            }
            
            UpdateResourceValue(cost.resourceType, cost.value);
        }
        
        public void UpdateResourceValue(ResourceType resource, int amount)
        {
            if (resource == ResourceType.None) return;
            
            _resources[resource].Value.SetValue(_resources[resource].Value + amount);
            if (_resources[resource].Value > _resources[resource].Max)
            {
                _resources[resource].Value.SetValue(_resources[resource].Value);
            }
        }

        private void UpdateProductionRate(ResourceType resource, int difference)
        {
            if (resource == ResourceType.None) return;
            
            _resources[resource].ProductionRate.SetValue(_resources[resource].ProductionRate+difference);
        }

        public bool CanAffordCost(ResourceValue cost)
        {
            if (cost.resourceType == ResourceType.None) return true;
            if (cost.value <= 0) return true;
            
            if (cost.resourceType == ResourceType.Population)
            {
                return _resources[ResourceType.Population].Value + cost.value <= _resources[ResourceType.Population].Max;
            }
            return _resources[cost.resourceType].Value >= cost.value;
        }

        private IEnumerator UpdateResourceLoop()
        {
            while (true)
            {
                try
                {
                    float time = TickTimer + Time.deltaTime;
                    if (time > _tickTime)
                    {
                        UpdateResources();
                        time = 0;
                    }

                    TickTimer.SetValue(time);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                yield return null;
            }
        }

        private void UpdateResources()
        {
            foreach (var resource in _resources.Values)
            {
                if (resource.stockpiles)
                {
                    resource.Value.SetValue(Mathf.Min(resource.Value + resource.ProductionRate, resource.Max));
                }
            }
        }

        private void Awake()
        {
            _resourceDefinitions.Init();
            _resourceUpdater = UpdateResourceLoop();
            
            foreach (var resource in _resourceDefinitions.resources)
            {
                _resources.Add(resource.resource, new Resource()
                {
                    stockpiles = resource.stockpiles,
                    hasMaximum = resource.hasMaximum,
                    ProductionRate = new BindableInt(0),
                    Value = new BindableInt(resource.startingAmount),
                    Max = new BindableInt(resource.startingMaximum)
                });
            }
        }

        private void Update()
        {
            if (_resourceUpdater == null)
            {
                _resourceUpdater = UpdateResourceLoop();
            }
            
            _resourceUpdater.MoveNext();
        }
    }

    [Bindable]
    public class Resource
    {
        public bool stockpiles;
        public bool hasMaximum;
        public BindableInt ProductionRate;
        public BindableInt Value;
        public BindableInt Max;
    }
    
    [Serializable]
    public struct ResourceValue
    {
        public ResourceType resourceType;
        public int value;
    }
    
    [Serializable]
    public struct ResourceProductionValue
    {
        public ResourceType resourceType;
        public bool perTile;
        public float value;
    }
}
