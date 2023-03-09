using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Altzone.Scripts.Model.Poco;
using Prg.Scripts.Common.Unity;
using UnityEngine;
using UnityEngine.Assertions;
#if UNITY_WEBGL
using System.Runtime.InteropServices;
#endif

namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Entry point to local POCO models.
    /// </summary>
    /// <remarks>
    /// WebGl builds have to manually flush changes to browser local storage/database after changes to be on the safe side.
    /// </remarks>
    internal class LocalModels
    {
        private const string StorageFilename = "LocalModels.json";
        private const int StorageVersionNumber = 1;
        private const int WebGlFramesToWaitFlush = 30;
        private static readonly Encoding Encoding = new UTF8Encoding(false, false);

        public readonly string StoragePath;

        private readonly StorageData _storageData;

#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void HelloWebGl();

        [DllImport("__Internal")]
        private static extern void FsSyncFs();
#endif
        private static UnityMonoHelper _monoHelper;
        private static Coroutine _fsSync;
        private static int _framesToWait;

        internal LocalModels(string storagePath)
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
#if UNITY_WEBGL
            if (!AppPlatform.IsEditor)
            {
                HelloWebGl();
            }
            _monoHelper = UnityMonoHelper.Instance;
            _fsSync = null;
#endif
        }

        [Conditional("UNITY_WEBGL")]
        private static void WebGlFsSyncFs()
        {
            if (AppPlatform.IsEditor)
            {
                return;
            }
            _framesToWait = WebGlFramesToWaitFlush;
            if (_fsSync != null)
            {
                Debug.Log("FsSyncFs - SKIP");
                return;
            }
            Debug.Log("FsSyncFs - START");
            _fsSync = _monoHelper.StartCoroutine(FsSync());

            IEnumerator FsSync()
            {
                while (--_framesToWait > 0)
                {
                    yield return null;
                }
                _fsSync = null;
                Debug.Log("FsSyncFs - SYNC");
#if UNITY_WEBGL
                FsSyncFs();
#endif
            }
        }

        #region PlayerData

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
                    playerData.Id = id;
                }
                _storageData.PlayerData.Add(playerData);
            }
            Debug.Log($"playerData {playerData}");
            SaveStorage(_storageData, StoragePath);
            WebGlFsSyncFs();
            return playerData;
        }

        #endregion

        #region BattleCharacter

        internal BattleCharacter GetBattleCharacter(int customCharacterId)
        {
            return _GetBattleCharacter(customCharacterId);
        }

        internal List<BattleCharacter> GetAllBattleCharacters()
        {
            var battleCharacters = new List<BattleCharacter>();
            foreach (var customCharacter in _storageData.CustomCharacters)
            {
                battleCharacters.Add(_GetBattleCharacter(customCharacter.Id));
            }
            return battleCharacters;
        }

        private BattleCharacter _GetBattleCharacter(int customCharacterId)
        {
            var customCharacter = _storageData.CustomCharacters.FirstOrDefault(x => x.Id == customCharacterId);
            if (customCharacter == null)
            {
                throw new UnityException($"CustomCharacter not found for {customCharacterId}");
            }
            var characterClass = _storageData.CharacterClasses.FirstOrDefault(x => x.Id == customCharacter.CharacterClassId);
            if (characterClass == null)
            {
                // Create fake CharacterClass so we can return even character class has been deleted.
                characterClass = new CharacterClass(customCharacter.CharacterClassId, "deleted", (Defence)1, 1, 1, 1, 1);
            }
            return BattleCharacter.Create(customCharacter, characterClass);
        }

        #endregion

        #region CharacterClass

        public List<CharacterClass> GetAllCharacterClassModels()
        {
            return _storageData.CharacterClasses;
        }

        public List<CustomCharacter> GetAllCustomCharacterModels()
        {
            return _storageData.CustomCharacters;
        }
        

        #endregion
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