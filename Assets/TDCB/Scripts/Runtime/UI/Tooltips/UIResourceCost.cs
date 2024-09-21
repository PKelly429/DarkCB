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
        [SerializeField] private  TMP_Text amount;

        public void SetValue(ResourceValue value)
        {
            icon.sprite = SceneReferences.Instance.resourceManager.GetResourceIcon(value.resourceType);
            amount.text = $"{value.value}";
        }
    }
}
