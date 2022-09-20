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
            _instance = null;
        }

        private static IAudioManager _instance;

        public static IAudioManager Get()
        {
            return _instance ??= UnitySingleton.CreateStaticSingleton<UnityAudioManager>();
        }
    }
}
