using System.Collections;
using Altzone.Scripts.Model;
using Altzone.Scripts.Model.Dto;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode.InstantiateTests
{
    /// <summary>
    /// Show all <c>IFurnitureModel</c> prefabs.
    /// </summary>
    public class FurnitureModelTest : PlayModeTestSupport
    {
        [UnityTest]
        public IEnumerator MainTestLoop()
        {
            Debug.Log($"test");

            var models = Store.GetAllFurnitureModels();
            var furnitureCount = models.Count;
            var rowCount = (furnitureCount + 1) / 2;
            var xOffset = 1f / (rowCount + 1);
            const int colCount = 3;
            const float yOffset = 1f / (colCount + 1);
            Vector3 viewPos = new Vector3();
            var furnitureIndex = 0;
            for (var row = 0; row < rowCount && furnitureIndex < furnitureCount; ++row)
            {
                for (var col = 0; col < colCount && furnitureIndex < furnitureCount; ++col)
                {
                    var furniture = models[furnitureIndex];
                    viewPos.x = yOffset + yOffset * col;
                    viewPos.y = xOffset + xOffset * row;
                    var worldPos = Camera.ViewportToWorldPoint(viewPos);
                    Vector2 furniturePos;
                    furniturePos.x = worldPos.x;
                    furniturePos.y = worldPos.y;
                    yield return ShowFurniturePiece(furniture, furniturePos);
                    ++furnitureIndex;
                }
            }

            // This test must be manually cancelled.
            yield return new WaitUntil(() => IsTestDone);
            Debug.Log($"done {Time.frameCount}");
        }

        private static IEnumerator ShowFurniturePiece(IFurnitureModel furnitureModel, Vector2 position)
        {
            Debug.Log($"{furnitureModel.Id} {furnitureModel.PrefabName} pos {position}");
            var instance = FurnitureModel.Instantiate(furnitureModel, position, Quaternion.identity);
            if (instance == null)
            {
                Debug.Log($"SKIPPED {furnitureModel.PrefabName}");
            }
            yield return null;
        }
    }
}