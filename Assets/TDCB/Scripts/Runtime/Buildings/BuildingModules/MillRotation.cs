using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TDCB
{
    public class MillRotation : MonoBehaviour, IBuildingPlacementFunctions
    {
        [SerializeField] private Transform _rotationPivot;
        [SerializeField] private float _minRotationSpeed;
        [SerializeField] private float _maxRotationSpeed;

        private const float RotationSpeedAcceleration = 5f;
        private const float MinRecalculateTime = 5f;
        private const float MaxRecalculateTime = 30f;
        private float _recalculateSpeedTime;
        
        private bool _isPlaced;
        private float _rotationSpeed;
        private float _targetRotationSpeed;

        public IEnumerator Start()
        {

            while (true)
            {
                if (_recalculateSpeedTime <= 0)
                {
                    _targetRotationSpeed = Random.Range(_minRotationSpeed, _maxRotationSpeed);
                    _recalculateSpeedTime = Random.Range(MinRecalculateTime, MaxRecalculateTime);
                }

                _rotationSpeed = Mathf.MoveTowards(_rotationSpeed, _targetRotationSpeed, RotationSpeedAcceleration * Time.deltaTime);
                _recalculateSpeedTime -= Time.deltaTime;
                yield return null;
            }
        }

        public void Update()
        {
            if (!_isPlaced) return;

            _rotationPivot.Rotate(Vector3.forward, _rotationSpeed * Time.deltaTime);
        }

        public void OnBeginPlacement()
        {
        }

        public void OnCancelPlacement()
        {
        }

        public void OnFinishPlacement()
        {
            _isPlaced = true;
        }
    }
}
