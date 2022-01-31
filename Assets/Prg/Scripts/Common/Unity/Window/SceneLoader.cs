using Prg.Scripts.Common.Unity.Window.ScriptableObjects;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
#if PHOTON_UNITY_NETWORKING
using Photon.Pun;
#endif

namespace Prg.Scripts.Common.Unity.Window
{
    /// <summary>
    /// Simple scene loader for <c>WindowManager</c>.
    /// </summary>
    internal static class SceneLoader
    {
        public static void LoadScene(WindowDef windowDef)
        {
            var scene = windowDef.Scene;
            Assert.IsNotNull(scene, "scene != null");
            Assert.IsFalse(string.IsNullOrEmpty(scene.SceneName), "string.IsNullOrEmpty(scene.SceneName)");
            if (scene.IsNetworkScene)
            {
                Debug.Log($"LoadScene NETWORK {scene.SceneName}");
#if PHOTON_UNITY_NETWORKING
                PhotonNetwork.LoadLevel(scene.SceneName);
#else
                throw new UnityException("PHOTON_UNITY_NETWORKING not available");
#endif
            }
            else
            {
                Debug.Log($"LoadScene LOCAL {scene.SceneName}");
                SceneManager.LoadScene(scene.SceneName);
            }
        }
    }
}