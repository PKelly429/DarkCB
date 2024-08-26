using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TDCB
{
    public class ShowTooltipOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Tooltip _tooltip;

        private bool _showing = false;

        public void SetTooltip(Tooltip tooltip)
        {
            _tooltip = tooltip;
            if (_showing)
            {
                UIReferences.Instance.tooltipManager.ShowTooltip(_tooltip);
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            UIReferences.Instance.tooltipManager.ShowTooltip(_tooltip);
            _showing = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            UIReferences.Instance.tooltipManager.HideTooltips();
            _showing = false;
        }
    }
}
