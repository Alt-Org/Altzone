using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Altzone.Scripts.Model;
using NUnit.Framework;
using UnityEngine;

namespace Assets.Tests.EditMode.StorefrontTests
{
    [TestFixture]
    public class StorefrontAsyncTest
    {
        private IStorefront _store;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Debug.Log("setup");
            _store = Storefront.Get();
            Assert.IsNotNull(_store);
            
            // Note that any file InventoryItems.json
            // 1) must exist and
            // 2) it must contain valid items that point to valid furniture
            // for inventory via store tests to pass.
            
            // Stupid busy loop wait for external cloud services...
            while (!_store.IsInventoryConnected)
            {
                Thread.Yield();
            }
        }

        [Test, Description("Test that there is RaidGameRoomModels and we can fetch one by id and name")]
        public async Task RaidGameRoomModelTest()
        {
            Debug.Log("test");
            var models = await _store.GetAllRaidGameRoomModels();
            var randomModel = GetRandomObject(models);
            
            // By Id.
            var model = await _store.GetRaidGameRoomModel(randomModel.Id);
            Assert.IsNotNull(model);
            Assert.AreEqual(randomModel.Id, model.Id);
            
            // By Name.
            model = await _store.GetRaidGameRoomModel(randomModel.Name);
            Assert.IsNotNull(model);
            Assert.AreEqual(randomModel.Id, model.Id);
        }

        [Test]
        public async Task GetAllInventoryItemsTest()
        {
            Debug.Log($"test");
            var items = await _store.GetAllInventoryItems();
            Assert.IsTrue(items.Count > 1);
            Debug.Log($"test items {items.Count}");
        }

        [Test]
        public async Task GetAllFurnitureModelsFromInventoryTest()
        {
            Debug.Log($"test");
            var models = await _store.GetAllFurnitureModelsFromInventory();
            Assert.IsTrue(models.Count > 1);
            Debug.Log($"test models {models.Count}");
        }

        private static T GetRandomObject<T>(IReadOnlyList<T> objectList)
        {
            Assert.IsTrue(objectList.Count > 0);
            var index = Random.Range(0, objectList.Count);
            var instance = objectList[index];
            return instance;
        }
    }
}