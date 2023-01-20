using System.Collections.Generic;
using System.IO;
using Altzone.Scripts.Model;
using Altzone.Scripts.Model.Dto;
using Altzone.Scripts.Model.LocalStorage;
using NUnit.Framework;
using UnityEngine;

namespace Assets.Tests.EditMode.ModelsTests
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
                // TestData/CustomCharacterModels.json uses same IDs
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
            const int modelId = 10;

            var storage1 = new CustomCharacterModelStorage(storageFilename);
            var templateModel = GetTemplateModel(modelId);
            storage1.Save(templateModel);

            var storage2 = new CustomCharacterModelStorage(storageFilename);

            var models = storage2.GetAll();
            Assert.AreEqual(1, models.Count);

            var customCharacterModel = storage2.GetCustomCharacterModel(modelId);
            AssertAreEqual(templateModel, customCharacterModel);
        }

        [Test]
        public void UpdateOneCharacterTest()
        {
            var storageFilename = "TestUpdateOneCharacter.json";
            DeleteStorage(storageFilename);

            Debug.Log($"test {storageFilename}");
            const int modelId = 10;

            var storage1 = new CustomCharacterModelStorage(storageFilename);
            ICustomCharacterModel templateModel = GetTemplateModel(modelId);

            // Add new object.
            const string name1 = "MyName";
            templateModel.Name = name1;
            storage1.Save(templateModel);
            var model1 = storage1.GetCustomCharacterModel(modelId);
            Assert.AreEqual(name1, model1.Name);

            // Update existing object.
            const string name2 = "NewName";
            templateModel.Name = name2;
            storage1.Save(templateModel);
            var model2 = storage1.GetCustomCharacterModel(modelId);
            Assert.AreEqual(name2, model2.Name);
            
            // Verify that updates are written to the storage file.
            var storage3 = new CustomCharacterModelStorage(storageFilename);
            var model3 = storage3.GetCustomCharacterModel(modelId);
            Assert.AreEqual(name2, model3.Name);
        }
        
        [Test]
        public void AddMoreCharactersTest()
        {
            var storageFilename = "TestMoreCharacters.json";
            DeleteStorage(storageFilename);

            Debug.Log($"test {storageFilename}");
            const int modelId1 = 10;
            const int modelId2 = 20;
            const int modelId3 = 30;
            var templateModel1 = GetTemplateModel(modelId1);
            var templateModel2 = GetTemplateModel(modelId2);
            var templateModel3 = GetTemplateModel(modelId3);

            new CustomCharacterModelStorage(storageFilename).Save(templateModel1);
            var count1 = new CustomCharacterModelStorage(storageFilename).GetAll().Count;
            Assert.AreEqual(1, count1);

            new CustomCharacterModelStorage(storageFilename).Save(templateModel2);
            var count2 = new CustomCharacterModelStorage(storageFilename).GetAll().Count;
            Assert.AreEqual(2, count2);

            new CustomCharacterModelStorage(storageFilename).Save(templateModel3);
            var count3 = new CustomCharacterModelStorage(storageFilename).GetAll().Count;
            Assert.AreEqual(3, count3);

            var savedModel1 = new CustomCharacterModelStorage(storageFilename).GetCustomCharacterModel(modelId1);
            AssertAreEqual(templateModel1, savedModel1);

            var savedModel2 = new CustomCharacterModelStorage(storageFilename).GetCustomCharacterModel(modelId2);
            AssertAreEqual(templateModel2, savedModel2);

            var savedModel3 = new CustomCharacterModelStorage(storageFilename).GetCustomCharacterModel(modelId3);
            AssertAreEqual(templateModel3, savedModel3);
        }

        [Test]
        public void DeleteCharacterTest()
        {
            var storageFilename = "TestDeleteCharacter.json";
            DeleteStorage(storageFilename);

            Debug.Log($"test {storageFilename}");
            const int modelId1 = 10;
            const int modelId2 = 20;

            var storage1 = new CustomCharacterModelStorage(storageFilename);
            storage1.Save(GetTemplateModel(modelId1));
            storage1.Save(GetTemplateModel(modelId2));
            
            var storage2 = new CustomCharacterModelStorage(storageFilename);
            var model1 = storage2.GetCustomCharacterModel(modelId1);
            Assert.IsNotNull(model1);
            storage2.Delete(modelId1);
            model1 = storage2.GetCustomCharacterModel(modelId1);
            Assert.IsNull(model1);
            var model2 = storage2.GetCustomCharacterModel(modelId2);
            Assert.IsNotNull(model2);

            var storage3 = new CustomCharacterModelStorage(storageFilename);
            var nullModel = storage3.GetCustomCharacterModel(modelId1);
            Assert.IsNull(nullModel);
            model2 = storage3.GetCustomCharacterModel(modelId2);
            Assert.IsNotNull(model2);
        }

        private static void AssertAreEqual(CustomCharacterModel expected, ICustomCharacterModel actual)
        {
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected._id, actual.Id);
            Assert.AreEqual(expected._characterModelId, actual.CharacterModelId);
            Assert.AreEqual(expected._name, actual.Name);
            Assert.AreEqual(expected._speed, actual.Speed);
            Assert.AreEqual(expected._resistance, actual.Resistance);
            Assert.AreEqual(expected._attack, actual.Attack);
            Assert.AreEqual(expected._defence, actual.Defence);
        }

        private static CustomCharacterModel GetTemplateModel(int id)
        {
            return new CustomCharacterModel(id, 20, "Testaaja", 1, 2, 3, 4);
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