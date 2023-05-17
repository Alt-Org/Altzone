using System.Collections;
using NUnit.Framework;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.TestTools;

namespace Prg.Tests
{
    [TestFixture]
    public class UnityHubTest
    {
        [UnityTest]
        public IEnumerator BasicGameObjectTest()
        {
            var game1 = new GameObject("game1");
            yield return null;
            Assert.IsNotNull(game1);
            Debug.Log($"created {game1}");

            var hub = game1.GetHub();
            Debug.Log($"<b>test</b> {hub}");
            Assert.IsNotNull(hub);

            var callbackCount = 0;
            game1.Subscribe<GameObject>(param =>
            {
                callbackCount += 1;
                Debug.Log($"callback for {param.name} count {callbackCount}");
            });
            var handlerCount = hub.CheckHandlerCount();
            Assert.AreEqual(1, handlerCount);
            yield return null;

            game1.Publish(game1);
            Assert.AreEqual(1, callbackCount);
            yield return null;

            game1.Publish(game1);
            Assert.AreEqual(2, callbackCount);
            yield return null;

            // Destroy and invalidate our reference - game1 should be null if referenced again..
            // Subscription should be removed automatically.
            Object.DestroyImmediate(game1);
            yield return null;
            // Jetbrains Rider can not understand this behaviour because game1 should be valid in normal applications - but in UNITY!
            if (game1 == null)
            {
                Assert.IsNull(game1);
            }
            else
            {
                Assert.IsNull(game1);
            }
            Debug.Log($"destroyed {game1}");

            // Subscription should be available.
            handlerCount = hub.CheckHandlerCount();
            Assert.AreEqual(1, handlerCount);

            // Nothing should happen as subscriber has been destroyed.
            game1.Publish(game1);
            Assert.AreEqual(2, callbackCount);
            yield return null;

            // Subscription should be removed automatically.
            handlerCount = hub.CheckHandlerCount();
            Assert.AreEqual(0, handlerCount);
        }

        [UnityTest]
        public IEnumerator UnsubscribeTest()
        {
            var game1 = new GameObject("game1");
            yield return null;
            Assert.IsNotNull(game1);
            Debug.Log($"created {game1}");

            var hub = game1.GetHub();
            Debug.Log($"<b>test</b> {hub}");
            Assert.IsNotNull(hub);

            var callbackCount = 0;
            game1.Subscribe<GameObject>(param =>
            {
                callbackCount += 1;
                Debug.Log($"callback-1 for {param.name} count {callbackCount}");
            });
            var handlerCount = hub.CheckHandlerCount();
            Assert.AreEqual(1, handlerCount);
            yield return null;

            game1.Subscribe<GameObject>(param =>
            {
                callbackCount += 1;
                Debug.Log($"callback-2 for {param.name} count {callbackCount}");
            });
            // No callbacks, handlers created.
            Assert.AreEqual(0, callbackCount);
            handlerCount = hub.CheckHandlerCount();
            Assert.AreEqual(2, handlerCount);
            yield return null;

            game1.Publish(game1);
            // Yes callbacks, handlers created.
            Assert.AreEqual(2, callbackCount);
            handlerCount = hub.CheckHandlerCount();
            Assert.AreEqual(2, handlerCount);
            yield return null;

            game1.Unsubscribe();
            // No more callbacks, handlers removed.
            Assert.AreEqual(2, callbackCount);
            handlerCount = hub.CheckHandlerCount();
            Assert.AreEqual(0, handlerCount);
            yield return null;

            game1.Publish(game1);
            // Same as before - no more callbacks, no handlers.
            Assert.AreEqual(2, callbackCount);
            handlerCount = hub.CheckHandlerCount();
            Assert.AreEqual(0, handlerCount);
        }
    }
}
