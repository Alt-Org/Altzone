using System;
using System.Collections;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

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
            var playerDataCache = gameConfig.PlayerSettings;
            var playerDataModel = gameConfig.PlayerDataModel;
            var customCharacterModelId = playerDataModel.CurrentCustomCharacterId;
            var store = Storefront.Get();
            BattleCharacter battleCharacter = null;
            var prefabId = 0;
            try
            {
                battleCharacter = store.GetBattleCharacter(customCharacterModelId);
                Debug.Log($"{battleCharacter}");
                Assert.IsFalse(string.IsNullOrWhiteSpace(battleCharacter.PlayerPrefabKey));
                prefabId =  int.Parse(battleCharacter.PlayerPrefabKey);
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
            var instance = InstantiatePrefab(playerPrefab.gameObject, Vector3.one, Quaternion.identity, null);
            instance.name = battleCharacter.Name;
        }

        private GameObject InstantiatePrefab(GameObject template, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            var instance = Object.Instantiate(template, position, rotation, parent);
            return instance;
        }
    }
}