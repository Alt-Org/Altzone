using UnityEngine;

namespace Battle.Scripts.Battle.Room
{
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