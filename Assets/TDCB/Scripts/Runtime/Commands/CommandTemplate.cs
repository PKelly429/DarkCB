using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TDCB
{
    [CreateAssetMenu(menuName = "Command/Command Template")]
    public class CommandTemplate : ScriptableObject 
    { 
        [ListDrawerSettings(ShowFoldout = false, HideRemoveButton = true, HideAddButton = true)]
        [RequiredListLength(5)]
        public BaseCommand[] commandRow1 = new BaseCommand[5];
        [ListDrawerSettings(ShowFoldout = false, HideRemoveButton = true, HideAddButton = true)]
        [RequiredListLength(5)]
        public BaseCommand[] commandRow2 = new BaseCommand[5];
        [ListDrawerSettings(ShowFoldout = false, HideRemoveButton = true, HideAddButton = true)]
        [RequiredListLength(5)]
        public BaseCommand[] commandRow3 = new BaseCommand[5];
    }
}
