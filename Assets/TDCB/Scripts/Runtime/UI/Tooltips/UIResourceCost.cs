using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TDCB
{
    public class UIResourceCost : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text amount;

        public void SetValue(ResourceValue value)
        {
            icon.sprite = SceneReferences.Instance.resourceManager.GetResourceIcon(value.resourceType);
            amount.text = $"{value.value}";
            amount.color = UIReferences.Instance.uiColors.DefaultText;
        }

        public void SetResourceProduction(ResourceValue value)
        {
            icon.sprite = SceneReferences.Instance.resourceManager.GetResourceIcon(value.resourceType);
            if (value.value > 0)
            {
                amount.text = $"+{value.value}";
                amount.color = UIReferences.Instance.uiColors.PositiveText;
            }
            else
            {
                amount.text = $"{value.value}";
                amount.color = UIReferences.Instance.uiColors.NegativeText;
            }
        }
        
        public void SetResourceProduction(ResourceProductionValue value)
        {
            icon.sprite = SceneReferences.Instance.resourceManager.GetResourceIcon(value.resourceType);
            if (value.perTile)
            {
                amount.text = $"+{value.value} per tile";
                amount.color = UIReferences.Instance.uiColors.DefaultText;
            }
            else
            {
                amount.text = $"{value.value}";
                amount.color = UIReferences.Instance.uiColors.DefaultText;
            }
        }
    }
}
