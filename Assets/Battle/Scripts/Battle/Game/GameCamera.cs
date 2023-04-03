using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Battle.Game
{
    /// <summary>
    /// Helper for game main camera to find it and change it if required.
    /// </summary>
    internal class GameCamera : MonoBehaviour
    {
        [SerializeField] private Camera _gameCamera;

        public Camera Camera => _gameCamera;

        private void Awake()
        {
            Assert.IsNotNull(_gameCamera, "_gameCamera must be assigned in Editor");
        }
    }
}