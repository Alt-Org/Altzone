using System;
using UnityEngine;

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
            Debug.Log($"{name}");
        }
    }
}