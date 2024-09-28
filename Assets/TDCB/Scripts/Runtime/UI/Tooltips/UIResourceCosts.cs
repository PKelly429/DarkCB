using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public class UIResourceCosts : MonoBehaviour
    {
        public List<UIResourceCost> resourceCosts = new List<UIResourceCost>();

        public void Set(ResourceValue[] costs, bool continuousCost)
        {
            for (int i= 0; i < resourceCosts.Count ; i++)
            {
                resourceCosts[i].gameObject.SetActive(i < costs.Length);
                if(i >= costs.Length) continue;
                if (continuousCost)
                {
                    resourceCosts[i].SetResourceProduction(costs[i]);
                }
                else
                {
                    resourceCosts[i].SetValue(costs[i]);   
                }
            }
        }
        
        public void Set(ResourceProductionValue[] production)
        {
            for (int i = 0; i < resourceCosts.Count; i++)
            {
                resourceCosts[i].gameObject.SetActive(i < production.Length);
                if(i >= production.Length) continue;
                resourceCosts[i].SetResourceProduction(production[i]);
            }
        }
    }
}
