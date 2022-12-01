using UnityEngine;

namespace Prg.Scripts.Common.Unity
{
    public class UnityMonoHelper : MonoBehaviour
    {
        public static UnityMonoHelper Instance
        {
            get
            {
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