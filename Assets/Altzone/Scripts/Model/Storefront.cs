using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Store for model and custom objects.
    /// </summary>
    public interface IStorefront
    {
        CharacterClassModel GetCharacterClassModel(int id);
        List<CharacterClassModel> GetAllCharacterClassModels();

        ICustomCharacterModel GetCustomCharacterModel(int id);
        List<ICustomCharacterModel> GetAllCustomCharacterModels();
        void Save(ICustomCharacterModel customCharacterModel);
        void Delete(int id);

        IBattleCharacter GetBattleCharacter(int id);
        List<IBattleCharacter> GetAllBattleCharacters();

        ClanModel GetClanModel(int id);
        List<ClanModel> GetAllClanModels();
    }

    public class Storefront : IStorefront
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            // Manual reset if UNITY Domain Reloading is disabled.
            _instance = null;
        }

        public static IStorefront Get()
        {
            return _instance ??= new Storefront();
        }

        private static Storefront _instance;

        private Storefront()
        {
            Models.Load();
            CustomCharacterModels.Load();
        }

        CharacterClassModel IStorefront.GetCharacterClassModel(int id)
        {
            var model = Models.FindById<CharacterClassModel>(id);
            if (model == null)
            {
                model = new CharacterClassModel(id, "Ööö", Defence.Desensitisation, 1, 1, 1, 1);
            }
            return model;
        }

        List<CharacterClassModel> IStorefront.GetAllCharacterClassModels()
        {
            return Models.GetAll<CharacterClassModel>();
        }

        ClanModel IStorefront.GetClanModel(int id)
        {
            return Models.FindById<ClanModel>(id);
        }

        List<ClanModel> IStorefront.GetAllClanModels()
        {
            return Models.GetAll<ClanModel>();
        }

        public ICustomCharacterModel GetCustomCharacterModel(int id)
        {
            return CustomCharacterModels.GetCustomCharacterModel(id);
        }

        public List<ICustomCharacterModel> GetAllCustomCharacterModels()
        {
            return CustomCharacterModels.LoadModels();
        }

        public void Save(ICustomCharacterModel customCharacterModel)
        {
            CustomCharacterModels.Save(customCharacterModel);
        }

        public void Delete(int id)
        {
            CustomCharacterModels.Delete(id);
        }

        public IBattleCharacter GetBattleCharacter(int customCharacterId)
        {
            var customCharacter = Get().GetCustomCharacterModel(customCharacterId);
            if (customCharacter == null)
            {
                throw new UnityException($"CustomCharacterModel not found for {customCharacterId}");
            }
            var character = Get().GetCharacterClassModel(customCharacter.CharacterModelId);
            if (character == null)
            {
                throw new UnityException($"CustomCharacter {customCharacterId} CharacterModel not found for {customCharacter.CharacterModelId}");
            }
            return new BattleCharacter(customCharacter, character);
        }

        public List<IBattleCharacter> GetAllBattleCharacters()
        {
            // Same as Custom Characters.
            var battleCharacters = new List<IBattleCharacter>();
            var customCharacters = Get().GetAllCustomCharacterModels();
            foreach (var customCharacter in customCharacters)
            {
                battleCharacters.Add(Get().GetBattleCharacter(customCharacter.Id));
            }
            return battleCharacters;
        }

        /// <summary>
        /// Dummy <c>IBattleCharacter</c> implementation for Battle game.
        /// </summary>
        private class BattleCharacter : IBattleCharacter
        {
            public string Name { get; }

            public int CustomCharacterModelId { get; }

            public Defence MainDefence { get; }
            public int Speed { get; }
            public int Resistance { get; }
            public int Attack { get; }
            public int Defence { get; }

            public BattleCharacter(ICustomCharacterModel custom, CharacterClassModel classModel)
            {
                Assert.IsTrue(custom.CharacterModelId == classModel.Id, "custom.CharacterId == model.Id");
                Name = custom.Name;
                CustomCharacterModelId = custom.Id;
                MainDefence = classModel.MainDefence;
                Speed = classModel.Speed + custom.Speed;
                Resistance = classModel.Resistance + custom.Resistance;
                Attack = classModel.Attack + custom.Attack;
                Defence = classModel.Defence + custom.Defence;
            }
        }
    }
}