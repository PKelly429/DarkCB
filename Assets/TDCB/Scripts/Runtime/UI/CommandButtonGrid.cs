using System.Collections;
using System.Collections.Generic;
using DataBinding;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace TDCB
{
    public class CommandButtonGrid : MonoBehaviour
    {
        private const int Commands = 15;
        
        [SerializeField] private GameObject commandButtonPrefab;

        [SerializeField] private GameObject[] commandButtonObjects = new GameObject[Commands];
        [SerializeField] private BinderGroup[] commandBinders = new BinderGroup[Commands];

        private void Start()
        {
            Unbind();
        }

        public void Bind(CommandTemplate unitCommands)
        {
            if (unitCommands == null)
            {
                Unbind();
                return;
            }
            
            int next = 0;
            for (int i = 0; i < unitCommands.commandRow1.Length; i++)
            {
                commandBinders[next].Bind(unitCommands.commandRow1[i]);
                next++;
            }
            for (int i = 0; i < unitCommands.commandRow2.Length; i++)
            {
                commandBinders[next].Bind(unitCommands.commandRow2[i]);
                next++;
            }
            for (int i = 0; i < unitCommands.commandRow3.Length; i++)
            {
                commandBinders[next].Bind(unitCommands.commandRow3[i]);
                next++;
            }
        }

        public void Unbind()
        {
            for (int i = 0; i < Commands; i++)
            {
                commandBinders[i].Bind(null);
            }
        }

        [Button]
        private void Generate()
        {
            for (int i = 0; i <commandButtonObjects.Length; i++)
            {
                DestroyImmediate(commandButtonObjects[i]);
            }

            for (int i = 0; i < Commands; i++)
            {
                commandButtonObjects[i] = (GameObject) PrefabUtility.InstantiatePrefab(commandButtonPrefab, transform);
                commandBinders[i] = commandButtonObjects[i].GetComponent<BinderGroup>();
            }
        }
    }
}
