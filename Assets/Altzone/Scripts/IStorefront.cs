using System.Collections.Generic;
using System.Threading.Tasks;
using Altzone.Scripts.Model;
using Altzone.Scripts.Model.Dto;

namespace Altzone.Scripts
{
    /// <summary>
    /// Store CRUD operations for model and custom objects.
    /// </summary>
    /// <remarks>
    /// https://github.com/Alt-Org/Altzone/wiki/Pelin-Tietovarastot
    /// </remarks>
    public interface IStorefront
    {
        bool IsInventoryConnected { get; }

        #region ICharacterClassModel

        ICharacterClassModel GetCharacterClassModel(int id);
        List<ICharacterClassModel> GetAllCharacterClassModels();

        #endregion

        #region ICustomCharacterModel

        ICustomCharacterModel GetCustomCharacterModel(int id);
        List<ICustomCharacterModel> GetAllCustomCharacterModels();
        int Save(ICustomCharacterModel customCharacterModel);
        void DeleteCustomCharacterModel(int id);

        #endregion

        #region IBattleCharacter

        IBattleCharacter GetBattleCharacter(int id);
        List<IBattleCharacter> GetAllBattleCharacters();

        #endregion

        #region IClanModel

        IClanModel GetClanModel(int id);
        List<IClanModel> GetAllClanModels();
        int Save(IClanModel clanModel);
        void DeleteClanModel(int id);

        #endregion

        #region IFurnitureModel

        IFurnitureModel GetFurnitureModel(int id);
        IFurnitureModel GetFurnitureModel(string name);
        List<IFurnitureModel> GetAllFurnitureModels();

        #endregion

        #region PlayerData (Async)

        Task<IPlayerDataModel> GetPlayerDataModel(int id);
        Task<List<IPlayerDataModel>> GetAllPlayerDataModels();

        Task<int> Save(IPlayerDataModel inventoryItem);
        Task DeletePlayerDataModel(int id);

        #endregion

        #region RaidGameRoomModel (Async)

        Task<IRaidGameRoomModel> GetClanGameRoomModel(int id);
        Task<IRaidGameRoomModel> GetClanGameRoomModel(string name);
        Task<List<IRaidGameRoomModel>> GetAllClanGameRoomModels();

        Task<int> Save(RaidGameRoomModel raidGameRoomModel);
        Task DeleteRaidGameRoomModel(int id);

        #endregion

        #region Inventory (Async)

        Task<IInventoryItem> GetInventoryItem(int id);
        Task<List<IInventoryItem>> GetAllInventoryItems();

        Task<List<IFurnitureModel>> GetAllFurnitureModelsFromInventory();

        Task<int> Save(IInventoryItem inventoryItem);
        Task DeleteInventoryItem(int id);

        #endregion
    }
}