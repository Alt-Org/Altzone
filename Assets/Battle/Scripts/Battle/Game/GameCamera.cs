using UnityEngine;

namespace Battle.Scripts.Battle.Game
{
    /// <summary>
    /// Helper for game main camera to find it and change it if required.
    /// </summary>
    internal class GameCamera : MonoBehaviour, IBattleCamera
    {
        [SerializeField] private Camera _gameCamera;

        Camera IBattleCamera.Camera => _gameCamera;

        bool IBattleCamera.IsRotated => _gameCamera.transform.rotation.z != 0f;

        void IBattleCamera.DisableAudio()
        {
            var cameraAudioListener = GetComponent<AudioListener>();
            if (cameraAudioListener != null)
            {
                cameraAudioListener.enabled = false;
            }
        }
    }
}
