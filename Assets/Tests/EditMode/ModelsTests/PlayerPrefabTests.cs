using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using NUnit.Framework;
using Assert = UnityEngine.Assertions.Assert;

namespace Assets.Tests.EditMode.ModelsTests
{
    [TestFixture]
    public class PlayerPrefabTests
    {
        [Test]
        public void GetCurrentPlayerPrefabTest()
        {
            Debug.Log($"test");
            var playerDataCache = GameConfig.Get().PlayerDataCache;
            var customCharacterModelId = playerDataCache.CustomCharacterModelId;
            var battleCharacter = Storefront.Get().GetBattleCharacter(customCharacterModelId);

            var prefabId = battleCharacter.PlayerPrefabId;
            Assert.IsTrue(prefabId >= 0);

            var playerPrefabs = GameConfig.Get().PlayerPrefabs;
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