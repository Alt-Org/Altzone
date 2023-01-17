using System.Collections;
using System.Linq;
using Altzone.Scripts.Model;
using NUnit.Framework;
using Prg.Scripts.Common.Unity.CameraUtil;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.PlayMode.InventoryTests
{
    public class InventoryTest
    {
        private const string TestCameraName = "TestCamera";

        private IStorefront _store;
        private MonoBehaviour _monoBehaviour;
        private bool _isTestDone;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _store = Storefront.Get();
            Assert.IsNotNull(_store);

            var scene = SceneManager.GetActiveScene();
            Debug.Log($"setup with scene {scene.buildIndex} {scene.name}");
            // Create Camera that we can see what is on the scene.
            var instance = (GameObject)Object.Instantiate(Resources.Load(TestCameraName));
            // Grab something that we need for testing, like starting coroutines.
            _monoBehaviour = instance.GetComponent<CameraAspectRatio>();
        }

        [UnityTest]
        public IEnumerator MainInventoryTestLoop()
        {
            var scene = SceneManager.GetActiveScene();
            Debug.Log($"test with scene {scene.buildIndex} {scene.name}");

            var task = _store.GetAllFurnitureModelsFromInventory();
            yield return new WaitUntil(() => task.IsCompleted);
            Assert.IsTrue(task.IsCompletedSuccessfully);
            var furnitureModels = task.Result;
            Assert.IsTrue(furnitureModels.Count > 0);

            // Create one furniture.
            var furniture = furnitureModels.First(x => x.FurnitureType == FurnitureType.OneSquare);
            _monoBehaviour.StartCoroutine(ShowFurnitureFromInventory(furniture, new Vector2(-1f, 1f)));

            // Create one bomb.
            var bomb = furnitureModels.First(x => x.FurnitureType == FurnitureType.Bomb);
            _monoBehaviour.StartCoroutine(ShowFurnitureFromInventory(bomb, new Vector2(1f, 1f)));

            while (!_isTestDone)
            {
                yield return null;
            }
            Debug.Log($"done {Time.frameCount}");
        }

        private IEnumerator ShowFurnitureFromInventory(FurnitureModel furnitureModel, Vector2 position)
        {
            Debug.Log($"{furnitureModel.Id} {furnitureModel.PrefabName}");
            var instance = furnitureModel.Instantiate(null);
            Assert.IsNotNull(instance);
            instance.transform.position = position;
            yield return new WaitForSeconds(5f * 60f);
            _isTestDone = true;
        }
    }
}