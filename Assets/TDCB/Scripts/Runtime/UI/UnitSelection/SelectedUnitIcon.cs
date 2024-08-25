using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TDCB
{
    public class SelectedUnitIcon : MonoBehaviour
    {
        [SerializeField] private Image icon;

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
        public void SetIcon(Sprite sprite)
        {
            icon.sprite = sprite;
        }
    }
}
