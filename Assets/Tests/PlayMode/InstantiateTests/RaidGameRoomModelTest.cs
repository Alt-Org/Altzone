using System.Collections;
using System.Threading.Tasks;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using Altzone.Scripts.Model.Dto;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Assets.Tests.PlayMode.InstantiateTests
{
    /// <summary>
    /// Simple <c>IRaidGameRoomModel</c> prefab instantiation test.
    /// </summary>
    public class RaidGameRoomModelTest : PlayModeTestSupport
    {
        protected override void MyOneTimeSetUp()
        {
            // Load used dependencies here so they are ready when needed. 
            GameConfig.Get();
        }

        [UnityTest]
        public IEnumerator MainTestLoop()
        {
            Debug.Log($"test");

            yield return null;
            GetFirstRaidGameRoomModel();

            // This test must be manually cancelled.
            yield return new WaitUntil(() => IsTestDone);
            Debug.Log($"done {Time.frameCount}");
        }

        private async Task GetFirstRaidGameRoomModel()
        {
            Debug.Log($"start {Time.frameCount}");
            var models = await Store.GetAllRaidGameRoomModels();
            Assert.IsTrue(models.Count > 0);
            MonoBehaviour.StartCoroutine(ShowRaidGameRoom(models[0]));
            Debug.Log($"done {Time.frameCount}");
        }

        private IEnumerator ShowRaidGameRoom(IRaidGameRoomModel room)
        {
            Debug.Log($"room {room}");
            foreach (var furnitureLocation in room.FurnitureLocations)
            {
                var model = Store.GetFurnitureModel(furnitureLocation.FurnitureId);
                Assert.IsNotNull(model);
                Debug.Log($"furniture {model.Id} {model.PrefabName} x,y={furnitureLocation.X},{furnitureLocation.Y}");
                var position = new Vector3(furnitureLocation.X, furnitureLocation.Y, 0);
                var gameObject = FurnitureModel.Instantiate(model, position, Quaternion.identity);
                Assert.NotNull(gameObject);
                yield return null;
            }
        }
    }
}