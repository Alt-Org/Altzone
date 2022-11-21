using System.Collections.Generic;
using Altzone.Scripts.Model.Loader;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Store for model and custom objects.
    /// </summary>
    public interface IStorefront
    {
        CharacterModel GetCharacterModel(int id);
        List<CharacterModel> GetAllCharacterModels();

        CustomCharacterModel GetCustomCharacterModel(int id);
        List<CustomCharacterModel> GetAllCustomCharacterModels();
        void Save(CustomCharacterModel customCharacterModel);

        IBattleCharacter GetBattleCharacter(int id);
        List<IBattleCharacter> GetAllBattleCharacters();

        ClanModel GetClanModel(int id);
        List<ClanModel> GetAllClanModels();
    }

    public class Storefront : IStorefront
    {
        public static IStorefront Get()
        {
            return _instance ??= new Storefront();
        }

        private static Storefront _instance;

        private Storefront()
        {
            Models.Load();
        }

        CharacterModel IStorefront.GetCharacterModel(int id)
        {
            var model = Models.FindById<CharacterModel>(id);
            if (model == null)
            {
                model = new CharacterModel(id, "Ööö", Defence.Desensitisation, 1, 1, 1, 1);
            }
            return model;
        }

        List<CharacterModel> IStorefront.GetAllCharacterModels()
        {
            return Models.GetAll<CharacterModel>();
        }

        ClanModel IStorefront.GetClanModel(int id)
        {
            return Models.FindById<ClanModel>(id);
        }

        List<ClanModel> IStorefront.GetAllClanModels()
        {
            return Models.GetAll<ClanModel>();
        }

        public CustomCharacterModel GetCustomCharacterModel(int id)
        {
            return CustomCharacterModels.GetCustomCharacterModel(id);
        }

        public List<CustomCharacterModel> GetAllCustomCharacterModels()
        {
            return CustomCharacterModels.LoadModels();
        }

        public void Save(CustomCharacterModel customCharacterModel)
        {
            CustomCharacterModels.Save(customCharacterModel);
        }

        public IBattleCharacter GetBattleCharacter(int customCharacterId)
        {
            var customCharacter = Get().GetCustomCharacterModel(customCharacterId);
            if (customCharacter == null)
            {
                throw new UnityException($"CustomCharacterModel not found for {customCharacterId}");
            }
            var character = Get().GetCharacterModel(customCharacter.CharacterModelId);
            if (customCharacter == null)
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

            public BattleCharacter(CustomCharacterModel custom, CharacterModel model)
            {
                Assert.IsTrue(custom.CharacterModelId == model.Id, "custom.CharacterId == model.Id");
                Name = custom.Name;
                CustomCharacterModelId = custom.Id;
                MainDefence = model.MainDefence;
                Speed = model.Speed + custom.Speed;
                Resistance = model.Resistance + custom.Resistance;
                Attack = model.Attack + custom.Attack;
                Defence = model.Defence + custom.Defence;
            }
        }
    }
}