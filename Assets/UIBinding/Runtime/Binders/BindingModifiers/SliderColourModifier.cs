using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DataBinding
{
    public class SliderColourModifier : MonoBehaviour, IBindingModifer<SliderBinder>
    {
        [SerializeField] private List<Graphic> images;
        [SerializeField] private List<ColourThreshold> colourThresholds;
        

        public void OnBindingChanged(SliderBinder binder)
        {
            if (colourThresholds.Count < 1) return;
            float threshold = Mathf.Clamp01(binder.Current / binder.Max);
            Color result = colourThresholds[0].colorTarget;
            foreach (var colorThreshold in colourThresholds)
            {
                if (threshold <= colorThreshold.normalisedValue)
                {
                    result = colorThreshold.colorTarget;
                }
            }

            foreach (var image in images)
            {
                image.color = result;   
            }
        }
    }

    [Serializable]
    public struct ColourThreshold
    {
        public float normalisedValue;
        public Color colorTarget;
    }
}
