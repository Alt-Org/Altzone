using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Altzone.Scripts.Model;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts
{
    /// <summary>
    /// Factory class for our <c>DataStore</c> and support for internal game update/upgrade operations.
    /// </summary>
    public static class Storefront
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            // Manual reset if UNITY Domain Reloading is disabled.
            _instance = null;
            DataStoreImpl.CachedMethods = new Dictionary<string, MethodInfo>();
        }

        private const string StorageFilename = "LocalModels.json";

        private static DataStoreImpl _instance;

        /// <summary>
        /// Gets or creates an <c>DataStore</c> static singleton instance. 
        /// </summary>
        public static DataStore Get() => _instance ??= new DataStoreImpl(StorageFilename);

        #region Game update/upgrade operations - internal access only!

        internal static DataStore ResetStorage(int storageVersionNumber)
        {
            // Reset storage and create it again with new version number.
            _instance.ResetStorage();
            _instance = new DataStoreImpl(StorageFilename, storageVersionNumber);
            return _instance;
        }

        internal static void Set(List<CharacterClass> characterClasses, Action<bool> callback) => _instance.Set(characterClasses, callback);

        internal static void Set(List<CustomCharacter> customCharacters, Action<bool> callback) => _instance.Set(customCharacters, callback);

        internal static void Set(List<GameFurniture> gameFurniture, Action<bool> callback) => _instance.Set(gameFurniture, callback);

        #endregion
    }

    /// <summary>
    /// Public <c>DataStore</c> interface.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface DataStore
    {
        /// <summary>
        /// Gets <c>PlayerData</c> entity using its <c>uniqueIdentifier</c> property.
        /// </summary>
        /// <remarks>
        /// Note that <c>uniqueIdentifier</c> is not the same as ID but should be generated once for given device so that
        /// new player can be identified unambiguously when it is created for first time.
        /// </remarks>
        void GetPlayerData(string uniqueIdentifier, Action<PlayerData> callback);

        /// <summary>
        /// Saves <c>PlayerData</c> entity.
        /// </summary>
        /// <remarks>
        /// If <c>PlayerData</c> was created its ID will be updated in returned entity.
        /// </remarks>
        void SavePlayerData(PlayerData playerData, Action<PlayerData> callback);

        /// <summary>
        /// Gets <c>ClanData</c> entity using its ID property.
        /// </summary>
        void GetClanData(string id, Action<ClanData> callback);

        /// <summary>
        /// Saves <c>ClanData</c> entity.
        /// </summary>
        /// <remarks>
        /// If <c>ClanData</c> was created its ID will be updated in returned entity.
        /// </remarks>
        void SaveClanData(ClanData clanData, Action<ClanData> callback);

        /// <summary>
        /// Get all read-only <c>CharacterClass</c> entities.
        /// </summary>
        void GetAllCharacterClasses(Action<ReadOnlyCollection<CharacterClass>> callback);

        /// <summary>
        /// Get all read-only <c>GameFurniture</c> entities.
        /// </summary>
        void GetAllGameFurniture(Action<ReadOnlyCollection<GameFurniture>> callback);

        /// <summary>
        /// Get all read-only <c>GameFurniture</c> entities.
        /// </summary>
        /// <returns><c>CustomYieldInstruction</c> that can be 'waited' in UNITY CoRoutine using <code>yield return</code></returns>
        CustomYieldInstruction GetAllGameFurnitureYield(Action<ReadOnlyCollection<GameFurniture>> callback);

        /// <summary>
        /// Gets <c>IDataStoreVersion</c> interface for game update/upgrade purposes.
        /// </summary>
        IDataStoreVersion Version { get; }

        /// <summary>
        /// Gets <c>ITestDataStore</c> helper interface for <c>DataStore</c> testing purposes.
        /// </summary>
        ITestDataStore ForTest { get; }
    }

    /// <summary>
    /// <c>DataStore</c> version info for detecting game updates/upgrades
    /// when data has been changed externally by game designers in the fame itself or in external database.
    /// </summary>
    public interface IDataStoreVersion
    {
        int VersionNumber { get; }

        int CharacterClassesVersion { get; set; }

        int CustomCharactersVersion { get; set; }

        int GameFurnitureVersion { get; set; }

        int PlayerDataVersion { get; set; }

        int ClanDataVersion { get; set; }
    }

    /// <summary>
    /// <c>DataStore</c> test interface, should be deleted when not needed.
    /// </summary>
    public interface ITestDataStore
    {
        void GetBattleCharacterTest(int customCharacterId, Action<BattleCharacter> callback);

        void GetAllBattleCharactersTest(Action<List<BattleCharacter>> callback);

        void GetAllCustomCharactersTest(Action<List<CustomCharacter>> callback);
    }

    /// <summary>
    /// <c>DataStore</c> implementation for the game data.<br />
    /// Data can be local, in our own hosted server or in some cloud based service.
    /// </summary>
    public class DataStoreImpl : DataStore, IDataStoreVersion, ITestDataStore
    {
        public static Dictionary<string, MethodInfo> CachedMethods = new();

        private readonly LocalModels _localModels;

        public DataStoreImpl(string storageFilename, int storageVersionNumber = 0)
        {
            _localModels = new LocalModels(storageFilename, storageVersionNumber);
        }

        internal void ResetStorage()
        {
            _localModels.ResetDataForReload();
        }

        #region Public API

        // PLayer

        public void GetPlayerData(string uniqueIdentifier, Action<PlayerData> callback) => _localModels.GetPlayerData(uniqueIdentifier, callback);

        public void SavePlayerData(PlayerData playerData, Action<PlayerData> callback) => _localModels.SavePlayerData(playerData, callback);

        // Clan

        public void GetClanData(string id, Action<ClanData> callback) => _localModels.GetClanData(id, callback);

        public void SaveClanData(ClanData clanData, Action<ClanData> callback) => _localModels.SaveClanData(clanData, callback);

        // Game static data

        public void GetAllCharacterClasses(Action<ReadOnlyCollection<CharacterClass>> callback) => _localModels.GetAllCharacterClassModels(callback);

        public void GetAllGameFurniture(Action<ReadOnlyCollection<GameFurniture>> callback) => _localModels.GetAllGameFurniture(callback);

        public CustomYieldInstruction GetAllGameFurnitureYield(Action<ReadOnlyCollection<GameFurniture>> callback) =>
            new CallbackYieldInstruction<ReadOnlyCollection<GameFurniture>>(this, nameof(GetAllGameFurniture), callback);

        #endregion

        #region Test API

        public ITestDataStore ForTest => this;

        public void GetBattleCharacterTest(int customCharacterId, Action<BattleCharacter> callback) =>
            _localModels.GetBattleCharacterTest(customCharacterId, callback);

        public void GetAllBattleCharactersTest(Action<List<BattleCharacter>> callback) => _localModels.GetAllBattleCharactersTest(callback);

        public void GetAllCustomCharactersTest(Action<List<CustomCharacter>> callback) => _localModels.GetAllCustomCharacterModelsTest(callback);

        #endregion

        #region Version info API

        public IDataStoreVersion Version => this;

        public int VersionNumber => _localModels.VersionNumber;

        public int CharacterClassesVersion
        {
            get => _localModels.CharacterClassesVersion;
            set => _localModels.CharacterClassesVersion = value;
        }

        public int CustomCharactersVersion
        {
            get => _localModels.CustomCharactersVersion;
            set => _localModels.CustomCharactersVersion = value;
        }

        public int GameFurnitureVersion
        {
            get => _localModels.GameFurnitureVersion;
            set => _localModels.GameFurnitureVersion = value;
        }

        public int PlayerDataVersion
        {
            get => _localModels.PlayerDataVersion;
            set => _localModels.PlayerDataVersion = value;
        }

        public int ClanDataVersion
        {
            get => _localModels.ClanDataVersion;
            set => _localModels.ClanDataVersion = value;
        }

        #endregion

        #region Internal API

        internal void Set(List<CharacterClass> characterClasses, Action<bool> callback) => _localModels.Set(characterClasses, callback);

        internal void Set(List<CustomCharacter> customCharacters, Action<bool> callback) => _localModels.Set(customCharacters, callback);

        internal void Set(List<GameFurniture> gameFurniture, Action<bool> callback) => _localModels.Set(gameFurniture, callback);

        #endregion

        #region CustomYieldInstruction support

        /// <summary>
        /// <c>CustomYieldInstruction</c> to invoke a method with callback in a UNITY <c>CoRoutine</c> using <code>yield return</code>.
        /// </summary>
        private class CallbackYieldInstruction<T> : CustomYieldInstruction
        {
            public override bool keepWaiting => _keepWaiting;

            private bool _keepWaiting = true;

            public CallbackYieldInstruction(object instance, string methodName, Action<T> callback)
            {
                void SafeCallbackWrapper(T result)
                {
                    try
                    {
                        callback(result);
                    }
                    finally
                    {
                        _keepWaiting = false;
                    }
                }

                if (!CachedMethods.TryGetValue(methodName, out var method))
                {
                    method = instance.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                    CachedMethods.Add(methodName, method);
                }
                if (method == null)
                {
                    _keepWaiting = false;
                    throw new UnityException($"public instance method {methodName} not found");
                }
                method.Invoke(instance, new object[] { (Action<T>)SafeCallbackWrapper });
            }
        }

        #endregion
    }
}