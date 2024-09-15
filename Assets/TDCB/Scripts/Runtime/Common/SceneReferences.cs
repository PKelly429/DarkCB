using System.Collections;
using System.Collections.Generic;
using TDCB.FrameTiming;
using UnityEngine;

namespace TDCB
{
    public class SceneReferences : MonoBehaviour
    {
        public FrameTimings frameTimings;
        public InputHandler inputHandler;
        public CameraController cameraController;
        public CommandManager commandManager;
        public SelectedUnitManager unitManager;
        public UnitHighlightManager highlightManager;
        public FogOfWarManager fogManager;
        public GridManagerJobs gridJobs;
        public GridManager gridManager;
        public SpatialHashManager playerUnitHash;
        public SpatialHashManager enemyUnitHash;
        public BuildingPlacement buildingPlacement;
        public ResourceManager resourceManager;
        
        #region Singleton
        public static SceneReferences Instance {get; private set;}

        private void Awake()
        {
            Instance = this;
        }
        #endregion
    }
}
