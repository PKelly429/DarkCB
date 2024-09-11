using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Profiling;
using VolumetricFogAndMist2;

namespace TDCB
{
    public class FogOfWarManager : MonoBehaviour
    {
        [SerializeField] private VolumetricFog fogOfWar;
        [SerializeField] private VolumetricFog fog;
        
        private static readonly HashSet<FogClearingObject> dynamicFogClearingObjects = new HashSet<FogClearingObject>();

        public void RegisterFogClearingObj(FogClearingObject obj)
        {
            dynamicFogClearingObjects.Add(obj);
        }
        
        public void DeregisterFogClearingObj(FogClearingObject obj)
        {
            dynamicFogClearingObjects.Remove(obj);
        }
        
        private void Start()
        {
            fogOfWar.ResetFogOfWar(1);
            fog.ResetFogOfWar(1);
        }
        
        #if UNITY_EDITOR
        private void OnDestroy()
        {
            if (Application.isPlaying)
            {
                fogOfWar.ResetFogOfWar(0);
                fog.ResetFogOfWar(0);
            }
        }
        #endif

        private void Update()
        {
            foreach (var obj in dynamicFogClearingObjects)
            {
                SetFogOfWarAlpha(obj.Position, obj.LightDistance, 0);
            }
        }
        public void SetFogOfWarAlpha(Vector3 position, float radius, float alpha)
        {
            fogOfWar.SetFogOfWarAlpha(position, radius+10, alpha, true, 0.1f, 1f, 1f, 0.5f);
            
            //fog.SetFogOfWarAlpha(position, radius, alpha, true, 2f, 1f, 1f, 3f);
             fog.SetFogOfWarAlpha(position, radius-5, alpha, true, 1f, 1f, 1f, 1f);
            if (alpha < 0.5f)
            {
                //fogOfWar.SetFogOfWarAlpha(position, radius+12, 0.5f, true, 0.5f, 1f, 1f, 0.5f);
                fog.SetFogOfWarAlpha(position, radius, 0.3f, true, 1f, 1f, 1f, 1f);
            }
        }
    }
}
