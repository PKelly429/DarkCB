using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace TDCB
{
    public class SelectedUnitGrid : MonoBehaviour
    {
        [SerializeField] private GameObject selectedUnitPrefab;
        [SerializeField] private RectTransform iconParent;
        [SerializeField] private int maxPerPage;
        private ObjectPool<SelectedUnitIcon> iconPool;

        private HashSet<SelectedUnitIcon> _activeIcons = new HashSet<SelectedUnitIcon>();

        private void Awake()
        {
            iconPool = new ObjectPool<SelectedUnitIcon>(CreateIcon, GetIcon, ReleaseIcon, DestroyIcon);
        }

        private void OnEnable()
        {
            SceneReferences.Instance.unitManager.OnSelectedUnitsChanged += UnitManagerOnOnSelectedUnitsChanged;
            UnitManagerOnOnSelectedUnitsChanged();
        }

        private void OnDisable()
        {
            SceneReferences.Instance.unitManager.OnSelectedUnitsChanged -= UnitManagerOnOnSelectedUnitsChanged;
        }
        
        private void UnitManagerOnOnSelectedUnitsChanged()
        {
            foreach (var icon in _activeIcons)
            {
                iconPool.Release(icon);
            }
            _activeIcons.Clear();

            int index = 0;
            foreach (var unit in SceneReferences.Instance.unitManager.OrderedUnits.unitsInPrioirtyOrder)
            {
                var newIcon = iconPool.Get();
                newIcon.SetUnit(unit);
                newIcon.transform.SetSiblingIndex(index);
                _activeIcons.Add(newIcon);
                index++;
                if (index >= maxPerPage) break;
            }
        }

        #region UnitHighlightPool
        private void DestroyIcon(SelectedUnitIcon obj)
        {
            Destroy(obj);
        }

        private void ReleaseIcon(SelectedUnitIcon obj)
        {
            obj.SetActive(false);
        }

        private void GetIcon(SelectedUnitIcon obj)
        {
            obj.SetActive(true);
        }

        private SelectedUnitIcon CreateIcon()
        {
            return Instantiate(selectedUnitPrefab, iconParent).GetComponent<SelectedUnitIcon>();
        }
        #endregion
    }
}
