using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TDCB
{
    public class HighlightOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public enum Mode
        {
            EnableGameObject,
            SwitchSprite
        }
        
        [SerializeField] private Mode mode;
        [SerializeField, ShowIf("mode", Mode.EnableGameObject)] private GameObject gameObject;
        [SerializeField, ShowIf("mode", Mode.SwitchSprite)] private Image image;
        [SerializeField, ShowIf("mode", Mode.SwitchSprite)] private Sprite sprite;
        private Sprite _originalSprite;

        public void SetSprites(Sprite nonHover, Sprite hover)
        {
            _originalSprite = nonHover;
            sprite = hover;
        }

        private void Awake()
        {
            if (mode == Mode.SwitchSprite)
            {
                _originalSprite = image.sprite;
            }
        }

        private void OnEnable()
        {
            switch (mode)
            {
                case Mode.EnableGameObject:
                    gameObject.SetActive(false);
                    break;
                case Mode.SwitchSprite:
                    image.sprite = _originalSprite;
                    break;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            switch (mode)
            {
                case Mode.EnableGameObject:
                    gameObject.SetActive(true);
                    break;
                case Mode.SwitchSprite:
                    image.sprite = sprite;
                    break;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            switch (mode)
            {
                case Mode.EnableGameObject:
                    gameObject.SetActive(false);
                    break;
                case Mode.SwitchSprite:
                    image.sprite = _originalSprite;
                    break;
            }
        }
    }
}
