using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public class SingleVsMultiSelectUI : MonoBehaviour
    {
        public GameObject multiSelect;
        public GameObject singleSelect;
        
        private void OnEnable()
        {
            SceneReferences.Instance.unitManager.OnSelectedUnitsChanged += UnitManagerOnOnSelectedUnitsChanged;
        }

        private void OnDisable()
        {
            SceneReferences.Instance.unitManager.OnSelectedUnitsChanged -= UnitManagerOnOnSelectedUnitsChanged;
        }
        
        private void UnitManagerOnOnSelectedUnitsChanged()
        {
            int count = SceneReferences.Instance.unitManager.SelectedUnitCount;
            multiSelect.SetActive(count > 1);
            singleSelect.SetActive(count == 1);
        }
    }
}
