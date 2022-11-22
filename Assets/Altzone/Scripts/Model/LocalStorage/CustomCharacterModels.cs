using System.Collections.Generic;

namespace Altzone.Scripts.Model.LocalStorage
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
        
        private static readonly CustomCharacterModelStorage Storage = new(StorageFilename);
        
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
    }
}