using System.Collections;
using System.Collections.Generic;
using DataBinding;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TDCB
{
    public class SingleUnitSelectionUI : MonoBehaviour
    {
        public GameObject unitPanel;
        public GameObject buildingPanel;
        
        [Header("Shared")] 
        public Image icon;
        public TMP_Text nameText;
        public SliderBinder healthSlider;

        [Header("Unit Panel")] 
        public UnitStatUI statUI;
        

        [Header("Building Panel")]
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
            
            icon.sprite = selected.Icon;
            
            var health = selected.health;
            if (health != null)
            {
                healthSlider.Bind(health);
            }
            else
            {
                healthSlider.Unbind();
            }

            if (selected.unit)
            {
                nameText.text = selected.unit.unitData.tooltip.header;
                
                statUI.Bind(selected.unit.unitData.stats);
            }
            else if (selected.building)
            {
                Building building = selected.building;
                
                nameText.text = building.BuildingData.tooltip.header;

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
