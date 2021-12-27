using System.Collections.Generic;

namespace Altzone.Scripts.Model
{
    public interface IStorefront
    {
        CharacterModel GetCharacterModel(int id);
        List<CharacterModel> GetAllCharacterModels();
        ClanModel GetClanModel(int id);
        List<ClanModel> GetAllClanModels();
    }

    public class Storefront : IStorefront
    {
        public static IStorefront Get()
        {
            return _instance;
        }

        private static Storefront _instance;

        public static void Create()
        {
            if (_instance == null)
            {
                _instance = new Storefront();
            }
        }

        private Storefront()
        {
            ModelLoader.LoadModels();
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
    }
}