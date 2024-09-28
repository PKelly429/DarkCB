using System;
using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using UnityEngine;

namespace TDCB
{
    [CreateAssetMenu(menuName = "Command/Build Command")]
    public class BuildCommand : TargetCommand
    {
        [SerializeField] private BuildingData building;

        private Tooltip _buildingTooltip;

        private void OnEnable()
        {
            _buildingTooltip = building.GetFullTooltip();
        }

        public override void Execute()
        {
            base.Execute();

            if (SceneReferences.Instance.unitManager.SelectedUnitCount > 0)
            {
                SceneReferences.Instance.buildingPlacement.StartPlacement(building, this, SceneReferences.Instance.unitManager.HighestPrioritySelectedUnit);
            }
        }

        public override void Execute(Vector3 position)
        {
            var newBuild = SceneReferences.Instance.buildingPlacement.TryCompletePlacement();

            if (!newBuild.Item1)
            {
                OnCancel();
                return;
            }

            if (newBuild.Item2.building != null && newBuild.Item2.building.GetComponent<IWorkerAssignment>() != null)
            {
                foreach (var observer in SceneReferences.Instance.unitManager.allControllableUnits)
                {
                    observer.Move(newBuild.Item2);
                }
            }
        }

        public override void OnBeforeExecute()
        {
            UIReferences.Instance.commandButtonGrid.SetBuildCanvasVisible(true);
        }
        
        public override void OnCancel()
        {
            UIReferences.Instance.commandButtonGrid.SetBuildCanvasVisible(false);
            SceneReferences.Instance.buildingPlacement.CancelPlacement();
        }
        
        public override Tooltip GetTooltip()
        {
            return _buildingTooltip;
        }
    }
}
