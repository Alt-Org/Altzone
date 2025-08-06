using System.Collections.Generic;
using Altzone.Scripts.ReferenceSheets;
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

        private string _fallbackMusicCategory = "";
        public string FallbackMusicCategory { get { return _fallbackMusicCategory; } }
        private string _fallbackMusicTrack = "";
        public string FallbackMusicTrack {  get { return _fallbackMusicTrack; } }

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
        }

        public void UpdateMaxVolume()
        {
            _sFXHandler.SetMaxVoulme(SettingsCarrier.Instance.SentVolume(SettingsCarrier.SoundType.sound));
            _musicHandler.SetMaxVolume(SettingsCarrier.Instance.SentVolume(SettingsCarrier.SoundType.music));
        }

        #region SFX

        /// <summary>
        /// Plays a sfx sound by given CategoryName and SFXName
        /// </summary>
        /// <param name="categoryName">Category name where the sfx sound resides in. (Note: Can be left empty but it is recommended to be given.)</param>
        /// <param name="sFXname">Name of the sfx audio that is wanted.</param>
        /// <returns>Returns the <c>AudioChannelPath</c> wich can be used to pause, continue or clear the audio playback if not OneShot type and is still playing.</returns>
        public ActiveChannelPath? PlaySfxAudio(string categoryName, string sFXname)
        {
            return _sFXHandler.Play(categoryName, sFXname, _musicHandler.MainMenuMusicName);
        }

        #region SFX All Commands
        public void StopAllSFXAudio()
        {
            _sFXHandler.PlaybackOperationAll(SFXHandler.SFXPlaybackOperationType.Stop);
        }

        public void ContinueAllSFXAudio()
        {
            _sFXHandler.PlaybackOperationAll(SFXHandler.SFXPlaybackOperationType.Continue);
        }

        public void ClearAllSFXAudio()
        {
            _sFXHandler.PlaybackOperationAll(SFXHandler.SFXPlaybackOperationType.Clear);
        }
        #endregion

        #region SFX Single Commands
        public void StopSFXAudioChannel(string sFXName)
        {
            _sFXHandler.PlaybackOperation(SFXHandler.SFXPlaybackOperationType.Stop, sFXName);
        }

        public void ContinueSFXAudioChannel(string sFXName)
        {
            _sFXHandler.PlaybackOperation(SFXHandler.SFXPlaybackOperationType.Continue, sFXName);
        }

        public void ClearSFXAudioChannel(string sFXName)
        {
            _sFXHandler.PlaybackOperation(SFXHandler.SFXPlaybackOperationType.Clear, sFXName);
        }

        public void StopSFXAudioChannel(ActiveChannelPath path)
        {
            _sFXHandler.PlaybackOperation(SFXHandler.SFXPlaybackOperationType.Stop, path);
        }

        public void ContinueSFXAudioChannel(ActiveChannelPath path)
        {
            _sFXHandler.PlaybackOperation(SFXHandler.SFXPlaybackOperationType.Continue, path);
        }

        public void ClearSFXAudioChannel(ActiveChannelPath path)
        {
            _sFXHandler.PlaybackOperation(SFXHandler.SFXPlaybackOperationType.Clear, path);
        }
        #endregion

        /// <summary>
        /// Changes the given sfx sound volume level if it's still playing.
        /// </summary>
        /// <param name="targetSFXName">Give either the sfx name or "all". (Note: "all" will change every active sfx channels volume level.)</param>
        public void ChangeSFXVolume(float volume, string targetSFXName)
        {
            _sFXHandler.ChangeVolume(volume, targetSFXName);
        }

        #endregion

        #region Music

        public List<MusicTrack> GetMusicList(string categoryName) { return _musicHandler.GetMusicList(categoryName); }

        public string PlayMusic(string categoryName, string trackName)
        {
            if (!CanPlay(categoryName))
            {
                _fallbackMusicCategory = categoryName;
                _fallbackMusicTrack = trackName;
                return "";
            }

            return _musicHandler.PlayMusic(categoryName, trackName);
        }

        public string PlayMusic(string categoryName)
        {
            if (!CanPlay(categoryName))
            {
                _fallbackMusicCategory = categoryName;
                _fallbackMusicTrack = "";
                return "";
            }

            return _musicHandler.PlayMusic(categoryName, "");
        }

        public string PlayMusic(string categoryName, MusicTrack musicTrack)
        {
            if (!CanPlay(categoryName))
            {
                _fallbackMusicCategory = categoryName;
                _fallbackMusicTrack = musicTrack.Name;
                return "";
            }

            return _musicHandler.PlayMusic(categoryName, musicTrack);
        }

        private bool CanPlay(string categoryName)
        {
            if (_musicHandler.CurrentCategory == null) return true; //Dont block if category is null.

            bool currentCategoryJukebox = _musicHandler.CurrentCategory.Name.ToLower() == "Jukebox".ToLower();
            bool hasCurrentTrack = JukeboxManager.Instance.CurrentMusicTrack != null;

            if (!currentCategoryJukebox || currentCategoryJukebox && !hasCurrentTrack) return true; //Dont block if category is jukebox but current track is null.

            int jukeboxSoulhome = PlayerPrefs.GetInt("JukeboxSoulHome");
            int jukeboxUI = PlayerPrefs.GetInt("JukeboxUI");
            int jukeboxBattle = PlayerPrefs.GetInt("JukeboxBattle");
            bool blockPlayRequest = (
                (jukeboxSoulhome == 1 && categoryName.ToLower() == "Soulhome".ToLower())
                || (jukeboxUI == 1 && categoryName.ToLower() == "MainMenu".ToLower())
                || (jukeboxBattle == 1 && categoryName.ToLower() == "Battle".ToLower())
                );

            if (blockPlayRequest) return false; //Block if current category is jukebox and has current track.

            return true;
        }

        public string NextMusicTrack()
        {
            string name = null;

            _musicHandler.SwitchMusic(MusicHandler.MusicListDirection.Next, sData => name = sData);

            return name;
        }

        public string PrevMusicTrack()
        {
            string name = null;

            _musicHandler.SwitchMusic(MusicHandler.MusicListDirection.Previous, sData => name = sData);

            return name;
        }

        public void StopMusic()
        {
            _musicHandler.StopMusic(_musicHandler.PrimaryChannel);
        }
        #endregion
    }
}
