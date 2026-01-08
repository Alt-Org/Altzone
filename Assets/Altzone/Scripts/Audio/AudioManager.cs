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

        private string _currentAreaName = "";
        public string CurrentAreaName { get { return _currentAreaName; } }

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

            StartCoroutine(WaitForSettingsCarrier());
        }

        private IEnumerator WaitForSettingsCarrier()
        {
            yield return new WaitUntil(() => SettingsCarrier.Instance != null);

            UpdateMaxVolume();
        }

        public void UpdateMaxVolume()
        {
            _sFXHandler.SetMaxVoulme(SettingsCarrier.Instance.SentVolume(SettingsCarrier.SoundType.sound));
            _musicHandler.SetMaxVolume(SettingsCarrier.Instance.SentVolume(SettingsCarrier.SoundType.music));
        }

        public float GetMusicVolume() { return _musicHandler.MaxVolume; }

        #region SFX

        /// <summary>
        /// Plays a sfx sound by given CategoryName and SFXName.
        /// </summary>
        /// <param name="categoryName">Category name where the sfx sound resides in. (Note: Can be left empty but it is recommended to be given.)</param>
        /// <param name="sFXname">Name of the sfx audio that is wanted.</param>
        /// <param name="pitch">Optional.</param>
        /// <returns>Returns the <c>AudioChannelPath</c> wich can be used to pause, continue or clear the audio playback if not OneShot type and is still playing.</returns>
        public ActiveChannelPath? PlaySfxAudio(string categoryName, string sFXname, float pitch = 1f)
        {
            return _sFXHandler.Play(categoryName, sFXname, _musicHandler.MainMenuMusicName, pitch);
        }

        /// <summary>
        /// Plays a sfx sound by given CategoryName and SFXName.
        /// </summary>
        /// <param name="categoryType">Category type where the sfx sound resides in. (Note: Can use None value but it is recommended to give a specific type.)</param>
        /// <param name="sFXname">Name of the sfx audio that is wanted.</param>
        /// <param name="pitch">Optional.</param>
        /// <returns>Returns the <c>AudioChannelPath</c> wich can be used to pause, continue or clear the audio playback if not OneShot type and is still playing.</returns>
        public ActiveChannelPath? PlaySfxAudio(AudioCategoryType categoryType, string sFXname, float pitch = 1f)
        {
            return _sFXHandler.Play(categoryType, sFXname, _musicHandler.MainMenuMusicName, pitch);
        }

        /// <summary>
        /// Plays a sfx sound by given CategoryName and SFXName.
        /// </summary>
        /// <param name="battleSFXName">Battle type name of the sfx audio that is wanted.</param>
        /// <param name="pitch">Optional.</param>
        /// <returns>Returns the <c>AudioChannelPath</c> wich can be used to pause, continue or clear the audio playback if not OneShot type and is still playing.</returns>
        public ActiveChannelPath? PlayBattleSfxAudio(BattleSFXNameTypes battleSFXName, float pitch = 1f)
        {
            return _sFXHandler.Play(AudioCategoryType.Battle, battleSFXName, pitch);
        }

        /// <summary>
        /// Plays a sfx sound by given CategoryName and SFXName.
        /// </summary>
        /// <param name="categoryName">Category name where the sfx sound resides in. (Note: Can be left empty but it is recommended to be given.)</param>
        /// <param name="sFXname">Name of the sfx audio that is wanted.</param>
        /// <param name="note">Optional.</param>
        /// <returns>Returns the <c>AudioChannelPath</c> wich can be used to pause, continue or clear the audio playback if not OneShot type and is still playing.</returns>
        public ActiveChannelPath? PlaySfxAudio(string categoryName, string sFXname, int note)
        {
            float pitch = Mathf.Pow(1.05946f, note);

            return _sFXHandler.Play(categoryName, sFXname, _musicHandler.MainMenuMusicName, pitch);
        }

        /// <summary>
        /// Plays a sfx sound by given CategoryName and SFXName.
        /// </summary>
        /// <param name="categoryType">Category type where the sfx sound resides in. (Note: Can be left empty but it is recommended to be given.)</param>
        /// <param name="sFXname">Name of the sfx audio that is wanted.</param>
        /// <param name="note">Optional.</param>
        /// <returns>Returns the <c>AudioChannelPath</c> wich can be used to pause, continue or clear the audio playback if not OneShot type and is still playing.</returns>
        public ActiveChannelPath? PlaySfxAudio(AudioCategoryType categoryType, string sFXname, int note)
        {
            float pitch = Mathf.Pow(1.05946f, note);

            return _sFXHandler.Play(categoryType, sFXname, _musicHandler.MainMenuMusicName, pitch);
        }

        /// <summary>
        /// Plays a sfx sound by given CategoryName and SFXName.
        /// </summary>
        /// <param name="battleSFXName">Name of the sfx audio that is wanted.</param>
        /// <param name="note">Optional.</param>
        /// <returns>Returns the <c>AudioChannelPath</c> wich can be used to pause, continue or clear the audio playback if not OneShot type and is still playing.</returns>
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
        public void SetCurrentAreaCategoryName(string name) { _currentAreaName = name; }

        public List<MusicTrack> GetMusicList(string categoryName) { return _musicHandler.GetMusicList(categoryName); }

        /// <summary>
        /// Plays music track by given track name.
        /// </summary>
        /// <returns>Played track name if successfully started playback.</returns>
        public string PlayMusic(string categoryName, string trackName, MusicSwitchType switchType = MusicSwitchType.CrossFade, bool forcePlay = false)
        {
            if (!HandleFallBack(categoryName, trackName)) return "";

            return _musicHandler.PlayMusic(categoryName, trackName, switchType, forcePlay);
        }

        /// <summary>
        /// Plays music track by given track name.
        /// </summary>
        /// <returns>Played track name if successfully started playback.</returns>
        public string PlayMusic(AudioCategoryType categoryType, string trackName, MusicSwitchType switchType = MusicSwitchType.CrossFade, bool forcePlay = false)
        {
            if (!HandleFallBack(categoryType, trackName)) return "";

            return _musicHandler.PlayMusic(categoryType, trackName, switchType, forcePlay);
        }

        /// <summary>
        /// Plays first music track in the given category.
        /// </summary>
        /// <returns>Played music track name if successfully started playback.</returns>
        public string PlayMusic(string categoryName, MusicSwitchType switchType = MusicSwitchType.CrossFade, bool forcePlay = false)
        {
            if (!HandleFallBack(categoryName, "")) return "";

            return _musicHandler.PlayMusic(categoryName, "", switchType, forcePlay);
        }

        /// <summary>
        /// Plays first music track in the given category.
        /// </summary>
        /// <returns>Played music track name if successfully started playback.</returns>
        public string PlayMusic(AudioCategoryType categoryType, MusicSwitchType switchType = MusicSwitchType.CrossFade, bool forcePlay = false)
        {
            if (!HandleFallBack(categoryType, "")) return "";

            return _musicHandler.PlayMusic(categoryType, "", switchType, forcePlay);
        }

        /// <summary>
        /// Plays the given <c>MusicTrack</c>.
        /// </summary>
        /// <returns>Played track name if successfully started playback.</returns>
        public string PlayMusic(string categoryName, MusicTrack musicTrack, MusicSwitchType switchType = MusicSwitchType.CrossFade, bool forcePlay = false)
        {
            if (!HandleFallBack(categoryName, musicTrack.Name)) return "";

            return _musicHandler.PlayMusic(categoryName, musicTrack, switchType, forcePlay);
        }

        /// <summary>
        /// Plays the given <c>MusicTrack</c>.
        /// </summary>
        /// <returns>Played track name if successfully started playback.</returns>
        public string PlayMusic(AudioCategoryType categoryType, MusicTrack musicTrack, MusicSwitchType switchType = MusicSwitchType.CrossFade, bool forcePlay = false)
        {
            if (!HandleFallBack(categoryType, musicTrack.Name)) return "";

            return _musicHandler.PlayMusic(categoryType, musicTrack, switchType, forcePlay);
        }

        /// <summary>
        /// Plays music track by given id.
        /// </summary>
        /// <returns>Played track name if successfully started playback.</returns>
        public string PlayMusicById(string categoryName, string musicTrackId, MusicSwitchType switchType, bool forcePlay = false) //TODO: Modify "HandleFallBack" for empty track name input.
        {
            if (!HandleFallBack(categoryName, "")) return "";

            return _musicHandler.PlayMusicById(categoryName, musicTrackId, switchType, forcePlay);
        }

        /// <summary>
        /// Plays music track by given id.
        /// </summary>
        /// <returns>Played track name if successfully started playback.</returns>
        public string PlayMusicById(AudioCategoryType categoryType, string musicTrackId, MusicSwitchType switchType, bool forcePlay = false) //TODO: Modify "HandleFallBack" for empty track name input.
        {
            if (!HandleFallBack(categoryType, "")) return "";

            return _musicHandler.PlayMusicById(categoryType, musicTrackId, switchType, forcePlay);
        }

        private bool HandleFallBack(AudioCategoryType categoryType, string trackName) { return HandleFallBack(categoryType.ToString(), trackName); }

        private bool HandleFallBack(string categoryName, string trackName)
        {
            if (categoryName.ToLower() != "Jukebox".ToLower())
            {
                _fallbackMusicCategory = categoryName;
                _fallbackMusicTrack = trackName;
            }

            if (!CanPlay(categoryName))
            {
                if (categoryName.ToLower() == "MainMenu".ToLower())
                    _musicHandler.SetMainMenuMusicName(SettingsCarrier.Instance.GetSelectionBoxData(SettingsCarrier.SelectionBoxType.MainMenuMusic));

                return false;
            }

            return true;
        }

        public string PlayFallBackTrack(MusicSwitchType switchType, bool forcePlay = false)
        {
            return _musicHandler.PlayMusic(_fallbackMusicCategory, _fallbackMusicTrack, switchType, forcePlay);
        }

        private bool CanPlay(string categoryName)
        {
            if (_musicHandler == null) _musicHandler = GetComponent<MusicHandler>();

            if (_musicHandler.CurrentCategory == null) return true; //Dont block if category is null.

            bool currentCategoryJukebox = _musicHandler.CurrentCategory.Name.ToLower() == "Jukebox".ToLower();
            bool hasCurrentTrack = (JukeboxManager.Instance != null && JukeboxManager.Instance.CurrentTrackQueueData != null);

            if (!currentCategoryJukebox || !hasCurrentTrack) return true; //Dont block if category is jukebox but current track is null.

            SettingsCarrier carrier = SettingsCarrier.Instance;

            bool jukeboxSoulhome = carrier.CanPlayJukeboxInArea(SettingsCarrier.JukeboxPlayArea.Soulhome);
            bool jukeboxMainMenu = carrier.CanPlayJukeboxInArea(SettingsCarrier.JukeboxPlayArea.MainMenu);
            bool jukeboxBattle = carrier.CanPlayJukeboxInArea(SettingsCarrier.JukeboxPlayArea.Battle);
            bool blockPlayRequest = (
                (categoryName.ToLower() == "Soulhome".ToLower() && jukeboxSoulhome)
                || (categoryName.ToLower() == "MainMenu".ToLower() && jukeboxMainMenu)
                || (categoryName.ToLower() == "Battle".ToLower() && jukeboxBattle)
                || (categoryName.ToLower() == "Jukebox".ToLower()
                && ( !jukeboxSoulhome && _currentAreaName.ToLower() == "Soulhome".ToLower()
                || !jukeboxMainMenu && _currentAreaName.ToLower() == "MainMenu".ToLower()
                || !jukeboxBattle && _currentAreaName.ToLower() == "Battle".ToLower()))
                );

            if (blockPlayRequest) return false; //Block playback.
            
            return true;
        }

        public string NextMusicTrack(MusicSwitchType switchType = MusicSwitchType.CrossFade)
        {
            string name = null;

            _musicHandler.SwitchMusic(MusicHandler.MusicListDirection.Next, sData => name = sData, switchType);

            return name;
        }

        public string PrevMusicTrack(MusicSwitchType switchType = MusicSwitchType.CrossFade)
        {
            string name = null;

            _musicHandler.SwitchMusic(MusicHandler.MusicListDirection.Previous, sData => name = sData, switchType);

            return name;
        }

        public void StopMusic()
        {
            _musicHandler.StopMusic(_musicHandler.PrimaryChannel);
        }

        public string ContinueMusic(string categoryName, string trackName, MusicSwitchType switchType, float startLocation, bool forcePlay = false)
        {
            if (!HandleFallBack(categoryName, trackName)) return "";

            return _musicHandler.PlayMusic(categoryName, trackName, switchType, startLocation);
        }

        public string ContinueMusic(AudioCategoryType categoryType, string trackName, MusicSwitchType switchType, float startLocation, bool forcePlay = false)
        {
            if (!HandleFallBack(categoryType, trackName)) return "";

            return _musicHandler.PlayMusic(categoryType, trackName, switchType, startLocation);
        }

        public string ContinueMusic(string categoryName, MusicTrack musicTrack, MusicSwitchType switchType, float startLocation, bool forcePlay = false)
        {
            if (!HandleFallBack(categoryName, musicTrack.Name)) return "";

            return _musicHandler.PlayMusic(categoryName, musicTrack, switchType, startLocation, forcePlay);
        }

        public string ContinueMusic(AudioCategoryType categoryType, MusicTrack musicTrack, MusicSwitchType switchType, float startLocation, bool forcePlay = false)
        {
            if (!HandleFallBack(categoryType, musicTrack.Name)) return "";

            return _musicHandler.PlayMusic(categoryType, musicTrack, switchType, startLocation, forcePlay);
        }
        #endregion
    }
}
