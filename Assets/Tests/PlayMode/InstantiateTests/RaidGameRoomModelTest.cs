using System.Collections;
using System.Threading.Tasks;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using Altzone.Scripts.Model.Dto;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

#pragma warning disable CS4014 // Ignore warnings about "not awaited" calls

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
        public IEnumerator ShowRaidGameRoomOne()
        {
            Debug.Log($"test");

            yield return null;
            ShowRaidGameRoom(1);

            // This test must be manually cancelled.
            yield return new WaitUntil(() => IsTestDone);
            Debug.Log($"done {Time.frameCount}");
        }

        [UnityTest]
        public IEnumerator ShowRaidGameRoomTwo()
        {
            Debug.Log($"test");

            yield return null;
            ShowRaidGameRoom(2);

            // This test must be manually cancelled.
            yield return new WaitUntil(() => IsTestDone);
            Debug.Log($"done {Time.frameCount}");
        }
        
        private async Task ShowRaidGameRoom(int id)
        {
            Debug.Log($"start {Time.frameCount}");
            var model = await Store.GetRaidGameRoomModel(id);
            Assert.IsNotNull(model);
            MonoBehaviour.StartCoroutine(ShowRaidGameRoom(model));
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