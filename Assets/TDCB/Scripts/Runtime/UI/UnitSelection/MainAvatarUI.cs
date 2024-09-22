using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TDCB
{
    public class MainAvatarUI : MonoBehaviour
    {
        public Image avatar;
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
            if (SceneReferences.Instance.unitManager.SelectedUnitCount > 0)
            {
                avatar.sprite = SceneReferences.Instance.unitManager.HighestPrioritySelectedUnit.Icon;
                avatar.enabled = true;
            }
            else
            {
                avatar.enabled = false;
            }
        }
    }
}
