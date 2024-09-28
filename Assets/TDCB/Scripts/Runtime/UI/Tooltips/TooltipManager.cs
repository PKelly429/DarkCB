using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public class TooltipManager : MonoBehaviour
    {
        public DisplayTooltipUI commandTooltip;
        public DisplayTooltipUI generalTooltip;

        private bool _tooltipLocked;
        
        public void ShowTooltip(Tooltip tooltip)
        {
            if (_tooltipLocked) return;
            
            if (string.IsNullOrEmpty(tooltip.body))
                return;
            
            if (tooltip.position == FixedPosition.Command)
            {
                commandTooltip.Display(tooltip);
                generalTooltip.Hide();
            }
            else
            {
                generalTooltip.Display(tooltip);
                commandTooltip.Hide();
            }
        }

        public void HideTooltips()
        {
            if (_tooltipLocked) return;
            commandTooltip.Hide();
            generalTooltip.Hide();
        }

        public void LockTooltip(Tooltip tooltip)
        {
            ShowTooltip(tooltip);
            _tooltipLocked = true;
        }

        public void UnlockTooltips()
        {
            _tooltipLocked = false;
            HideTooltips();
        }
    }
}
