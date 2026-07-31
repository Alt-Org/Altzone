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

            StartCoroutine(WaitForSettingsCarrier());
        }

        private IEnumerator WaitForSettingsCarrier()
        {
            yield return new WaitUntil(() => SettingsCarrier.Instance);

            UpdateMaxVolume();
        }

        /// <summary>
        /// Updates maximum volume of sound effects and music.
        /// </summary>
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
        /// Plays a sfx sound by given CategoryName and SFXName. Use only in Battle!
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
        /// Plays a sfx sound by given CategoryName and SFXName. Use only in Battle!
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

        public List<MusicTrack> GetMusicList(string categoryName) { return _musicHandler.GetMusicList(categoryName); }

        //TODO: add more ACTUAL functionality to "bool forcePlay" params when battle sets it to false or default. Or just remove it.

        /// <summary>
        /// Handles the first part of trying to play music.
        /// </summary>
        /// <param name="categoryType">Music category in which the wanted music track resides.</param>
        /// <param name="trackName">Music tracks name.</param>
        /// <returns>Jukebox music name if successful in starting or continuing it, otherwise returns empty or null <c>string</c></returns>
        private string PlayMusic_BeginningPart(AudioCategoryType categoryType, string trackName)
        {
            // Try to set fallback music category/track and set area type.
            HandleFallBack(categoryType, trackName);

            // Try to play jukebox music if there are any and if it has the correct area permissions and is not muted.
            if (categoryType != AudioCategoryType.Jukebox && SettingsCarrier.Instance.CanPlayJukeboxInArea(_currentAreaType) && JukeboxManager.Instance)
            {
                string result = JukeboxManager.Instance.TryPlayTrack();

                if (!string.IsNullOrEmpty(result)) return result;
            }

            return "";
        }

        /// <summary>
        /// Tries to play music track by given track name.
        /// </summary>
        /// <param name="categoryType">Music category in which the wanted music track resides.</param>
        /// <param name="trackName">Music tracks name.</param>
        /// <param name="switchType"></param>
        /// <param name="forcePlay">It is not recommended to use this. May have unwanted effects if jukebox is currently active and has permission to play.</param>
        /// <returns>Music name if successful in starting or continuing it, otherwise returns empty or null <c>string</c></returns>
        public string PlayMusic(AudioCategoryType categoryType, string trackName, MusicSwitchType switchType = MusicSwitchType.CrossFade, bool forcePlay = false)
        {
            string result = PlayMusic_BeginningPart(categoryType, trackName);

            if (!string.IsNullOrEmpty(result)) return result;

            // Try to play the given music track.
            return CanPlay(categoryType) ? _musicHandler.PlayMusic(categoryType, trackName, switchType, forcePlay) : "";
        }

        /// <summary>
        /// Tries to play first music track in the given category.
        /// </summary>
        /// <returns>Music name if successful in starting or continuing it, otherwise returns empty or null <c>string</c></returns>
        [Obsolete("Please use AudioCategoryType version instead of this string version.")]
        public string PlayMusic(string categoryName, MusicSwitchType switchType = MusicSwitchType.CrossFade, bool forcePlay = false)
        {
            HandleFallBack(categoryName, "");

            if (categoryName.ToLower() != "Jukebox".ToLower() && SettingsCarrier.Instance.CanPlayJukeboxInArea(_currentAreaType) && JukeboxManager.Instance)
            {
                string result = JukeboxManager.Instance.TryPlayTrack();

                if (!string.IsNullOrEmpty(result)) return result;
            }

            return CanPlay(categoryName) ? _musicHandler.PlayMusic(categoryName, "", switchType, forcePlay) : "";
        }

        /// <summary>
        /// Tries to play first music track in the given category.
        /// </summary>
        /// <param name="categoryType">Music category where the music is wanted from.</param>
        /// <param name="switchType"></param>
        /// <param name="forcePlay">It is not recommended to use this. May have unwanted effects if jukebox is currently active and has permission to play.</param>
        /// <returns>Music name if successful in starting or continuing it, otherwise returns empty or null <c>string</c></returns>
        public string PlayMusic(AudioCategoryType categoryType, MusicSwitchType switchType = MusicSwitchType.CrossFade, bool forcePlay = false)
        {
            string result = PlayMusic_BeginningPart(categoryType, "");

            if (!string.IsNullOrEmpty(result)) return result;

            // Try to play the given music track.
            return CanPlay(categoryType) ? _musicHandler.PlayMusic(categoryType, "", switchType, forcePlay) : "";
        }

        /// <summary>
        /// Tries to play the given <c>MusicTrack</c>.
        /// </summary>
        /// <param name="categoryType">Music category in which the wanted music track resides.</param>
        /// <param name="musicTrack">Music track class.</param>
        /// <param name="switchType"></param>
        /// <param name="forcePlay">It is not recommended to use this. May have unwanted effects if jukebox is currently active and has permission to play.</param>
        /// <returns>Music name if successful in starting or continuing it, otherwise returns empty or null <c>string</c></returns>
        public string PlayMusic(AudioCategoryType categoryType, MusicTrack musicTrack, MusicSwitchType switchType = MusicSwitchType.CrossFade, bool forcePlay = false)
        {
            string result = PlayMusic_BeginningPart(categoryType, musicTrack.Name);

            if (!string.IsNullOrEmpty(result)) return result;

            // Try to play the given music track.
            return CanPlay(categoryType) ? _musicHandler.PlayMusic(categoryType, musicTrack, switchType, forcePlay) : "";
        }

        /// <summary>
        /// Tries to play music track by given id.
        /// </summary>
        /// <param name="categoryType">Music category in which the wanted music track resides.</param>
        /// <param name="musicTrackId">Music tracks id.</param>
        /// <param name="switchType"></param>
        /// <param name="forcePlay">It is not recommended to use this. May have unwanted effects if jukebox is currently active and has permission to play.</param>
        /// <returns>Music name if successful in starting or continuing it, otherwise returns empty or null <c>string</c></returns>
        public string PlayMusicById(AudioCategoryType categoryType, string musicTrackId, MusicSwitchType switchType, bool forcePlay = false)
        {
            string result = PlayMusic_BeginningPart(categoryType, "");

            if (!string.IsNullOrEmpty(result)) return result;

            // Try to play the given music track.
            return CanPlay(categoryType) ? _musicHandler.PlayMusicById(categoryType, musicTrackId, switchType, forcePlay) : "";
        }

        [Obsolete("Please use AudioCategoryType version instead of this string version.")]
        private void HandleFallBack(string categoryName, string trackName)
        {
            bool success = Enum.TryParse<AudioCategoryType>(categoryName, out var categoryType);

            if (success) HandleFallBack(categoryType, trackName);
        }

        /// <summary>
        /// Sets fallback category and track and current area type if not jukebox. Also sets main menu music name if <c>AudioCategoryType</c> is <c>MainMenu</c>.
        /// </summary>
        /// <param name="categoryType">Music category in which the wanted music track resides.</param>
        /// <param name="trackName">Track name which is wanted to be played when fallback is used.</param>
        private void HandleFallBack(AudioCategoryType categoryType, string trackName)
        {
            if (categoryType != AudioCategoryType.Jukebox)
            {
                _currentAreaType = categoryType;
                _fallbackMusicCategory = categoryType;
                _fallbackMusicTrack = trackName;
            }

            if (categoryType == AudioCategoryType.MainMenu)
                _musicHandler.SetMainMenuMusicName(
                    SettingsCarrier.Instance.GetSelectionBoxData(SettingsCarrier.SelectionBoxType.MainMenuMusic));
        }

        /// <summary>
        /// Tries to play the given fallback category's given fallback track.
        /// </summary>
        /// <param name="switchType"></param>
        /// <param name="forcePlay">It is not recommended to use this. May have unwanted effects if jukebox is currently active and has permission to play.</param>
        /// <returns>Music name if successful in starting or continuing it, otherwise returns empty or null <c>string</c></returns>
        public string PlayFallBackTrack(MusicSwitchType switchType = MusicSwitchType.CrossFade, bool forcePlay = false)
        {
            string result = PlayMusic_BeginningPart(_fallbackMusicCategory, _fallbackMusicTrack);

            if (!string.IsNullOrEmpty(result)) return result;

            // Try to play the given music track.
            return CanPlay(_fallbackMusicCategory) ? _musicHandler.PlayMusic(_fallbackMusicCategory, _fallbackMusicTrack, switchType, forcePlay) : "";
        }

        [Obsolete("Please use AudioCategoryType version instead of this string version.")]
        private bool CanPlay(string categoryName)
        {
            bool success = Enum.TryParse<AudioCategoryType>(categoryName, out var categoryType);

            return success && CanPlay(categoryType);
            return false;
        }

        /// <summary>
        /// Checks if can play given music category.
        /// </summary>
        /// <param name="categoryType">Music category in which the wanted music track resides.</param>
        /// <returns>True if can play.</returns>
        private bool CanPlay(AudioCategoryType categoryType)
        {
            if (!_musicHandler) _musicHandler = GetComponent<MusicHandler>();

            if (_musicHandler.CurrentCategory == null) return true; //Don't block if category is null.

            SettingsCarrier carrier = SettingsCarrier.Instance;

            bool jukeboxHasActiveTrack = JukeboxManager.Instance.CurrentTrackQueueData != null || JukeboxManager.Instance.TrackPreviewActive;

            bool jukeboxAreaPermissionOk = (
                _jukeboxWindowOpen
                || (carrier.CanPlayJukeboxInArea(AudioCategoryType.SoulHome) && _currentAreaType == AudioCategoryType.SoulHome)
                || (carrier.CanPlayJukeboxInArea(AudioCategoryType.MainMenu) && _currentAreaType == AudioCategoryType.MainMenu)
                || (carrier.CanPlayJukeboxInArea(AudioCategoryType.Battle) && _currentAreaType == AudioCategoryType.Battle)
            );

            bool canPlayJukebox = jukeboxHasActiveTrack && !JukeboxManager.Instance.JukeboxMuted && jukeboxAreaPermissionOk;

            return categoryType == AudioCategoryType.Jukebox ? canPlayJukebox : !canPlayJukebox;
        }

        /// <summary>
        /// Plays the next track from that music category. Usage not recommended at the moment.
        /// </summary>
        /// <param name="switchType"></param>
        /// <returns>Music name if successful in starting it, otherwise returns empty or null <c>string</c></returns>
        public string NextMusicTrack(MusicSwitchType switchType = MusicSwitchType.CrossFade)
        {
            string trackName = null;

            _musicHandler.SwitchMusic(MusicHandler.MusicListDirection.Next, sData => trackName = sData, switchType);

            return trackName;
        }

        /// <summary>
        /// Plays the previous track from that music category. Usage not recommended at the moment.
        /// </summary>
        /// <param name="switchType"></param>
        /// <returns>Music name if successful in starting it, otherwise returns empty or null <c>string</c></returns>
        public string PrevMusicTrack(MusicSwitchType switchType = MusicSwitchType.CrossFade)
        {
            string trackName = null;

            _musicHandler.SwitchMusic(MusicHandler.MusicListDirection.Previous, sData => trackName = sData, switchType);

            return trackName;
        }

        /// <summary>
        /// Stops music playback on primary channel.
        /// </summary>
        public void StopMusic()
        {
            _musicHandler.StopMusic(_musicHandler.PrimaryChannel);
        }

        /// <summary>
        /// Tries to play music from given start location.
        /// </summary>
        /// <param name="categoryType">Music category in which the wanted music track resides.</param>
        /// <param name="trackName">Music tracks name.</param>
        /// <param name="switchType"></param>
        /// <param name="startLocation">Start location of the music track.</param>
        /// <returns>Music name if successful in starting or continuing it, otherwise returns empty or null <c>string</c></returns>
        public string ContinueMusic(AudioCategoryType categoryType, string trackName, MusicSwitchType switchType, float startLocation)
        {
            string result = PlayMusic_BeginningPart(categoryType, trackName);

            if (!string.IsNullOrEmpty(result)) return result;

            // Try to play the given music track.
            return CanPlay(categoryType) ? _musicHandler.PlayMusic(categoryType, trackName, switchType, startLocation) : "";
        }

        /// <summary>
        /// Tries to play music from given start location.
        /// </summary>
        /// <param name="categoryType">Music category in which the wanted music track resides.</param>
        /// <param name="musicTrack">Music tracks class.</param>
        /// <param name="switchType"></param>
        /// <param name="startLocation">Start location of the music track.</param>
        /// <param name="forcePlay">It is not recommended to use this. May have unwanted effects if jukebox is currently active and has permission to play.</param>
        /// <returns>Music name if successful in starting or continuing it, otherwise returns empty or null <c>string</c></returns>
        public string ContinueMusic(AudioCategoryType categoryType, MusicTrack musicTrack, MusicSwitchType switchType, float startLocation, bool forcePlay = false)
        {
            string result = PlayMusic_BeginningPart(categoryType, musicTrack.Name);

            if (!string.IsNullOrEmpty(result)) return result;

            // Try to play the given music track.
            return CanPlay(categoryType) ? _musicHandler.PlayMusic(categoryType, musicTrack, switchType, startLocation, forcePlay) : "";
        }
        #endregion
    }
}
