using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace Editor.Prg.Util
{
    /// <summary>
    /// Example class to show how to access <c>SerializedObject</c> and <c>SerializedProperty</c> from code.
    /// </summary>
    public class MenuShowVersionInfo : MonoBehaviour
    {
        [MenuItem("Window/ALT-Zone/Util/Show Version Info")]
        private static void ShowVersionInfo()
        {
            void printProperty(SerializedObject anObject, string propName)
            {
                var _serializedProp = anObject.FindProperty(propName);
                var propValue = "";
                switch (_serializedProp.propertyType)
                {
                    case SerializedPropertyType.String:
                        propValue = _serializedProp.stringValue;
                        break;
                    case SerializedPropertyType.Integer:
                        propValue = _serializedProp.intValue.ToString();
                        break;
                    case SerializedPropertyType.Float:
                        propValue = _serializedProp.floatValue.ToString(CultureInfo.InvariantCulture);
                        break;
                    default:
                        propValue = "<unsopported property type>";
                        break;
                }
                Debug.Log($"{_serializedProp.displayName}={propValue} [{_serializedProp.propertyType}]");
            }

            // Find out what kind of (type of) object you have:
            // var asset0 = AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/GraphicsSettings.asset");
            // Debug.Log("asset=" + asset0);

            // https://docs.unity3d.com/ScriptReference/AssetDatabase.LoadAssetAtPath.html
            // https://docs.unity3d.com/ScriptReference/SerializedProperty.html
            // https://docs.unity3d.com/ScriptReference/SerializedPropertyType.html

            var asset = AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/ProjectSettings.asset");
            Debug.Log("asset=" + asset);

            var serializedObject = new SerializedObject(asset);
            printProperty(serializedObject, "productName");
            printProperty(serializedObject, "bundleVersion");
            printProperty(serializedObject, "AndroidBundleVersionCode");
        }
    }
}