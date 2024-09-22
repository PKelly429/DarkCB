using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using UnityEngine;

namespace TDCB
{
    [CreateAssetMenu(menuName="Command/Train Unit")]
    public class TrainUnitCommand : BaseCommand
    {
        [SerializeField] private UnitData unit;
        [SerializeField] private SoundData failToTrainSFX;
        
        protected readonly HashSet<TrainUnits> observers = new HashSet<TrainUnits>();
        
        private Tooltip _unitTooltip;

        private void OnEnable()
        {
            _unitTooltip = new Tooltip()
            {
                header = tooltip.header,
                body = tooltip.body,
                position = tooltip.position,
                ResourceCosts = unit.costs,
                type = TooltipType.ResourceCost
            };
        }
        
        public override void Execute()
        {
            foreach (var cost in unit.costs)
            {
                if (cost.resourceType == ResourceType.Population) continue;
                
                if (!SceneReferences.Instance.resourceManager.CanAffordCost(cost))
                {
                    PlayInvalidSFX();
                    return;
                }
            }

            TrainUnits chosenBuilding = null;
            int best = int.MaxValue;
            foreach (var building in observers)
            {
                if(building.AtCapacity) continue;
                
                int queue = building.UnitsInQueue;
                if (queue == 0)
                {
                    chosenBuilding = building;
                    break;
                }
                
                if (queue < best)
                {
                    best = queue;
                    chosenBuilding = building;
                }
            }

            if (chosenBuilding != null)
            {
                chosenBuilding.TrainUnit(unit);
            }
            else
            {
                PlayInvalidSFX();
            }
        }
        
        public void Register(TrainUnits observer) => observers.Add(observer);
        public void Deregister(TrainUnits observer) => observers.Remove(observer);

        private void PlayInvalidSFX()
        {
            SoundManager.Instance.CreateSoundBuilder().Play(failToTrainSFX);
        }
        
        public override Tooltip GetTooltip()
        {
            return _unitTooltip;
        }
    }
}
