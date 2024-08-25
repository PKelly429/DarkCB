using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public class StopCommand : BaseCommand
    {
        private readonly HashSet<IControllableUnit> observers = new HashSet<IControllableUnit>();

        public override void Execute()
        {
            foreach (var observer in observers)
            {
                observer.Stop();
            }
        }
            
        public void Register(IControllableUnit observer) => observers.Add(observer);
        public void Deregister(IControllableUnit observer) => observers.Remove(observer);
    }
}
