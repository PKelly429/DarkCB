using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TDCB
{
    public class FogClearingObject : MonoBehaviour
    {
        [SerializeField] private bool isStatic;
        [SerializeField] private int radius = 30;

        public bool HasBeenAdded { get; set; }
        public Vector3 Position => transform.position;
        public bool IsStatic => isStatic;
        public int Radius => radius;
        
        public GridCell GridPosition { get; set; }
        
        private void OnEnable()
        {
            SceneReferences.Instance.fogManager.RegisterFogClearingObj(this);
            SceneReferences.Instance.gridJobs.RegisterFogClearingObj(this);
        }
        
        private void OnDisable()
        {
            SceneReferences.Instance.fogManager.DeregisterFogClearingObj(this);
            SceneReferences.Instance.gridJobs.DeregisterFogClearingObj(this);
        }
    }
}
