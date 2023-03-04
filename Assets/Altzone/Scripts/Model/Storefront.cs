using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Altzone.Scripts.Model.Dto;
using Altzone.Scripts.Model.ModelStorage;
using GameServer.Scripts;
using GameServer.Scripts.Dto;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model
{
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

        private readonly IGameServer _gameServer;
        private IInventory _inventory;
        private RaidGameRoomModels _clanGameRoomModels;
        private RaidGameRoomModels _playerGameRoomModels;

        public bool IsInventoryConnected => _inventory != null;

        private Storefront()
        {
            Debug.Log($"start");
            Models.Load();
            CustomCharacterModels.Load();
            var gameServerFolder = Path.Combine(Application.persistentDataPath, "GameServer");
            var playerGameRoomModelsFilename = Path.Combine(Application.persistentDataPath, GameFiles.PlayerGameRoomModelsFilename);
            var clanGameRoomModelsFilename = Path.Combine(Application.persistentDataPath, GameFiles.ClanGameRoomModelsFilename);
            var inventoryItemsPath = Path.Combine(Application.persistentDataPath, GameFiles.ClanInventoryItemsFilename);
            _gameServer = GameServerFactory.CreateLocal(gameServerFolder);
            Task.Run(() => { AsyncInit(playerGameRoomModelsFilename, clanGameRoomModelsFilename, inventoryItemsPath); });
            Debug.Log($"exit");
        }

        private void AsyncInit(string playerGameRoomModelsFilename, string clanGameRoomModelsFilename, string inventoryItemsPath)
        {
            Debug.Log($"start");
            try
            {
                _playerGameRoomModels = new RaidGameRoomModels();
                var playerConnectResult = _playerGameRoomModels.Connect(playerGameRoomModelsFilename);
                _clanGameRoomModels = new RaidGameRoomModels();
                var clanConnectResult = _clanGameRoomModels.Connect(clanGameRoomModelsFilename);
                var inventoryResult = InventoryFactory.Create(inventoryItemsPath);
                var gameServerResult = _gameServer.Initialize();
                Task.WaitAll(playerConnectResult, clanConnectResult, inventoryResult, gameServerResult);
                Assert.IsTrue(playerConnectResult.Result);
                Assert.IsTrue(clanConnectResult.Result);
                _inventory = inventoryResult.Result;
                Assert.IsNotNull(_inventory);
                Assert.IsTrue(gameServerResult.Result);
            }
            catch (Exception x)
            {
                Debug.LogWarning($"error: {x.GetType().FullName} {x.Message}");
            }
            Debug.Log($"exit");
        }

        ICharacterClassModel IStorefront.GetCharacterClassModel(int id)
        {
            return Models.FindById<CharacterClassModel>(id);
        }

        List<ICharacterClassModel> IStorefront.GetAllCharacterClassModels()
        {
            return Models.GetAll<CharacterClassModel>().Cast<ICharacterClassModel>().ToList();
        }

        async Task<IClanModel> IStorefront.GetClanModel(int id)
        {
            var clan = await _gameServer.Clan.Get(id);
            return clan != null ? new ClanModel(clan) : null;
        }

        async Task<List<IClanModel>> IStorefront.GetAllClanModels()
        {
            var dtoList = await _gameServer.Clan.GetAll();
            return dtoList.Select(x => new ClanModel(x)).Cast<IClanModel>().ToList();
        }

        Task<bool> IStorefront.Save(IClanModel clanModel)
        {
            var dto = clanModel is ClanModel model ? model.ToDto() : null;
            return clanModel.Id == 0 
                ? _gameServer.Clan.Save(dto)
                : _gameServer.Clan.Update(dto);
        }

        Task IStorefront.DeleteClanModel(int id)
        {
            return _gameServer.Clan.Delete(id);
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

        public Task<bool> Save(IPlayerDataModel playerDataModel)
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

        public Task<IRaidGameRoomModel> GetClanGameRoomModel(int id)
        {
            return _clanGameRoomModels.GetById(id);
        }

        public Task<IRaidGameRoomModel> GetClanGameRoomModel(string name)
        {
            return _clanGameRoomModels.GetByName(name);
        }

        public Task<List<IRaidGameRoomModel>> GetAllClanGameRoomModels()
        {
            return _clanGameRoomModels.GetAll();
        }

        public Task<bool> SaveClanGameRoomModel(RaidGameRoomModel raidGameRoomModel)
        {
            return _clanGameRoomModels.Save(raidGameRoomModel);
        }

        public Task DeleteClanGameRoomModel(int modelId)
        {
            return _clanGameRoomModels.Delete(modelId);
        }

        public Task<IRaidGameRoomModel> GetPlayerGameRoomModel(int id)
        {
            return _playerGameRoomModels.GetById(id);
        }

        public Task<IRaidGameRoomModel> GetPlayerGameRoomModel(string name)
        {
            return _playerGameRoomModels.GetByName(name);
        }

        public Task<List<IRaidGameRoomModel>> GetAllPlayerGameRoomModels()
        {
            return _playerGameRoomModels.GetAll();
        }

        public Task<bool> SavePlayerGameRoomModel(RaidGameRoomModel raidGameRoomModel)
        {
            return _playerGameRoomModels.Save(raidGameRoomModel);
        }

        public Task DeletePlayerGameRoomModel(int id)
        {
            return _playerGameRoomModels.Delete(id);
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

        public Task<bool> Save(IInventoryItem inventoryItem)
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
                    // Patch BattleCharacter to make it return ok even if custom character exists without corresponding character class.
                    character = new CharacterClassModel(customCharacter.CharacterModelId,
                        "Ööö", Altzone.Scripts.Model.Defence.Desensitisation, 1, 1, 1, 1);
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