using System.Linq;
using Altzone.Scripts.Model;
using NUnit.Framework;
using Assert = UnityEngine.Assertions.Assert;

namespace Assets.Tests.EditMode.StorefrontTests
{
    [TestFixture]
    public class StorefrontTest
    {
        private IStorefront _store;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Debug.Log("setup");
            _store = Storefront.Get();
            Assert.IsNotNull(_store);
        }

        [Test]
        public void CharacterModelTest()
        {
            Debug.Log("test");
            var models = _store.GetAllCharacterModels();
            Assert.IsTrue(models.Count > 0);
            var first = models.First(x => x.Id > 0);
            Assert.IsNotNull(first);
            var model = _store.GetCharacterModel(first.Id);
            Assert.IsNotNull(model);
            Assert.AreEqual(first.Id, model.Id);
        }

        [Test, Description("Test that non existing Character Model is returned (all values should be '1')")]
        public void CharacterModelNotFoundTest()
        {
            Debug.Log("test");
            const int modelId = int.MaxValue;
            var model = _store.GetCharacterModel(modelId);
            Assert.IsNotNull(model);
            Assert.AreEqual(modelId, model.Id);
            Assert.AreEqual(Defence.Desensitisation, model.MainDefence);
            Assert.AreEqual(4, model.Attack + model.Defence + model.Resistance + model.Speed);
        }

        [Test]
        public void CustomCharacterModelTest()
        {
            Debug.Log("test");
            var models = _store.GetAllCustomCharacterModels();
            Assert.IsTrue(models.Count > 0);
            var first = models.First(x => x.Id > 0);
            Assert.IsNotNull(first);
            var model = _store.GetCustomCharacterModel(first.Id);
            Assert.IsNotNull(model);
            Assert.AreEqual(first.Id, model.Id);
        }

        [Test, Description("Test that Character Model can be found from Custom Character")]
        public void CustomCharacterModelTest2()
        {
            Debug.Log("test");
            var models = _store.GetAllCustomCharacterModels();
            var index = models.Count / 2;
            Assert.IsTrue(index >= 0);
            var customCharacterModel = models[index];
            var characterModelId = customCharacterModel.CharacterModelId;
            var model = _store.GetCharacterModel(characterModelId);
            Assert.AreEqual(customCharacterModel.CharacterModelId, model.Id);
        }

        [Test]
        public void BattleCharacterTest()
        {
            Debug.Log("test");
            var characters = _store.GetAllBattleCharacters();
            Assert.IsTrue(characters.Count > 0);
            var first = characters.First(x => x.CustomCharacterModelId > 0);
            Assert.IsNotNull(first);
            var character = _store.GetBattleCharacter(first.CustomCharacterModelId);
            Assert.IsNotNull(character);
            Assert.AreEqual(first.CustomCharacterModelId, character.CustomCharacterModelId);
        }

        [Test, Description("Test that Battle Character can be found from Custom Character")]
        public void BattleCharacterTest2()
        {
            Debug.Log("test");
            var models = _store.GetAllCustomCharacterModels();
            var index = models.Count / 2;
            Assert.IsTrue(index >= 0);
            var customCharacterModel = models[index];
            var customCharacterModelId = customCharacterModel.Id;
            var character = _store.GetBattleCharacter(customCharacterModelId);
            Assert.AreEqual(customCharacterModel.Id, character.CustomCharacterModelId);
        }
    }
}