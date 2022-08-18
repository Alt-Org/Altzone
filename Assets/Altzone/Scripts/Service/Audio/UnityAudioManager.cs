using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Altzone.Scripts.Config;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Service.Audio
{
    /// <summary>
    /// AudioManager service with simple and most stupid (KISS) UNITY implementation.
    /// </summary>
    public class UnityAudioManager : MonoBehaviour, IAudioManager
    {
        private const float DefaultVolume = 1.0f;

        [Header("Live Data"), SerializeField] private float _masterVolume;
        [SerializeField] private float _menuEffectsVolume;
        [SerializeField] private float _gameEffectsVolume;
        [SerializeField] private float _gameMusicVolume;

        private readonly Dictionary<string, AudioClip> _audioClips = new();

        float IAudioManager.MasterVolume
        {
            get => _masterVolume;
            set => _masterVolume = value;
        }

        float IAudioManager.MenuEffectsVolume
        {
            get => _menuEffectsVolume;
            set => _menuEffectsVolume = value;
        }

        float IAudioManager.GameEffectsVolume
        {
            get => _gameEffectsVolume;
            set => _gameEffectsVolume = value;
        }

        float IAudioManager.GameMusicVolume
        {
            get => _gameMusicVolume;
            set => _gameMusicVolume = value;
        }

        private void Awake()
        {
            _masterVolume = PlayerPrefs.GetFloat(PlayerPrefKeys.MasterVolume, DefaultVolume);
            _menuEffectsVolume = PlayerPrefs.GetFloat(PlayerPrefKeys.MenuSfxVolume, DefaultVolume);
            _gameEffectsVolume = PlayerPrefs.GetFloat(PlayerPrefKeys.GameSfxVolume, DefaultVolume);
            _gameMusicVolume = PlayerPrefs.GetFloat(PlayerPrefKeys.GameMusicVolume, DefaultVolume);
        }

        void IAudioManager.RegisterAudioClip(string audioClipName, AudioClip audioClip)
        {
            Assert.IsFalse(_audioClips.ContainsKey(audioClipName), "_audioClips.ContainsKey(audioClipName)");
            _audioClips.Add(audioClipName, audioClip);
        }

        void IAudioManager.PlayMenuEffect(string audioClipName)
        {
            Assert.IsTrue(_audioClips.ContainsKey(audioClipName), "_audioClips.ContainsKey(audioClipName)");
            ((IAudioManager)this).PlayMenuEffect(_audioClips[audioClipName]);
        }

        void IAudioManager.PlayGameEffect(string audioClipName)
        {
            Assert.IsTrue(_audioClips.ContainsKey(audioClipName), "_audioClips.ContainsKey(audioClipName)");
            ((IAudioManager)this).PlayGameEffect(_audioClips[audioClipName]);
        }

        void IAudioManager.PlayGameEffect(string audioClipName, Vector3 worldPosition)
        {
            Assert.IsTrue(_audioClips.ContainsKey(audioClipName), "_audioClips.ContainsKey(audioClipName)");
            ((IAudioManager)this).PlayGameEffect(_audioClips[audioClipName], worldPosition);
        }

        void IAudioManager.PlayGameMusic(string audioClipName)
        {
            Assert.IsTrue(_audioClips.ContainsKey(audioClipName), "_audioClips.ContainsKey(audioClipName)");
            ((IAudioManager)this).PlayGameMusic(_audioClips[audioClipName]);
        }

        void IAudioManager.PlayMenuEffect(AudioClip audioClip)
        {
            PlayAudio(audioClip, _menuEffectsVolume * _masterVolume);
        }

        void IAudioManager.PlayGameEffect(AudioClip audioClip)
        {
            PlayAudio(audioClip, _gameEffectsVolume * _masterVolume);
        }

        void IAudioManager.PlayGameEffect(AudioClip audioClip, Vector3 worldPosition)
        {
            PlayAudio(audioClip, _gameEffectsVolume * _masterVolume, worldPosition);
        }

        void IAudioManager.PlayGameMusic(AudioClip audioClip)
        {
            PlayAudio(audioClip, _gameMusicVolume * _masterVolume);
        }

        private static void PlayAudio(AudioClip clip, float volume, [CallerMemberName] string memberName = null)
        {
            if (!(volume > Mathf.Epsilon))
            {
                return;
            }
            Debug.Log($"{memberName} {clip.name} {volume}");
            AudioSource.PlayClipAtPoint(clip, Vector3.zero, volume);
        }

        private static void PlayAudio(AudioClip clip, float volume, Vector3 position, [CallerMemberName] string memberName = null)
        {
            if (!(volume > 0))
            {
                return;
            }
            Debug.Log($"{memberName} {clip.name} {volume} {position}");
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }
    }
}