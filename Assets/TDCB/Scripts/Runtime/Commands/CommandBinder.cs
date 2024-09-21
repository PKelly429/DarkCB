using System.Collections;
using System.Collections.Generic;
using DataBinding;
using UnityEngine;
using UnityEngine.UI;

namespace TDCB
{
    public class CommandBinder : AbstractBinder
    {
        [SerializeField] private Image frame;
        [SerializeField] private Button button;
        [SerializeField] private Image buttonIcon;
        [SerializeField] private Image icon;
        [SerializeField] private HighlightOnHover hover;
        [SerializeField] private ShowTooltipOnHover tooltip;
        
        public override void Bind(object obj)
        {
            Unbind();
            if (obj == null) return;
            BaseCommand command = (BaseCommand)obj;
            if (command == null) return;
            buttonIcon.enabled = true;
            button.enabled = true;
            button.onClick.AddListener(command.Execute);
            icon.enabled = true;
            frame.enabled = false;
            icon.sprite = command.image;
            hover.SetSprites(command.image, command.imageHover);
            tooltip.SetTooltip(command.GetTooltip());
        }

        public override void Unbind()
        {
            buttonIcon.enabled = false;
            button.enabled = false;
            button.onClick.RemoveAllListeners();
            icon.enabled = false;
            frame.enabled = true;
        }

        public override void DebugBinder()
        {
            
        }
        
#if UNITY_EDITOR
        public override void Reset()
        {
            base.Reset();
            button = GetComponent<Button>();
        }
#endif
    }
}
