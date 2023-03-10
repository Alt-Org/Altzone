using System;
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
    /// Entry point to local POCO models with 'built-in' WebGL support.
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

        private readonly string _storagePath;
        private readonly StorageData _storageData;

        public int CharacterClassesVersion
        {
            get => _storageData.CharacterClassesVersion;
            set
            {
                _storageData.CharacterClassesVersion = value;
                SaveStorage(_storageData, _storagePath);
            }
        }

        public int CustomCharactersVersion
        {
            get => _storageData.CustomCharactersVersion;
            set
            {
                _storageData.CustomCharactersVersion = value;
                SaveStorage(_storageData, _storagePath);
            }
        }

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
            _storagePath = Path.Combine(storagePath, StorageFilename);
            if (AppPlatform.IsWindows)
            {
                _storagePath = AppPlatform.ConvertToWindowsPath(_storagePath);
            }
            var exists = File.Exists(_storagePath);
            Debug.Log($"StoragePath {_storagePath} exists {exists}");
            _storageData = exists
                ? LoadStorage(_storagePath)
                : CreateDefaultStorage(_storagePath);
            Debug.Log($"CharacterClasses {_storageData.CharacterClasses.Count}");
            Debug.Log($"CustomCharacters {_storageData.CustomCharacters.Count}");
            Debug.Log($"PlayerData {_storageData.PlayerData.Count}");
            Assert.IsTrue(_storageData.CharacterClasses.Count > 0);
            Assert.IsTrue(_storageData.CustomCharacters.Count > 0);
            // Player data validity can not be detected here!
#if UNITY_WEBGL
            if (!AppPlatform.IsEditor)
            {
                // Javascript call.
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
                // Javascript call.
                FsSyncFs();
#endif
            }
        }

        #region PlayerData

        internal void GetPlayerData(string uniqueIdentifier, Action<PlayerData> callback)
        {
            callback(_storageData.PlayerData.FirstOrDefault(x => x.UniqueIdentifier == uniqueIdentifier));
        }

        internal void SavePlayerData(PlayerData playerData, Action<PlayerData> callback)
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
            SaveStorage(_storageData, _storagePath);
            callback?.Invoke(playerData);
        }

        #endregion

        #region BattleCharacter

        internal void GetBattleCharacter(int customCharacterId, Action<BattleCharacter> callback)
        {
            callback(_GetBattleCharacter(customCharacterId));
        }

        internal void GetAllBattleCharacters(Action<List<BattleCharacter>> callback)
        {
            var battleCharacters = new List<BattleCharacter>();
            foreach (var customCharacter in _storageData.CustomCharacters)
            {
                battleCharacters.Add(_GetBattleCharacter(customCharacter.Id));
            }
            callback(battleCharacters);
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

        public void GetAllCharacterClassModels(Action<List<CharacterClass>> callback)
        {
            callback(_storageData.CharacterClasses);
        }

        public void GetAllCustomCharacterModels(Action<List<CustomCharacter>> callback)
        {
            callback(_storageData.CustomCharacters);
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
            WebGlFsSyncFs();
        }
    }

    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    internal class StorageData
    {
        public int VersionNumber = 1;
        public int CharacterClassesVersion = 1;
        public int CustomCharactersVersion = 1;
        public List<CharacterClass> CharacterClasses = new();
        public List<CustomCharacter> CustomCharacters = new();
        public List<PlayerData> PlayerData = new();
    }
}