using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB.FrameTiming
{
    public class EarlyUpdateTrigger : MonoBehaviour
    {
        [SerializeField] private FrameTimings frameTimings;

        private void Update()
        {
            frameTimings.TriggerEarlyUpdate();
        }
    }
}
