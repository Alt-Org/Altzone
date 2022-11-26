using System.Linq;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using NUnit.Framework;
using Assert = UnityEngine.Assertions.Assert;

namespace Assets.Tests.EditMode.StorefrontTests
{
    /// <summary>
    /// Tests for <c>IStorefront</c> implementation.
    /// </summary>
    /// <remarks>
    /// Note that most tests rely on that 'enough' Custom Character Models and Character Models exists for tests to succeed.<br />
    /// There is no specific test data for these tests - nor facilities to create them easily.
    /// </remarks>
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

        [Test, Description("Test that there is Character Models and we can fetch one by id")]
        public void CharacterModelTest()
        {
            Debug.Log("test");
            var models = _store.GetAllCharacterClassModels();
            Assert.IsTrue(models.Count > 0);
            var first = models.First(x => x.Id > 0);
            Assert.IsNotNull(first);
            var model = _store.GetCharacterClassModel(first.Id);
            Assert.IsNotNull(model);
            Assert.AreEqual(first.Id, model.Id);
        }

        [Test, Description("Test that non existing Character Model is returned (all values should be '1')")]
        public void CharacterModelNotFoundTest()
        {
            Debug.Log("test");
            const int modelId = int.MaxValue;
            var model = _store.GetCharacterClassModel(modelId);
            Assert.IsNotNull(model);
            Assert.AreEqual(modelId, model.Id);
            Assert.AreEqual(Defence.Desensitisation, model.MainDefence);
            Assert.AreEqual(4, model.Attack + model.Defence + model.Resistance + model.Speed);
        }

        [Test, Description("Test that there is Custom Character Models and we can fetch one by id")]
        public void CustomCharacterModelTest()
        {
            Debug.Log("test");
            var customCharacterModels = _store.GetAllCustomCharacterModels();
            Assert.IsTrue(customCharacterModels.Count > 0);
            var first = customCharacterModels.First(x => x.Id > 0);
            Assert.IsNotNull(first);
            var model = _store.GetCustomCharacterModel(first.Id);
            Assert.IsNotNull(model);
            Assert.AreEqual(first.Id, model.Id);
        }

        [Test, Description("Test that Character Model can be found from Custom Character")]
        public void CustomCharacterModelTest2()
        {
            Debug.Log("test");
            var customCharacterModels = _store.GetAllCustomCharacterModels();
            var index = customCharacterModels.Count / 2;
            Assert.IsTrue(index >= 0);
            var customCharacterModel = customCharacterModels[index];
            var characterModelId = customCharacterModel.CharacterModelId;
            var model = _store.GetCharacterClassModel(characterModelId);
            Assert.AreEqual(customCharacterModel.CharacterModelId, model.Id);
        }

        [Test, Description("Test that there is Battle Characters and we can fetch one by id")]
        public void BattleCharacterTest()
        {
            Debug.Log("test");
            var battleCharacters = _store.GetAllBattleCharacters();
            Assert.IsTrue(battleCharacters.Count > 0);
            var first = battleCharacters.First(x => x.CustomCharacterModelId > 0);
            Assert.IsNotNull(first);
            var character = _store.GetBattleCharacter(first.CustomCharacterModelId);
            Assert.IsNotNull(character);
            Assert.AreEqual(first.CustomCharacterModelId, character.CustomCharacterModelId);
        }

        [Test, Description("Test that Battle Character can be found from Custom Character Model")]
        public void BattleCharacterTest2()
        {
            Debug.Log("test");
            var customCharacterModels = _store.GetAllCustomCharacterModels();
            Assert.IsTrue(customCharacterModels.Count > 0);
            var index = customCharacterModels.Count / 2;
            Assert.IsTrue(index >= 0);
            var customCharacterModel = customCharacterModels[index];
            var customCharacterModelId = customCharacterModel.Id;
            var character = _store.GetBattleCharacter(customCharacterModelId);
            Assert.AreEqual(customCharacterModel.Id, character.CustomCharacterModelId);
        }

        [Test, Description("Test that Battle Character can be found from PlayerDataCache")]
        public void PlayerDataCacheTest()
        {
            var playerDataCache = RuntimeGameConfig.Get().PlayerDataCache;
            var characterModelId = playerDataCache.CustomCharacterModelId;
            var character = _store.GetBattleCharacter(characterModelId);
            Assert.IsNotNull(character);
            Assert.AreEqual(characterModelId, character.CustomCharacterModelId);
        }
    }
}