using System.Collections.Generic;
using System.IO;
using Altzone.Scripts;
using Altzone.Scripts.Model.LocalStorage;
using NUnit.Framework;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace Assets.Tests.EditMode.CharacterModelsTests
{
    [TestFixture]
    public class CustomCharacterModelsTest
    {
        private const string DefaultStorageFilename = "TestCustomCharacterModels.json";

        [OneTimeSetUp, Description("Create stable default test storage")]
        public void OneTimeSetUp()
        {
            Debug.Log($"setup {DefaultStorageFilename}");
            DeleteStorage(DefaultStorageFilename);
            
            var storage = new CustomCharacterModelStorage(DefaultStorageFilename);
            var models = new List<CustomCharacterModel>()
            {
                new(1, 1, "Koulukiusaaja", 10, 20, 30, 40),
                new(2, 2, "Vitsiniekka", 10, 20, 30, 40),
                new(3, 3, "Pappi", 10, 20, 30, 40),
                new(4, 4, "Taiteilija", 10, 20, 30, 40),
                new(5, 5, "Hodariläski", 10, 20, 30, 40),
                new(6, 6, "Älykkö", 10, 20, 30, 40),
                new(7, 7, "Tytöt", 10, 20, 30, 40),
            };
            foreach (var model in models)
            {
                storage.Save(model);
            }
        }

        [Test]
        public void DefaultStorageTest()
        {
            Debug.Log($"test {DefaultStorageFilename}");
            var storage = new CustomCharacterModelStorage(DefaultStorageFilename);
            var models = storage.GetAll();
            Assert.AreEqual(7, models.Count);
            var model = models[0];
            Assert.AreEqual(10, model.Speed);
            Assert.AreEqual(20, model.Resistance);
            Assert.AreEqual(30, model.Attack);
            Assert.AreEqual(40, model.Defence);
        }

        [Test]
        public void StorageFileNotFoundTest()
        {
            var storageFilename = "TestNotFound.json";
            DeleteStorage(storageFilename);

            Debug.Log($"test {storageFilename}");
            var storage = new CustomCharacterModelStorage(storageFilename);
            var models = storage.GetAll();
            Assert.AreEqual(0, models.Count);
        }

        [Test]
        public void AddOneCharacterTest()
        {
            var storageFilename = "TestOneCharacter.json";
            DeleteStorage(storageFilename);

            Debug.Log($"test {storageFilename}");
            var storage1 = new CustomCharacterModelStorage(storageFilename);
            storage1.Save(new CustomCharacterModel(
                10, 20, "Testaaja", 1, 2, 3, 4));

            var storage2 = new CustomCharacterModelStorage(storageFilename);
            var model = storage2.GetCustomCharacterModel(10);
            
            Assert.IsNotNull(model);
            Assert.AreEqual(10, model.Id);
            Assert.AreEqual(20, model.CharacterModelId);
            Assert.AreEqual("Testaaja", model.Name);
            Assert.AreEqual(1, model.Speed);
            Assert.AreEqual(2, model.Resistance);
            Assert.AreEqual(3, model.Attack);
            Assert.AreEqual(4, model.Defence);
        }

        private static void DeleteStorage(string storageFilename)
        {
            var storagePath = Path.Combine(Application.persistentDataPath, storageFilename);
            if (!File.Exists(storagePath))
            {
                File.Delete(storagePath);
            }
        }
    }
}