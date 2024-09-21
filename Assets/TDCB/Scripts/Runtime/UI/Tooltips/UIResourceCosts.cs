using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public class UIResourceCosts : MonoBehaviour
    {
        public List<UIResourceCost> resourceCosts = new List<UIResourceCost>();

        public void Set(ResourceValue[] costs)
        {
            for (int i= 0; i < resourceCosts.Count ; i++)
            {
                resourceCosts[i].gameObject.SetActive(i < costs.Length);
                if(i >= costs.Length) continue;
                resourceCosts[i].SetValue(costs[i]);
            }
        }
    }
}
