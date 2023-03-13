using Altzone.Scripts;
using Altzone.Scripts.Model;
using NUnit.Framework;
using Prg.Scripts.Common.Unity.CameraUtil;
using Prg.Scripts.Common.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tests.PlayMode
{
    public abstract class PlayModeTestSupport
    {
        private const string TestCameraName = "TestCamera";

        protected DataStore Store;
        protected Camera Camera;
        protected MonoBehaviour MonoBehaviour;
        protected bool IsTestDone;
        private LogFileWriter _logFileWriter;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _logFileWriter = LogFileWriter.CreateLogFileWriter();
            Store = Storefront.Get();
            Assert.IsNotNull(Store);

            var scene = SceneManager.GetActiveScene();
            Debug.Log($"setup with scene {scene.buildIndex} {scene.name}");
            // Create Camera that we can see what is on the scene.
            var instance = (GameObject)Object.Instantiate(Resources.Load(TestCameraName));
            // Get our test components.
            Camera = instance.GetComponent<Camera>();
            MonoBehaviour = instance.GetComponent<CameraAspectRatio>();
            IsTestDone = false;
            MyOneTimeSetUp();
            Debug.Log($"done");
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            Debug.Log("exit");
            _logFileWriter.Close();
        }

        protected virtual void MyOneTimeSetUp()
        {
        }
    }
}