using System;
using Altzone.Scripts.Config;
using FMOD;
using FMODUnity;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Service.Audio
{
    /// <summary>
    /// AudioManager service FMOD implementation.
    /// </summary>
    public class FmodAudioManager : MonoBehaviour, IAudioManager
    {
        public float MasterVolume
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public float MenuEffectsVolume
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public float GameEffectsVolume
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public float GameMusicVolume
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        private void Awake()
        {
            var result = RuntimeManager.CoreSystem.getVersion(out var version);
            Assert.AreEqual(RESULT.OK, result);
            Assert.IsTrue(RuntimeManager.IsInitialized, "FMODUnity.RuntimeManager.IsInitialized");
            var verMajor = version >> 16;
            var verMinor = (version >> 8) & 0xF;
            var verDev = version & 0xF;
            var features = RuntimeGameConfig.Get().Features;
            var isMuted = features._isMuteAllSounds;
            Debug.Log($"{name} FMOD ver {verMajor}.{verMinor:00}.{verDev:00} mute {isMuted}");

            RuntimeManager.MuteAllEvents(isMuted);
        }
    }
}