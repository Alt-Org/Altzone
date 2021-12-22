using UnityEngine;
using UnityEngine.SceneManagement;

namespace Prg.Scripts.Common.Unity.Window.ScriptableObjects
{
    /// <summary>
    /// Scene definition for <c>WindowManager</c> and <c>SceneLoader</c>.<br />
    /// It contains UNITY scene name and flag to indicate if it is a networked game scene.
    /// </summary>
    [CreateAssetMenu(menuName = "ALT-Zone/SceneDef", fileName = "scene-")]
    public class SceneDef : ScriptableObject
    {
        [SerializeField] private UnitySceneName _sceneName;
        [SerializeField] private bool _isNetworkScene;

        public string SceneName => _sceneName.sceneName;
        public bool IsNetworkScene => _isNetworkScene;

        public bool NeedsSceneLoad()
        {
            return SceneName != SceneManager.GetActiveScene().name;
        }

        public override string ToString()
        {
            return $"SceneDef: {SceneName}, network: {_isNetworkScene}";
        }
    }
}