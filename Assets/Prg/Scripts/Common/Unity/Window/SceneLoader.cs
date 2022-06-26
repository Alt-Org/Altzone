using Prg.Scripts.Common.Unity.Window.ScriptableObjects;
using UnityEngine;
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
            int FindFirstSceneIndex(string sceneName)
            {
                var sceneCount = SceneManager.sceneCountInBuildSettings;
                // scenePath = Assets/UiProto/Scenes/10-uiProto.unity
                var unitySceneName = $"/{sceneName}.unity";
                for (var index = 0; index < sceneCount; ++index)
                {
                    var scenePath = SceneUtility.GetScenePathByBuildIndex(index);
                    if (scenePath.EndsWith(unitySceneName))
                    {
                        return index;
                    }
                }
                throw new UnityException($"scene not found: {sceneName}");
            }
            var scene = windowDef.Scene;
            Assert.IsNotNull(scene, "scene != null");
            Assert.IsFalse(string.IsNullOrEmpty(scene.SceneName), "string.IsNullOrEmpty(scene.SceneName)");
            var sceneIndex = FindFirstSceneIndex(scene.SceneName);
            if (scene.IsNetworkScene)
            {
                Debug.Log($"NETWORK {scene.SceneName} ({sceneIndex})", windowDef);
#if PHOTON_UNITY_NETWORKING
                PhotonNetwork.LoadLevel(scene.SceneName);
                return;
#else
                throw new UnityException("PHOTON_UNITY_NETWORKING not available");
#endif
            }
            Debug.Log($"LOCAL {scene.SceneName} ({sceneIndex})", windowDef);
            SceneManager.LoadScene(sceneIndex);
        }
    }
}