using System;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using NUnit.Framework;

namespace Assets.Tests.EditMode.ModelsTests
{
    [TestFixture]
    public class PlayerPrefabTests
    {
        [Test]
        public void GetCurrentPlayerPrefabTest()
        {
            Debug.Log($"test");
            var gameConfig = GameConfig.Get();
            var playerDataCache = GameConfig.Get().PlayerSettings;
            var customCharacterModelId = playerDataCache.CustomCharacterModelId;
            var store = Storefront.Get();
            var prefabId = 0;
            try
            {
                var battleCharacter = store.GetBattleCharacter(customCharacterModelId);
                Debug.Log($"{battleCharacter}");
                prefabId = battleCharacter.PlayerPrefabId;
                Assert.IsTrue(prefabId >= 0);
            }
            catch (Exception e)
            {
                Debug.Log($"GetBattleCharacter failed {e.Message}");
                Assert.Fail("Check that CustomCharacterModels exist or restart UNITY to reset Storefront");
            }

            var playerPrefabs = gameConfig.PlayerPrefabs;
            var playerPrefab = playerPrefabs.GetPlayerPrefab(prefabId);
            Assert.IsNotNull(playerPrefab);
        }

        [Test]
        public void GetAllPlayerPrefabsTest()
        {
            Debug.Log($"test");
            var playerPrefabs = GameConfig.Get().PlayerPrefabs;
            var battleCharacters = Storefront.Get().GetAllBattleCharacters();
            foreach (var battleCharacter in battleCharacters)
            {
                var prefabId = battleCharacter.PlayerPrefabId;
                Assert.IsTrue(prefabId >= 0);
                var playerPrefab = playerPrefabs.GetPlayerPrefab(prefabId);
                Assert.IsNotNull(playerPrefab);
            }
        }
    }
}