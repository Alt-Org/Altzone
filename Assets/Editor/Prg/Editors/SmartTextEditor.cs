using Prg.Scripts.Common.Unity.Localization;
using UnityEditor;
using UnityEngine;

namespace Editor.Prg.Editors
{
    [CustomEditor(typeof(SmartText)), CanEditMultipleObjects]
    public class SmartTextEditor : SmartComponentEditor
    {
    }

    [CustomEditor(typeof(SmartButton)), CanEditMultipleObjects]
    public class SmartButtonEditor : SmartComponentEditor
    {
    }

    [CustomEditor(typeof(SmartToggle)), CanEditMultipleObjects]
    public class SmartToggleEditor : SmartComponentEditor
    {
    }

    public class SmartComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (!serializedObject.isEditingMultipleObjects)
            {
                if (GUILayout.Button("Create Localization Key"))
                {
                    serializedObject.Update();
                    UpdateState(serializedObject);
                    serializedObject.ApplyModifiedProperties();
                }
            }
            if (GUILayout.Button("Reset"))
            {
                serializedObject.Update();
                ResetState(serializedObject);
                serializedObject.ApplyModifiedProperties();
            }
            GUILayout.Space(20);
            DrawDefaultInspector();
        }

        private static void UpdateState(SerializedObject serializedObject)
        {
            var localizationKey = serializedObject.FindProperty("_localizationKey");
            var curValue = localizationKey.stringValue;
            if (string.IsNullOrWhiteSpace(curValue))
            {
                if (serializedObject.targetObject is SmartText smartText)
                {
                    localizationKey.stringValue = smartText.ComponentName;
                }
            }
        }

        private static void ResetState(SerializedObject serializedObject)
        {
            var localizationKey = serializedObject.FindProperty("_localizationKey");
            localizationKey.stringValue = string.Empty;
        }
    }
}