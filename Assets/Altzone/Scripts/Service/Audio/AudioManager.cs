using UnityEngine;

namespace Altzone.Scripts.Service.Audio
{
    /// <summary>
    /// AudioManager service loader.
    /// </summary>
    public static class AudioManager
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            // Manual reset if UNITY Domain Reloading is disabled.
            _audioManager = null;
        }

        private static IAudioManager _audioManager;

        public static IAudioManager Get()
        {
            if (_audioManager == null)
            {
                _audioManager = UnityExtensions.CreateGameObjectAndComponent<UnityAudioManager>(nameof(UnityAudioManager), true);
            }
            return _audioManager;
        }
    }
}
