using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model.LocalStorage
{
    /// <summary>
    /// Manages <c>RaidGameRoomModel</c> instances in local file storage using UNITY <c>JsonUtility</c> for serialization.
    /// </summary>
    public class RaidGameRoomModelStorage
    {
        private const int StorageVersionNUmber = 1;

        private readonly string _storagePath;
        private readonly List<RaidGameRoomModel> _models;

        public string StoragePath => _storagePath;

        public RaidGameRoomModelStorage(string storageFilename)
        {
            _storagePath = Path.Combine(Application.persistentDataPath, storageFilename);
            if (AppPlatform.IsWindows)
            {
                _storagePath = AppPlatform.ConvertToWindowsPath(_storagePath);
            }
            if (!File.Exists(_storagePath))
            {
                _models = new List<RaidGameRoomModel>();
                return;
            }
            var storageData = LoadStorage(_storagePath);
            _models = storageData.ModelList;
        }

        public RaidGameRoomModel GetCustomCharacterModel(int id)
        {
            return _models.FirstOrDefault(x => x._id == id);
        }

        public List<RaidGameRoomModel> GetAll()
        {
            return _models;
        }

        public void Save(RaidGameRoomModel model)
        {
            var index = _models.FindIndex(x => x._id == model._id);
            if (index >= 0)
            {
                _models[index] = model;
            }
            else
            {
                _models.Add(model);
            }
            SaveStorage(_models, _storagePath);
        }

        public void Delete(int id)
        {
            var index = _models.FindIndex(x => x._id == id);
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

        private static void SaveStorage(List<RaidGameRoomModel> models, string storagePath)
        {
            var storageData = new StorageData
            {
                VersionNUmber = StorageVersionNUmber,
                ModelList = models,
            };
            var json = JsonUtility.ToJson(storageData);
            File.WriteAllText(storagePath, json);
        }

        private class StorageData
        {
            public int VersionNUmber;
            public List<RaidGameRoomModel> ModelList = new();
        }
    }
}