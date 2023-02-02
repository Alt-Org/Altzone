using System.Collections;
using System.Linq;
using Altzone.Scripts.Model;
using Altzone.Scripts.Model.Dto;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode.InstantiateTests
{
    /// <summary>
    /// Simple Inventory <c>IFurnitureModel</c> prefab instantiation test.
    /// </summary>
    public class InventoryTest : PlayModeTestSupport
    {
        [UnityTest]
        public IEnumerator MainTestLoop()
        {
            Debug.Log($"test");

            var task = Store.GetAllFurnitureModelsFromInventory();
            yield return new WaitUntil(() => task.IsCompleted);
            Assert.IsTrue(task.IsCompletedSuccessfully);
            var furnitureModels = task.Result;
            Assert.IsTrue(furnitureModels.Count > 0);

            // Find and show one furniture - fails if none found.
            var furniture = furnitureModels.First(x => x.FurnitureType == FurnitureType.OneSquare);
            MonoBehaviour.StartCoroutine(ShowFurniturePiece(furniture, new Vector2(-1.5f, 1.5f)));

            // Find and show one bomb - fails if none found.
            var bomb = furnitureModels.First(x => x.FurnitureType == FurnitureType.Bomb);
            MonoBehaviour.StartCoroutine(ShowFurniturePiece(bomb, new Vector2(1f, 1f)));

            // This test must be manually cancelled.
            yield return new WaitUntil(() => IsTestDone);
            Debug.Log($"done {Time.frameCount}");
        }

        private static IEnumerator ShowFurniturePiece(IFurnitureModel furnitureModel, Vector2 position)
        {
            Debug.Log($"{furnitureModel.Id} {furnitureModel.PrefabName}");
            var instance = FurnitureModel.Instantiate(furnitureModel, position, Quaternion.identity);
            Assert.IsNotNull(instance);
            yield return null;
        }
    }
}