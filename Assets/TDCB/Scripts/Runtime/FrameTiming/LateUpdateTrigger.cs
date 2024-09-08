using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB.FrameTiming
{
    public class LateUpdateTrigger : MonoBehaviour
    {
        [SerializeField] private FrameTimings frameTimings;

        private void Update()
        {
            frameTimings.TriggerLateUpdate();
        }
    }
}
