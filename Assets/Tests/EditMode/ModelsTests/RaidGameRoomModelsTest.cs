using System.Collections.Generic;
using System.IO;
using Altzone.Scripts.Model;
using Altzone.Scripts.Model.LocalStorage;
using NUnit.Framework;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace Assets.Tests.EditMode.ModelsTests
{
    [TestFixture]
    public class RaidGameRoomModelsTest
    {
        private const string DefaultStorageFilename = "TestRaidGameRoomModels.json";

        [OneTimeSetUp, Description("Create stable default test storage")]
        public void OneTimeSetUp()
        {
            Debug.Log($"setup {DefaultStorageFilename}");
            DeleteStorage(DefaultStorageFilename);

            var storage = new RaidGameRoomModelStorage(DefaultStorageFilename);
            Debug.Log($"storage {storage.StoragePath}");
            var models = new List<RaidGameRoomModel>()
            {
                new RaidGameRoomModel(1, "test10", 10, 10),
                new RaidGameRoomModel(2, "test20", 20, 20),
            };
            foreach (var model in models)
            {
                model._bombLocations.Add(new RaidGameRoomModel.BombLocation(1,2));
                model._bombLocations.Add(new RaidGameRoomModel.BombLocation(2,3));
                storage.Save(model);
            }
        }

        [Test]
        public void DefaultStorageTest()
        {
            Debug.Log($"test {DefaultStorageFilename}");
            var storage = new CustomCharacterModelStorage(DefaultStorageFilename);
            var models = storage.GetAll();
            Assert.IsTrue(models.Count > 1);
        }

        private static void DeleteStorage(string storageFilename)
        {
            var storagePath = Path.Combine(Application.persistentDataPath, storageFilename);
            if (File.Exists(storagePath))
            {
                File.Delete(storagePath);
            }
        }
    }
}