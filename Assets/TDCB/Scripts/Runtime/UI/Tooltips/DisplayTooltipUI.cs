using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TDCB
{
    public class DisplayTooltipUI : MonoBehaviour
    {
        [SerializeField] private GameObject headerObject;
        [SerializeField] private TMP_Text header;
        [SerializeField] private GameObject bodyObject;
        [SerializeField] private TMP_Text body;
        [SerializeField] private GameObject resourceCostsGameObject;
        [SerializeField] private GameObject providedResourcesGameObject;
        [SerializeField] private GameObject consumedResourcesGameObject;
        [SerializeField] private UIResourceCosts resourceCosts;
        [SerializeField] private UIResourceCosts providedResources;
        [SerializeField] private UIResourceCosts consumedResources;

        [SerializeField] private bool fixedPosition;
        [SerializeField] private LayoutElement controlWidthLayoutElement;

        public void Display(Tooltip tooltip)
        {
            gameObject.SetActive(true);
            
            headerObject.gameObject.SetActive(tooltip.ShowHeader());
            header.text = tooltip.header;
            headerObject.gameObject.SetActive(tooltip.ShowBody());
            body.text = tooltip.body;
            bool showResourceCosts = tooltip.ShowResourceCosts() && tooltip.ResourceCosts != null;
            
            if (showResourceCosts)
            {
                if (tooltip.ResourceCosts.Costs.Length > 0)
                {
                    resourceCostsGameObject.SetActive(true);
                    resourceCosts.Set(tooltip.ResourceCosts.Costs, false);
                }
                else
                {
                    resourceCostsGameObject.SetActive(false);
                }
                
                if (tooltip.ResourceCosts.Provides.Length > 0)
                {
                    providedResourcesGameObject.SetActive(true);
                    providedResources.Set(tooltip.ResourceCosts.Provides);
                }
                else
                {
                    providedResourcesGameObject.SetActive(false);
                }
                
                if (tooltip.ResourceCosts.Consumed.Length > 0)
                {
                    consumedResourcesGameObject.SetActive(true);
                    consumedResources.Set(tooltip.ResourceCosts.Consumed, true);
                }
                else
                {
                    consumedResourcesGameObject.SetActive(false);
                }
            }
            else
            {
                resourceCostsGameObject.SetActive(false);
                providedResourcesGameObject.SetActive(false);
                consumedResourcesGameObject.SetActive(false);
            }

            if (fixedPosition) return;

            bool headerOverflow = tooltip.ShowHeader() && header.preferredWidth > controlWidthLayoutElement.preferredWidth;
            bool bodyOverflow = tooltip.ShowBody() && body.preferredWidth > controlWidthLayoutElement.preferredWidth;

            controlWidthLayoutElement.enabled = headerOverflow || bodyOverflow;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
