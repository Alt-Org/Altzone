using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Store CRUD operations for model and custom objects.
    /// </summary>
    public interface IStorefront
    {
        #region CharacterClassModel

        CharacterClassModel GetCharacterClassModel(int id);
        List<CharacterClassModel> GetAllCharacterClassModels();

        #endregion

        #region ICustomCharacterModel

        ICustomCharacterModel GetCustomCharacterModel(int id);
        List<ICustomCharacterModel> GetAllCustomCharacterModels();
        void Save(ICustomCharacterModel customCharacterModel);
        void Delete(int id);

        #endregion

        #region IBattleCharacter

        IBattleCharacter GetBattleCharacter(int id);
        List<IBattleCharacter> GetAllBattleCharacters();

        #endregion

        #region ClanModel

        ClanModel GetClanModel(int id);
        List<ClanModel> GetAllClanModels();

        #endregion

        #region FurnitureModel

        FurnitureModel GetFurnitureModel(int id);
        FurnitureModel GetFurnitureModel(string name);
        List<FurnitureModel> GetAllFurnitureModels();

        #endregion

        #region RaidGameRoomModel (Async)

        Task<RaidGameRoomModel> GetRaidGameRoomModel(int id);
        Task<RaidGameRoomModel> GetRaidGameRoomModel(string name);
        Task<List<RaidGameRoomModel>> GetAllRaidGameRoomModels();

        #endregion

        #region Inventory (Async)

        Task<List<InventoryItem>> GetAllInventoryItems();

        Task<List<FurnitureModel>> GetAllFurnitureModelsFromInventory();

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

        FurnitureModel IStorefront.GetFurnitureModel(int id)
        {
            return Models.FindById<FurnitureModel>(id);
        }

        FurnitureModel IStorefront.GetFurnitureModel(string name)
        {
            return Models.Find<FurnitureModel>(x => x.Name == name);
        }

        List<FurnitureModel> IStorefront.GetAllFurnitureModels()
        {
            return Models.GetAll<FurnitureModel>();
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
            return BattleCharacter.GetBattleCharacter(this, customCharacterId);
        }

        public List<IBattleCharacter> GetAllBattleCharacters()
        {
            return BattleCharacter.GetAllBattleCharacters(this);
        }

        public Task<RaidGameRoomModel> GetRaidGameRoomModel(int id)
        {
            return RaidGameRoomModels.GetById(id);
        }

        public Task<RaidGameRoomModel> GetRaidGameRoomModel(string name)
        {
            return RaidGameRoomModels.GetByName(name);
        }

        public Task<List<RaidGameRoomModel>> GetAllRaidGameRoomModels()
        {
            return RaidGameRoomModels.GetAll();
        }

        public Task<List<InventoryItem>> GetAllInventoryItems()
        {
            return _inventory.GetAll();
        }

        public Task<List<FurnitureModel>> GetAllFurnitureModelsFromInventory()
        {
            return _inventory.GetAllFurnitureModelsFromInventory();
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

            public BattleCharacter(ICustomCharacterModel custom, CharacterClassModel classModel)
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