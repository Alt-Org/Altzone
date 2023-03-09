using System.Collections.Generic;
using System.IO;
using System.Text;
using Altzone.Scripts.Model.Poco;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Entry point to local POCO models.
    /// </summary>
    internal class Models
    {
        private const string StorageFilename = "LocalModels.json";
        private const int StorageVersionNUmber = 1;
        private static readonly Encoding Encoding = new UTF8Encoding(false, false);

        public readonly string StoragePath;

        private readonly StorageData StorageData;

        internal Models(string storagePath)
        {
            StoragePath = Path.Combine(storagePath, StorageFilename);
            if (AppPlatform.IsWindows)
            {
                StoragePath = AppPlatform.ConvertToWindowsPath(StoragePath);
            }
            Debug.Log($"StoragePath {StoragePath}");
            StorageData = File.Exists(StoragePath)
                ? LoadStorage(StoragePath)
                : CreateDefaultStorage(StoragePath);
            Debug.Log($"CharacterClasses {StorageData.CharacterClasses.Count}");
            Debug.Log($"CustomCharacters {StorageData.CustomCharacters.Count}");
            Debug.Log($"PlayerData {StorageData.PlayerData.Count}");
            Assert.IsTrue(StorageData.CharacterClasses.Count > 0);
            Assert.IsTrue(StorageData.CustomCharacters.Count > 0);
            Assert.IsTrue(StorageData.PlayerData.Count > 0);
        }

        private static StorageData CreateDefaultStorage(string storagePath)
        {
            var storageData = new StorageData
            {
                VersionNUmber = StorageVersionNUmber
            };
            storageData.CharacterClasses.AddRange(CreateDefaultModels.CreateCharacterClasses());
            storageData.CustomCharacters.AddRange(CreateDefaultModels.CreateCustomCharacters());
            storageData.PlayerData.AddRange(CreateDefaultModels.CreatePlayerData());
            SaveStorage(storageData, storagePath);
            return storageData;
        }

        private static StorageData LoadStorage(string storagePath)
        {
            var jsonData = File.ReadAllText(storagePath, Encoding);
            var storageData = JsonUtility.FromJson<StorageData>(jsonData);
            Assert.AreEqual(StorageVersionNUmber, storageData.VersionNUmber);
            return storageData;
        }

        private static void SaveStorage(StorageData storageData, string storagePath)
        {
            var json = JsonUtility.ToJson(storageData);
            File.WriteAllText(storagePath, json, Encoding);
        }
    }

    internal class StorageData
    {
        public int VersionNUmber;
        public List<CharacterClass> CharacterClasses = new();
        public List<CustomCharacter> CustomCharacters = new();
        public List<PlayerData> PlayerData = new();
    }
}