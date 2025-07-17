using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Audio
{
    public enum AudioSourceType
    {
        Sfx,
        Music
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private SFXHandler _sFXHandler;
        private MusicHandler _musicHandler;

        #region Delegates & Events

        public delegate void MusicVolumeChange(float value);
        public static event System.Action<MusicVolumeChange> OnMusicVolumeChange;

        //Either target a specific AudioSourceHandler or "all" AudioSourceHandler's.?
        public delegate void SFXVolumeChange(float value, string target);
        public event System.Action<SFXVolumeChange> OnSFXVolumeChange;

        #endregion

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
            _sFXHandler = GetComponent<SFXHandler>();
            _musicHandler = GetComponent<MusicHandler>();

            if (SettingsCarrier.Instance == null) return;

            UpdateMaxVolume();
            _sFXHandler.SetVolume(1f, "all");
        }

        public void UpdateMaxVolume()
        {
            _sFXHandler.SetMaxVoulme(SettingsCarrier.Instance.SentVolume(SettingsCarrier.SoundType.sound));
            _musicHandler.SetMaxVoulme(SettingsCarrier.Instance.SentVolume(SettingsCarrier.SoundType.music));
        }

        #region SFX

        public void PlaySfxAudio(string categoryName, string name)
        {
            _sFXHandler.Play(categoryName, name, _musicHandler.MainMenuMusicName);
        }

        #endregion

        #region Music

        public List<MusicTrack> GetMusicList(string categoryName) { return _musicHandler.GetMusicList(categoryName); }

        public string PlayMusic(string categoryName, string trackName)
        {
            return _musicHandler.PlayMusic(categoryName, trackName);
        }

        public string PlayMusic(string categoryName)
        {
            return _musicHandler.PlayMusic(categoryName, "");
        }

        public string PlayMusic(string categoryName, MusicTrack musicTrack)
        {
            return _musicHandler.PlayMusic(categoryName, musicTrack);
        }

        public string NextMusicTrack()
        {
            string name = null;

            _musicHandler.SwitchMusic(1, sData => name = sData);

            return name;
        }

        public string PrevMusicTrack()
        {
            string name = null;

            _musicHandler.SwitchMusic(-1, sData => name = sData);

            return name;
        }

        public void StopMusic()
        {
            _musicHandler.StopMusic(_musicHandler.PrimaryChannel);
        }
        #endregion
    }
}
