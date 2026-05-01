using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.ReferenceSheets;
using Assets.Altzone.Scripts.Reference_Sheets;
using UnityEngine;
using static Altzone.Scripts.Audio.MusicHandler;

namespace Altzone.Scripts.Audio
{
    public enum AudioCategoryType
    {
        None,
        MainMenu,
        SoulHome,
        Jukebox,
        Battle,
        Raid,
        IntroStory,
        General,
        Other
    }

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

        private AudioCategoryType _currentAreaType = AudioCategoryType.None;
        public AudioCategoryType CurrentAreaType { get { return _currentAreaType; } }

        private AudioCategoryType _fallbackMusicCategory = AudioCategoryType.None;
        public AudioCategoryType FallbackMusicCategory { get { return _fallbackMusicCategory; } }

        private string _fallbackMusicTrack = "";
        public string FallbackMusicTrack {  get { return _fallbackMusicTrack; } }

        private bool _jukeboxWindowOpen = false;

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

            StartCoroutine(WaitForSettingsCarrier());
        }

        private IEnumerator WaitForSettingsCarrier()
        {
            yield return new WaitUntil(() => SettingsCarrier.Instance);

            UpdateMaxVolume();
        }

        public void UpdateMaxVolume()
        {
            _sFXHandler.SetMaxVolume(SettingsCarrier.Instance.SentVolume(SettingsCarrier.SoundType.sound));
            _musicHandler.SetMaxVolume(SettingsCarrier.Instance.SentVolume(SettingsCarrier.SoundType.music));
        }

        public float GetMusicVolume() { return _musicHandler.MaxVolume; }

        public void SetJukeboxWindowState(bool value) { _jukeboxWindowOpen = value; }

        #region SFX
        /// <summary>
        /// Plays a sfx sound by given CategoryName and SFXName.
        /// </summary>
        /// <param name="categoryName">Category name where the sfx sound resides in. (Note: Can be left empty but it is recommended to be given.)</param>
        /// <param name="sFXName">Name of the sfx audio that is wanted.</param>
        /// <param name="pitch">Optional.</param>
        /// <returns>Returns the <c>AudioChannelPath</c> which can be used to pause, continue or clear the audio playback if not OneShot type and is still playing.</returns>
        public ActiveChannelPath? PlaySfxAudio(string categoryName, string sFXName, float pitch = 1f)
        {
            return _sFXHandler.Play(categoryName, sFXName, _musicHandler.MainMenuMusicName, pitch);
        }

        /// <summary>
        /// Plays a sfx sound by given CategoryName and SFXName.
        /// </summary>
        /// <param name="categoryType">Category type where the sfx sound resides in. (Note: Can use None value but it is recommended to give a specific type.)</param>
        /// <param name="sFXName">Name of the sfx audio that is wanted.</param>
        /// <param name="pitch">Optional.</param>
        /// <returns>Returns the <c>AudioChannelPath</c> which can be used to pause, continue or clear the audio playback if not OneShot type and is still playing.</returns>
        public ActiveChannelPath? PlaySfxAudio(AudioCategoryType categoryType, string sFXName, float pitch = 1f)
        {
            return _sFXHandler.Play(categoryType, sFXName, _musicHandler.MainMenuMusicName, pitch);
        }

        /// <summary>
        /// Plays a sfx sound by given CategoryName and SFXName.
        /// </summary>
        /// <param name="battleSFXName">Battle type name of the sfx audio that is wanted.</param>
        /// <param name="pitch">Optional.</param>
        /// <returns>Returns the <c>AudioChannelPath</c> which can be used to pause, continue or clear the audio playback if not OneShot type and is still playing.</returns>
        public ActiveChannelPath? PlayBattleSfxAudio(BattleSFXNameTypes battleSFXName, float pitch = 1f)
        {
            return _sFXHandler.Play(AudioCategoryType.Battle, battleSFXName, pitch);
        }

        /// <summary>
        /// Plays a sfx sound by given CategoryName and SFXName.
        /// </summary>
        /// <param name="categoryName">Category name where the sfx sound resides in. (Note: Can be left empty but it is recommended to be given.)</param>
        /// <param name="sFXName">Name of the sfx audio that is wanted.</param>
        /// <param name="note">Optional.</param>
        /// <returns>Returns the <c>AudioChannelPath</c> which can be used to pause, continue or clear the audio playback if not OneShot type and is still playing.</returns>
        public ActiveChannelPath? PlaySfxAudio(string categoryName, string sFXName, int note)
        {
            float pitch = Mathf.Pow(1.05946f, note);

            return _sFXHandler.Play(categoryName, sFXName, _musicHandler.MainMenuMusicName, pitch);
        }

        /// <summary>
        /// Plays a sfx sound by given CategoryName and SFXName.
        /// </summary>
        /// <param name="categoryType">Category type where the sfx sound resides in. (Note: Can be left empty but it is recommended to be given.)</param>
        /// <param name="sFXName">Name of the sfx audio that is wanted.</param>
        /// <param name="note">Optional.</param>
        /// <returns>Returns the <c>AudioChannelPath</c> which can be used to pause, continue or clear the audio playback if not OneShot type and is still playing.</returns>
        public ActiveChannelPath? PlaySfxAudio(AudioCategoryType categoryType, string sFXName, int note)
        {
            float pitch = Mathf.Pow(1.05946f, note);

            return _sFXHandler.Play(categoryType, sFXName, _musicHandler.MainMenuMusicName, pitch);
        }

        /// <summary>
        /// Plays a sfx sound by given CategoryName and SFXName.
        /// </summary>
        /// <param name="battleSFXName">Name of the sfx audio that is wanted.</param>
        /// <param name="note">Optional.</param>
        /// <returns>Returns the <c>AudioChannelPath</c> which can be used to pause, continue or clear the audio playback if not OneShot type and is still playing.</returns>
        public ActiveChannelPath? PlayBattleSfxAudio(BattleSFXNameTypes battleSFXName, int note)
        {
            float pitch = Mathf.Pow(1.05946f, note);

            return _sFXHandler.Play(AudioCategoryType.Battle, battleSFXName, pitch);
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

        public void ChangePitchSFXAudioChannel(string sFXName, float pitch)
        {
            _sFXHandler.PlaybackOperation(SFXHandler.SFXPlaybackOperationType.Pitch, sFXName, pitch);
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

        public void ChangePitchSFXAudioChannel(ActiveChannelPath path, float pitch)
        {
            _sFXHandler.PlaybackOperation(SFXHandler.SFXPlaybackOperationType.Pitch, path, pitch);
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

        private void SetCurrentAreaCategoryType(AudioCategoryType categoryType) { _currentAreaType = categoryType; }

        public List<MusicTrack> GetMusicList(string categoryName) { return _musicHandler.GetMusicList(categoryName); }

        /// <summary>
        /// Plays music track by given track name.
        /// </summary>
        /// <returns>Played track name if successfully started playback.</returns>
        public string PlayMusic(AudioCategoryType categoryType, string trackName, MusicSwitchType switchType = MusicSwitchType.CrossFade, bool forcePlay = false)
        {
            return HandleFallBack(categoryType, trackName) ? _musicHandler.PlayMusic(categoryType, trackName, switchType, forcePlay) : "";
        }

        /// <summary>
        /// Plays first music track in the given category.
        /// </summary>
        /// <returns>Played music track name if successfully started playback.</returns>
        [Obsolete("Please use AudioCategoryType version instead of this string version.")]
        public string PlayMusic(string categoryName, MusicSwitchType switchType = MusicSwitchType.CrossFade, bool forcePlay = false)
        {
            return HandleFallBack(categoryName, "") ? _musicHandler.PlayMusic(categoryName, "", switchType, forcePlay) : "";
        }

        /// <summary>
        /// Plays first music track in the given category.
        /// </summary>
        /// <returns>Played music track name if successfully started playback.</returns>
        public string PlayMusic(AudioCategoryType categoryType, MusicSwitchType switchType = MusicSwitchType.CrossFade, bool forcePlay = false)
        {
            return HandleFallBack(categoryType, "") ? _musicHandler.PlayMusic(categoryType, "", switchType, forcePlay) : "";
        }

        /// <summary>
        /// Plays the given <c>MusicTrack</c>.
        /// </summary>
        /// <returns>Played track name if successfully started playback.</returns>
        public string PlayMusic(AudioCategoryType categoryType, MusicTrack musicTrack, MusicSwitchType switchType = MusicSwitchType.CrossFade, bool forcePlay = false)
        {
            return HandleFallBack(categoryType, musicTrack.Name) ? _musicHandler.PlayMusic(categoryType, musicTrack, switchType, forcePlay) : "";
        }

        /// <summary>
        /// Plays music track by given id.
        /// </summary>
        /// <returns>Played track name if successfully started playback.</returns>
        public string PlayMusicById(AudioCategoryType categoryType, string musicTrackId, MusicSwitchType switchType, bool forcePlay = false)
        {
            return HandleFallBack(categoryType, "") ? _musicHandler.PlayMusicById(categoryType, musicTrackId, switchType, forcePlay) : "";
        }

        [Obsolete("Please use AudioCategoryType version instead of this string version.")]
        private bool HandleFallBack(string categoryName, string trackName)
        {
            bool success = Enum.TryParse<AudioCategoryType>(categoryName, out var categoryType);

            return success && HandleFallBack(categoryType, trackName);
        }

        private bool HandleFallBack(AudioCategoryType categoryType, string trackName)
        {
            if (categoryType != AudioCategoryType.Jukebox)
            {
                _fallbackMusicCategory = categoryType;
                _fallbackMusicTrack = trackName;
                SetCurrentAreaCategoryType(categoryType);
            }

            if (categoryType == AudioCategoryType.MainMenu)
                _musicHandler.SetMainMenuMusicName(
                    SettingsCarrier.Instance.GetSelectionBoxData(SettingsCarrier.SelectionBoxType.MainMenuMusic));

            return CanPlay(categoryType);
        }

        public string PlayFallBackTrack(MusicSwitchType switchType = MusicSwitchType.CrossFade, bool forcePlay = false)
        {
            return HandleFallBack(_fallbackMusicCategory, _fallbackMusicTrack) ?
                _musicHandler.PlayMusic(_fallbackMusicCategory, _fallbackMusicTrack, switchType, forcePlay) : "";
        }

        private bool CanPlay(AudioCategoryType categoryType) //TODO: Fix rare null reference possibilities.
        {
            if (!_musicHandler) _musicHandler = GetComponent<MusicHandler>();

            if (_musicHandler.CurrentCategory == null) return true; //Dont block if category is null.

            SettingsCarrier carrier = SettingsCarrier.Instance;

            bool canPlayJukebox = (JukeboxManager.Instance.CurrentTrackQueueData != null ||
                                   JukeboxManager.Instance.TrackPreviewActive) && !JukeboxManager.Instance.JukeboxMuted && (
                _jukeboxWindowOpen
                || carrier.CanPlayJukeboxInArea(SettingsCarrier.JukeboxPlayArea.Soulhome) && _currentAreaType == AudioCategoryType.SoulHome
                || carrier.CanPlayJukeboxInArea(SettingsCarrier.JukeboxPlayArea.MainMenu) && _currentAreaType == AudioCategoryType.MainMenu
                || carrier.CanPlayJukeboxInArea(SettingsCarrier.JukeboxPlayArea.Battle) && _currentAreaType == AudioCategoryType.Battle
            );

            return categoryType != AudioCategoryType.Jukebox ? !canPlayJukebox : canPlayJukebox;
        }

        public string NextMusicTrack(MusicSwitchType switchType = MusicSwitchType.CrossFade)
        {
            string trackName = null;

            _musicHandler.SwitchMusic(MusicHandler.MusicListDirection.Next, sData => trackName = sData, switchType);

            return trackName;
        }

        public string PrevMusicTrack(MusicSwitchType switchType = MusicSwitchType.CrossFade)
        {
            string trackName = null;

            _musicHandler.SwitchMusic(MusicHandler.MusicListDirection.Previous, sData => trackName = sData, switchType);

            return trackName;
        }

        public void StopMusic()
        {
            _musicHandler.StopMusic(_musicHandler.PrimaryChannel);
        }

        public string ContinueMusic(AudioCategoryType categoryType, string trackName, MusicSwitchType switchType, float startLocation)
        {
            return HandleFallBack(categoryType, trackName) ? _musicHandler.PlayMusic(categoryType, trackName, switchType, startLocation) : "";
        }

        public string ContinueMusic(AudioCategoryType categoryType, MusicTrack musicTrack, MusicSwitchType switchType, float startLocation, bool forcePlay = false)
        {
            return HandleFallBack(categoryType, musicTrack.Name) ? _musicHandler.PlayMusic(categoryType, musicTrack, switchType, startLocation, forcePlay) : "";
        }
        #endregion
    }
}
