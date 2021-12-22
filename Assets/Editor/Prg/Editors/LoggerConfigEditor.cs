using System.Linq;
using System.Text;
using Prg.Scripts.Common.Util;
using UnityEditor;
using UnityEngine;

namespace Editor.Prg.Editors
{
    [CustomEditor(typeof(LoggerConfig))]
    public class LoggerConfigEditor : UnityEditor.Editor
    {
        private static readonly string[] ExcludedFolders =
        {
            "Assets/Photon",
            "Assets/TextMesh Pro",
            "Assets/Prototype Textures",
        };

        public override void OnInspectorGUI()
        {
            if (serializedObject.isEditingMultipleObjects)
            {
                DrawDefaultInspector();
                return;
            }
            if (GUILayout.Button("Create Default Config"))
            {
                serializedObject.Update();
                UpdateState(serializedObject);
                serializedObject.ApplyModifiedProperties();
            }
            GUILayout.Space(20);
            DrawDefaultInspector();
        }

        private static void UpdateState(SerializedObject serializedObject)
        {
            var loggerRules = serializedObject.FindProperty("_loggerRules");
            var curValue = loggerRules.stringValue;
            var newValue = LoadAssetFolders();
            loggerRules.stringValue = newValue;
        }

        private static string LoadAssetFolders()
        {
            var folders = AssetDatabase.GetSubFolders("Assets");
            var builder = new StringBuilder();
            builder.Append(".*PhotonListener.*=1");
            foreach (var folder in folders)
            {
                if (ExcludedFolders.Contains(folder))
                {
                    continue;
                }
                var line = folder.Replace("Assets/", "^");
                line += ".*=0";
                builder.AppendLine().Append(line);
            }
            return builder.ToString();
        }
    }
}