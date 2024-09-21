using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public class BuildingRenderer : MonoBehaviour, IBuildingPlacementFunctions, IBuildingPlacementValidFunction
    {
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private Material buildingPlacementMaterial;
        
        private static readonly int Valid = Shader.PropertyToID("_Valid");
        
        private List<Material[]> _defaultMaterials;
        
        public void OnBeginPlacement()
        {
            _defaultMaterials = new List<Material[]>(renderers.Length);
            foreach (var renderer in renderers)
            {
                _defaultMaterials.Add(renderer.materials);
                
                var swapMats = renderer.materials;
                for(int i=0; i<swapMats.Length; i++)
                {
                    swapMats[i] = buildingPlacementMaterial;
                }
                renderer.materials = swapMats;
            }
        }

        public void OnCancelPlacement()
        {
        }

        public void OnFinishPlacement()
        {
            if (_defaultMaterials == null) return;
            
            for(int i=0; i<_defaultMaterials.Count; i++)
            {
                renderers[i].materials = _defaultMaterials[i];
            }
        }

        public bool IsValid()
        {
            return true;
        }

        public void UpdateBuildingPlacementValid(bool valid)
        {
            buildingPlacementMaterial.SetFloat(Valid, valid ? 1 : 0);
        }
    }
}
