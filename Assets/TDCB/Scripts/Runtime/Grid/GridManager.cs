using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TDCB
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private Material gridMaterial;
        
        private static readonly int MousePos = Shader.PropertyToID("_MousePos");
        private LocalKeyword _showGraph;

        private bool _showGrid;
        public bool ShowGrid
        {
            get => _showGrid;
            set
            {
                _showGrid = value;
                gridMaterial.SetKeyword(_showGraph, value);
            }
        }

        private void Start()
        {
            _showGraph = new LocalKeyword(gridMaterial.shader, "_SHOWGRID_ON");
        }

        public void Update()
        {
            Vector3 worldPos = SceneReferences.Instance.inputHandler.MousePosition;
            Vector2 mousePos = new Vector2(worldPos.x, worldPos.z);
            gridMaterial.SetVector(MousePos, mousePos);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                ShowGrid = !ShowGrid;
            }
        }
    }
}
