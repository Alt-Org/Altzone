using Altzone.Scripts.Model;
using NUnit.Framework;
using Assert = UnityEngine.Assertions.Assert;

namespace Assets.Tests.EditMode.ModelsTests
{
    [TestFixture]
    public class FurnitureModelTest
    {
        private const string ModelName = "WhiteBall";
        
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
            var model = _store.GetFurnitureModel(ModelName);
            Assert.IsNotNull(model);
            Assert.AreEqual(model.Name, ModelName);
            var gameObject = model.Instantiate(null);
            Assert.IsNotNull(gameObject);
            Assert.AreEqual(model.Name, gameObject.name);
        }        

        [Test]
        public void FurnitureModelInstantiateFailTest()
        {
            Debug.Log("test");
            var model = _store.GetFurnitureModel(1);
            Assert.IsNotNull(model);
            var gameObject = model.Instantiate(null);
            Assert.IsNull(gameObject);
        }        
    }
}