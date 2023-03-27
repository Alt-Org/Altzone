using System.IO;
using UnityEditor;

namespace Prg.Editor.Util
{
    internal static class CreateStandardFolders
    {
        public static void CreateProjectStandardFolders()
        {
            Debug.Log("*");
            var activeObject = Selection.activeObject;
            if (activeObject == null || Selection.assetGUIDs.Length != 1)
            {
                Debug.Log("No can do: Select a single FOLDER in 'Project' tab and 'Assets' Pane");
                return;
            }
            var selectedGuid = Selection.assetGUIDs[0];
            var parentFolder = AssetDatabase.GUIDToAssetPath(selectedGuid);
            if (!Directory.Exists(parentFolder))
            {
                Debug.Log($"No can do: Selected object '{parentFolder}' is not a directory");
                return;
            }
            Debug.Log($"Create UNITY standard folders for: '{parentFolder}'");

            var folders = new[]
            {
                "Animations",
                "Fonts",
                "Graphics",
                "Materials",
                "Models",
                "Prefabs",
                "Resources",
                "Scenes",
                "ScriptableObjects",
                "Scripts",
                "Sounds",
                "Textures"
            };
            foreach (var folder in folders)
            {
                var path = Path.Combine(parentFolder, folder);
                if (Directory.Exists(path) || File.Exists(path))
                {
                    Debug.Log($"Skip: {RichText.White(folder)}");
                    continue;
                }
                var guid = AssetDatabase.CreateFolder(parentFolder, folder);
                Debug.Log($"Folder: {RichText.White(folder)} created with guid {guid}");
            }
            Debug.Log("Done");
        }
    }
}