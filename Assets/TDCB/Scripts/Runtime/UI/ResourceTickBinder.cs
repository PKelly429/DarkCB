using System;
using System.Collections;
using System.Collections.Generic;
using DataBinding;
using UnityEngine;

namespace TDCB
{
    public class ResourceTickBinder : MonoBehaviour
    {
        public SliderBinder tickSlider;

        private void OnEnable()
        {
            tickSlider.sliderField.maxValue = SceneReferences.Instance.resourceManager.TickTime;
            tickSlider.Bind(SceneReferences.Instance.resourceManager);
        }
    }
}
