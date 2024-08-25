using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DataBinding.Editor
{
    [CustomPropertyDrawer(typeof(BindingField))]
    public class BindingDrawer : PropertyDrawer
    {
        private string _typeId;
        string[] _targetNames;

        protected virtual void Initialize(SerializedProperty bindingType, SerializedProperty targetProperty)
        {
            string typeId = bindingType.FindPropertyRelative("assemblyQualifiedName").stringValue;
        
            if (_targetNames != null && _typeId == typeId) return;
            _typeId = typeId;

            if (!string.IsNullOrEmpty(typeId))
            {
                try
                {
                    var type = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(assembly => assembly.GetTypes())
                        .First(t => t.AssemblyQualifiedName == typeId);
                    
                    Type targetType = typeof(AbstractBindableVariable);

                    try
                    {
                        var attributes = EditorUtils.GetAttributes<BindingTypeAttribute>(targetProperty, false);
                        if (attributes.Length > 0)
                        {
                            BindingTypeAttribute attribute = attributes[0];
                            if (attribute != null)
                            {
                                if (attribute.bindingType == typeof(Enum))
                                {
                                    _targetNames = type.GetFields().Where(t => typeof(AbstractBindableVariable).IsAssignableFrom(t.FieldType))
                                        .Where(t => t.FieldType.BaseType.IsGenericType && t.FieldType.BaseType.GetGenericTypeDefinition() == typeof(BindableEnum<>))
                                        .Select(t => t.Name).ToArray();
                                    return;
                                }
                                else
                                {
                                    targetType = typeof(BindableVariable<>).MakeGenericType(attribute.bindingType);   
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        // could not find attribute
                    }

                    _targetNames = type.GetFields().Where(t => targetType.IsAssignableFrom(t.FieldType)).Select(t => t.Name).ToArray();
                }
                catch (Exception e)
                {
                    typeId = null;
                    _targetNames = Array.Empty<string>();
                }
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) 
        {
            var typeProperty = property.FindPropertyRelative("bindingType");
            var targetProperty = property.FindPropertyRelative("property");
            
            Initialize(typeProperty, targetProperty);
        
            EditorGUI.PropertyField(position, typeProperty, new GUIContent("Bindable Type"));

            if (!string.IsNullOrEmpty(typeProperty.FindPropertyRelative("assemblyQualifiedName").stringValue)) 
            {
                position = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.objectField);
            
                var currentIndex = string.IsNullOrEmpty(targetProperty.stringValue) ? 0 : Array.IndexOf(_targetNames, targetProperty.stringValue);

                if (_targetNames == null || _targetNames.Length < 1)
                {
                    EditorGUI.LabelField(position,"Field", "No Matching Fields");

                    string helpMessage = "Add fields extending from BindableVariable<T>\n e.g. StringBindableVariable";
                    position = GUILayoutUtility.GetRect(new GUIContent(helpMessage), EditorStyles.helpBox);
                    EditorGUI.HelpBox(position, helpMessage, MessageType.Info);
                    
                    return;
                }
                
                var selectedIndex = EditorGUI.Popup(position, "Field", currentIndex, _targetNames);
            
                if (String.IsNullOrEmpty(targetProperty.stringValue) || (selectedIndex >= 0 && selectedIndex != currentIndex)) 
                {
                    targetProperty.stringValue = _targetNames[selectedIndex];
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
        }
    
    }
}