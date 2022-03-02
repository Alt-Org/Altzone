#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Editor.UiProto
{
    public class MenuWindowReport : MonoBehaviour
    {
        private static readonly string[] excludedFolders =
        {
            "Assets/Photon",
        };

        private const string OpenWindowButtonGuid = "a019d4d3f61e52648b2acac7801c5e4b";

        [MenuItem("Window/ALT-Zone/Ui Proto/Window Report")]
        private static void WindowReport()
        {
            Debug.Log("*");
            Debug.Log("* " + nameof(MenuWindowReport));
            Debug.Log("*");
            var excluded = new List<Regex>();
            foreach (var excludedFolder in excludedFolders)
            {
                excluded.Add(new Regex(excludedFolder));
            }

            var stopWatch = Stopwatch.StartNew();
            var folders = AssetDatabase.GetSubFolders("Assets");
            var context = new Context();
            foreach (var folder in folders)
            {
                handleSubFolder(folder, context, excluded);
            }
            stopWatch.Stop();
            Debug.Log(
                $"Project contains {context.folderCount} folders and {context.fileCount} files (took {stopWatch.ElapsedMilliseconds} ms). " +
                $"Excluded {context.excludedColderCount} folders.");
            if (context.unknownFileCount > 0)
            {
                Debug.Log($"There is {context.unknownFileCount} unknown files");
            }
            context.getButtonCount(out var buttonCount, out var prefabCount);
            Debug.Log($"Project has {buttonCount} buttons in {prefabCount} prefabs.");

            Debug.Log($"Reports saved in {Path.GetFullPath(".")}");
            save("windows_report1.txt", context.getButtonText());
            save("windows_report2.txt", context.getSortedText());
            save("windows_report3.txt", context.getRawText());

            void save(string filename, string content)
            {
                var path = Path.GetFullPath(filename);
                var oldContent = File.Exists(path) ? File.ReadAllText(path) : "";
                if (oldContent == content)
                {
                    Debug.Log($"same {filename}");
                    return;
                }
                Debug.Log(oldContent == "" ? $"new {filename}" : $"replace {filename}");
                Debug.Log(oldContent.Length.ToString());
                Debug.Log(content.Length.ToString());
                File.WriteAllText(path, content);
            }
        }

        private static void handleSubFolder(string parent, Context context, List<Regex> excluded)
        {
            if (excludedFolders.Contains(parent))
            {
                context.excludedColderCount += 1;
                return;
            }
            context.folderCount += 1;
            string[] guids = AssetDatabase.FindAssets(null, new[] { parent });
            context.add($"{parent}:{guids.Length}");
            foreach (var guid in guids)
            {
                context.fileCount += 1;
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var isExclude = false;
                foreach (var regex in excluded)
                {
                    if (regex.IsMatch(path))
                    {
                        Debug.Log($"skip {path} ({regex})");
                        context.excludedColderCount += 1;
                        isExclude = true;
                        break;
                    }
                }
                if (!isExclude)
                {
                    var assetInfo = new AssetInfo(guid, path);
                    if (assetInfo.isPrefab)
                    {
                        assetInfo.checkOpenWindowButton(OpenWindowButtonGuid);
                    }
                    context.add(assetInfo);
                    if (!assetInfo.isWellKnown)
                    {
                        context.unknownFileCount += 1;
                    }
                }
            }
        }

        private class Context
        {
            public int folderCount;
            public int excludedColderCount;
            public int fileCount;
            public int unknownFileCount;

            private readonly StringBuilder builder;
            private readonly List<AssetInfo> assets = new List<AssetInfo>();

            public Context()
            {
                builder = new StringBuilder();
            }

            public void add(string info)
            {
                builder.Append(info).AppendLine();
            }

            public void add(AssetInfo assetInfo)
            {
                if (assets.Contains(assetInfo))
                {
                    return;
                }
                builder.Append($"{assetInfo}").AppendLine();
                assets.Add(assetInfo);
            }

            public void getButtonCount(out int buttonCount, out int prefabCount)
            {
                buttonCount = 0;
                prefabCount = 0;
                foreach (var assetInfo in assets)
                {
                    if (assetInfo.buttonCount > 0)
                    {
                        buttonCount += assetInfo.buttonCount;
                        prefabCount += 1;
                    }
                }
            }

            public string getRawText()
            {
                return builder.ToString();
            }

            public string getSortedText()
            {
                assets.Sort((a, b) => String.Compare(a.path, b.path, StringComparison.Ordinal));
                return string.Join("\r\n", assets);
            }

            public string getButtonText()
            {
                var buttonAssets = assets.Where(x => x.buttonCount > 0).ToList();
                buttonAssets.Sort((a, b) => String.Compare(a.path, b.path, StringComparison.Ordinal));
                var assetPrefabs = assets.Where(x => x.isPrefab).ToList();
                var buttonBuilder = new StringBuilder();
                foreach (var buttonAsset in buttonAssets)
                {
                    buttonBuilder.Append(buttonAsset.AsChild()).AppendLine();
                    getParents(assetPrefabs, buttonAsset, buttonBuilder, 1);
                }
                return buttonBuilder.ToString();
            }

            private void getParents(List<AssetInfo> assetPrefabs, AssetInfo assetChild, StringBuilder buttonBuilder, int level)
            {
                foreach (var assetParent in assetPrefabs)
                {
                    if (assetParent.hasChildAsset(assetChild))
                    {
                        buttonBuilder.Append(assetParent.AsParent(level)).AppendLine();
                        getParents(assetPrefabs, assetParent, buttonBuilder, level + 1);
                    }
                }
            }
        }

        private class AssetInfo : IComparable
        {
            private const string PrefabName = "Prefab";
            private const string UnknownFormat = "<<{0}>>";

            private readonly string guid;
            public readonly string path;
            private readonly string id;
            private readonly string typeName;
            public bool isPrefab => typeName == PrefabName;
            public bool isWellKnown => !typeName.StartsWith("<<");

            public int buttonCount => buttons.Count;
            private readonly List<string> buttons = new List<string>();

            private string _assetContent;

            public AssetInfo(string guid, string path)
            {
                string getNativeFormat(string assetPath)
                {
                    var assetContent = getAssetContent();
                    if (assetContent.Contains("MonoBehaviour:"))
                    {
                        return "ScriptableObject";
                    }
                    if (assetContent.Contains("AnimationClip:"))
                    {
                        return "Animation";
                    }
                    if (assetContent.Contains("Material:"))
                    {
                        return "Material";
                    }
                    if (assetContent.Contains("PhysicsMaterial2D:"))
                    {
                        return "PhysicsMaterial2D";
                    }
                    return string.Format(UnknownFormat, Path.GetExtension(path));
                }

                this.guid = guid;
                this.path = path;
                id = $"{guid}:{path}";
                if (path.EndsWith("LightingData.asset"))
                {
                    typeName = "LightingData";
                    return;
                }
                var metaFilePath = AssetDatabase.GetTextMetaFilePathFromAssetPath(path);
                var metaText = File.ReadAllText(metaFilePath);
                var hasFeature = metaText.Contains("folderAsset: yes");
                if (hasFeature)
                {
                    typeName = "Folder";
                    return;
                }
                hasFeature = metaText.Contains("PrefabImporter:");
                if (hasFeature)
                {
                    typeName = PrefabName;
                    return;
                }
                hasFeature = metaText.Contains("NativeFormatImporter:");
                if (hasFeature)
                {
                    typeName = getNativeFormat(path);
                    return;
                }
                var importers = new[]
                {
                    "DefaultImporter:", // scene
                    "MonoImporter:", // UNITY component aka C#
                    "TextureImporter:",
                    "TextScriptImporter:",
                    "VideoClipImporter:",
                    "ModelImporter:", // Blender & Co
                    "ShaderImporter:",
                };
                foreach (var importer in importers)
                {
                    hasFeature = metaText.Contains(importer);
                    if (hasFeature)
                    {
                        typeName = importer.Replace("Importer:", "");
                        return;
                    }
                }
                hasFeature = path.ToLower().EndsWith(".cs"); // Other C# files
                if (hasFeature)
                {
                    typeName = "CSharp";
                    return;
                }
                if (Directory.Exists(path))
                {
                    typeName = "Folder"; // meta files without directory "flag"
                    return;
                }
                typeName = string.Format(UnknownFormat, "?");
            }

            public void checkOpenWindowButton(string buttonGuid)
            {
                var assetContent = getAssetContent();
                var pos = 0;
                for (;;)
                {
                    pos = assetContent.IndexOf(buttonGuid, pos + 1, StringComparison.Ordinal);
                    if (pos == -1)
                    {
                        break;
                    }
                    var tuple = getButtonInfo(assetContent, pos);
                    buttons.Add(tuple.Item1 != "0" ? tuple.Item2 : "???");
                }
            }

            public bool hasChildAsset(AssetInfo assetInfo)
            {
                var assetContent = getAssetContent();
                var pos = assetContent.IndexOf(assetInfo.guid, StringComparison.Ordinal);
                return pos != -1;
            }

            private Tuple<string, string> getButtonInfo(string assetContent, int startPos)
            {
                // TODO: window name is not available in prefab anymore
                var windowId = grabText(assetContent, startPos, "windowId:");
                var windowText = windowId;
                return new Tuple<string, string>(windowId, windowText);
            }

            private string grabText(string assetContent, int startPos, string keyword)
            {
                var pos1 = assetContent.IndexOf(keyword, startPos, StringComparison.Ordinal) + keyword.Length;
                var pos2 = assetContent.IndexOf("\n", pos1, StringComparison.Ordinal);
                var text = assetContent.Substring(pos1, pos2 - pos1).Trim();
                return text.Length == 0 ? "unknown" : text;
            }

            private string getAssetContent()
            {
                if (_assetContent == null)
                {
                    try
                    {
                        _assetContent = File.ReadAllText(path);
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogException(e);
                    }
                }
                return _assetContent;
            }

            public int CompareTo(object obj)
            {
                if (obj is AssetInfo other)
                {
                    return String.Compare(id, other.id, StringComparison.Ordinal);
                }
                return GetHashCode().CompareTo(obj?.GetHashCode() ?? 0);
            }

            private bool Equals(AssetInfo other)
            {
                return id == other.id;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }
                if (ReferenceEquals(this, obj))
                {
                    return true;
                }
                if (obj.GetType() != this.GetType())
                {
                    return false;
                }
                return Equals((AssetInfo) obj);
            }

            public override int GetHashCode()
            {
                return id.GetHashCode();
            }

            public string AsChild()
            {
                var typeNameLabel = typeName;
                var count = buttonCount;
                if (count == 0)
                {
                    return $"{typeNameLabel}\t{path}";
                }
                return $"{typeNameLabel}\t{path}\tButtons({count})\t{string.Join(", ", buttons)}";
            }

            public string AsParent(int level)
            {
                var typeNameLabel = $"Parent-{level}";
                var count = buttonCount;
                if (count == 0)
                {
                    return $"{typeNameLabel}\t{path}";
                }
                return $"{typeNameLabel}\t{path}\tButtons({count})\t{string.Join(", ", buttons)}";
            }

            public override string ToString()
            {
                var count = buttonCount;
                if (count == 0)
                {
                    return $"{guid}\t{typeName}\t{path}";
                }
                return $"{guid}\t{typeName}\t{path}\tB#{count}:\t{string.Join(", ", buttons)}";
            }
        }
    }
}
#endif