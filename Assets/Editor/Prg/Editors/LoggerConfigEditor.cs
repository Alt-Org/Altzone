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
        private const string DefaultLoggingState = "1";
        
        private static readonly string[] RulesForReset =
        {
            ".*PhotonListener.*=1",
            ".*SceneLoader.*=1",
            ".*ScoreFlash.*=1",
        };

        private static readonly string[] ExcludedFoldersForReset =
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
            if (GUILayout.Button("Reset Folders for Logger Rules"))
            {
                Debug.Log("*");
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
            loggerRules.stringValue = LoadAssetFolders();
        }

        private static string LoadAssetFolders()
        {
            var folders = AssetDatabase.GetSubFolders("Assets");
            var builder = new StringBuilder();
            foreach (var defaultRule in RulesForReset)
            {
                builder.Append(defaultRule).AppendLine();
            }
            foreach (var folder in folders)
            {
                if (ExcludedFoldersForReset.Contains(folder))
                {
                    continue;
                }
                var line = folder.Replace("Assets/", "^");
                line += $".*={DefaultLoggingState}";
                builder.Append(line).AppendLine();
                if (line.StartsWith("^Tests.*"))
                {
                    // PlayMode tests namespace can be absolute for some reason!?
                    builder.Append($"^Assets\\.Tests\\..*={DefaultLoggingState}").AppendLine();
                }
            }
            while (builder[^1] == '\r' || builder[^1] == '\n')
            {
                builder.Length -= 1;
            }
            return builder.ToString();
        }
    }
}