using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DataBinding.Editor
{
    [CustomEditor(typeof(ActivateGameObjectBinder))]
    public class GameObjectBinderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            ActivateGameObjectBinder binder = target as ActivateGameObjectBinder;
            
            if(binder == null) return;
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bindingMode"));
            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical(new GUIStyle("box"));
            switch (binder.bindingMode)
            {
                case ActivateGameObjectBinder.BindingMode.BindingObjectType:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("bindingType"));
                    EditorGUILayout.HelpBox("Enabled when bound with a matching type, otherwise disabled", MessageType.Info);
                    break;
                case ActivateGameObjectBinder.BindingMode.BindToBool:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("target"));
                    break;
                case ActivateGameObjectBinder.BindingMode.Function:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("bindingFunction"));
                    EditorGUILayout.HelpBox("Make a custom binding function by inheriting from 'GameObjectBindingFunction'", MessageType.Info);
                    break;
            }
            EditorGUILayout.EndVertical();
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
