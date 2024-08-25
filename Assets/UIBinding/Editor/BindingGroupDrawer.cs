using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DataBinding.Editor
{
    [CustomEditor((typeof(BinderGroup)))]
    public class BindingGroupDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            BinderGroup binderGroup = target as BinderGroup;
            DrawDefaultInspector();

            if(binderGroup == null) return;
            if (GUILayout.Button("Add Children"))
            {
                binderGroup.AddChildren();
            }
        }
    }
}
