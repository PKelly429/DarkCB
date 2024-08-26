using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    [CreateAssetMenu(menuName = "Command/Build Command")]
    public class BuildCommand : PositionCommand
    {
        [SerializeField] private BuildingData building;
        
        public override void Execute()
        {
            base.Execute();
            SceneReferences.Instance.buildingPlacement.StartPlacement(building);
        }

        public override void Execute(Vector3 position)
        {
            SceneReferences.Instance.buildingPlacement.TryCompletePlacement();
        }

        public override void OnBeforeExecute()
        {
            UIReferences.Instance.commandButtonGrid.SetBuildCanvasVisible(true);
        }
        
        public override void OnAfterExecuteOrCancel()
        {
            UIReferences.Instance.commandButtonGrid.SetBuildCanvasVisible(false);
            SceneReferences.Instance.buildingPlacement.CancelPlacement();
        }
    }
}
