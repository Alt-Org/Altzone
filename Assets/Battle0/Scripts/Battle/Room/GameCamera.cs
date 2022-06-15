using UnityEngine;

namespace Battle0.Scripts.Battle.Room
{
    /// <summary>
    /// Helper for game main camera to find it and change it if required.
    /// </summary>
    public class GameCamera : MonoBehaviour
    {
        [SerializeField] private Camera _gameCamera;

        public Camera Camera => _gameCamera;

        public bool IsRotated => _gameCamera.transform.rotation.z != 0f;

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