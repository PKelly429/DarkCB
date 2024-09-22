using System;
using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using DataBinding;
using UnityEngine;
using UnityEngine.Serialization;

namespace TDCB
{
    public class TrainUnits : MonoBehaviour, IBuildingSelectionFunctions, ICommandRegister, ICommandListener<Vector3>
    {
        public const int MaxUnitQueue = 5;
        
        [SerializeField] private MoveCommand moveCommand;
        [SerializeField] private GameObject flagPrefab;
        
        
        [SerializeField] private List<TrainUnitCommand> commands;
        [SerializeField] private Transform spawnPosition;

        private List<UnitData> trainUnitQueue = new List<UnitData>(MaxUnitQueue);

        public int UnitsInQueue => trainUnitQueue.Count;
        public bool AtCapacity => UnitsInQueue >= MaxUnitQueue;
        public UnitData GetUnitInQueue(int position) => trainUnitQueue[position];

        public BindableBool InProgress { get; private set; } = new BindableBool(false);
        public BindableFloat Progress { get; private set; } = new BindableFloat(0);

        public Action OnUnitAddedOrRemoved;

        private bool rallyFlagCreated;
        private Transform rallyFlag;
        public Vector3 rallyPosition { get; private set; }

        public void Update()
        {
            bool inProgress = UnitsInQueue > 0;

            if (inProgress && !InProgress)
            {
                StartTraining(trainUnitQueue[0]);
            }

            if (!InProgress) return;

            UnitData toTrain = trainUnitQueue[0];
            
            float trainSpeed = toTrain.TrainingTime > 0 ? Mathf.Clamp01(1 / toTrain.TrainingTime) : 1f;
            float progress = Progress + (trainSpeed * Time.deltaTime);

            if (progress < 1)
            {
                Progress.SetValue(progress);
                return;
            }
            
            progress = 0;
            Progress.SetValue(progress);
            
            trainUnitQueue.RemoveAt(0);
            var newUnitObject = Instantiate(toTrain.unitPrefab, spawnPosition.position, spawnPosition.rotation);
            IControllableUnit controllableUnit = newUnitObject.GetComponent<IControllableUnit>();
            if (controllableUnit != null)
            {
                controllableUnit.Move(rallyPosition);
            }
            
            
            SoundManager.Instance.CreateSoundBuilder().Play(toTrain.trainSound);

            if (UnitsInQueue > 0)
            {
                StartTraining(trainUnitQueue[0]);
            }
            else
            {
                InProgress.SetValue(false);
            }
            
            OnUnitAddedOrRemoved?.Invoke();
        }

        private void StartTraining(UnitData unit)
        {
            // Pay population cost
            foreach (var cost in unit.costs)
            {
                if (cost.resourceType == ResourceType.Population)
                {
                    if (!SceneReferences.Instance.resourceManager.CanAffordCost(cost))
                    {
                        InProgress.SetValue(false);
                        return;
                    }
                    
                    SceneReferences.Instance.resourceManager.PayResourceCost(cost);
                    break;
                }
            }
            InProgress.SetValue(true);
        }

        public void TrainUnit(UnitData unit)
        {
            foreach (var cost in unit.costs)
            {
                if (cost.resourceType == ResourceType.Population) continue; // Applied when unit starts production
                
                SceneReferences.Instance.resourceManager.PayResourceCost(cost);
            }
            
            trainUnitQueue.Add(unit);
            OnUnitAddedOrRemoved?.Invoke();
        }

        public void CancelUnit(int position)
        {
            if(position >= trainUnitQueue.Count) return;

            bool populationCostPayed = InProgress && position == 0;
            
            var unitData = trainUnitQueue[position];
            foreach (var cost in unitData.costs)
            {
                if (!populationCostPayed && cost.resourceType == ResourceType.Population) continue;
                
                SceneReferences.Instance.resourceManager.RefundResourceCost(cost);
            }
            trainUnitQueue.RemoveAt(position);
            
            if (populationCostPayed)
            {
                Progress.SetValue(0);
                
                if (UnitsInQueue > 0)
                {
                    StartTraining(trainUnitQueue[0]);
                }
                else
                {
                    InProgress.SetValue(false);
                }
            }
            
            OnUnitAddedOrRemoved?.Invoke();
        }
        
        public void Register()
        {
            moveCommand.Register(this);
            foreach (var command in commands)
            {
                command.Register(this);
            }
        }

        public void Deregister()
        {
            moveCommand.Deregister(this);
            foreach (var command in commands)
            {
                command.Deregister(this);
            }
        }

        public void Raise(Vector3 position) //TODO: Should make a separate set rally point command
        {
            rallyPosition = position;
            if (rallyFlag == null)
            {
                var flagGO = Instantiate(flagPrefab, position, Quaternion.identity, transform);
                rallyFlag = flagGO.transform;
                rallyFlagCreated = true;
            }

            rallyFlag.position = position;
        }

        #region IBuildingSelectionFunctions
        public void OnHoverBegin()
        {
        }

        public void OnHoverEnd()
        {
        }

        public void OnSelect()
        {
            if (!rallyFlagCreated) return;
            rallyFlag.gameObject.SetActive(true);
        }

        public void OnDeselect()
        {
            if (!rallyFlagCreated) return;
            rallyFlag.gameObject.SetActive(false);
        }
        #endregion
    }
}
