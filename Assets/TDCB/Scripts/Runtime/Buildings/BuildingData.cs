using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    [CreateAssetMenu (menuName="TDCB/Building")]
    public class BuildingData : ScriptableObject
    {
        public GameObject prefab;
        public Sprite image;
    }
}
