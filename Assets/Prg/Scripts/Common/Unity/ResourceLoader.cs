using UnityEngine;

namespace Prg.Scripts.Common.Unity
{
    /// <summary>
    /// Custom resource loader for ScriptableObject resources with some rules to narrow or widen UNITY default resource search rules.
    /// </summary>
    public abstract class ResourceLoader
    {
        public static ResourceLoader Get(string assetFolder = null, bool isSearchable = false)
        {
            return new ResourceLoader1(assetFolder, isSearchable);
        }

        public static ResourceLoader Get(string primaryConfigFolder, string developmentConfigFolder)
        {
            return new ResourceLoader2(primaryConfigFolder, developmentConfigFolder);
        }

        public abstract T LoadAsset<T>(string assetName) where T : ScriptableObject;

        private class ResourceLoader1 : ResourceLoader
        {
            public ResourceLoader1(string assetFolder, bool isSearchable)
            {
                this.assetFolder = assetFolder;
                this.isSearchable = isSearchable;
            }

            private readonly string assetFolder;
            private readonly bool isSearchable;

            public override T LoadAsset<T>(string assetName)
            {
                var hasAssetFolder = !string.IsNullOrEmpty(assetFolder);
                var path = hasAssetFolder ? $"{assetFolder}/{assetName}" : assetName;
                var asset = Resources.Load<T>(path);
                if (asset == null && hasAssetFolder && isSearchable)
                {
                    // try using default search rules
                    asset = Resources.Load<T>(assetName);
                }
                if (asset == null)
                {
                    Debug.LogWarning(hasAssetFolder
                        ? $"Asset '{assetName}' not found in {assetFolder} or anywhere else, returning new empty asset"
                        : $"Asset '{assetName}' not found, returning new empty asset");
                    asset = ScriptableObject.CreateInstance<T>();
                }
                return asset;
            }
        }

        private class ResourceLoader2 : ResourceLoader
        {
            private readonly string primaryConfigFolder;
            private readonly string developmentConfigFolder;

            public ResourceLoader2(string primaryConfigFolder, string developmentConfigFolder)
            {
                this.primaryConfigFolder = primaryConfigFolder;
                this.developmentConfigFolder = developmentConfigFolder;
            }

            public override T LoadAsset<T>(string assetName)
            {
                var path = string.IsNullOrWhiteSpace(developmentConfigFolder) ? assetName : $"{developmentConfigFolder}/{assetName}";
                var asset = Resources.Load<T>(path);
                if (asset != null)
                {
                    return asset;
                }
                path = path = string.IsNullOrWhiteSpace(primaryConfigFolder) ? assetName : $"{primaryConfigFolder}/{assetName}";
                asset = Resources.Load<T>(path);
                if (asset != null)
                {
                    return asset;
                }
                Debug.LogWarning($"Asset '{assetName}' not found in '{developmentConfigFolder}' or '{primaryConfigFolder}', returning new empty asset");
                return ScriptableObject.CreateInstance<T>();
            }
        }
    }
}