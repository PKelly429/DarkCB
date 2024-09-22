using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TDCB
{
    public class TrainUnitUIPanel : MonoBehaviour
    {
        [SerializeField] private List<TrainUnitUIIcon> _trainIcons;
        [SerializeField] private GameObject _idlPanel;
        [SerializeField] private GameObject _inProgressPanel;
        [SerializeField] private Image _progressBar;

        private TrainUnits _currentBuilding;
        private bool _bound;

        private void OnEnable()
        {
            foreach (var trainIcon in _trainIcons)
            {
                trainIcon.CancelUnit += CancelUnit;
            }
        }

        private void OnDisable()
        {
            foreach (var trainIcon in _trainIcons)
            {
                if(trainIcon == null) continue;
                trainIcon.CancelUnit -= CancelUnit;
            }
        }
        
        private void CancelUnit(int pos)
        {
            if (_currentBuilding == null) return;
            _currentBuilding.CancelUnit(pos);
        }

        public void Bind(TrainUnits buildingModule)
        {
            UnBind();
            if (buildingModule == null) return;
            
            _currentBuilding = buildingModule;
            _bound = true;

            _currentBuilding.OnUnitAddedOrRemoved += UpdateIcons;
            _currentBuilding.InProgress.onValueChanged += UpdateIcons;
            _currentBuilding.Progress.onValueChanged += UpdateProgressBar;

            UpdateIcons();
            UpdateProgressBar();
        }

        private void UpdateIcons()
        {
            _idlPanel.SetActive(!_currentBuilding.InProgress);
            _inProgressPanel.SetActive(_currentBuilding.InProgress);
            
            int unitsInQueue = _currentBuilding.UnitsInQueue;
            for (int i = 0; i < _trainIcons.Count; i++)
            {
                if (i < unitsInQueue)
                {
                    _trainIcons[i].SetToUnit(_currentBuilding.GetUnitInQueue(i));
                }
                else
                {
                    _trainIcons[i].Clear();
                }
            }
        }
        
        private void UpdateProgressBar()
        {
            if (!_bound) return;
            _progressBar.fillAmount = _currentBuilding.Progress.GetValue();
        }

        public void UnBind()
        {
            _bound = false;
            if (_currentBuilding == null) return;
            
            _currentBuilding.OnUnitAddedOrRemoved -= UpdateIcons;
            _currentBuilding.InProgress.onValueChanged -= UpdateIcons;
            _currentBuilding.Progress.onValueChanged -= UpdateProgressBar;

            _currentBuilding = null;
        }
    }
}
