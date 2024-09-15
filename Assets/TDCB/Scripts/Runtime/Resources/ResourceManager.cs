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
        [SerializeField] private int _defaultMaxStockpile;

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

        private void UpdateProductionRate(ResourceType resource, int difference)
        {
            _resources[resource].ProductionRate.SetValue(_resources[resource].ProductionRate+difference);
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
                    ProductionRate = new BindableInt(0),
                    Value = new BindableInt(0),
                    Max = new BindableInt(_defaultMaxStockpile)
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
        public BindableInt ProductionRate;
        public BindableInt Value;
        public BindableInt Max;
    }
}
