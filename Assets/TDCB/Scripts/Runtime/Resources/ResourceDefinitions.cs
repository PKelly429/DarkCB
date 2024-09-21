using System;
using System.Collections.Generic;
using DataBinding;
using UnityEngine;

namespace TDCB
{
    [CreateAssetMenu(menuName = "TDCB/Resource Definitions")]
    public class ResourceDefinitions : ScriptableObject
    {
        public List<ResourceDefinition> resources;
        private Dictionary<ResourceType, ResourceDefinition> resourceLookup = new Dictionary<ResourceType, ResourceDefinition>();

        public ResourceDefinition GetResourceDefinition(ResourceType resource)
        {
            return resourceLookup[resource];
        }
        
        public void Init()
        {
            foreach (var resource in resources)
            {
                resourceLookup.TryAdd(resource.resource, resource);
            }
        }
    }

    [Serializable]
    public struct ResourceDefinition
    {
        public ResourceType resource;
        public bool stockpiles;
        public bool hasMaximum;
        public int startingAmount;
        public int startingMaximum;
        public Sprite icon;
    }
}