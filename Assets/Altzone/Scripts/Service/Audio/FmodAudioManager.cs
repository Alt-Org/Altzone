using System;
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

        [AOT.MonoPInvokeCallback(typeof(SYSTEM_CALLBACK))]
        private static RESULT IgnoreDeviceLostCallback(IntPtr system, SYSTEM_CALLBACK_TYPE type, IntPtr commandData1, IntPtr commandData2, IntPtr userdata)
        {
            // Try to avoid error messages by installing this callback.
            // - when running DEVELOPMENT_BUILD we can have unnecessary error messages in devices which is not nice :-(
            // [FMOD] OutputWASAPI::mixerThread : GetCurrentPadding returned 0x88890004. Device was unplugged!
            return RESULT.OK;
        }

        private void Awake()
        {
            var result = RuntimeManager.CoreSystem.getVersion(out var version);
            Assert.AreEqual(RESULT.OK, result);
            Assert.IsTrue(RuntimeManager.IsInitialized, "FMODUnity.RuntimeManager.IsInitialized");
            var verMajor = version >> 16;
            var verMinor = (version >> 8) & 0xF;
            var verDev = version & 0xF;
            Debug.Log($"{name} FMOD ver {verMajor}.{verMinor:00}.{verDev:00}");
            
            result = RuntimeManager.CoreSystem.setCallback(IgnoreDeviceLostCallback, SYSTEM_CALLBACK_TYPE.DEVICELOST);
            Assert.AreEqual(RESULT.OK, result);
        }
    }
}