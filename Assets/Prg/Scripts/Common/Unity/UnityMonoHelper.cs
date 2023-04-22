using UnityEditor;
using UnityEngine;

namespace Prg.Scripts.Common.Unity
{
    /// <summary>
    /// Singleton to give access to <c>MonoBehaviour</c> public methods for those classes that are not <c>MonoBehaviour</c> themselves.<br />
    /// Typical use case is to call StartCoroutine().
    /// </summary>
    /// <remarks>
    /// See also <c>UnityExtensions</c> ExecuteOnNextFrame() and ExecuteAsCoroutine() if you want to call any <c>Action</c> as <c>Coroutine</c>.
    /// </remarks>
    public class UnityMonoHelper : MonoBehaviour
    {
        public static UnityMonoHelper Instance
        {
            get
            {
#if UNITY_EDITOR
                if (!EditorApplication.isPlaying)
                {
                    // If game is not running there is no use to create this helper!
                    return null;
                }
#endif
                var instance = FindObjectOfType<UnityMonoHelper>();
                if (instance == null)
                {
                    var parent = new GameObject(nameof(UnityMonoHelper));
                    DontDestroyOnLoad(parent);
                    instance = parent.AddComponent<UnityMonoHelper>();
                }
                return instance;
            }
        }
    }
}