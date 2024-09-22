using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TDCB
{
    public class LightBuilding : MonoBehaviour, IBuildingPlacementFunctions
    {
        [SerializeField, Required] private FogClearingObject _fogClearingObject;
        [SerializeField, Required] private GameObject light;


        public void OnBeginPlacement()
        {
            
        }

        public void OnCancelPlacement()
        {
            
        }

        public void OnFinishPlacement()
        {
            _fogClearingObject.enabled = true;
            light.SetActive(true);
        }
    }
}
