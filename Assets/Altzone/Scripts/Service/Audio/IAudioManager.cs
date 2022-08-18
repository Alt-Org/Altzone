using UnityEngine;

namespace Altzone.Scripts.Service.Audio
{
    /// <summary>
    /// AudioManager service.
    /// </summary>
    public interface IAudioManager
    {
        float MasterVolume { get; set; }
        float MenuEffectsVolume { get; set; }
        float GameEffectsVolume { get; set; }
        float GameMusicVolume { get; set; }

        void RegisterAudioClip(string audioClipName, AudioClip audioClip);
        
        void PlayMenuEffect(string audioClipName);
        void PlayGameEffect(string audioClipName);
        void PlayGameEffect(string audioClipName, Vector3 worldPosition);
        void PlayGameMusic(string audioClipName);

        void PlayMenuEffect(AudioClip audioClip);
        void PlayGameEffect(AudioClip audioClip);
        void PlayGameEffect(AudioClip audioClip, Vector3 worldPosition);
        void PlayGameMusic(AudioClip audioClip);
    }
}