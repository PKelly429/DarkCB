using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public interface IWorkerAssignment
    {
        public AssignmentArgs Assign(Unit unit);
        public void Unassign(Unit unit);
        public Vector3 Position { get; }
    }

    public struct AssignmentArgs
    {
        public bool WasAssigned;
        public IWorkerAssignment AssignedTo;
    }
}
