using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace Editor.Prg.Util
{
    /// <summary>
    /// Example class to show how to access <c>SerializedObject</c> and <c>SerializedProperty</c> from code.
    /// </summary>
    internal static class MenuShowVersionInfo
    {
        public static void ShowVersionInfo()
        {
            void PrintProperty(SerializedObject anObject, string propName)
            {
                var serializedProp = anObject.FindProperty(propName);
                string propValue;
                switch (serializedProp.propertyType)
                {
                    case SerializedPropertyType.String:
                        propValue = serializedProp.stringValue;
                        break;
                    case SerializedPropertyType.Integer:
                        propValue = serializedProp.intValue.ToString();
                        break;
                    case SerializedPropertyType.Float:
                        propValue = serializedProp.floatValue.ToString(CultureInfo.InvariantCulture);
                        break;
                    default:
                        propValue = "<unsupported property type>";
                        break;
                }
                Debug.Log($"{serializedProp.displayName}={propValue} [{serializedProp.propertyType}]");
            }

            // Find out what kind of (type of) object you have:
            // var asset0 = AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/GraphicsSettings.asset");
            // Debug.Log("asset=" + asset0);

            // https://docs.unity3d.com/ScriptReference/AssetDatabase.LoadAssetAtPath.html
            // https://docs.unity3d.com/ScriptReference/SerializedProperty.html
            // https://docs.unity3d.com/ScriptReference/SerializedPropertyType.html

            Debug.Log("*");
            var asset = AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/ProjectSettings.asset");
            Debug.Log("asset=" + asset);

            var serializedObject = new SerializedObject(asset);
            PrintProperty(serializedObject, "productName");
            PrintProperty(serializedObject, "bundleVersion");
            PrintProperty(serializedObject, "AndroidBundleVersionCode");
        }
    }
}