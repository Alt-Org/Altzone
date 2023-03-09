using System.Collections.Generic;
using Altzone.Scripts.Model;
using Altzone.Scripts.Model.Poco;
using UnityEngine;

namespace Altzone.Scripts
{
    /// <summary>
    /// Factory class for <c>IStorefront</c>.
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
        /// Gets or creates an <c>IStorefront</c> static instance. 
        /// </summary>
        public static DataStore Get() => _instance ??= new DataStore();
    }

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