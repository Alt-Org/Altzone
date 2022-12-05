using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model.LocalStorage
{
    /// <summary>
    /// Manages <c>ICustomCharacterModel</c> instances in local file storage using UNITY <c>JsonUtility</c> for serialization.
    /// </summary>
    /// <remarks>
    /// We use interface to protect some public fields being changed inadvertently.<br />
    /// Casting between <c>ICustomCharacterModel</c> and <c>CustomCharacterModel</c> is done when file is read or written.
    /// </remarks>
    public class CustomCharacterModelStorage
    {
        private const int StorageVersionNUmber = 1;
        
        private readonly string _storagePath;
        private readonly List<ICustomCharacterModel> _models;

        public string StoragePath => _storagePath;
        
        public CustomCharacterModelStorage(string storageFilename)
        {
            _storagePath = Path.Combine(Application.persistentDataPath, storageFilename);
            if (AppPlatform.IsWindows)
            {
                _storagePath = AppPlatform.ConvertToWindowsPath(_storagePath);
            }
            if (!File.Exists(_storagePath))
            {
                _models = new List<ICustomCharacterModel>();
                return;
            }
            var storageData = LoadStorage(_storagePath);
            _models = storageData.ModelList.Cast<ICustomCharacterModel>().ToList();
        }
        
        public ICustomCharacterModel GetCustomCharacterModel(int id)
        {
            return _models.FirstOrDefault(x => x.Id == id);
        }

        public List<ICustomCharacterModel> GetAll()
        {
            return _models;
        }

        public void Save(ICustomCharacterModel customCharacterModel)
        {
            var index = _models.FindIndex(x => x.Id == customCharacterModel.Id);
            if (index >= 0)
            {
                _models[index] = customCharacterModel as CustomCharacterModel;
            }
            else
            {
                _models.Add(customCharacterModel as CustomCharacterModel);
            }
            SaveStorage(_models, _storagePath);
        }

        public void Delete(int id)
        {
            var index = _models.FindIndex(x => x.Id == id);
            if (index == -1)
            {
                return;
            }
            _models.RemoveAt(index);
            SaveStorage(_models, _storagePath);
        }
        
        private static StorageData LoadStorage(string storagePath)
        {
            var jsonData = File.ReadAllText(storagePath);
            var storageData = JsonUtility.FromJson<StorageData>(jsonData);
            Assert.AreEqual(StorageVersionNUmber, storageData.VersionNUmber);
            return storageData;
        }
        
        private static void SaveStorage(List<ICustomCharacterModel> models, string storagePath)
        {
            var storageData = new StorageData
            {
                VersionNUmber = StorageVersionNUmber,
                ModelList = models.Cast<CustomCharacterModel>().ToList(),
            };
            var json = JsonUtility.ToJson(storageData);
            File.WriteAllText(storagePath, json);
        }
        
        private class StorageData
        {
            public int VersionNUmber;
            public List<CustomCharacterModel> ModelList = new ();
        }
    }
}