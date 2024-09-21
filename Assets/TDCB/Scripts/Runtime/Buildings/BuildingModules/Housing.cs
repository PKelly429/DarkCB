using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public class Housing : MonoBehaviour, IBuildingPlacementFunctions, IBuildingDestroyFunction
    {
        [SerializeField] private int Amount;
        
        public void OnBeginPlacement()
        {
        }

        public void OnCancelPlacement()
        {
        }

        public void OnFinishPlacement()
        {
            SceneReferences.Instance.resourceManager.UpdateResourceMaximum(ResourceType.Population, Amount);
        }

        public bool IsValid()
        {
            return true;
        }

        public void OnBuildingDestroyed()
        {
            SceneReferences.Instance.resourceManager.UpdateResourceMaximum(ResourceType.Population, -Amount);
        }
    }
}
