using System;
using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using Sirenix.Utilities;
using UnityEngine;

namespace TDCB
{
    public class SelectedUnitManager : MonoBehaviour
    {
        private const int ControlGroupCount = 9;
        
        private InputControls _inputControls;
        
        public readonly OrderedUnitList OrderedUnits = new OrderedUnitList();
        public readonly HashSet<IControllableUnit> allControllableUnits = new HashSet<IControllableUnit>();

        public delegate void SelectedUnitsChanged();
        public event SelectedUnitsChanged OnSelectedUnitsChanged; 

        private bool _listChanged;
        
        private readonly HashSet<ISelectable>[] controlGroups = new HashSet<ISelectable>[ControlGroupCount];

        public int SelectedUnitCount => OrderedUnits.Count;
        public ISelectable HighestPrioritySelectedUnit => OrderedUnits.HighestPriorityUnit;

        public void ClearSelection()
        {
            foreach (var unit in OrderedUnits)
            {
                unit.OnDeSelect();
            }
            allControllableUnits.Clear();
            OrderedUnits.Clear();
        }

        public void SetUnitSelection(ISelectable toSelect, bool replaceSelection = true)
        {
            if (replaceSelection)
            {
                ClearSelection();
            }

            AddUnitToSelection(toSelect);
            
            SoundManager.Instance.CreateSoundBuilder().WithPosition(toSelect.Position).Play(toSelect.SelectionClip);

            if (toSelect.HasCommands)
            {
                UIReferences.Instance.commandButtonGrid.Bind(toSelect.Commands);
            }
            else
            {
                UIReferences.Instance.commandButtonGrid.Unbind();
            }
        }

        public void SetUnitSelection(HashSet<ISelectable> toSelect, bool replaceSelection = true)
        {
            if (replaceSelection)
            {
                ClearSelection();
            }

            bool selectAll = true; // controllable units have priority
            foreach (var unit in toSelect)
            {
                if (unit.IsControllable)
                {
                    selectAll = false;
                    break;
                }
            }

            foreach (var unit in toSelect)
            {
                if (selectAll || unit.IsControllable)
                {
                    AddUnitToSelection(unit);
                }
            }

            if (OrderedUnits.Count > 0)
            {
                var highestPrioUnit = OrderedUnits.HighestPriorityUnit;
                if (highestPrioUnit.IsAlive())
                {
                    SoundManager.Instance.CreateSoundBuilder().WithPosition(highestPrioUnit.Position).Play(highestPrioUnit.SelectionClip);
                }
                
                if (highestPrioUnit.HasCommands)
                {
                    UIReferences.Instance.commandButtonGrid.Bind(highestPrioUnit.Commands);
                }
                else
                {
                    UIReferences.Instance.commandButtonGrid.Unbind();
                }
            }
        }

        public void SetControlGroup(int id)
        {
            if (controlGroups[id] == null)
            {
                controlGroups[id] = new HashSet<ISelectable>();
            }
            else
            {
                controlGroups[id].Clear();
            }
            
            controlGroups[id].AddRange(OrderedUnits);
        }
        
        public void SwitchToControlGroup(int id)
        {
            if (controlGroups[id] != null)
            {
                SetUnitSelection(controlGroups[id]);
            }
        }


        private void AddUnitToSelection(ISelectable unit)
        {
            if (unit.IsControllable)
            {
                allControllableUnits.Add(unit.ControllableUnit);   
            }
            OrderedUnits.Add(unit);   
            unit.OnSelect();
        }

        private void RemoveUnitFromSelection(ISelectable unit)
        {
            if (unit.IsControllable)
            {
                allControllableUnits.Remove(unit.ControllableUnit);   
            }
            OrderedUnits.Remove(unit);  
            unit.OnDeSelect();
        }

        private void MarkListChanged()
        {
            _listChanged = true;
        }

        private void Awake()
        {
            _inputControls = SceneReferences.Instance.inputHandler.InputControls;
            _inputControls.ControlGroups.Enable();

            _inputControls.ControlGroups.ControlGroup1.performed += context => { PressControlGroup(0); };
            _inputControls.ControlGroups.ControlGroup2.performed += context => { PressControlGroup(1); };
            _inputControls.ControlGroups.ControlGroup3.performed += context => { PressControlGroup(2); };
            _inputControls.ControlGroups.ControlGroup4.performed += context => { PressControlGroup(3); };
            _inputControls.ControlGroups.ControlGroup5.performed += context => { PressControlGroup(4); };
            _inputControls.ControlGroups.ControlGroup6.performed += context => { PressControlGroup(5); };
            _inputControls.ControlGroups.ControlGroup7.performed += context => { PressControlGroup(6); };
            _inputControls.ControlGroups.ControlGroup8.performed += context => { PressControlGroup(7); };
            _inputControls.ControlGroups.ControlGroup9.performed += context => { PressControlGroup(8); };
        }

        private void PressControlGroup(int id)
        {
            if (SceneReferences.Instance.inputHandler.ControlDown)
            {
                SetControlGroup(id);
            }
            else
            {
                SwitchToControlGroup(id);
            }
        }

        private void OnEnable()
        {
            OrderedUnits.OnControllableUnitListChanged += MarkListChanged;
        }
        
        private void OnDisable()
        {
            OrderedUnits.OnControllableUnitListChanged -= MarkListChanged;
        }

        private void LateUpdate()
        {
            if (_listChanged)
            {
                _listChanged = false;
                OnSelectedUnitsChanged?.Invoke();
            }
        }
    }
}
