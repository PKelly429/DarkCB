using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TDCB
{
    public class ControlGroupUI : MonoBehaviour
    {
        [SerializeField] private GameObject groupGameObject;
        [SerializeField] private TMP_Text idText;
        [SerializeField] private Image frame;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text count;
        
        [SerializeField] private Sprite unselected;
        [SerializeField] private Sprite selected;

        private int id;
        private OrderedUnitList data;

        public void SetId(int id)
        {
            this.id = id;
            if (id >= 9)
            {
                idText.text = "0";
                return;
            }
            idText.text = $"{id+1}";
        }

        public void SetSelected(int selectedId)
        {
            frame.sprite = selectedId == id ? selected : unselected;
        }

        public void Bind(OrderedUnitList units)
        {
            UnBind();
            
            data = units;
            UpdateDisplay();
            if (data != null)
            {
                data.OnControllableUnitListChanged += UpdateDisplay;
            }
        }

        private void UnBind()
        {
            if (data != null)
            {
                data.OnControllableUnitListChanged += UpdateDisplay;
            }
        }

        private void UpdateDisplay()
        {
            if (data == null || data.Count == 0)
            {
                groupGameObject.SetActive(false);
                return;
            }
            
            groupGameObject.SetActive(true);
            
            icon.sprite = data.HighestPriorityUnit.Icon;
            count.text = $"{data.Count}";
        }
    }
}
