using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TDCB
{
    public class SelectedUnitIcon : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private Image icon;

        private ISelectable _unit;

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
            if (!active) _unit = null;
        }
        public void SetUnit(ISelectable selectable)
        {
            _unit = selectable;
            icon.sprite = selectable.Icon;
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            SceneReferences.Instance.unitManager.SetUnitSelection(_unit);
        }
    }
}
