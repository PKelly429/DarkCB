using System;
using System.Collections;
using System.Collections.Generic;
using DataBinding;
using TMPro;
using UnityEngine;

namespace DataBinding
{
    [Serializable]
    public class TMPTextBinding : AbstractTextBinder
    {
        [SerializeField] private TMP_Text text;

        protected override void OnValueChanged()
        {
            text.text = GetBoundText();
        }
        
        private void Awake()
        {
            if (text == null)
            {
                text = GetComponent<TMP_Text>();
            }
        }

#if UNITY_EDITOR
        public override void Reset()
        {
            base.Reset();
            text = GetComponent<TMP_Text>();
        }
#endif
    }
}