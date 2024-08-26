using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public static class EnumExtensions
    {
        public static bool HasAnyFlag(this Enum value, Enum flag)
        {
            long lValue = Convert.ToInt64(value);
            long lFlag = Convert.ToInt64(flag);
            return (lValue | lFlag) == lFlag;
        }
    }
}
