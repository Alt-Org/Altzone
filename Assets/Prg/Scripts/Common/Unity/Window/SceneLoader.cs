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
                Debug.Log($"LoadScene NETWORK {scene.SceneName}", windowDef);
#if PHOTON_UNITY_NETWORKING
                PhotonNetwork.LoadLevel(scene.SceneName);
                return;
#else
                throw new UnityException("PHOTON_UNITY_NETWORKING not available");
#endif
            }

            var sceneCount = SceneManager.sceneCountInBuildSettings;
            // scenePath = Assets/UiProto/Scenes/10-uiProto.unity
            var unitySceneNameMatcher = $"/{scene.SceneName}.unity";
            for (var index = 0; index < sceneCount; ++index)
            {
                var scenePath = SceneUtility.GetScenePathByBuildIndex(index);
                if (scenePath.EndsWith(unitySceneNameMatcher))
                {
                    Debug.Log($"LoadScene LOCAL {scene.SceneName} ({index})");
                    SceneManager.LoadScene(index);
                    return;
                }
            }
            var firstScenePath = SceneUtility.GetScenePathByBuildIndex(0);
            Debug.LogWarning($"LoadScene LOCAL FALLBACK {firstScenePath} (0)");
            SceneManager.LoadScene(0);
        }
    }
}