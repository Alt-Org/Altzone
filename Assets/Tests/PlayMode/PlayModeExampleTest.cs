using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Assets.Tests.PlayMode
{
    /// <summary>
    /// This is PlayMode example tests of some UNITY PlayMode test features.
    /// </summary>
    public class PlayModeExampleTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            const int sceneIndex = 0;
            
            Debug.Log($"setup sceneIndex {sceneIndex}");
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                Debug.Log($"sceneLoaded {scene.buildIndex} {scene.name}");
            };
            SceneManager.LoadScene(sceneIndex);
        }

        [UnityTest]
        public IEnumerator WaitForSecondsTest()
        {
            var scene = SceneManager.GetActiveScene();
            Debug.Log($"test with scene {scene.buildIndex} {scene.name}");
            yield return new WaitForSeconds(5f);
            Debug.Log($"done");
        }   
        
        // A Test behaves as an ordinary method
        [Test]
        public void NewTestScriptSimplePasses()
        {
            var scene = SceneManager.GetActiveScene();
            Debug.Log($"test with scene {scene.buildIndex} {scene.name}");
            // Use the Assert class to test conditions
            var frameCount = Time.frameCount;
            Assert.IsTrue(frameCount > 0);
            Debug.Log($"frameCount {frameCount}");
        }

        // A UnityTest behaves like a coroutine in Play Mode.
        // In Play Mode you can use `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator NewTestScriptWithEnumeratorPasses()
        {
            var scene = SceneManager.GetActiveScene();
            Debug.Log($"test with scene {scene.buildIndex} {scene.name}");
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
            var frameCount = Time.frameCount;
            Assert.IsTrue(frameCount > 0);
            Debug.Log($"frameCount {frameCount}");
        }
    }
}
