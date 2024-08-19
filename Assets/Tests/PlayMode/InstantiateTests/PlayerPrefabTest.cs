using System.Collections;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode.InstantiateTests
{
    /// <summary>
    /// Simple PlayerPrefab <c>GameObject</c> prefab instantiation test.
    /// </summary>
    public class PlayerPrefabTest : PlayModeTestSupport
    {
        [UnityTest]
        public IEnumerator MainTestLoop()
        {
            Debug.Log($"test");

            InstantiatePlayerPrefab();

            // This test must be manually cancelled.
            yield return new WaitUntil(() => IsTestDone);
            Debug.Log($"done {Time.frameCount}");
        }

        private void InstantiatePlayerPrefab()
        {
            var gameConfig = GameConfig.Get();
            var playerSettings = gameConfig.PlayerSettings;
            var playerGuid = playerSettings.PlayerGuid;
            var store = Storefront.Get();
            store.GetPlayerData(playerGuid, playerData =>
            {
                var battleCharacter = playerData.BattleCharacter;
                Assert.IsNotNull(battleCharacter);
                Debug.Log($"{battleCharacter}");
                var playerPrefabs = gameConfig.PlayerPrefabs;
                var playerPrefab = playerPrefabs.GetPlayerPrefab((int)battleCharacter.CustomCharacterId);
                Assert.IsNotNull(playerPrefab);
                var instance = InstantiatePrefab(playerPrefab.gameObject, Vector3.one, Quaternion.identity, null);
                instance.name = battleCharacter.Name;
            });
        }

        private static GameObject InstantiatePrefab(GameObject template, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            var instance = Object.Instantiate(template, position, rotation, parent);
            return instance;
        }
    }
}
