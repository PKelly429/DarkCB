using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public enum ResourceType : byte
    {
        None,
        Population,
        Food,
        Wood,
        Stone,
        Iron,
    }
    
    public static class ResourceTypeExtension
    {
        public static byte GetResourceTexMapColour(this ResourceType resourceType)
        {
            switch (resourceType)
            {
                case ResourceType.None:
                    return 0;
                case ResourceType.Wood:
                    return 64;
                case ResourceType.Stone:
                    return 128;
                case ResourceType.Iron:
                    return 192;
            }

            return 0;
        }
    }
}
