using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.ModelV2.Internal;
using Altzone.Scripts.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Prg.Scripts.Common.Unity;
using UnityEngine;
#if UNITY_WEBGL
using System.Runtime.InteropServices;
#endif
using Debug = UnityEngine.Debug;

namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Entry point to local POCO models JSON storage implementation with 'built-in' WebGL support.
    /// </summary>
    /// <remarks>
    /// WebGl builds have to manually flush changes to browser local storage/database after changes to be on the safe side.
    /// </remarks>
    ///

    internal class LocalModels
    {
        const int STORAGEVERSION = 7;

        private const int WebGlFramesToWaitFlush = 10;
        private static readonly Encoding Encoding = new UTF8Encoding(false, false);

        private readonly string _storagePath;
        private StorageData _storageData;

        private static List<BaseCharacter> _characters;

        private bool _saving = false;

        #region WebGL support

#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void HelloWebGl();

        [DllImport("__Internal")]
        private static extern void FsSyncFs();

        private static void CallHelloWebGl()
        {
            Debug.Log("Call Javascript Library");
            try
            {
                HelloWebGl();
            }
            catch (Exception e)
            {
                Debug.Log($"Exception {e.GetType().FullName} {e.Message}");
            }
        }

        private static void CallFsSyncFs()
        {
            Debug.Log("Call Javascript Library");
            try
            {
                FsSyncFs();
            }
            catch (Exception e)
            {
                Debug.Log($"Exception {e.GetType().FullName} {e.Message}");
            }
        }

        private void InitFsSyncFs()
        {
            if (!AppPlatform.IsEditor)
            {
                // Javascript call.
                CallHelloWebGl();
            }
            _monoHelper = UnityMonoHelper.Instance;
            _fsSync = null;
        }
#endif
        private static UnityMonoHelper _monoHelper;
        private static Coroutine _fsSync;
        private static int _framesToWait;

        #endregion

        internal LocalModels(string storageFilename)
        {
#if UNITY_WEBGL
            InitFsSyncFs();
#endif
            // Files can only be in Application.persistentDataPath for WebGL compatibility! 
            _storagePath = Path.Combine(Application.persistentDataPath, storageFilename);
            if (AppPlatform.IsWindows)
            {
                _storagePath = AppPlatform.ConvertToWindowsPath(_storagePath);
            }
            Debug.Log($"StorageFilename {_storagePath}");
            _characters = CharacterStorage.Instance.CharacterList;
            _storageData = File.Exists(_storagePath)
                ?  LoadStorage(_storagePath)
                : CreateDefaultStorage(_storagePath);
        }

        #region WebGL support for file system level sync aka flush data.

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
                CallFsSyncFs();
#endif
            }
        }

        #endregion

        #region PlayerData

        internal void GetPlayerData(string uniqueIdentifier, Action<PlayerData> callback)
        {
            new WaitUntil(() => _saving = false);
            var playerData = _storageData.PlayerData.FirstOrDefault(x => x.UniqueIdentifier == uniqueIdentifier);
            Debug.Log($"playerData {playerData}");
            callback(playerData);
        }

        internal void SavePlayerData(PlayerData playerData, Action<PlayerData> callback)
        {
            _saving = true;
            var index = _storageData.PlayerData.FindIndex(x => x.Id == playerData.Id);
            if (index >= 0 && _storageData.PlayerData.Count > index)
            {
                _storageData.PlayerData[index] = playerData;
                
            }
            else
            {
                if (string.IsNullOrEmpty(playerData.Id))
                {
                    playerData.Id = CreateDefaultModels.FakeMongoDbId();
                }
                _storageData.PlayerData.Add(playerData);
            }
            Debug.Log($"playerData {playerData}");
            SaveStorage(_storageData, _storagePath);
            _saving = false;
            callback?.Invoke(playerData);
        }

        internal void DeletePlayerData(PlayerData playerData, Action<bool> callback)
        {
            var index = _storageData.PlayerData.FindIndex(x => x.Id == playerData.Id);
            if (index >= 0)
            {
                _storageData.PlayerData.Remove(_storageData.PlayerData[index]);
                SaveStorage(_storageData, _storagePath);
                callback?.Invoke(true);
            }
            else
            {
                callback?.Invoke(false);
            }
        }

        #endregion

        #region ClanData

        internal void GetClanData(string id, Action<ClanData> callback)
        {
            callback(_storageData.ClanData.FirstOrDefault(x => x.Id == id));
        }

        internal void SaveClanData(ClanData clanData, Action<ClanData> callback)
        {
            _saving = true;
            var index = _storageData.ClanData.FindIndex(x => x.Id == clanData.Id);
            if (index >= 0)
            {
                _storageData.ClanData[index] = clanData;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(clanData.Id))
                {
                    var id = _storageData.ClanData.Count + 1001;
                    clanData.Id = id.ToString();
                }
                _storageData.ClanData.Add(clanData);
            }
            Debug.Log($"clanData {clanData}");
            SaveStorage(_storageData, _storagePath);
            _saving = false;
            clanData.CallDataUpdate();
            callback?.Invoke(clanData);
        }

        #endregion

        #region Temporary Test API

        internal void GetAllCustomCharacterModelsTest(Action<List<CustomCharacter>> callback)
        {
            callback(_storageData.CustomCharacters);
        }

        internal void GetBattleCharacterTest(CharacterID customCharacterId, Action<BattleCharacter> callback)
        {
            callback(_GetBattleCharacter(customCharacterId));
        }

        internal void GetAllBattleCharactersTest(Action<List<BattleCharacter>> callback)
        {
            callback(_GetAllBattleCharacters());
        }

        private List<BattleCharacter> _GetAllBattleCharacters()
        {
            var battleCharacters = new List<BattleCharacter>();
            foreach (var customCharacter in _storageData.CustomCharacters)
            {
                battleCharacters.Add(_GetBattleCharacter(customCharacter.Id));
            }
            return battleCharacters;
        }

        private BattleCharacter _GetBattleCharacter(CharacterID customCharacterId)
        {
            var customCharacter = _storageData.CustomCharacters.FirstOrDefault(x => x.Id == customCharacterId);
            if (customCharacter == null)
            {
                throw new UnityException($"CustomCharacter not found for {customCharacterId}");
            }
            var characterClass =
                _storageData.CharacterClasses.FirstOrDefault(x => x.Type == customCharacter.CharacterClassType);
            if (characterClass == null)
            {
                // Create fake CharacterClass so we can return 'valid' object even character class has been deleted.
                characterClass = CharacterClass.CreateDummyFor(customCharacter.CharacterClassType);
            }
            return BattleCharacter.Create(customCharacter, characterClass);
        }

        #endregion

        #region Game non-mutable internal static data created by game designers

        internal void GetAllCharacterClassModels(Action<ReadOnlyCollection<CharacterClass>> callback)
        {
            callback(new ReadOnlyCollection<CharacterClass>(_storageData.CharacterClasses));
        }

        internal void GetAllGameFurniture(Action<ReadOnlyCollection<GameFurniture>> callback)
        {
            callback(new ReadOnlyCollection<GameFurniture>(_storageData.GameFurniture));
        }

        internal void GetAllBaseCharacters(Action<ReadOnlyCollection<BaseCharacter>> callback)
        {
            callback(new ReadOnlyCollection<BaseCharacter>(_storageData.Characters));
        }

        internal void GetAllDefaultCharacters(Action<ReadOnlyCollection<CustomCharacter>> callback)
        {
            callback(new ReadOnlyCollection<CustomCharacter>(_storageData.CustomCharacters));
        }

        internal void GetPlayerTasks(Action<ClanTasks> callback)
        {
            callback(_storageData.PlayerTasks);
        }

        internal void SavePlayerTasks(ClanTasks tasks, Action<ClanTasks> callback)
        {
            _storageData.PlayerTasks = tasks;
            callback(_storageData.PlayerTasks);
        }

        #endregion

        #region Setters for bulk data updates for base models.

        internal void Set(List<CharacterClass> characterClasses, Action<bool> callback)
        {
            _storageData.CharacterClasses = characterClasses;
            SaveStorage(_storageData, _storagePath);
            callback?.Invoke(true);
        }

        internal void Set(List<CustomCharacter> customCharacters, Action<bool> callback)
        {
            _storageData.CustomCharacters = customCharacters;
            SaveStorage(_storageData, _storagePath);
            callback?.Invoke(true);
        }

        internal void Set(List<GameFurniture> gameFurniture, Action<bool> callback)
        {
            _storageData.GameFurniture = gameFurniture;
            SaveStorage(_storageData, _storagePath);
            callback?.Invoke(true);
        }

        #endregion

        private static StorageData CreateDefaultStorage(string storagePath)
        {
            Debug.LogWarning("Creating new Default Storage.");
            var storageData = new StorageData();

            storageData.SetCharacters(CharacterStorage.Instance.CharacterList);
            //storageData.CharacterClasses.AddRange(CreateDefaultModels.CreateCharacterClasses());
            storageData.CustomCharacters.AddRange(CreateDefaultModels.CreateCustomCharacters(storageData.Characters));

            var playerGuid = new PlayerSettings().PlayerGuid;
            var clanGuid = playerGuid;
            var customCharacterId = storageData.CustomCharacters[0].Id;
            storageData.PlayerData.Add(CreateDefaultModels.CreatePlayerData(playerGuid, clanGuid, (int)customCharacterId));
            storageData.ClanData.Add(CreateDefaultModels.CreateClanData(clanGuid, storageData.GameFurniture));

            storageData.StorageVersion = STORAGEVERSION;

            SaveStorage(storageData, storagePath);
            return storageData;
        }

        private static StorageData LoadStorage(string storagePath)
        {
            var jsonText = File.ReadAllText(storagePath, Encoding);
            StorageData storageData;
            StorageSaveData loadedData;
            JObject parsedData = JObject.Parse(jsonText);
            Debug.LogWarning(parsedData);
            if (int.Parse(parsedData["StorageVersion"].ToString()) != STORAGEVERSION) storageData = CreateDefaultStorage(storagePath);
            else
            {
                loadedData = parsedData.ToObject<StorageSaveData>(
                    JsonSerializer.Create(new JsonSerializerSettings {
                        TypeNameHandling = TypeNameHandling.Auto,
                        NullValueHandling = NullValueHandling.Ignore,
                    }));
                storageData = new(loadedData);

                storageData.SetCharacters(CharacterStorage.Instance.CharacterList);

                // Loading custom characters
                storageData.CustomCharacters = new();
                storageData.CustomCharacters.AddRange(CreateDefaultModels.CreateCustomCharacters(storageData.Characters));

                storageData.GameFurniture = new();
                //storageData.GameFurniture.AddRange(CreateDefaultModels.CreateGameFurniture());
                storageData.PlayerTasks = null;
            }

            return storageData;
        }

        private static void SaveStorage(StorageData storageData, string storagePath)
        {
            string jsonText = JObject.FromObject(
                    new StorageSaveData(storageData),
                    JsonSerializer.CreateDefault(new JsonSerializerSettings {
                        NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                        TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                        Formatting = Newtonsoft.Json.Formatting.Indented,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    })
                ).ToString();
            //Debug.LogWarning(jsonText);
            File.WriteAllText(storagePath, jsonText, Encoding);
            WebGlFsSyncFs();
        }

        public void SetFurniture(List<GameFurniture> gameFurnitures, Action<bool> callback)
        {
            _storageData.GameFurniture = gameFurnitures;
            callback?.Invoke(true);
        }
    }

    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    internal class StorageData
    {
        public int StorageVersion = 0;
        private List<BaseCharacter> _characters = new();
        public List<CharacterClass> CharacterClasses = new();
        public List<CustomCharacter> CustomCharacters = new();
        public List<GameFurniture> GameFurniture = new();
        public List<PlayerData> PlayerData = new();
        public List<ClanData> ClanData = new();
        public ClanTasks PlayerTasks= null;

        public List<BaseCharacter> Characters { get => _characters; }

        public void SetCharacters(List<BaseCharacter> characters)
        {
            _characters = characters;
        }
        
        internal StorageData()
        {
            StorageVersion = 0;
            CustomCharacters = new();
            PlayerData = new();
            ClanData =  new();
            PlayerTasks = null;
        }

        internal StorageData(StorageSaveData data)
        {
            StorageVersion = data.StorageVersion;
            CustomCharacters = data.CustomCharacters;
            PlayerData = data.PlayerData;
            ClanData = data.ClanData;
            PlayerTasks = data.PlayerTasks;
        }
    }

    internal class StorageSaveData
    {
        public int StorageVersion = 0;
        public List<CustomCharacter> CustomCharacters = new();
        public List<PlayerData> PlayerData = new();
        public List<ClanData> ClanData = new();
        public ClanTasks PlayerTasks = null;

        [JsonConstructor]
        private StorageSaveData(){ }

        internal StorageSaveData(StorageData data)
        {
            StorageVersion = data.StorageVersion;
            CustomCharacters = data.CustomCharacters;
            PlayerData = data.PlayerData;
            ClanData = data.ClanData;
            PlayerTasks = data.PlayerTasks;
        }
    }
}
