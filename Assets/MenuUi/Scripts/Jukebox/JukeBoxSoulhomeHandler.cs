using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Audio;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine;
using UnityEngine.UI;
using MenuUI.Scripts;

public class JukeBoxSoulhomeHandler : MonoBehaviour
{
    [SerializeField] private GameObject _jukeboxObject;
    [SerializeField] private JukeboxPlaylistNavigationHandler _playlistNavigationHandler;
    [SerializeField] private JukeboxMainDiskHandler _mainDiskHandler;
    [SerializeField] private List<TextAutoScroll> _trackNames;
    [SerializeField] private List<TextAutoScroll> _trackCreditsNames;
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _soundMuteButton;
    [SerializeField] private Image _soundMuteImage;
    [SerializeField] private FavoriteButtonHandler _favoriteButtonHandler;
    [SerializeField] private Button _addMusicInfoButton;
    [SerializeField] private GameObject _addMusicInfoPopup;
    [SerializeField] private JukeboxInfoPopupHandler _jukeboxInfoPopupHandler;
    [SerializeField] private PopupController _jukeboxTextPopup;

    private Coroutine _diskSpinCoroutine;
    private Coroutine _enableCoroutine;

    private bool _applicationQuitting = false;
    public bool JukeBoxOpen { get => _jukeboxObject.activeSelf; }

    private const string NoSongName = "Ei valittua biisiä";
    private const string NoCreditsNames = "...";

    private enum JukeboxWindowType
    {
        PlaylistNavigation,
        MusicPlayer,
        ManagePlaylist
    }

    private JukeboxWindowType _currentWindowType = JukeboxWindowType.PlaylistNavigation;

    private List<string> _playlistNames;

    //private AudioCategoryType _previousAreaType = AudioCategoryType.None;

    public delegate void ChangeJukeBoxSong(MusicTrack track);
    public static event ChangeJukeBoxSong OnChangeJukeBoxSong;

    private void Awake() { Application.quitting += Quitting; }

    void Start() { StartCoroutine(Setup()); }

    private void OnEnable() { _enableCoroutine = StartCoroutine(Enable()); }

    private void OnDisable()
    {
        ToggleJukeboxScreen(false);
        StopJukeboxVisuals();

        if (_enableCoroutine != null)
        {
            StopCoroutine(_enableCoroutine);
            _enableCoroutine = null;

            if (!_applicationQuitting && !JukeboxManager.Instance) Debug.LogError("JukeboxManager was never found!");
            if (!_applicationQuitting && !MusicHandler.Instance) Debug.LogError("MusicHandler was never found!");

            return;
        }

        JukeboxManager.Instance.OnSetSongInfo -= SetSongInfo;
        JukeboxManager.Instance.OnStopJukeboxVisuals -= StopJukeboxVisuals;
        JukeboxManager.Instance.OnClearJukeboxVisuals -= ClearJukeboxVisuals;
        JukeboxManager.Instance.OnJukeboxMute -= SetMuteImage;
        MusicHandler.Instance.OnVolumeChange -= MainDiskIndicatorControl;
    }

    private IEnumerator Setup()
    {
        _jukeboxObject.SetActive(false);
        SetMuteImage(false);

        _closeButton.onClick.AddListener(() => ToggleJukeboxScreen(false));

        _soundMuteButton.onClick.AddListener(() => MuteJukeboxToggle());
        _addMusicInfoButton.onClick.AddListener(() => { _addMusicInfoPopup.SetActive(true); });

        yield return new WaitUntil(() => JukeboxManager.Instance);

        JukeboxManager.Instance.OnMusicTrackInfoPressed += OpenMusicTrackInfoPopup;
        _mainDiskHandler.OnMultiUseButtonPressed += UnmuteOnlyButton;
        JukeboxManager.Instance.OnPreviewStart += JukeboxPreviewPlaybackStart;
        JukeboxManager.Instance.OnPreviewEnd += JukeboxPreviewPlaybackEnd;
        JukeboxManager.Instance.OnShowTextPopup += _jukeboxTextPopup.ActivatePopUp;
    }

