using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public class SceneReferences : MonoBehaviour
    {
        public InputHandler inputHandler;
        public CameraController cameraController;
        public CommandManager commandManager;
        public SelectedUnitManager unitManager;
        public UnitHighlightManager highlightManager;
        public FogOfWarManager fogManager;
        public GridManager gridManager;
        public BuildingPlacement buildingPlacement;
        
        #region Singleton
        public static SceneReferences Instance {get; private set;}

        private void Awake()
        {
            Instance = this;
        }
        #endregion
    }
}
