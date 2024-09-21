using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TDCB
{
    public class SetObjectsInSphere : MonoBehaviour
    {
        public float radius;
        public Vector3 offset;
        public List<Transform> objs;

        [Button]
        public void PlaceRandomly()
        {
            foreach (var obj in objs)
            {
                obj.localPosition = offset + (Random.rotation * Vector3.forward * radius);
            }
        }
    }
}
