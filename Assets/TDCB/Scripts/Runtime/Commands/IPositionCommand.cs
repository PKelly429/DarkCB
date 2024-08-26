using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public interface IPositionCommand
    {
        public void Execute(Vector3 value);
        public void OnAfterExecuteOrCancel();
    }
}
