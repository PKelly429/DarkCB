using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace TDCB
{
    public class UnitHighlightManager : MonoBehaviour
    {
        public GameObject unitHighlightPrefab;
        public GameObject buildingHighlightPrefab;
        private ObjectPool<UnitHighlight> unitHighlights = new ObjectPool<UnitHighlight>(CreateUnitHighlight, GetUnitHighlight, ReleaseUnitHighlight, DestroyUnitHighlight);
        private ObjectPool<BuildingHighlight> buildingHighlights = new ObjectPool<BuildingHighlight>(CreateBuildingHighlight, GetBuildingHighlight, ReleaseBuildingHighlight, DestroyBuildingHighlight);

        private HashSet<ISelectable> registry = new HashSet<ISelectable>();
        private List<ISelectable> selectables = new List<ISelectable>();
        private List<IHighlight> highlights = new List<IHighlight>();
        public IHighlight RegisterUnit(ISelectable selectable)
        {
            if (!registry.Add(selectable)) return null;
            
            selectables.Add(selectable);
            if (selectable.selectableType == SelectableType.Unit)
            {
                var highlight = unitHighlights.Get();
                highlights.Add(highlight);
                highlight.SetSize(selectable.Size);
                highlight.SetPosition(selectable.Position);
                return highlight;
            }
            else
            {
                var highlight = buildingHighlights.Get();
                highlights.Add(highlight);
                highlight.SetSize(selectable.Size);
                highlight.SetPosition(selectable.Position);
                return highlight;
            }

            return null;
        }
        
        public void DeregisterUnit(ISelectable selectable)
        {
            if (!registry.Remove(selectable)) return;
            
            int index = selectables.IndexOf(selectable);
            selectables.RemoveAt(index);
            if (selectable.selectableType == SelectableType.Unit)
            {
                if(highlights[index].IsAlive()) unitHighlights.Release(highlights[index] as UnitHighlight);
            }
            else
            {
                if(highlights[index].IsAlive()) buildingHighlights.Release(highlights[index] as BuildingHighlight);
            }
            highlights.RemoveAt(index);
        }
        
        private void Awake()
        {
            UnitHighlightPrefab = unitHighlightPrefab;
            BuildingHighlightPrefab = buildingHighlightPrefab;
        }

        private void LateUpdate()
        {
            float deltaAnimationPosition = Time.deltaTime * 0.5f;
            for (int i = 0; i < selectables.Count; i++)
            {
                if (highlights[i].Hovered || highlights[i].Selected)
                {
                    highlights[i].UpdatePosition(selectables[i].Position, deltaAnimationPosition);
                }
            }
        }

        #region UnitHighlightPool

        private static GameObject UnitHighlightPrefab;
        private static void DestroyUnitHighlight(UnitHighlight obj)
        {
            Destroy(obj);
        }

        private static void ReleaseUnitHighlight(UnitHighlight obj)
        {
            obj.SetActive(false);
        }

        private static void GetUnitHighlight(UnitHighlight obj)
        {
            obj.SetActive(true);
        }

        private static UnitHighlight CreateUnitHighlight()
        {
            return Instantiate(UnitHighlightPrefab).GetComponent<UnitHighlight>();
        }
        #endregion
        
        #region BuildingHighlightPool

        private static GameObject BuildingHighlightPrefab;
        private static void DestroyBuildingHighlight(BuildingHighlight obj)
        {
            Destroy(obj);
        }

        private static void ReleaseBuildingHighlight(BuildingHighlight obj)
        {
            obj.SetActive(false);
        }

        private static void GetBuildingHighlight(BuildingHighlight obj)
        {
            obj.SetActive(true);
        }

        private static BuildingHighlight CreateBuildingHighlight()
        {
            return Instantiate(BuildingHighlightPrefab).GetComponent<BuildingHighlight>();
        }
        #endregion
    }
}
