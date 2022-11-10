using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Store for model objects.
    /// </summary>
    public interface IStorefront
    {
        CharacterModel GetCharacterModel(int id);
        List<CharacterModel> GetAllCharacterModels();
        ClanModel GetClanModel(int id);
        List<ClanModel> GetAllClanModels();

        IBattleCharacter GetCharacterModelForSkill(Defence defence);
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
            return Models.FindById<CharacterModel>(id);
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

        /// <summary>
        /// Gets <c>IBattleCharacter</c> for given <c>Defence</c>.
        /// </summary>
        public IBattleCharacter GetCharacterModelForSkill(Defence defence)
        {
            var character = Get().GetCharacterModel((int)defence);
            Assert.IsNotNull(character, "character != null");
            return new BattleCharacter(character);
        }

        /// <summary>
        /// Dummy <c>IBattleCharacter</c> implementation for Battle game.
        /// </summary>
        private class BattleCharacter : IBattleCharacter
        {
            public Defence MainDefence { get; }
            public int Speed { get; }
            public int Resistance { get; }
            public int Attack { get; }
            public int Defence { get; }

            public BattleCharacter(CharacterModel model)
            {
                MainDefence = model.MainDefence;
                Speed = model.Speed;
                Resistance = model.Resistance;
                Attack = model.Attack;
                Defence = model.Defence;
            }
        }
    }
}