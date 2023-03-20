using System;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using NUnit.Framework;

namespace Tests.EditMode.ModelsTests
{
    [TestFixture]
    public class PlayerPrefabTest
    {
        [Test]
        public void GetCurrentPlayerPrefabTest()
        {
            Debug.Log($"test");
            var gameConfig = GameConfig.Get();
            var playerSettings = gameConfig.PlayerSettings;
            var playerGuid = playerSettings.PlayerGuid;
            var store = Storefront.Get();
            store.GetPlayerData(playerGuid, playerData =>
            {
                var battleCharacter = playerData.BattleCharacter;
                Assert.IsNotNull(battleCharacter);
                Debug.Log($"{battleCharacter}");
                Assert.IsFalse(string.IsNullOrWhiteSpace(battleCharacter.UnityKey));
                var prefabId = int.Parse(battleCharacter.UnityKey);
                Assert.IsTrue(prefabId >= 0);
                var playerPrefabs = gameConfig.PlayerPrefabs;
                var playerPrefab = playerPrefabs.GetPlayerPrefab(prefabId);
                Assert.IsNotNull(playerPrefab);
            });
        }

        [Test]
        public void GetAllPlayerPrefabsTest()
        {
            Debug.Log($"test");
            var playerPrefabs = GameConfig.Get().PlayerPrefabs;
            Storefront.Get().ForTest.GetAllBattleCharactersTest(battleCharacters =>
            {
                foreach (var battleCharacter in battleCharacters)
                {
                    var prefabId = 0;
                    Assert.IsFalse(string.IsNullOrWhiteSpace(battleCharacter.UnityKey));
                    prefabId = int.Parse(battleCharacter.UnityKey);
                    Assert.IsTrue(prefabId >= 0);
                    var playerPrefab = playerPrefabs.GetPlayerPrefab(prefabId);
                    Assert.IsNotNull(playerPrefab);
                }
            });
        }
    }
}