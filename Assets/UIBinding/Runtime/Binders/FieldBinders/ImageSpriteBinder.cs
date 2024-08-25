using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DataBinding
{
    public class ImageSpriteBinder : AbstractBinder<Sprite>
    {
        [SerializeField] private Image graphic;
        [BindingType(typeof(Sprite))] public BindingField target;

        protected override BindingField BindingField => target;

        protected override void OnBindingValueChanged()
        {
            graphic.sprite = bindableVariable.GetValue();
        }
        
        private void Awake()
        {
            if (graphic == null)
            {
                graphic = GetComponent<Image>();
            }
        }
        
#if UNITY_EDITOR
        public override void Reset()
        {
            base.Reset();
            graphic = GetComponent<Image>();
        }
#endif
    }
}
