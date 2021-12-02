using Prg.Scripts.Common.Unity;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Editor.Prg
{
    /// <summary>
    /// Property drawer for <c>LayerSelector</c> attribute.
    /// </summary>
    [CustomPropertyDrawer(typeof(TagSelectorAttribute))]
    public class TagSelectorPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }
            EditorGUI.BeginProperty(position, label, property);
            {
                if (attribute is TagSelectorAttribute && TagSelectorAttribute.UseDefaultTagFieldDrawer)
                {
                    property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
                }
                else
                {
                    var tagList = new List<string> { "Untagged" };
                    tagList.AddRange(InternalEditorUtility.tags);
                    var propertyString = property.stringValue;
                    var index = propertyString == string.Empty ? 0 : tagList.FindIndex(x => x == propertyString);
                    index = EditorGUI.Popup(position, label.text, index, tagList.ToArray());
                    if (index == 0)
                    {
                        property.stringValue = string.Empty;
                    }
                    else if (index >= 1)
                    {
                        property.stringValue = tagList[index];
                    }
                    else
                    {
                        property.stringValue = string.Empty;
                    }
                }
            }
            EditorGUI.EndProperty();
        }
    }
}