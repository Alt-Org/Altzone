using System.Collections.Generic;
using System.Threading.Tasks;
using Altzone.Scripts.Model;
using Altzone.Scripts.Model.Dto;

namespace Altzone.Scripts
{
    /// <summary>
    /// Hardcoded files names for local storage.
    /// </summary>
    public static class GameFiles
    {
        public const string ClanGameRoomModelsFilename = "GameClanGameRoomModels.json";
        public const string ClanInventoryItemsFilename = "GameClanInventoryItems.json";
        public const string PlayerGameRoomModelsFilename = "GamePlayerClanGameRoomModels.json";
        public const string PlayerCustomCharacterModelsFilename = "GamePlayerCustomCharacterModels.json";
    }

    /// <summary>
    /// Store CRUD operations for model and custom objects.
    /// </summary>
    /// <remarks>
    /// https://github.com/Alt-Org/Altzone/wiki/Pelin-Tietovarastot
    /// </remarks>
    public interface IStorefront
    {
        bool IsGameServerConnected { get; }
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

        #region IClanModel (Async)

        Task<IClanModel> GetClanModel(int id);
        Task<List<IClanModel>> GetAllClanModels();
        Task<bool> Save(IClanModel clanModel);
        Task DeleteClanModel(int id);

        #endregion

        #region IFurnitureModel

        IFurnitureModel GetFurnitureModel(int id);
        IFurnitureModel GetFurnitureModel(string name);
        List<IFurnitureModel> GetAllFurnitureModels();

        #endregion

        #region PlayerData (Async)

        Task<IPlayerDataModel> GetPlayerDataModel(int id);
        Task<List<IPlayerDataModel>> GetAllPlayerDataModels();

        Task<bool> SavePlayerDataModel(IPlayerDataModel playerDataModel);
        Task DeletePlayerDataModel(int id);

        #endregion

        #region RaidGameRoomModel for a Clan (Async)

        Task<IRaidGameRoomModel> GetClanGameRoomModel(int id);
        Task<IRaidGameRoomModel> GetClanGameRoomModel(string name);
        Task<List<IRaidGameRoomModel>> GetAllClanGameRoomModels();

        Task<bool> SaveClanGameRoomModel(RaidGameRoomModel raidGameRoomModel);
        Task DeleteClanGameRoomModel(int modelId);

        #endregion

        #region RaidGameRoomModel for a Player (Async)

        Task<IRaidGameRoomModel> GetPlayerGameRoomModel(int id);
        Task<IRaidGameRoomModel> GetPlayerGameRoomModel(string name);
        Task<List<IRaidGameRoomModel>> GetAllPlayerGameRoomModels();

        Task<bool> SavePlayerGameRoomModel(RaidGameRoomModel raidGameRoomModel);
        Task DeletePlayerGameRoomModel(int id);

        #endregion

        #region Inventory for a Clan (Async)

        Task<IInventoryItem> GetInventoryItem(int id);
        Task<List<IInventoryItem>> GetAllInventoryItems();

        Task<List<IFurnitureModel>> GetAllFurnitureModelsFromInventory();

        Task<bool> Save(IInventoryItem inventoryItem);
        Task DeleteInventoryItem(int id);

        #endregion
    }
}