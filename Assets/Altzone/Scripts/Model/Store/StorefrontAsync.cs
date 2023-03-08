using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Altzone.Scripts.Model.Dto;
using Altzone.Scripts.Model.ModelStorage;
using GameServer.Scripts;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model.Store
{
    /// <summary>
    /// <c>IStorefront</c> front end dispatcher to actual worker classes.
    /// </summary>
    internal class StorefrontAsync : IStorefront
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            // Manual reset if UNITY Domain Reloading is disabled.
            _instance = null;
        }

        public static IStorefront Get()
        {
            return _instance ??= new StorefrontAsync();
        }

        private const int ServiceTimeoutTimeMs = 3000;

        private static IStorefront _instance;

        private readonly IGameServer _gameServer;
        private IInventory _inventory;
        private RaidGameRoomModelsAsync _clanGameRoomModels;
        private RaidGameRoomModelsAsync _playerGameRoomModels;

        public bool IsGameServerConnected => _gameServer.IsConnected;
        public bool IsInventoryConnected => _inventory != null;

        internal StorefrontAsync()
        {
            Debug.Log($"start");
            Models.Load();
            CustomCharacterModels.Load();
            var gameServerFolder = GetGameServerFolder();
            _gameServer = GameServerFactory.CreateLocal(gameServerFolder);
            var playerGameRoomModelsFilename = Path.Combine(Application.persistentDataPath, GameFiles.PlayerGameRoomModelsFilename);
            var clanGameRoomModelsFilename = Path.Combine(Application.persistentDataPath, GameFiles.ClanGameRoomModelsFilename);
            var inventoryItemsPath = Path.Combine(Application.persistentDataPath, GameFiles.ClanInventoryItemsFilename);
            StartServices(playerGameRoomModelsFilename, clanGameRoomModelsFilename, inventoryItemsPath);
            Debug.Log($"exit");
        }

        private static string GetGameServerFolder()
        {
#if UNITY_WEBGL
            // Problem: Files saved to Application.persistentDataPath don’t persist
            // Unity WebGL stores all files that must persist between sessions (files saved in persistentDataPath) to the browser IndexedDB.
            // This is an asynchronous API, so you don’t know when it’s going to complete.
            return Application.persistentDataPath;
#else
            return Path.Combine(Application.persistentDataPath, "GameServer");
#endif
        }
        
        private void StartServices(string playerGameRoomModelsFilename, string clanGameRoomModelsFilename, string inventoryItemsPath)
        {
#if UNITY_WEBGL
            WebGlInit(playerGameRoomModelsFilename, clanGameRoomModelsFilename, inventoryItemsPath);
#else
            Task.Run(() => { AsyncInit(playerGameRoomModelsFilename, clanGameRoomModelsFilename, inventoryItemsPath); });
#endif
        }
        
        [Conditional("UNITY_WEBGL")]
        private void WebGlInit(string playerGameRoomModelsFilename, string clanGameRoomModelsFilename, string inventoryItemsPath)
        {
            Debug.Log($"start webgl");
            try
            {
                _playerGameRoomModels = new RaidGameRoomModelsAsync();
                _playerGameRoomModels.Connect(playerGameRoomModelsFilename);
                _clanGameRoomModels = new RaidGameRoomModelsAsync();
                _clanGameRoomModels.Connect(clanGameRoomModelsFilename);
                _inventory = InventoryFactory.Create(inventoryItemsPath).Result;
                _gameServer.Initialize();
                Assert.IsNotNull(_inventory);
            }
            catch (Exception x)
            {
                Debug.LogWarning($"error: {x.GetType().FullName} {x.Message}");
            }
            Debug.Log($"exit webgl");
        }

        private void AsyncInit(string playerGameRoomModelsFilename, string clanGameRoomModelsFilename, string inventoryItemsPath)
        {
            Debug.Log($"start async");
            try
            {
                _playerGameRoomModels = new RaidGameRoomModelsAsync();
                var playerConnectResult = _playerGameRoomModels.Connect(playerGameRoomModelsFilename);
                _clanGameRoomModels = new RaidGameRoomModelsAsync();
                var clanConnectResult = _clanGameRoomModels.Connect(clanGameRoomModelsFilename);
                var inventoryResult = InventoryFactory.Create(inventoryItemsPath);
                var gameServerResult = _gameServer.Initialize();
                Task.WaitAll(new Task[] { playerConnectResult, clanConnectResult, inventoryResult, gameServerResult }, ServiceTimeoutTimeMs);
                Debug.Log($"{playerConnectResult.Result} {clanConnectResult.Result} {inventoryResult.Result} {gameServerResult.Result}");
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
            Debug.Log($"exit async");
        }

        #region ICharacterClassModel

        ICharacterClassModel IStorefront.GetCharacterClassModel(int id)
        {
            return Models.FindById<CharacterClassModel>(id);
        }

        List<ICharacterClassModel> IStorefront.GetAllCharacterClassModels()
        {
            return Models.GetAll<CharacterClassModel>().Cast<ICharacterClassModel>().ToList();
        }

        #endregion

        #region IClanModel (Async)

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

        async Task<bool> IStorefront.Save(IClanModel clanModel)
        {
            if (clanModel is not ClanModel model)
            {
                throw new UnityException($"Invalid model object {clanModel}");
            }
            var dto = model.ToDto();
            var isNew = clanModel.Id == 0;
            var result = isNew
                ? await _gameServer.Clan.Save(dto)
                : await _gameServer.Clan.Update(dto);
            if (result && isNew)
            {
                model.SetId(dto.Id);
            }
            return result;
        }

        Task IStorefront.DeleteClanModel(int id)
        {
            return _gameServer.Clan.Delete(id);
        }

        #endregion

        #region IFurnitureModel

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

        #endregion

        #region PlayerData (Async)

        public async Task<IPlayerDataModel> GetPlayerDataModel(int id)
        {
            var player = await _gameServer.Player.Get(id);
            return player != null ? new PlayerDataModel(player) : null;
        }

        public async Task<IPlayerDataModel> GetPlayerDataModel(string uniqueIdentifier)
        {
            var player = await _gameServer.Player.Get(uniqueIdentifier);
            return player != null ? new PlayerDataModel(player) : null;
        }

        public async Task<List<IPlayerDataModel>> GetAllPlayerDataModels()
        {
            var dtoList = await _gameServer.Player.GetAll();
            return dtoList.Select(x => new PlayerDataModel(x)).Cast<IPlayerDataModel>().ToList();
        }

        public async Task<bool> SavePlayerDataModel(IPlayerDataModel playerDataModel)
        {
            if (playerDataModel is not PlayerDataModel model)
            {
                throw new UnityException($"Invalid model object {playerDataModel}");
            }
            var dto = model.ToDto();
            var isNew = playerDataModel.Id == 0;
            var result = isNew
                ? await _gameServer.Player.Save(dto)
                : await _gameServer.Player.Update(dto);
            if (result && isNew)
            {
                model.SetId(dto.Id);
            }
            return result;
        }

        public Task DeletePlayerDataModel(int id)
        {
            return _gameServer.Player.Delete(id);
        }

        #endregion

        #region ICustomCharacterModel

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

        #endregion

        #region IBattleCharacter

        public IBattleCharacter GetBattleCharacter(int customCharacterId)
        {
            return BattleCharacter.GetBattleCharacter(this, customCharacterId);
        }

        public List<IBattleCharacter> GetAllBattleCharacters()
        {
            return BattleCharacter.GetAllBattleCharacters(this);
        }

        #endregion

        #region RaidGameRoomModel for a Clan (Async)

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

        #endregion

        #region RaidGameRoomModel for a Player (Async)

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

        #endregion

        #region Inventory for a Clan (Async)

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

        #endregion

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
                        "Ööö", Model.Defence.Desensitisation, 1, 1, 1, 1);
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
                    battleCharacters.Add(store.GetBattleCharacter(customCharacter.Id));
                }
                return battleCharacters;
            }
        }
    }
}