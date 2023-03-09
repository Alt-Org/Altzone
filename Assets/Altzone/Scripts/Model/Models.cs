using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
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
        private const int StorageVersionNumber = 1;
        private static readonly Encoding Encoding = new UTF8Encoding(false, false);

        public readonly string StoragePath;

        private readonly StorageData _storageData;

        internal Models(string storagePath)
        {
            StoragePath = Path.Combine(storagePath, StorageFilename);
            if (AppPlatform.IsWindows)
            {
                StoragePath = AppPlatform.ConvertToWindowsPath(StoragePath);
            }
            var exists = File.Exists(StoragePath);
            Debug.Log($"StoragePath {StoragePath} exists {exists}");
            _storageData = exists
                ? LoadStorage(StoragePath)
                : CreateDefaultStorage(StoragePath);
            Debug.Log($"CharacterClasses {_storageData.CharacterClasses.Count}");
            Debug.Log($"CustomCharacters {_storageData.CustomCharacters.Count}");
            Debug.Log($"PlayerData {_storageData.PlayerData.Count}");
            Assert.IsTrue(_storageData.CharacterClasses.Count > 0);
            Assert.IsTrue(_storageData.CustomCharacters.Count > 0);
            // Player data validity can not be detected here!
        }

        internal PlayerData GetPlayerData(string uniqueIdentifier)
        {
            return _storageData.PlayerData.FirstOrDefault(x => x.UniqueIdentifier == uniqueIdentifier);
        }
        internal PlayerData SavePlayerData(PlayerData playerData)
        {
            var index = _storageData.PlayerData.FindIndex(x => x.Id == playerData.Id);
            if (index >= 0)
            {
                _storageData.PlayerData[index] = playerData;
            }
            else
            {
                if (playerData.Id == 0)
                {
                    var id = _storageData.PlayerData.Count == 0
                        ? 1
                        : _storageData.PlayerData.Max(x => x.Id) + 1;
                }
                _storageData.PlayerData.Add(playerData);
            }
            SaveStorage(_storageData, StoragePath);
            return playerData;
        }
        
        private static StorageData CreateDefaultStorage(string storagePath)
        {
            var storageData = new StorageData
            {
                VersionNumber = StorageVersionNumber
            };
            storageData.CharacterClasses.AddRange(CreateDefaultModels.CreateCharacterClasses());
            storageData.CustomCharacters.AddRange(CreateDefaultModels.CreateCustomCharacters());
            // Player data should not be created automatically!
            Assert.IsTrue(storageData.PlayerData.Count == 0, "do not create PlayerData here");
            SaveStorage(storageData, storagePath);
            return storageData;
        }

        private static StorageData LoadStorage(string storagePath)
        {
            var jsonData = File.ReadAllText(storagePath, Encoding);
            var storageData = JsonUtility.FromJson<StorageData>(jsonData);
            Assert.AreEqual(StorageVersionNumber, storageData.VersionNumber);
            return storageData;
        }

        private static void SaveStorage(StorageData storageData, string storagePath)
        {
            var json = JsonUtility.ToJson(storageData);
            File.WriteAllText(storagePath, json, Encoding);
        }
    }

    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    internal class StorageData
    {
        public int VersionNumber;
        public List<CharacterClass> CharacterClasses = new();
        public List<CustomCharacter> CustomCharacters = new();
        public List<PlayerData> PlayerData = new();
    }
}