using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TDCB
{
    public class TrainUnitUIIcon : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private int id;
        [SerializeField] private Image icon;
        [SerializeField] private Image frame;
        [SerializeField] private GameObject text;
        [SerializeField] private Color frameHover;
        [SerializeField] private Color frameNormal;

        public Action<int> CancelUnit;

        public void SetToUnit(UnitData unit)
        {
            icon.enabled = true;
            icon.sprite = unit.icon;
            text.SetActive(false);
        }

        public void Clear()
        {
            icon.enabled = false;
            text.SetActive(true);
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            CancelUnit?.Invoke(id);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            frame.color = frameHover;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            frame.color = frameNormal;
        }
    }
}
