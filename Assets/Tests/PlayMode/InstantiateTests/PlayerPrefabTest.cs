using System;
using System.Collections;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Assets.Tests.PlayMode.InstantiateTests
{
    /// <summary>
    /// Simple PlayerPrefab <c>GameObject</c> prefab instantiation test.
    /// </summary>
    public class PlayerPrefabTest : PlayModeTestSupport
    {
        [UnityTest]
        public IEnumerator MainTestLoop()
        {
            var scene = SceneManager.GetActiveScene();
            Debug.Log($"test with scene {scene.buildIndex} {scene.name}");

            InstantiatePlayerPrefab();

            // This test must be manually cancelled.
            while (!IsTestDone)
            {
                yield return null;
            }
            Debug.Log($"done {Time.frameCount}");
        }

        private void InstantiatePlayerPrefab()
        {
            var gameConfig = GameConfig.Get();
            var playerDataCache = gameConfig.PlayerSettings;
            var customCharacterModelId = playerDataCache.CustomCharacterModelId;
            var store = Storefront.Get();
            IBattleCharacter battleCharacter = null;
            var prefabId = 0;
            try
            {
                battleCharacter = store.GetBattleCharacter(customCharacterModelId);
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
            var instance = InstantiatePrefab(playerPrefab, Vector3.one, Quaternion.identity, null);
            instance.name = battleCharacter.Name;
        }

        private GameObject InstantiatePrefab(GameObject template, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            var instance = Object.Instantiate(template, position, rotation, parent);
            return instance;
        }
    }
}