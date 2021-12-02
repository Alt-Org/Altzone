using Prg.Scripts.Common.Unity;
using Prg.Scripts.Common.Util;
using UnityEditor;
using UnityEngine;

namespace Editor.Prg
{
    [CustomEditor(typeof(StringProperty))]
    public class StringPropertyEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (serializedObject.isEditingMultipleObjects)
            {
                DrawDefaultInspector();
                return;
            }
            if (GUILayout.Button("Compress/Un-compress"))
            {
                serializedObject.Update();
                UpdateState(serializedObject);
                serializedObject.ApplyModifiedProperties();
            }
            var curValue = serializedObject.FindProperty("_propertyValue").stringValue;
            var isCompressed = StringProperty.IsCompressed(curValue);
            GUILayout.Space(20);
            GUI.enabled = !isCompressed;
            DrawDefaultInspector();
            GUI.enabled = true;
            GUILayout.Space(20);
            var labelName = isCompressed
                ? "Plaintext"
                : "Compressed";
            var labelValue = isCompressed
                ? StringSerializer.Decode(StringProperty.GetCompressedPayload(curValue))
                : StringProperty.FormatCompressedPayload(StringSerializer.Encode(curValue));
            GUILayout.Label($"{labelName}: {labelValue}");
        }

        private static void UpdateState(SerializedObject serializedObject)
        {
            var propertyValue = serializedObject.FindProperty("_propertyValue");
            var curValue = propertyValue.stringValue;
            var isCompressed = StringProperty.IsCompressed(curValue);
            var newValue = isCompressed
                ? StringSerializer.Decode(StringProperty.GetCompressedPayload(curValue))
                : StringProperty.FormatCompressedPayload(StringSerializer.Encode(curValue));
            propertyValue.stringValue = newValue;
        }
    }
}