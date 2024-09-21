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
        [SerializeField] private UIResourceCosts resourceCosts;

        [SerializeField] private bool fixedPosition;
        [SerializeField] private LayoutElement controlWidthLayoutElement;

        public void Display(Tooltip tooltip)
        {
            gameObject.SetActive(true);
            
            headerObject.gameObject.SetActive(tooltip.ShowHeader());
            header.text = tooltip.header;
            headerObject.gameObject.SetActive(tooltip.ShowBody());
            body.text = tooltip.body;
            bool showResourceCosts = tooltip.ShowResourceCosts();
            resourceCostsGameObject.SetActive(showResourceCosts);
            {
                if (showResourceCosts)
                {
                    resourceCosts.Set(tooltip.ResourceCosts);
                }
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
