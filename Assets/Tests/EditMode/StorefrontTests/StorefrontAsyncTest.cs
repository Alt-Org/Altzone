using System.Collections.Generic;
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
        }

        [Test, Description("Test that there is RaidGameRoomModels and we can fetch one by id and name")]
        public async Task RaidGameRoomModelTest()
        {
            Debug.Log("test");
            var models = await _store.GetAllRaidGameRoomModels();
            var randomModel = GetRandomObject(models);
            
            // By Id.
            var model = await _store.GetRaidGameRoomModel(randomModel._id);
            Assert.IsNotNull(model);
            Assert.AreEqual(randomModel._id, model._id);
            
            // By Name.
            model = await _store.GetRaidGameRoomModel(randomModel._name);
            Assert.IsNotNull(model);
            Assert.AreEqual(randomModel._id, model._id);
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