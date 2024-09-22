using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TDCB
{
    public class SingleUnitSelectionUI : MonoBehaviour
    {
        public GameObject unitPanel;
        public GameObject buildingPanel;

        [Header("Unit Panel")] 
        public Image unitIcon;
        public TMP_Text unitName;
        
        [Header("Building Panel")] 
        public Image buildingIcon;
        public TMP_Text buildingName;
        
        public GameObject trainUnitPanel;
        public TrainUnitUIPanel trainUnitUI;
        
        private void OnEnable()
        {
            SceneReferences.Instance.unitManager.OnSelectedUnitsChanged += UpdateSelection;
            UpdateSelection();
        }

        private void OnDisable()
        {
            SceneReferences.Instance.unitManager.OnSelectedUnitsChanged -= UpdateSelection;
        }
        
        private void UpdateSelection()
        {
            if (SceneReferences.Instance.unitManager.SelectedUnitCount < 1) return;
            ISelectable selected = SceneReferences.Instance.unitManager.HighestPrioritySelectedUnit;
            
            unitPanel.SetActive(selected.unit);
            buildingPanel.SetActive(selected.building);

            if (selected.unit)
            {
                unitIcon.sprite = selected.Icon;
                unitName.text = selected.unit.unitData.tooltip.header;
            }
            else if (selected.building)
            {
                Building building = selected.building;
                
                buildingIcon.sprite = selected.Icon;
                buildingName.text = selected.building.BuildingData.tooltip.header;

                TrainUnits trainUnits = building.GetComponent<TrainUnits>();
                if (trainUnits != null)
                {
                    trainUnitPanel.SetActive(true);
                    trainUnitUI.Bind(trainUnits);
                }
                else
                {
                    trainUnitPanel.SetActive(false);
                    trainUnitUI.UnBind();
                }
            }
        }
    }
}
