using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Altzone.Scripts.Model.Dto;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model
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

        Task<IRaidGameRoomModel> GetRaidGameRoomModel(int id);
        Task<IRaidGameRoomModel> GetRaidGameRoomModel(string name);
        Task<List<IRaidGameRoomModel>> GetAllRaidGameRoomModels();

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

        private const string RaidGameRoomModelsFilename = "RaidGameRoomModels.json";
        private const string InventoryItemsFilename = "InventoryItems.json";

        private static Storefront _instance;

        private IInventory _inventory;

        public bool IsInventoryConnected => _inventory != null;

        private Storefront()
        {
            Debug.Log($"start initialization");
            Models.Load();
            CustomCharacterModels.Load();
            var raidGameRoomModelsPath = Path.Combine(Application.persistentDataPath, RaidGameRoomModelsFilename);
            var inventoryItemsPath = Path.Combine(Application.persistentDataPath, InventoryItemsFilename);
            Task.Run(() =>
            {
                try
                {
                    var connectResult = RaidGameRoomModels.Connect(raidGameRoomModelsPath);
                    var inventoryResult = InventoryFactory.Create(inventoryItemsPath);
                    Task.WaitAll(connectResult, inventoryResult);
                    Assert.IsTrue(connectResult.Result);
                    _inventory = inventoryResult.Result;
                    Assert.IsNotNull(_inventory);
                }
                catch (Exception x)
                {
                    Debug.LogWarning($"error: {x.GetType().FullName} {x.Message}");
                }
                Debug.Log($"done initialization");
            });
        }

        ICharacterClassModel IStorefront.GetCharacterClassModel(int id)
        {
            var model = Models.FindById<CharacterClassModel>(id);
            if (model == null)
            {
                model = new CharacterClassModel(id, "Ööö", Defence.Desensitisation, 1, 1, 1, 1);
            }
            return model;
        }

        List<ICharacterClassModel> IStorefront.GetAllCharacterClassModels()
        {
            return Models.GetAll<CharacterClassModel>().Cast<ICharacterClassModel>().ToList();
        }

        IClanModel IStorefront.GetClanModel(int id)
        {
            return Models.FindById<ClanModel>(id);
        }

        List<IClanModel> IStorefront.GetAllClanModels()
        {
            return Models.GetAll<ClanModel>().Cast<IClanModel>().ToList();
        }

        public int Save(IClanModel clanModel)
        {
            throw new NotImplementedException();
        }

        public void DeleteClanModel(int id)
        {
            throw new NotImplementedException();
        }

        IFurnitureModel IStorefront.GetFurnitureModel(int id)
        {
            return Models.FindById<FurnitureModel>(id);
        }

        IFurnitureModel IStorefront.GetFurnitureModel(string name)
        {
            return Models.Find<FurnitureModel>(x => x.Name == name);
        }

        List<IFurnitureModel> IStorefront.GetAllFurnitureModels()
        {
            return Models.GetAll<FurnitureModel>().Cast<IFurnitureModel>().ToList();
        }

        public Task<IPlayerDataModel> GetPlayerDataModel(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<IPlayerDataModel>> GetAllPlayerDataModels()
        {
            throw new NotImplementedException();
        }

        public Task<int> Save(IPlayerDataModel playerDataModel)
        {
            throw new NotImplementedException();
        }

        public Task DeletePlayerDataModel(int id)
        {
            throw new NotImplementedException();
        }

        public ICustomCharacterModel GetCustomCharacterModel(int id)
        {
            return CustomCharacterModels.GetCustomCharacterModel(id);
        }

        public List<ICustomCharacterModel> GetAllCustomCharacterModels()
        {
            return CustomCharacterModels.LoadModels();
        }

        public int Save(ICustomCharacterModel customCharacterModel)
        {
            return CustomCharacterModels.Save(customCharacterModel);
        }

        public void DeleteCustomCharacterModel(int id)
        {
            CustomCharacterModels.Delete(id);
        }

        public IBattleCharacter GetBattleCharacter(int customCharacterId)
        {
            return BattleCharacter.GetBattleCharacter(this, customCharacterId);
        }

        public List<IBattleCharacter> GetAllBattleCharacters()
        {
            return BattleCharacter.GetAllBattleCharacters(this);
        }

        public Task<IRaidGameRoomModel> GetRaidGameRoomModel(int id)
        {
            return RaidGameRoomModels.GetById(id);
        }

        public Task<IRaidGameRoomModel> GetRaidGameRoomModel(string name)
        {
            return RaidGameRoomModels.GetByName(name);
        }

        public Task<List<IRaidGameRoomModel>> GetAllRaidGameRoomModels()
        {
            return RaidGameRoomModels.GetAll();
        }

        public Task<int> Save(RaidGameRoomModel raidGameRoomModel)
        {
            throw new NotImplementedException();
        }

        public Task DeleteRaidGameRoomModel(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IInventoryItem> GetInventoryItem(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<IInventoryItem>> GetAllInventoryItems()
        {
            return _inventory.GetAll();
        }

        public Task<List<IFurnitureModel>> GetAllFurnitureModelsFromInventory()
        {
            return _inventory.GetAllFurnitureModelsFromInventory();
        }

        public Task<int> Save(IInventoryItem inventoryItem)
        {
            throw new NotImplementedException();
        }

        public Task DeleteInventoryItem(int id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Dummy <c>IBattleCharacter</c> implementation for Battle game.
        /// </summary>
        private class BattleCharacter : IBattleCharacter
        {
            public string Name { get; }

            public int CustomCharacterModelId { get; }

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
                CustomCharacterModelId = custom.Id;
                PlayerPrefabId = custom.PlayerPrefabId;
                MainDefence = classModel.MainDefence;
                Speed = classModel.Speed + custom.Speed;
                Resistance = classModel.Resistance + custom.Resistance;
                Attack = classModel.Attack + custom.Attack;
                Defence = classModel.Defence + custom.Defence;
            }

            public static IBattleCharacter GetBattleCharacter(IStorefront store, int customCharacterId)
            {
                var customCharacter = store.GetCustomCharacterModel(customCharacterId);
                if (customCharacter == null)
                {
                    throw new UnityException($"CustomCharacterModel not found for {customCharacterId}");
                }
                var character = store.GetCharacterClassModel(customCharacter.CharacterModelId);
                if (character == null)
                {
                    throw new UnityException($"CustomCharacter {customCharacterId} CharacterModel not found for {customCharacter.CharacterModelId}");
                }
                return new BattleCharacter(customCharacter, character);
            }

            public static List<IBattleCharacter> GetAllBattleCharacters(IStorefront store)
            {
                // Same as Custom Characters.
                var battleCharacters = new List<IBattleCharacter>();
                var customCharacters = store.GetAllCustomCharacterModels();
                foreach (var customCharacter in customCharacters)
                {
                    battleCharacters.Add(Get().GetBattleCharacter(customCharacter.Id));
                }
                return battleCharacters;
            }
        }
    }
}