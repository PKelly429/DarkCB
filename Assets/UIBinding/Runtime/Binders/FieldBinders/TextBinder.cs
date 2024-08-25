using UnityEngine;
using UnityEngine.UI;

namespace DataBinding
{
    public class TextBinder : AbstractTextBinder
    {
        [SerializeField] private Text text;

        protected override void OnValueChanged()
        {
            text.text = GetBoundText();
        }
        
        private void Awake()
        {
            if (text == null)
            {
                text = GetComponent<Text>();
            }
        }

#if UNITY_EDITOR
        public override void Reset()
        {
            base.Reset();
            text = GetComponent<Text>();
        }
#endif
    }
}
