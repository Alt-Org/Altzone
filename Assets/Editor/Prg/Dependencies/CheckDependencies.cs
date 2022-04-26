using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor.Prg.Dependencies
{
    /// <summary>
    /// Utility script to check dependencies of selected objects in UNITY <c>Editor</c> based on their <c>GUID</c>.
    /// </summary>
    /// <remarks>
    /// List of supported object types (in selection) is limited to some "well known" types used in UNITY.
    /// </remarks>
    internal static class CheckDependencies
    {
        private const string AssetRootName = "Assets";

        public static void CheckUsages()
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
                ".inputactions",
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
            var searchFolders = new[] { AssetRootName };
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

        public static void ShowFolders()
        {
            Debug.Log("*");
            var paths = GetPathsForSelectedGuids();
            if (paths == null)
            {
                Debug.Log("Nothing is selected");
                return;
            }
            var uniquePaths = new HashSet<string>();
            var specialPaths = new HashSet<string>();
            foreach (var path in paths)
            {
                var dirname = Path.GetDirectoryName(RemoveAssetRootName(path));
                if (path.StartsWith(AssetRootName))
                {
                    uniquePaths.Add(dirname);
                }
                else
                {
                    specialPaths.Add(dirname);
                }
            }
            paths = uniquePaths.ToList();
            paths.Sort();
            foreach (var path in paths)
            {
                Debug.Log($"{RichText.Yellow(path)}");
            }
            paths = specialPaths.ToList();
            paths.Sort();
            foreach (var path in paths)
            {
                Debug.Log($"{RichText.Brown(path)}");
            }
        }

        public static void SortSelection()
        {
            Debug.Log("*");
            var paths = GetPathsForSelectedGuids();
            if (paths == null)
            {
                Debug.Log("Nothing is selected");
                return;
            }
            paths.Sort();
            foreach (var path in paths)
            {
                var asset = AssetDatabase.LoadMainAssetAtPath(path);
                var dirname = Path.GetDirectoryName(RemoveAssetRootName(path));
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

        private static List<string> GetPathsForSelectedGuids()
        {
            var selectedGuids = Selection.assetGUIDs;
            if (selectedGuids.Length == 0)
            {
                return null;
            }
            var paths = new List<string>();
            foreach (var t in selectedGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(t);
                paths.Add(path);
            }
            return paths;
        }

        private static string RemoveAssetRootName(string path)
        {
            if (path.StartsWith(AssetRootName) && path[AssetRootName.Length] == '/')
            {
                path = path.Replace(AssetRootName, "").Substring(1);
            }
            return path;
        }
    }
}