using UnityEngine;

namespace Altzone.Scripts.Service.Audio
{
    /// <summary>
    /// Simple AudioManager service that can play <c>AudioClip</c>'s trough 3 simple logical channels.<br />
    /// These are menu effects, game effects and game music.
    /// </summary>
    /// <remarks>
    /// Game effects can have a position for 3D sound placement.
    /// </remarks>
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