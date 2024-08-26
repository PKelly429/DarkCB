using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public static class UnityExtensions
    {
        public static bool IsAlive(this object aObj)
        {
            var o = aObj as UnityEngine.Object;
            return o != null;
        }
    }
}
