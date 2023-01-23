using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Assets.Tests.PlayMode.InstantiateTests
{
    /// <summary>
    /// Simple <c>IRaidGameRoomModel</c> prefab instantiation test.
    /// </summary>
    public class RaidGameRoomModelTest : PlayModeTestSupport
    {
        [UnityTest]
        public IEnumerator MainTestLoop()
        {
            var scene = SceneManager.GetActiveScene();
            Debug.Log($"test with scene {scene.buildIndex} {scene.name}");

            CreateRaidGameRoomModel();

            // This test must be manually cancelled.
            while (!IsTestDone)
            {
                yield return null;
            }
            Debug.Log($"done {Time.frameCount}");
        }

        private void CreateRaidGameRoomModel()
        {
        }
    }
}