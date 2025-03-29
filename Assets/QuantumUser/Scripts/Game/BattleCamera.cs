using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.View.Game
{
    public class BattleCamera : MonoBehaviour
    {
        [SerializeField] private Camera _camera;

        public static Camera Camera { get; private set; }

        public static void Rotate180()
        {
            Camera.transform.rotation *= Quaternion.Euler(0, 0, 180);
        }

        private void Awake()
        {
            Assert.IsNotNull(_camera, "_camera must be assigned in Editor");
            Camera = _camera;
        }
    }
}
