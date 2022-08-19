using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Altzone.Scripts.Config;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Service.Audio
{
    /// <summary>
    /// UNITY implementation for AudioManager service.
    /// </summary>
    /// <remarks>
    /// Cached <c>AudioSource</c> will be created for all registered <c>AudioClip</c>'s and
    /// ad hoc <c>AudioClip</c>'s will be played using temporary <c>AudioSource</c> that creates some overhead and garbage.
    /// </remarks>
    public class UnityAudioManager : MonoBehaviour, IAudioManager
    {
        private const float DefaultVolume = 1.0f;

        [Header("Settings"), SerializeField] private float _minVolumeThreshold = 0.01f;

        [Header("Live Data"), SerializeField] private float _masterVolume;
        [SerializeField] private float _menuEffectsVolume;
        [SerializeField] private float _gameEffectsVolume;
        [SerializeField] private float _gameMusicVolume;

        private readonly Dictionary<string, AudioSource> _audioClips = new();

        float IAudioManager.MasterVolume
        {
            get => _masterVolume;
            set => _masterVolume = ValidateVolume(value, _minVolumeThreshold);
        }

        float IAudioManager.MenuEffectsVolume
        {
            get => _menuEffectsVolume;
            set => _menuEffectsVolume = ValidateVolume(value, _minVolumeThreshold);
        }

        float IAudioManager.GameEffectsVolume
        {
            get => _gameEffectsVolume;
            set => _gameEffectsVolume = ValidateVolume(value, _minVolumeThreshold);
        }

        float IAudioManager.GameMusicVolume
        {
            get => _gameMusicVolume;
            set => _gameMusicVolume = ValidateVolume(value, _minVolumeThreshold);
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
            if (_audioClips.ContainsKey(audioClipName))
            {
                return;
            }
            var audioSource = CreateAudioSource(gameObject, audioClip, audioClipName);
            _audioClips.Add(audioClipName, audioSource);
        }

        void IAudioManager.PlayMenuEffect(string audioClipName)
        {
            Assert.IsTrue(_audioClips.ContainsKey(audioClipName), "_audioClips.ContainsKey(audioClipName)");
            var audioSource = _audioClips[audioClipName];
            PlayAudio2D(audioSource, _menuEffectsVolume * _masterVolume);
        }

        void IAudioManager.PlayGameEffect(string audioClipName)
        {
            Assert.IsTrue(_audioClips.ContainsKey(audioClipName), "_audioClips.ContainsKey(audioClipName)");
            var audioSource = _audioClips[audioClipName];
            PlayAudio(audioSource, _gameEffectsVolume * _masterVolume);
        }

        void IAudioManager.PlayGameEffect(string audioClipName, Vector3 worldPosition)
        {
            Assert.IsTrue(_audioClips.ContainsKey(audioClipName), "_audioClips.ContainsKey(audioClipName)");
            var audioSource = _audioClips[audioClipName];
            PlayAudio(audioSource, _gameEffectsVolume * _masterVolume, worldPosition);
        }

        void IAudioManager.PlayGameMusic(string audioClipName)
        {
            Assert.IsTrue(_audioClips.ContainsKey(audioClipName), "_audioClips.ContainsKey(audioClipName)");
            var audioSource = _audioClips[audioClipName];
            PlayAudio2D(audioSource, _gameMusicVolume * _masterVolume, true);
        }

        void IAudioManager.PlayMenuEffect(AudioClip audioClip)
        {
            PlayAudio2D(audioClip, _menuEffectsVolume * _masterVolume);
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
            PlayAudio2D(audioClip, _gameMusicVolume * _masterVolume, true);
        }

        #region Play AudioClip

        private static void PlayAudio2D(AudioClip clip, float volume, bool isLooping = false, [CallerMemberName] string memberName = null)
        {
            if (!(volume > Mathf.Epsilon))
            {
                return;
            }
            Debug.Log($"{memberName} {clip.name} {volume}");
            PlayClipAtPoint(clip, Vector3.zero, volume, 0, isLooping);
        }

        private static void PlayAudio(AudioClip clip, float volume, [CallerMemberName] string memberName = null)
        {
            if (!(volume > Mathf.Epsilon))
            {
                return;
            }
            Debug.Log($"{memberName} {clip.name} {volume}");
            PlayClipAtPoint(clip, Vector3.zero, volume, 1);
        }

        private static void PlayAudio(AudioClip clip, float volume, Vector3 position, [CallerMemberName] string memberName = null)
        {
            if (!(volume > Mathf.Epsilon))
            {
                return;
            }
            Debug.Log($"{memberName} {clip.name} {volume} {position}");
            PlayClipAtPoint(clip, position, volume, 1f);
        }

        private static void PlayClipAtPoint(AudioClip clip, Vector3 position, float volume, float spatialBlend, bool isLooping = false)
        {
            // Copied from AudioSource - there is big difference how PlayOneShot and Play works!
            // static method: https://docs.unity3d.com/ScriptReference/AudioSource.PlayOneShot.html
            // instance method: https://docs.unity3d.com/ScriptReference/AudioSource.Play.html

            var gameObject = new GameObject($"clip {position} {spatialBlend:0} {isLooping}  {clip.name}")
            {
                transform =
                {
                    position = position
                }
            };
            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.spatialBlend = spatialBlend;
            audioSource.volume = volume;
            audioSource.loop = isLooping;
            audioSource.playOnAwake = false;
            audioSource.Play();
            if (isLooping)
            {
                return;
            }
            Destroy(gameObject, clip.length * (Time.timeScale < 0.00999999977648258 ? 0.01f : Time.timeScale));
        }

        #endregion

        #region Play AudioSource

        private static void PlayAudio2D(AudioSource audioSource, float volume, bool isLooping = false, [CallerMemberName] string memberName = null)
        {
            if (!(volume > Mathf.Epsilon))
            {
                return;
            }
            Debug.Log($"{memberName} {audioSource.clip.name} {volume}");
            audioSource.spatialBlend = 0;
            audioSource.volume = volume;
            audioSource.loop = isLooping;
            audioSource.playOnAwake = false;
            audioSource.Play();
        }

        private static void PlayAudio(AudioSource audioSource, float volume, [CallerMemberName] string memberName = null)
        {
            if (!(volume > Mathf.Epsilon))
            {
                return;
            }
            Debug.Log($"{memberName} {audioSource.clip.name} {volume}");
            audioSource.spatialBlend = 1f;
            audioSource.volume = volume;
            audioSource.loop = false;
            audioSource.playOnAwake = false;
            audioSource.Play();
        }

        private static void PlayAudio(AudioSource audioSource, float volume, Vector3 position, [CallerMemberName] string memberName = null)
        {
            if (!(volume > Mathf.Epsilon))
            {
                return;
            }
            Debug.Log($"{memberName} {audioSource.clip.name} {volume}");
            audioSource.spatialBlend = 1f;
            audioSource.volume = volume;
            audioSource.loop = false;
            audioSource.playOnAwake = false;
            audioSource.transform.position = position;
            audioSource.Play();
        }

        private static AudioSource CreateAudioSource(GameObject parent, AudioClip clip, string audioClipName)
        {
            var gameObject = new GameObject($"clip {audioClipName}")
            {
                transform =
                {
                    parent = parent.transform,
                    position = Vector3.zero
                }
            };
            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = clip;
            Debug.Log($"{audioClipName} [{audioSource.clip.name}]");
            return audioSource;
        }

        #endregion

        private static float ValidateVolume(float volume, float threshold)
        {
            if (volume > 0 && volume < threshold)
            {
                return threshold;
            }
            return Mathf.Clamp(volume, 0, 1f);
        }
    }
}