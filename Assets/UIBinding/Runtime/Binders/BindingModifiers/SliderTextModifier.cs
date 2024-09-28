using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DataBinding
{
    public class SliderTextModifier : MonoBehaviour, IBindingModifer<SliderBinder>
    {
        [SerializeField] private TMP_Text text;
        public void OnBindingChanged(SliderBinder binder)
        {
            text.text = $"{Mathf.FloorToInt(binder.Current)}/{Mathf.FloorToInt(binder.Max)}";
        }
    }
}