    private IEnumerator Enable()
    {
        yield return new WaitUntil(() => JukeboxManager.Instance && MusicHandler.Instance);

        JukeboxManager.Instance.OnSetSongInfo += SetSongInfo;
        JukeboxManager.Instance.OnStopJukeboxVisuals += StopJukeboxVisuals;
        JukeboxManager.Instance.OnClearJukeboxVisuals += ClearJukeboxVisuals;
        JukeboxManager.Instance.OnJukeboxMute += SetMuteImage;
        MusicHandler.Instance.OnVolumeChange += MainDiskIndicatorControl;

        if (JukeboxManager.Instance.CurrentTrackQueueData != null)
        {
            SetSongInfo(JukeboxManager.Instance.CurrentTrackQueueData.MusicTrack, false);
        }
        else
        {
            foreach (TextAutoScroll text in _trackNames) text.SetContent(NoSongName);
            foreach (TextAutoScroll text in _trackCreditsNames) text.SetContent(NoCreditsNames);
        }
    }

    private void Quitting() { _applicationQuitting = true; }

    private void MutedFromSettingsIndicator()
    {
        _mainDiskHandler.ToggleCustomIndicatorImage(false);
        _mainDiskHandler.SetIndicatorText(JukeboxMainDiskHandler.JukeboxDiskTextType.VolumeZero);
    }

    #region Mute
    private void UnmuteOnlyButton()
    {
        if (!JukeboxManager.Instance.JukeboxMuted || JukeboxManager.Instance.TrackPreviewActive) return;

        MuteJukeboxToggle();
    }

    private void MuteJukeboxToggle()
    {
        JukeboxManager manager = JukeboxManager.Instance;
        bool result = manager.PlaybackToggle(true);

        if (result) //Stopped
        {
            SetMuteImage(true);
            StopJukeboxVisuals();
            MainDiskIndicatorControl();
        }
        else //Playing
        {
            SetMuteImage(false);

            if (manager.CurrentTrackQueueData != null)
            {
                _mainDiskHandler.StartSpinDisk();

                if (AudioManager.Instance.GetMusicVolume() != 0)
                    _mainDiskHandler.ToggleIndicatorHolder(false);
                else
                    MutedFromSettingsIndicator();
            }
            else
                MainDiskIndicatorControl();
        }

        if (manager.CurrentTrackQueueData == null && !manager.TrackPreviewActive) _mainDiskHandler.ClearDisk();
    }

    private void SetMuteImage(bool onOff) { _soundMuteImage.gameObject.SetActive(onOff); }
    #endregion

    #region Visuals
    public void StopJukeboxVisuals()
    {
        if (_diskSpinCoroutine != null)
        {
            StopCoroutine(_diskSpinCoroutine);
            _diskSpinCoroutine = null;
        }

        _mainDiskHandler.StopDiskSpin();
        OnChangeJukeBoxSong?.Invoke(null);
    }

    public void ClearJukeboxVisuals()
    {
        foreach (TextAutoScroll text in _trackNames) text.SetContent(NoSongName);
        foreach (TextAutoScroll text in _trackCreditsNames) text.SetContent(NoCreditsNames);

        _mainDiskHandler.ClearDisk();
        MainDiskIndicatorControl();
    }

    public void ToggleJukeboxScreen(bool toggle)
    {
        AudioManager audioManager = AudioManager.Instance;

        audioManager?.SetJukeboxWindowState(toggle);
        _jukeboxObject.SetActive(toggle);

        if (toggle) //Open
        {
            JukeboxManager manager = JukeboxManager.Instance;

            if (!manager) return;

            SetMuteImage(manager.JukeboxMuted);

            //_previousAreaType = audioManager.CurrentAreaType;
            //audioManager.SetCurrentAreaCategoryName("Jukebox");

            if (string.IsNullOrEmpty(manager.TryPlayTrack(false)))
            {
                MainDiskIndicatorControl();
            }
            else
            {
                _mainDiskHandler.ToggleIndicatorHolder(false);
                _mainDiskHandler.StartSpinDisk();
            }

            if (manager.CurrentTrackQueueData == null && !manager.TrackPreviewActive) _mainDiskHandler.ClearDisk();
        }
        else if (audioManager) //Close
        {
            JukeboxManager jukeboxManager = JukeboxManager.Instance;

            //audioManager.SetCurrentAreaCategoryType(_previousAreaType);

            if (jukeboxManager && jukeboxManager.TrackPreviewActive && jukeboxManager.CurrentTrackQueueData != null)
                jukeboxManager.StopMusicPreview();
            else
                audioManager.PlayFallBackTrack();
                //audioManager.PlayMusic(_previousAreaType);

            _mainDiskHandler.StopDiskSpin();
        }
    }

