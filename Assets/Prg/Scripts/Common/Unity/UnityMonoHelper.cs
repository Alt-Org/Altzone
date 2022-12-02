using UnityEditor;
using UnityEngine;

namespace Prg.Scripts.Common.Unity
{
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
                    var parent = new GameObject
                    {
                        name = nameof(UnityMonoHelper)
                    };
                    DontDestroyOnLoad(parent);
                    instance = parent.AddComponent<UnityMonoHelper>();
                }
                return instance;
            }
        }
    }
}