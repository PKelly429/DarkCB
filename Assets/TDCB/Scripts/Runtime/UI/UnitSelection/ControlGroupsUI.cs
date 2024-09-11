using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public class ControlGroupsUI : MonoBehaviour
    {
        [SerializeField] private ControlGroupUI[] controlGroups;
        
        private void OnEnable()
        {
            SceneReferences.Instance.unitManager.OnControlGroupChanged += UnitManagerOnOnControlGroupChanged;
            SceneReferences.Instance.unitManager.OnControlGroupSelectionChanged += UnitManagerOnOnControlGroupSelectionChanged;

            for (int i = 0; i < SelectedUnitManager.ControlGroupCount; i++)
            {
                UnitManagerOnOnControlGroupChanged(i);
                controlGroups[i].SetId(i);
            }
        }

        private void OnDisable()
        {
            SceneReferences.Instance.unitManager.OnControlGroupChanged -= UnitManagerOnOnControlGroupChanged;
            SceneReferences.Instance.unitManager.OnControlGroupSelectionChanged -= UnitManagerOnOnControlGroupSelectionChanged;
        }
        
        private void UnitManagerOnOnControlGroupChanged(int id)
        {
            controlGroups[id].Bind(SceneReferences.Instance.unitManager.GetControlGroup(id));
        }
        
        private void UnitManagerOnOnControlGroupSelectionChanged(int id)
        {
            for (int i = 0; i < SelectedUnitManager.ControlGroupCount; i++)
            {
                controlGroups[i].SetSelected(id);
            }
        }
    }
}
