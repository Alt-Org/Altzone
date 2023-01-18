using System.Collections.Generic;
using Altzone.Scripts.Model.LocalStorage;

namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Helper class for external <c>ICustomCharacterModel</c> operations.
    /// </summary>
    public static class CustomCharacterModels
    {
        private const string StorageFilename = "CustomCharacterModels.json";
        
        private static CustomCharacterModelStorage _storage;

        public static void Load()
        {
            _storage = new CustomCharacterModelStorage(StorageFilename);
            Debug.Log($"storage file {_storage.StoragePath}");
        }
        
        public static ICustomCharacterModel GetCustomCharacterModel(int id)
        {
            return _storage.GetCustomCharacterModel(id);
        }

        public static List<ICustomCharacterModel> LoadModels()
        {
            return _storage.GetAll();
        }

        public static int Save(ICustomCharacterModel customCharacterModel)
        {
            return _storage.Save(customCharacterModel);
        }

        public static void Delete(int id)
        {
            _storage.Delete(id);
        }
    }
}