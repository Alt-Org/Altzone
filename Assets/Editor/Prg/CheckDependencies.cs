using System;
using System.Collections.Generic;
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

        [MenuItem(MenuRoot + "Check Usages", false, 10)]
        private static void _CheckDependencies()
        {
            Debug.Log("*");
            var selectedGuids = Selection.assetGUIDs;
            if (selectedGuids.Length == 0)
            {
                Debug.Log("Nothing is selected");
                return;
            }
            // Keep extensions lowercase!
            var validExtensions = new[]
            {
                ".anim",
                ".asset",
                ".blend",
                ".controller",
                ".cs",
                ".cubemap",
                ".flare",
                ".gif",
                ".mat",
                ".mp3",
                ".otf",
                ".physicMaterial",
                ".physicsmaterial2d",
                ".png",
                ".prefab",
                ".psd",
                ".shader",
                ".tga",
                ".tif",
                ".ttf",
                ".wav",
            };
            var hasShaders = false;
            foreach (var guid in selectedGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var extension = Path.HasExtension(path) ? Path.GetExtension(path).ToLower() : string.Empty;
                if (!hasShaders && extension == ".shader")
                {
                    hasShaders = true;
                }
                if (validExtensions.Contains(extension))
                {
                    continue;
                }
                var asset = AssetDatabase.LoadMainAssetAtPath(path);
                Debug.LogWarning($"Selected object is not supported asset: {path}", asset);
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
                    var asset = AssetDatabase.LoadMainAssetAtPath(path);
                    Debug.LogWarning($"{path} has <b>{RichText.Brown("NO dependencies")}</b> in this search", asset);
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
                    var asset = AssetDatabase.LoadMainAssetAtPath(path);
                    Debug.LogWarning($"{path} {message}", asset);
                }
            }
            if (hasShaders)
            {
                Debug.LogWarning($"{RichText.Yellow("Shaders are referenced by name and can not be detected with this script")}");
            }
        }

        [MenuItem(MenuRoot + "Sort Selection", false, 11)]
        private static void _SortSelection()
        {
            Debug.Log("*");
            var selectedGuids = Selection.assetGUIDs;
            if (selectedGuids.Length == 0)
            {
                Debug.Log("Nothing is selected");
                return;
            }
            var list = new List<string>();
            foreach (var t in selectedGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(t);
                list.Add(path);
            }
            list.Sort();
            foreach (var path in list)
            {
                var asset = AssetDatabase.LoadMainAssetAtPath(path);
                var dirname = Path.GetDirectoryName(path)?.Replace("Assets\\", "");
                var filename = Path.GetFileName(path);
                Debug.Log($"{RichText.Yellow(dirname)} {RichText.Brown(filename)}", asset);
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
                        var go = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
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