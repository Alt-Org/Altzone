using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Service.Audio
{
    /// <summary>
    /// AudioManager service loader.
    /// </summary>
    public static class AudioManager
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoad()
        {
            _audioManager = UnityExtensions.CreateGameObjectAndComponent<FmodAudioManager>(nameof(FmodAudioManager), true);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            // Manual reset if UNITY Domain Reloading is disabled.
            _audioManager = null;
        }

        private static IAudioManager _audioManager;

        public static IAudioManager Get()
        {
            Assert.IsNotNull(_audioManager, "_audioManager != null");
            return _audioManager;
        }
    }
}