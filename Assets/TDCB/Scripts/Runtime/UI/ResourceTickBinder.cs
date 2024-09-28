using System;
using System.Collections;
using System.Collections.Generic;
using DataBinding;
using UnityEngine;
using UnityEngine.Serialization;

namespace TDCB
{
    public class ResourceTickBinder : MonoBehaviour
    {
        [FormerlySerializedAs("tickSlider")] public FixedMaxSliderBinder tickFixedMaxSlider;

        private void OnEnable()
        {
            tickFixedMaxSlider.sliderField.maxValue = SceneReferences.Instance.resourceManager.TickTime;
            tickFixedMaxSlider.Bind(SceneReferences.Instance.resourceManager);
        }
    }
}
