using System;
using System.Collections.Generic;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine;
using static Altzone.Scripts.Audio.MusicHandler;

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

        #region Obsolete / Removal pending...
        /// <summary>
        /// Use newer version that takes <c>MusicSwitchType</c> as a parameter! <br/>
        /// Plays music track by given track name.
        /// </summary>
        /// <returns>Played track name if successfully started playback.</returns>
        [Obsolete]
        public string PlayMusic(string categoryName, string trackName)
        {
            if (!HandleFallBack(categoryName, trackName)) return "";

            return _musicHandler.PlayMusic(categoryName, trackName, MusicSwitchType.CrossFade);
        }

        /// <summary>
        /// Use newer version that takes <c>MusicSwitchType</c> as a parameter! <br/>
        /// Plays first music track in the given category.
        /// </summary>
        /// <returns>Played music track name if successfully started playback.</returns>
        [Obsolete]
        public string PlayMusic(string categoryName)
        {
            if (!HandleFallBack(categoryName, "")) return "";

            return _musicHandler.PlayMusic(categoryName, "", MusicSwitchType.CrossFade);
        }

        /// <summary>
        /// Use newer version that takes <c>MusicSwitchType</c> as a parameter! <br/>
        /// Plays the given <c>MusicTrack</c>.
        /// </summary>
        /// <returns>Played track name if successfully started playback.</returns>
        [Obsolete]
        public string PlayMusic(string categoryName, MusicTrack musicTrack)
        {
            if (!HandleFallBack(categoryName, musicTrack.Name)) return "";

            return _musicHandler.PlayMusic(categoryName, musicTrack, MusicSwitchType.CrossFade);
        }

        /// <summary>
        /// Use newer version that takes <c>MusicSwitchType</c> as a parameter! <br/>
        /// Plays music track by given id.
        /// </summary>
        /// <returns>Played track name if successfully started playback.</returns>
        [Obsolete]
        public string PlayMusicById(string categoryName, string musicTrackId) //TODO: Modify "HandleFallBack" for empty track name input.
        {
            if (!HandleFallBack(categoryName, "")) return "";

            return _musicHandler.PlayMusicById(categoryName, musicTrackId, MusicSwitchType.CrossFade);
        }
        #endregion

        /// <summary>
        /// Plays music track by given track name.
        /// </summary>
        /// <returns>Played track name if successfully started playback.</returns>
        public string PlayMusic(string categoryName, string trackName, MusicSwitchType switchType)
        {
            if (!HandleFallBack(categoryName, trackName)) return "";

            return _musicHandler.PlayMusic(categoryName, trackName, switchType);
        }

        /// <summary>
        /// Plays first music track in the given category.
        /// </summary>
        /// <returns>Played music track name if successfully started playback.</returns>
        public string PlayMusic(string categoryName, MusicSwitchType switchType)
        {
            if (!HandleFallBack(categoryName, "")) return "";

            return _musicHandler.PlayMusic(categoryName, "", switchType);
        }

        /// <summary>
        /// Plays the given <c>MusicTrack</c>.
        /// </summary>
        /// <returns>Played track name if successfully started playback.</returns>
        public string PlayMusic(string categoryName, MusicTrack musicTrack, MusicSwitchType switchType)
        {
            if (!HandleFallBack(categoryName, musicTrack.Name)) return "";

            return _musicHandler.PlayMusic(categoryName, musicTrack, switchType);
        }

        /// <summary>
        /// Plays music track by given id.
        /// </summary>
        /// <returns>Played track name if successfully started playback.</returns>
        public string PlayMusicById(string categoryName, string musicTrackId, MusicSwitchType switchType) //TODO: Modify "HandleFallBack" for empty track name input.
        {
            if (!HandleFallBack(categoryName, "")) return "";

            return _musicHandler.PlayMusicById(categoryName, musicTrackId, switchType);
        }

        private bool HandleFallBack(string categoryName, string trackName)
        {
            //Debug.LogError(categoryName + ", " + trackName);
            if (!CanPlay(categoryName))
            {
                _fallbackMusicCategory = categoryName;
                _fallbackMusicTrack = trackName;
                return false;
            }

            if (categoryName.ToLower() != "Jukebox".ToLower())
            {
                _fallbackMusicCategory = categoryName;
                _fallbackMusicTrack = trackName;
            }

            return true;
        }

        [Obsolete]
        public string PlayFallBackTrack()
        {
            return _musicHandler.PlayMusic(_fallbackMusicCategory, _fallbackMusicTrack, MusicSwitchType.CrossFade);
        }

        public string PlayFallBackTrack(MusicSwitchType switchType)
        {
            return _musicHandler.PlayMusic(_fallbackMusicCategory, _fallbackMusicTrack, switchType);
        }

        private bool CanPlay(string categoryName)
        {
            if (_musicHandler == null) _musicHandler = GetComponent<MusicHandler>();

            if (_musicHandler.CurrentCategory == null) return true; //Dont block if category is null.

            bool currentCategoryJukebox = _musicHandler.CurrentCategory.Name.ToLower() == "Jukebox".ToLower();
            bool hasCurrentTrack = JukeboxManager.Instance.CurrentTrackQueueData != null;

            if (!currentCategoryJukebox || currentCategoryJukebox && !hasCurrentTrack) return true; //Dont block if category is jukebox but current track is null.

            SettingsCarrier carrier = SettingsCarrier.Instance;

            bool jukeboxSoulhome = carrier.CanPlayJukeboxInArea(SettingsCarrier.JukeboxPlayArea.Soulhome);
            bool jukeboxMainMenu = carrier.CanPlayJukeboxInArea(SettingsCarrier.JukeboxPlayArea.MainMenu);
            bool jukeboxBattle = carrier.CanPlayJukeboxInArea(SettingsCarrier.JukeboxPlayArea.Battle);
            bool blockPlayRequest = (
                (jukeboxSoulhome && categoryName.ToLower() == "Soulhome".ToLower())
                || (jukeboxMainMenu && categoryName.ToLower() == "MainMenu".ToLower())
                || (jukeboxBattle && categoryName.ToLower() == "Battle".ToLower())
                );

            if (blockPlayRequest) return false; //Block if current category is jukebox and has current track.

            return true;
        }

        [Obsolete]
        public string NextMusicTrack()
        {
            string name = null;

            _musicHandler.SwitchMusic(MusicHandler.MusicListDirection.Next, sData => name = sData, MusicSwitchType.CrossFade);

            return name;
        }

        public string NextMusicTrack(MusicSwitchType switchType)
        {
            string name = null;

            _musicHandler.SwitchMusic(MusicHandler.MusicListDirection.Next, sData => name = sData, switchType);

            return name;
        }

        [Obsolete]
        public string PrevMusicTrack()
        {
            string name = null;

            _musicHandler.SwitchMusic(MusicHandler.MusicListDirection.Previous, sData => name = sData, MusicSwitchType.CrossFade);

            return name;
        }

        public string PrevMusicTrack(MusicSwitchType switchType)
        {
            string name = null;

            _musicHandler.SwitchMusic(MusicHandler.MusicListDirection.Previous, sData => name = sData, switchType);

            return name;
        }

        public void StopMusic()
        {
            _musicHandler.StopMusic(_musicHandler.PrimaryChannel);
        }

        public string ContinueMusic(string categoryName, string trackName, MusicSwitchType switchType, float startLocation)
        {
            if (!CanPlay(categoryName))
            {
                _fallbackMusicCategory = categoryName;
                _fallbackMusicTrack = trackName;
                return "";
            }

            return _musicHandler.PlayMusic(categoryName, trackName, switchType, startLocation);
        }

        [Obsolete]
        public string ContinueMusic(string categoryName, MusicTrack musicTrack, float startLocation)
        {
            if (!CanPlay(categoryName))
            {
                _fallbackMusicCategory = categoryName;
                _fallbackMusicTrack = musicTrack.Name;
                return "";
            }

            return _musicHandler.PlayMusic(categoryName, musicTrack, MusicSwitchType.CrossFade, startLocation);
        }

        public string ContinueMusic(string categoryName, MusicTrack musicTrack, MusicSwitchType switchType, float startLocation)
        {
            if (!CanPlay(categoryName))
            {
                _fallbackMusicCategory = categoryName;
                _fallbackMusicTrack = musicTrack.Name;
                return "";
            }

            return _musicHandler.PlayMusic(categoryName, musicTrack, switchType, startLocation);
        }
        #endregion
    }
}
