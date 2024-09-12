using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public class OrderedUnitList : IEnumerable<ISelectable>
    {
        public delegate void ControllableUnitListChanged();

        public event ControllableUnitListChanged OnControllableUnitListChanged;

        public readonly HashSet<ISelectable> containedUnits;
        public readonly List<ISelectable> unitsInPrioirtyOrder;

        public OrderedUnitList(int capacity)
        {
            containedUnits = new HashSet<ISelectable>(capacity);
            unitsInPrioirtyOrder = new List<ISelectable>(capacity);
        }

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

        public void AddRange(OrderedUnitList other)
        {
            foreach (var unit in other)
            {
                Add(unit);
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((OrderedUnitList)obj);
        }
        private bool Equals(OrderedUnitList other)
        {
            if (containedUnits.Count != other.Count) return false;

            foreach (var unit in containedUnits)
            {
                if (!other.containedUnits.Contains(unit)) return false;
            }

            return true;
        }

    }
}
