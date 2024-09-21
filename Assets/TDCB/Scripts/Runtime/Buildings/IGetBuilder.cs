using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public interface IGetBuilder // Get the object that built this building
    {
        public void GetBuilder(ISelectable builder);
    }
}
