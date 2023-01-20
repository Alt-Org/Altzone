using Altzone.Scripts.Model;
using Altzone.Scripts.Model.Dto;
using NUnit.Framework;

namespace Assets.Tests.EditMode.ModelsTests
{
    [TestFixture]
    public class FurnitureModelTest
    {
        private const string WhiteBallName = "WhiteBall";
        private const string NotFoundPrefabName = "NotFoundPrefab";
        
        private IStorefront _store;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Debug.Log("setup");
            _store = Storefront.Get();
            Assert.IsNotNull(_store);
        }
        
        [Test]
        public void FurnitureModelInstantiateTest()
        {
            Debug.Log("test");
            var model = _store.GetFurnitureModel(WhiteBallName);
            Assert.IsNotNull(model);
            Assert.AreEqual(model.Name, WhiteBallName);
            var gameObject = FurnitureModel.Instantiate(model);
            Assert.IsNotNull(gameObject);
            Assert.AreEqual(model.Name, gameObject.name);
            Debug.Log($"gameObject {gameObject.name}");
        }        

        [Test]
        public void FurnitureModelInstantiateFailTest()
        {
            Debug.Log("test");
            var model = _store.GetFurnitureModel(NotFoundPrefabName);
            Assert.IsNotNull(model);
            var gameObject = FurnitureModel.Instantiate(model);
            Assert.IsNull(gameObject);
        }        
    }
}