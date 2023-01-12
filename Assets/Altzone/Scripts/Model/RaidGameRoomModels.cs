using System.Collections.Generic;
using Altzone.Scripts.Model.LocalStorage;

namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Helper class for external <c>RaidGameRoomModel</c> operations.
    /// </summary>
    public static class RaidGameRoomModels
    {
        private const string StorageFilename = "RaidGameRoomModels.json";

        private static RaidGameRoomModelStorage _storage;

        public static void Load()
        {
            _storage = new RaidGameRoomModelStorage(StorageFilename);
            Debug.Log($"storage file {_storage.StoragePath}");
        }

        public static RaidGameRoomModel GetRaidGameRoomModel(int id)
        {
            return _storage.GetCustomCharacterModel(id);
        }

        public static RaidGameRoomModel GetRaidGameRoomModel(string name)
        {
            return _storage.GetAll().Find(x => x._name == name);
        }

        public static List<RaidGameRoomModel> LoadModels()
        {
            return _storage.GetAll();
        }

        public static void Save(RaidGameRoomModel model)
        {
            _storage.Save(model);
        }

        public static void Delete(int id)
        {
            _storage.Delete(id);
        }
    }
}