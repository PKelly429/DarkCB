using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public interface ITargetCommand
    {
        public void Execute(ISelectable target);
        public void Execute(Vector3 value);
        public void OnAfterExecuteOrCancel();
    }
}
