using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public class TooltipManager : MonoBehaviour
    {
        public DisplayTooltipUI commandTooltip;
        public DisplayTooltipUI generalTooltip;
        public void ShowTooltip(Tooltip tooltip)
        {
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
            commandTooltip.Hide();
            generalTooltip.Hide();
        }
    }
}
