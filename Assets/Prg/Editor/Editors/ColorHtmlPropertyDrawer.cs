using System;
using Prg.Scripts.EditorSupport.Attributes;
using UnityEditor;
using UnityEngine;

namespace Prg.Editor.Editors
{
    /// <summary>
    /// Add support for HTML color strings like #RRGGBBAA.<br />
    /// See: ColorUtility.TryParseHtmlString
    /// https://docs.unity3d.com/ScriptReference/ColorUtility.TryParseHtmlString.html
    /// </summary>
    /// <remarks>
    /// Color individual float value are truncated to 7 digits
    /// (last floating point digit removed to keep values 'accurate' or 'stable')
    /// </remarks>
    [CustomPropertyDrawer(typeof(ColorHtmlPropertyAttribute))]
    public class ColorHtmlPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var htmlField = new Rect(
                position.x, position.y, position.width * 0.6f, position.height);
            var colorField = new Rect(
                position.x + htmlField.width, position.y, position.width - htmlField.width, position.height);
            var htmlValue = EditorGUI.TextField(
                htmlField, label, $"#{ColorUtility.ToHtmlStringRGBA(property.colorValue)}");

            if (ColorUtility.TryParseHtmlString(htmlValue, out var color))
            {
                if (color.r != 0)
                {
                    color.r = (float)Math.Round(color.r, 7);
                }
                if (color.g != 0)
                {
                    color.g = (float)Math.Round(color.g, 7);
                }
                if (color.b != 0)
                {
                    color.b = (float)Math.Round(color.b, 7);
                }
                if (color.a != 0)
                {
                    color.a = (float)Math.Round(color.a, 7);
                }
                if (property.colorValue != color)
                {
                    property.colorValue = color;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
            property.colorValue = EditorGUI.ColorField(colorField, property.colorValue);
        }
    }
}
