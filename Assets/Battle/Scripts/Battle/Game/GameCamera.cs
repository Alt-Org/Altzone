using UnityEngine;

namespace Battle.Scripts.Battle.Game
{
    /// <summary>
    /// Helper for game main camera to find it and change it if required.
    /// </summary>
    internal class GameCamera : MonoBehaviour, IBattleCamera
    {
        private Camera _gameCamera;

        public Camera Camera => _gameCamera;

        public bool IsRotated => _gameCamera.transform.rotation.z != 0f;

        private void Awake()
        {
            _gameCamera = GetComponent<Camera>();
        }

        public void DisableAudio()
        {
            var cameraAudioListener = GetComponent<AudioListener>();
            if (cameraAudioListener != null)
            {
                cameraAudioListener.enabled = false;
            }
        }
    }
}
