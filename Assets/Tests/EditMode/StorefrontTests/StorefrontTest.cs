using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using Altzone.Scripts.Model.Dto;
using NUnit.Framework;
using UnityEngine;

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

        [Test]
        public void FurnitureModelTestGetOne()
        {
            Debug.Log("test");
            var models = _store.GetAllFurnitureModels();
            var randomModel = GetRandomObject(models);
            var model = _store.GetCharacterClassModel(randomModel.Id);
            Assert.IsNotNull(model);
            Assert.AreEqual(randomModel.Id, model.Id);
        }

        [Test]
        public void FurnitureModelTestGetAll()
        {
            Debug.Log("test");
            var models = _store.GetAllFurnitureModels();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Count > 0);
        }

        [Test, Description("Test that there is Character Models and we can fetch one by id")]
        public void CharacterModelTest()
        {
            Debug.Log("test");
            var characterClassModels = _store.GetAllCharacterClassModels();
            var characterClassModel = GetRandomObject(characterClassModels);
            Assert.IsNotNull(characterClassModel);
            var model = _store.GetCharacterClassModel(characterClassModel.Id);
            Assert.IsNotNull(model);
            Assert.AreEqual(characterClassModel.Id, model.Id);
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
            var customCharacterModel = GetRandomObject(customCharacterModels);
            Assert.IsNotNull(customCharacterModel);
            var model = _store.GetCustomCharacterModel(customCharacterModel.Id);
            Assert.IsNotNull(model);
            Assert.AreEqual(customCharacterModel.Id, model.Id);
        }

        [Test, Description("Test that Character Model can be found from Custom Character")]
        public void CustomCharacterModelTest2()
        {
            Debug.Log("test");
            var customCharacterModels = _store.GetAllCustomCharacterModels();
            var customCharacterModel = GetRandomObject(customCharacterModels);
            var characterModelId = customCharacterModel.CharacterModelId;
            var model = _store.GetCharacterClassModel(characterModelId);
            Assert.AreEqual(customCharacterModel.CharacterModelId, model.Id);
        }

        [Test, Description("Test that there is Battle Characters and we can fetch one by id")]
        public void BattleCharacterTest()
        {
            Debug.Log("test");
            var battleCharacters = _store.GetAllBattleCharacters();
            var battleCharacter = GetRandomObject(battleCharacters);
            var customCharacterModelId = battleCharacter.CustomCharacterModelId;
            var character = _store.GetBattleCharacter(customCharacterModelId);
            Assert.IsNotNull(character);
            Assert.AreEqual(battleCharacter.CustomCharacterModelId, character.CustomCharacterModelId);
        }

        [Test, Description("Test that Battle Character can be found from Custom Character Model")]
        public void BattleCharacterTest2()
        {
            Debug.Log("test");
            var customCharacterModels = _store.GetAllCustomCharacterModels();
            var customCharacterModel = GetRandomObject(customCharacterModels);
            var customCharacterModelId = customCharacterModel.Id;
            var character = _store.GetBattleCharacter(customCharacterModelId);
            Assert.AreEqual(customCharacterModel.Id, character.CustomCharacterModelId);
        }

        [Test, Description("Test that Battle Character can be found from PlayerDataCache")]
        public void PlayerDataCacheTest()
        {
            var playerDataCache = GameConfig.Get().PlayerSettings;
            var characterModelId = playerDataCache.CustomCharacterModelId;
            var battleCharacter = _store.GetBattleCharacter(characterModelId);
            Assert.IsNotNull(battleCharacter);
            Assert.AreEqual(characterModelId, battleCharacter.CustomCharacterModelId);
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