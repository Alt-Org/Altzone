using System.Collections.Generic;
using Altzone.Scripts.Model.LocalStorage;

namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Helper class for external <c>ICustomCharacterModel</c> operations.
    /// </summary>
    /// <remarks>
    /// WIKI: https://github.com/Alt-Org/Altzone/wiki/ModelLoader
    /// </remarks>
    public static class CustomCharacterModels
    {
        private const string StorageFilename = "CustomCharacterModels.json";
        
        private static readonly CustomCharacterModelStorage Storage;

        static CustomCharacterModels()
        {
            // Note that we do not use
            // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            // if UNITY Domain Reloading is disabled.
            // This means that storage is not updated between Editor "runs" if it is edited in file system by hand during testing.
            // Only compiling will force storage to be loaded again.
            Storage = new CustomCharacterModelStorage(StorageFilename);
            Debug.Log($"storage file {Storage.StoragePath}");
        }
        
        public static ICustomCharacterModel GetCustomCharacterModel(int id)
        {
            return Storage.GetCustomCharacterModel(id);
        }

        public static List<ICustomCharacterModel> LoadModels()
        {
            return Storage.GetAll();
        }

        public static void Save(ICustomCharacterModel customCharacterModel)
        {
            Storage.Save(customCharacterModel);
        }

        public static void Delete(int id)
        {
            Storage.Delete(id);
        }
    }
}