using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TDCB
{
    public class SelectionBox : MonoBehaviour
    {
        [SerializeField] private RectTransform selectionBoxTransform;
        [SerializeField] private Image selectionBoxImage;

        private bool _update;
        private Vector2 _startPosition;
        private Vector2 _currentPosition;

        private void OnEnable()
        {
            SceneReferences.Instance.inputHandler.OnSelectionDragStart += StartSelection;
            SceneReferences.Instance.inputHandler.OnSelectionDragFinish += EndSelection;
            selectionBoxImage.enabled = false;
        }
        
        private void OnDisable()
        {
            SceneReferences.Instance.inputHandler.OnSelectionDragStart += StartSelection;
            SceneReferences.Instance.inputHandler.OnSelectionDragFinish += EndSelection;
        }

        private void Update()
        {
            if (!_update) return;
            _currentPosition = GetNormalizedPosition(SceneReferences.Instance.inputHandler.MouseScreenPosition);

            UpdatePosition();
        }

        private void UpdatePosition()
        {
            selectionBoxTransform.anchorMin = new Vector2(Mathf.Min(_startPosition.x, _currentPosition.x), Mathf.Min(_startPosition.y, _currentPosition.y));
            selectionBoxTransform.anchorMax = new Vector2(Mathf.Max(_startPosition.x, _currentPosition.x), Mathf.Max(_startPosition.y, _currentPosition.y));
        }

        private void StartSelection()
        {
            _update = true;
            _startPosition = GetNormalizedPosition(SceneReferences.Instance.inputHandler.MouseScreenPosition);
            _currentPosition = GetNormalizedPosition(SceneReferences.Instance.inputHandler.MouseScreenPosition);
            UpdatePosition();
            selectionBoxImage.enabled = true;
        }
        
        private void EndSelection()
        {
            _update = false;
            selectionBoxImage.enabled = false;
        }

        private Vector2 GetNormalizedPosition(Vector2 pos)
        {
            return new Vector2(pos.x / Screen.width, pos.y / Screen.height);
        }
    }
}
