using System;
using System.Collections.Generic;
using Altzone.Scripts.Model;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

namespace Altzone.Scripts
{
    /// <summary>
    /// Factory class for our <c>DataStore</c> implementation.
    /// </summary>
    public static class Storefront
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            // Manual reset if UNITY Domain Reloading is disabled.
            _instance = null;
        }

        private const string StorageFilename = "LocalModels.json";

        private static DataStore _instance;

        /// <summary>
        /// Gets or creates an <c>DataStore</c> static singleton instance. 
        /// </summary>
        public static DataStore Get() => _instance ??= new DataStore(StorageFilename);
    }

    /// <summary>
    /// General Data Store for game data.<br />
    /// Data can be local, in our own hosted server or in some cloud based service.
    /// </summary>
    public class DataStore
    {
        private readonly LocalModels _localModels;

        public DataStore(string storageFilename)
        {
            _localModels = new LocalModels(storageFilename);
        }

        #region Public API

        // PLayer

        public void GetPlayerData(string uniqueIdentifier, Action<PlayerData> callback) => _localModels.GetPlayerData(uniqueIdentifier, callback);

        public void SavePlayerData(PlayerData playerData, Action<PlayerData> callback) => _localModels.SavePlayerData(playerData, callback);

        // Clan

        public void GetClanData(int id, Action<ClanData> callback) => _localModels.GetClanData(id, callback);

        public void SaveClanData(ClanData clanData, Action<ClanData> callback) => _localModels.SaveClanData(clanData, callback);

        // Game

        public void GetBattleCharacter(int customCharacterId, Action<BattleCharacter> callback) =>
            _localModels.GetBattleCharacter(customCharacterId, callback);

        public void GetAllBattleCharacters(Action<List<BattleCharacter>> callback) => _localModels.GetAllBattleCharacters(callback);

        public void GetAllCharacterClasses(Action<List<CharacterClass>> callback) => _localModels.GetAllCharacterClassModels(callback);

        public void GetAllCustomCharacters(Action<List<CustomCharacter>> callback) => _localModels.GetAllCustomCharacterModels(callback);

        public void GetAllGameFurniture(Action<List<GameFurniture>> callback) => _localModels.GetAllGameFurniture(callback);

        #endregion

        #region Internal API

        internal int CharacterClassesVersion
        {
            get => _localModels.CharacterClassesVersion;
            set => _localModels.CharacterClassesVersion = value;
        }

        internal int CustomCharactersVersion
        {
            get => _localModels.CustomCharactersVersion;
            set => _localModels.CustomCharactersVersion = value;
        }

        internal int GameFurnitureVersion
        {
            get => _localModels.GameFurnitureVersion;
            set => _localModels.GameFurnitureVersion = value;
        }

        internal int PlayerDataVersion
        {
            get => _localModels.PlayerDataVersion;
            set => _localModels.PlayerDataVersion = value;
        }

        internal int ClanDataVersion
        {
            get => _localModels.ClanDataVersion;
            set => _localModels.ClanDataVersion = value;
        }

        internal void Set(List<CharacterClass> characterClasses, Action<bool> callback) => _localModels.Set(characterClasses, callback);

        internal void Set(List<CustomCharacter> customCharacters, Action<bool> callback) => _localModels.Set(customCharacters, callback);

        internal void Set(List<GameFurniture> gameFurniture, Action<bool> callback) => _localModels.Set(gameFurniture, callback);

        #endregion
    }
}