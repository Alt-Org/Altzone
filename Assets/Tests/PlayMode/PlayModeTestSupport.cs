using Altzone.Scripts.Model;
using NUnit.Framework;
using Prg.Scripts.Common.Unity.CameraUtil;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tests.PlayMode
{
    public abstract class PlayModeTestSupport
    {
        private const string TestCameraName = "TestCamera";

        protected IStorefront Store;
        protected MonoBehaviour MonoBehaviour;
        protected bool IsTestDone;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Store = Storefront.Get();
            Assert.IsNotNull(Store);

            var scene = SceneManager.GetActiveScene();
            Debug.Log($"setup with scene {scene.buildIndex} {scene.name}");
            // Create Camera that we can see what is on the scene.
            var instance = (GameObject)Object.Instantiate(Resources.Load(TestCameraName));
            // Grab something that we need for testing, like starting coroutines.
            MonoBehaviour = instance.GetComponent<CameraAspectRatio>();
            IsTestDone = false;
            MyOneTimeSetUp();
            Debug.Log($"done");
        }

        protected virtual void MyOneTimeSetUp()
        {
        }
    }
}