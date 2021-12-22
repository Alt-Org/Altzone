using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor.Prg
{
    /// <summary>
    /// Utility script to check dependencies of selected objects in UNITY <c>Editor</c> based on their <c>GUID</c>.
    /// </summary>
    /// <remarks>
    /// List of supported object types (in selection) is limited to some "well known" types used in UNITY.
    /// </remarks>
    public static class CheckDependencies
    {
        private const string MenuRoot = "Window/ALT-Zone/Dependencies/";

        [MenuItem(MenuRoot + "Check Dependencies", false, 10)]
        private static void _CheckDependencies()
        {
            Debug.Log("*");
            var activeObject = Selection.activeObject;
            if (activeObject == null)
            {
                Debug.Log("Nothing is selected");
                return;
            }
            var selectedGuids = Selection.assetGUIDs;
            // Keep extensions lowercase!
            var validExtensions = new[]
            {
                ".asset",
                ".cs",
                ".gif",
                ".mat",
                ".otf",
                ".physicMaterial",
                ".physicsmaterial2d",
                ".png",
                ".prefab",
                ".psd",
                ".ttf"
            };
            foreach (var guid in selectedGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.Contains(activeObject))
                {
                    var extension = Path.HasExtension(path) ? Path.GetExtension(path).ToLower() : string.Empty;
                    if (validExtensions.Contains(extension))
                    {
                        continue;
                    }
                }
                Debug.Log($"Selected object is not supported asset: {path}");
                return;
            }
            Debug.Log($"Search dependencies for {selectedGuids.Length} assets in {string.Join(", ", validExtensions)}");
            const string assetRoot = "Assets";
            var searchFolders = new[] { assetRoot };
            var foundCount = new int[selectedGuids.Length];
            Array.Clear(foundCount, 0, foundCount.Length);

            var assetFilters = new[] { "t:Scene", "t:Prefab", "t:ScriptableObject" };
            var totalCount = 0;
            foreach (var assetFilter in assetFilters)
            {
                var foundAssets = AssetDatabase.FindAssets(assetFilter, searchFolders);
                var searchCount = CheckForGuidInAssets(selectedGuids, ref foundCount, foundAssets);
                totalCount += searchCount;
                Debug.Log($"search {assetFilter}:{foundAssets.Length} found={searchCount}");
            }
            Debug.Log(">");
            var noDepCount = 0;
            for (var i = 0; i < selectedGuids.Length; ++i)
            {
                if (foundCount[i] == 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(selectedGuids[i]);
                    Debug.Log($"{path} has <b>{RichText.Brown("NO dependencies")}</b> in this search");
                    noDepCount += 1;
                }
            }
            if (totalCount > 0)
            {
                if (noDepCount > 0)
                {
                    Debug.Log(">");
                }
                for (var i = 0; i < selectedGuids.Length; ++i)
                {
                    var path = AssetDatabase.GUIDToAssetPath(selectedGuids[i]);
                    var depCount = foundCount[i];
                    var message = depCount > 0
                        ? $"has <b>{depCount} dependencies</b>"
                        : $"does not have <i>any dependencies</i> and <b>can be safely deleted</b>";
                    Debug.Log($"{path} {message}");
                }
            }
        }

        private static int CheckForGuidInAssets(string[] selectedGuids, ref int[] foundCount, string[] assetGuids)
        {
            var count = 0;
            foreach (var assetGuid in assetGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(assetGuid);
                var assetContent = File.ReadAllText(path);
                for (var guidIndex = 0; guidIndex < selectedGuids.Length; ++guidIndex)
                {
                    var guid = selectedGuids[guidIndex];
                    if (assetContent.Contains(guid))
                    {
                        var source = AssetDatabase.GUIDToAssetPath(guid);
                        var go = AssetDatabase.LoadAssetAtPath (path, typeof(GameObject)) as GameObject;
                        Debug.LogWarning($"{source} found in {path}", go);
                        foundCount[guidIndex] += 1;
                        count += 1;
                    }
                }
            }
            return count;
        }
    }
}