    private void SetSongInfo(MusicTrack track, bool useAnimations = true)
    {
        if (track == null)
        {
            _mainDiskHandler.ClearDisk();

            foreach (TextAutoScroll text in _trackNames) text.SetContent(NoSongName, false, useAnimations);
            foreach (TextAutoScroll text in _trackCreditsNames) text.SetContent(NoCreditsNames, false, useAnimations);

            if (!JukeboxManager.Instance.JukeboxMuted && _jukeboxObject.activeSelf) MainDiskIndicatorControl();

            return;
        }

        string credits = track.JukeboxInfo.GetArtistNames();

        foreach (TextAutoScroll text in _trackNames) text.SetContent(track.Name, false, useAnimations);
        foreach (TextAutoScroll text in _trackCreditsNames) text.SetContent(credits, false, useAnimations);

        _mainDiskHandler.SetDisk(track.JukeboxInfo.Disk);

        if (!JukeboxManager.Instance.JukeboxMuted && _jukeboxObject.activeSelf && track.Music && _mainDiskHandler.StartSpinDisk())
            MainDiskIndicatorControl();

        _favoriteButtonHandler.Setup(JukeboxManager.Instance.GetTrackFavoriteType(track.Id), track.Id);
        OnChangeJukeBoxSong?.Invoke(track);
    }

    private void MainDiskIndicatorControl()
    {
        if (JukeboxManager.Instance.JukeboxMuted)
        {
            _mainDiskHandler.ToggleCustomIndicatorImage(true);
            _mainDiskHandler.SetIndicatorText(JukeboxMainDiskHandler.JukeboxDiskTextType.None);
        }
        else if (AudioManager.Instance.GetMusicVolume() == 0)
        {
            MutedFromSettingsIndicator();
        }
        else if (JukeboxManager.Instance.CurrentTrackQueueData != null)
        {
            _mainDiskHandler.ToggleIndicatorHolder(false);
            _mainDiskHandler.StartSpinDisk();
        }
        else
        {
            _mainDiskHandler.ToggleCustomIndicatorImage(false);
            _mainDiskHandler.SetIndicatorText(JukeboxMainDiskHandler.JukeboxDiskTextType.Empty);
        }
    }
    #endregion

    private void OpenMusicTrackInfoPopup(MusicTrack musicTrack, JukeboxManager.MusicTrackFavoriteType likeType)
    {
        _jukeboxInfoPopupHandler.Set(musicTrack, likeType);
    }

    private void JukeboxPreviewPlaybackStart()
    {
        if (AudioManager.Instance.GetMusicVolume() != 0)
        {
            _mainDiskHandler.SetIndicatorText(JukeboxMainDiskHandler.JukeboxDiskTextType.Preview);
            _mainDiskHandler.ToggleCustomIndicatorImage(false);
        }
        else
            MutedFromSettingsIndicator();
    }

    private void JukeboxPreviewPlaybackEnd()
    {
        bool muted = JukeboxManager.Instance.JukeboxMuted;

        if (muted)
        {
            _mainDiskHandler.SetIndicatorText(JukeboxMainDiskHandler.JukeboxDiskTextType.None);
            _mainDiskHandler.ToggleIndicatorHolder(true);
            _mainDiskHandler.ToggleCustomIndicatorImage(true);
        }
        else
        {
            _mainDiskHandler.ToggleIndicatorHolder(false);
        }
    }
}
