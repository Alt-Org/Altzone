using System.Collections.Generic;
using Altzone.Scripts.Model;
using Altzone.Scripts.Model.Poco;
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

        private static DataStore _instance;

        /// <summary>
        /// Gets or creates an <c>DataStore</c> static singleton instance. 
        /// </summary>
        public static DataStore Get() => _instance ??= new DataStore();
    }

    /// <summary>
    /// General Data Store for game data.<br />
    /// Data can be local, in our own hosted server or in some cloud based service.
    /// </summary>
    public class DataStore
    {
        private readonly LocalModels _localModels = new(Application.persistentDataPath);

        public PlayerData GetPlayerData(string uniqueIdentifier) => _localModels.GetPlayerData(uniqueIdentifier);

        public PlayerData SavePlayerData(PlayerData playerData) => _localModels.SavePlayerData(playerData);

        public BattleCharacter GetBattleCharacter(int customCharacterId) => _localModels.GetBattleCharacter(customCharacterId);

        public List<BattleCharacter> GetAllBattleCharacters() => _localModels.GetAllBattleCharacters();

        public List<CharacterClass> GetAllCharacterClasses() => _localModels.GetAllCharacterClassModels();

        public List<CustomCharacter> GetAllCustomCharacters() => _localModels.GetAllCustomCharacterModels();
    }
}