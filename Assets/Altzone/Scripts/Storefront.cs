using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Model;
using Altzone.Scripts.Model.Poco;
using Altzone.Scripts.Temp;
using UnityEngine;
using UnityEngine.Assertions;
using Models = Altzone.Scripts.Temp.Models;

namespace Altzone.Scripts
{
    /// <summary>
    /// Hardcoded files names for local storage.
    /// </summary>
    public static class GameFiles
    {
        public const string ClanGameRoomModelsFilename = "GameClanGameRoomModels.json";
        public const string ClanInventoryItemsFilename = "GameClanInventoryItems.json";
        public const string PlayerCustomCharacterModelsFilename = "GamePlayerCustomCharacterModels.json";
    }

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
        private readonly Altzone.Scripts.Model.Models _models = new(Application.persistentDataPath);

        public DataStore()
        {
            Models.Load();
            CustomCharacterModels.Load();
        }

        public PlayerData GetPlayerData(string uniqueIdentifier) => _models.GetPlayerData(uniqueIdentifier);

        public PlayerData SavePlayerData(PlayerData playerData) => _models.SavePlayerData(playerData);

        public IBattleCharacter GetBattleCharacter(int customCharacterId)
        {
            return BattleCharacter.GetBattleCharacter(customCharacterId);
        }

        public List<IBattleCharacter> GetAllBattleCharacters()
        {
            return BattleCharacter.GetAllBattleCharacters();
        }

        public List<ICharacterClassModel> GetAllCharacterClassModels()
        {
            return Models.GetAll<CharacterClassModel>().Cast<ICharacterClassModel>().ToList();
        }

        public List<ICustomCharacterModel> GetAllCustomCharacterModels()
        {
            return CustomCharacterModels.LoadModels();
        }

        /// <summary>
        /// Default <c>IBattleCharacter</c> implementation.
        /// </summary>
        private class BattleCharacter : IBattleCharacter
        {
            public string Name { get; }

            public string CharacterClassName { get; }
            public int CustomCharacterModelId { get; }
            public int CharacterClassModelId { get; }

            public int PlayerPrefabId { get; }

            public Defence MainDefence { get; }
            public int Speed { get; }
            public int Resistance { get; }
            public int Attack { get; }
            public int Defence { get; }

            private BattleCharacter(ICustomCharacterModel custom, ICharacterClassModel classModel)
            {
                Assert.IsTrue(custom.CharacterModelId == classModel.Id, "custom.CharacterId == model.Id");
                Name = custom.Name;
                CharacterClassName = classModel.Name;
                CustomCharacterModelId = custom.Id;
                CharacterClassModelId = classModel.Id;
                PlayerPrefabId = custom.PlayerPrefabId;
                MainDefence = classModel.MainDefence;
                Speed = classModel.Speed + custom.Speed;
                Resistance = classModel.Resistance + custom.Resistance;
                Attack = classModel.Attack + custom.Attack;
                Defence = classModel.Defence + custom.Defence;
            }

            public override string ToString()
            {
                return $"Name: {Name}, CharacterClass: {CharacterClassName}, " +
                       $"CustomCharacterModel: {CustomCharacterModelId}, CharacterClassModel: {CharacterClassModelId}, " +
                       $"Defence: {MainDefence}, Speed: {Speed}, Resistance: {Resistance}, Attack: {Attack}, Defence: {Defence}, " +
                       $"PlayerPrefab: {PlayerPrefabId}";
            }

            public static IBattleCharacter GetBattleCharacter(int customCharacterId)
            {
                var customCharacter = CustomCharacterModels.GetCustomCharacterModel(customCharacterId);
                if (customCharacter == null)
                {
                    throw new UnityException($"CustomCharacterModel not found for {customCharacterId}");
                }
                var character = GetCharacterClassModel(customCharacter.CharacterModelId);
                if (character == null)
                {
                    // Patch BattleCharacter to make it return ok even if custom character exists without corresponding character class.
                    character = new CharacterClassModel(customCharacter.CharacterModelId,
                        "Ööö", Model.Defence.Desensitisation, 1, 1, 1, 1);
                }
                return new BattleCharacter(customCharacter, character);
            }

            public static List<IBattleCharacter> GetAllBattleCharacters()
            {
                // Same as Custom Characters.
                var battleCharacters = new List<IBattleCharacter>();
                var customCharacters = CustomCharacterModels.LoadModels();
                foreach (var customCharacter in customCharacters)
                {
                    battleCharacters.Add(GetBattleCharacter(customCharacter.Id));
                }
                return battleCharacters;
            }

            private static ICharacterClassModel GetCharacterClassModel(int id)
            {
                return Models.FindById<CharacterClassModel>(id);
            }
        }
    }
}