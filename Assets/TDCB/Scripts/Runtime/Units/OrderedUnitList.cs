using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public class OrderedUnitList : IEnumerable<ISelectable>
    {
        public delegate void ControllableUnitListChanged();

        public event ControllableUnitListChanged OnControllableUnitListChanged; 
        
        public readonly HashSet<ISelectable> containedUnits = new HashSet<ISelectable>();
        public readonly List<ISelectable> unitsInPrioirtyOrder = new List<ISelectable>();

        public int Count => containedUnits.Count;
        public ISelectable HighestPriorityUnit => unitsInPrioirtyOrder[0];
        
        public void Add(ISelectable unit)
        {
            if (containedUnits.Add(unit))
            {
                unitsInPrioirtyOrder.Add(unit);
                unitsInPrioirtyOrder.Sort((x, y) => y.Priority.CompareTo(x.Priority));
                
                OnControllableUnitListChanged?.Invoke();
            }
        }
        
        public void Remove(ISelectable unit)
        {
            if (containedUnits.Remove(unit))
            {
                unitsInPrioirtyOrder.Remove(unit);
                
                OnControllableUnitListChanged?.Invoke();
            }
        }

        public void Clear()
        {
            containedUnits.Clear();
            unitsInPrioirtyOrder.Clear();
            OnControllableUnitListChanged?.Invoke();
        }

        public IEnumerator<ISelectable> GetEnumerator()
        {
            return unitsInPrioirtyOrder.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
