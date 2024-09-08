using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB.FrameTiming
{
    public class FrameTimings : MonoBehaviour
    {
        public HashSet<IOnUpdateEarly> earlyUpdateSubscribers = new HashSet<IOnUpdateEarly>();
        public HashSet<IOnUpdateLate> lateUpdateSubscribers = new HashSet<IOnUpdateLate>();

        public void Register(object obj)
        {
            if (obj is IOnUpdateEarly earlyUpdateSubscriber)
            {
                earlyUpdateSubscribers.Add(earlyUpdateSubscriber);
            }
            if (obj is IOnUpdateLate lateUpdateSubscriber)
            {
                lateUpdateSubscribers.Add(lateUpdateSubscriber);
            }
        }

        public void Deregister(object obj)
        {
            if (obj is IOnUpdateEarly earlyUpdateSubscriber)
            {
                earlyUpdateSubscribers.Remove(earlyUpdateSubscriber);
            }
            if (obj is IOnUpdateLate lateUpdateSubscriber)
            {
                lateUpdateSubscribers.Remove(lateUpdateSubscriber);
            }
        }

        internal void TriggerEarlyUpdate()
        {
            foreach (var earlySubscriber in earlyUpdateSubscribers)
            {
                earlySubscriber.OnEarlyFrameUpdate();
            }
        }
        
        internal void TriggerLateUpdate()
        {
            foreach (var lateSubscriber in lateUpdateSubscribers)
            {
                lateSubscriber.OnLateFrameUpdate();
            }
        }
    }
}