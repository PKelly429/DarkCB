using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TDCB
{
    public class FogClearingObject : MonoBehaviour
    {
        [SerializeField] private float radius;

        public static readonly HashSet<FogClearingObject> AllFogClearingObjects = new HashSet<FogClearingObject>();

        public Vector3 Position => transform.position;
        public float Radius => radius;
        
        private void OnEnable()
        {
            AllFogClearingObjects.Add(this);
        }
        
        private void OnDisable()
        {
            AllFogClearingObjects.Remove(this);
        }
    }
}
