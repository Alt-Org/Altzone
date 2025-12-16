using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Audio;
using Altzone.Scripts.ReferenceSheets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MenuUI.Scripts;

public class JukeBoxSoulhomeHandler : MonoBehaviour
{
    [SerializeField] private GameObject _jukeboxObject;
    [SerializeField] private JukeboxPlaylistNavigationHandler _playlistNavigationHandler;

    [Header("Disk")]
    //[SerializeField] private List<Image> _diskImage;
    //[SerializeField] private List<Transform> _diskTransform;
    //[SerializeField] private float _diskRotationSpeed = 100f;
    //[SerializeField] private Sprite _emptyDisk;
    [SerializeField] private JukeboxMainDiskHandler _mainDiskHandler;

    //[Header("Multiple locations")]
    [SerializeField] private List<TextAutoScroll> _trackNames;
    [SerializeField] private List<TextAutoScroll> _trackCreditsNames;
    //[SerializeField] private List<Button> _playButtons;
    //[SerializeField] private List<Image> _playButtonImages;
    //[SerializeField] private List<Button> _trackGoBackButtons;
    //[SerializeField] private List<Button> _trackGoForwardButtons;
    //[Space]
    //[SerializeField] private Sprite _playSprite;
    //[SerializeField] private Sprite _stopSprite;

    //[Header("BottomBar")]
    //[SerializeField] private GameObject _bottomBarObject;
    //[SerializeField] private Button _alternativeGoToMusicPlayerButton;

    //[Header("TopBarControls")]
    [SerializeField] private Button _closeButton;
    //[SerializeField] private Toggle _playlistNavigationButton;
    //[SerializeField] private Toggle _musicPlayerButton;
    //[SerializeField] private Toggle _managePlaylistButton;
    //[SerializeField] private TMP_Dropdown _playlistDropdown;

    //[Header("Windows")]
    //[SerializeField] private GameObject _playlistNavigationWindow;
    //[SerializeField] private GameObject _musicPlayerWindow;
    //[SerializeField] private GameObject _managePlaylistWindow;

    [SerializeField] private Button _soundMuteButton;
    [SerializeField] private Image _soundMuteImage;

    [SerializeField] private FavoriteButtonHandler _favoriteButtonHandler;
    //[SerializeField] private TextAutoScroll _textAutoScroll;

    [SerializeField] private Button _addMusicInfoButton;
    [SerializeField] private GameObject _addMusicInfoPopup;

    [SerializeField] private JukeboxInfoPopupHandler _jukeboxInfoPopupHandler;
    [SerializeField] private PopupController _jukeboxTextPopup;

    private Coroutine _diskSpinCoroutine;

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

    private string _previousAreaName = "";

    public delegate void ChangeJukeBoxSong(MusicTrack track);
    public static event ChangeJukeBoxSong OnChangeJukeBoxSong;

    void Start()
    {
        _jukeboxObject.SetActive(false);
        SetMuteImage(false);

        _closeButton.onClick.AddListener(() => ToggleJukeboxScreen(false));

        //_playlistNavigationButton.onValueChanged.AddListener((value) => { if (value) SwitchMainWindow(JukeboxWindowType.PlaylistNavigation); });
        //_musicPlayerButton.onValueChanged.AddListener((value) => { if (value) SwitchMainWindow(JukeboxWindowType.MusicPlayer); });
        //_managePlaylistButton.onValueChanged.AddListener((value) => { if (value) SwitchMainWindow(JukeboxWindowType.ManagePlaylist); });

        //_alternativeGoToMusicPlayerButton.onClick.AddListener(() => {
        //    SwitchMainWindow(JukeboxWindowType.MusicPlayer);
        //    _musicPlayerButton.isOn = true;
        //});
        //_playlistDropdown.onValueChanged.AddListener((value) => { PlaylistChange(value); });

        //foreach (Button button in _playButtons) button.onClick.AddListener(() => PlayStopButtonActivated());

        _soundMuteButton.onClick.AddListener(() => MuteJukeboxToggle());
        _addMusicInfoButton.onClick.AddListener(() => { _addMusicInfoPopup.SetActive(true); });

        _playlistNavigationHandler.OnInfoPressed += OpenMusicTrackInfoPopup;
        _mainDiskHandler.OnMultiUseButtonPressed += UnmuteOnlyButton;
        JukeboxManager.Instance.OnPreviewStart += JukeboxPreviewPlaybackStart;
        JukeboxManager.Instance.OnPreviewEnd += JukeboxPreviewPlaybackEnd;
        JukeboxManager.Instance.OnShowTextPopup += _jukeboxTextPopup.ActivatePopUp;
    }

    private void OnEnable()
    {
        JukeboxManager.Instance.OnSetSongInfo += SetSongInfo;
        JukeboxManager.Instance.OnStopJukeboxVisuals += StopJukeboxVisuals;
        JukeboxManager.Instance.OnClearJukeboxVisuals += ClearJukeboxVisuals;
        //JukeboxManager.Instance.OnSetPlayButtonImages += SetPlayButtonStates;
        JukeboxManager.Instance.OnJukeboxMute += SetMuteImage;

        if (JukeboxManager.Instance.CurrentTrackQueueData != null)
            SetSongInfo(JukeboxManager.Instance.CurrentTrackQueueData.MusicTrack);
        else
        {
            foreach (TextAutoScroll text in _trackNames) text.SetContent(NoSongName);
            foreach (TextAutoScroll text in _trackCreditsNames) text.SetContent(NoCreditsNames);
        }
    }

    private void OnDisable()
    {
        JukeboxManager.Instance.OnSetSongInfo -= SetSongInfo;
        JukeboxManager.Instance.OnStopJukeboxVisuals -= StopJukeboxVisuals;
        JukeboxManager.Instance.OnClearJukeboxVisuals -= ClearJukeboxVisuals;
        //JukeboxManager.Instance.OnSetPlayButtonImages -= SetPlayButtonStates;
        JukeboxManager.Instance.OnJukeboxMute -= SetMuteImage;

        ToggleJukeboxScreen(false);
        StopJukeboxVisuals();
    }

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

    private void PlayStopButtonActivated()
    {
        if (JukeboxManager.Instance.CurrentTrackQueueData == null) return;

        bool result = JukeboxManager.Instance.PlaybackToggle(false);

        //foreach (Image image in _playButtonImages)
        //{
        //    if (result) //Stopped
        //    {
        //        image.sprite = _playSprite;
        //        StopJukeboxVisuals();
        //    }
        //    else //Playing
        //    {
        //        image.sprite = _stopSprite;

        //        if (_diskSpinCoroutine != null)
        //        {
        //            StopCoroutine(_diskSpinCoroutine);
        //            _diskSpinCoroutine = null;
        //            foreach (Transform rotationT in _diskTransform) rotationT.rotation = Quaternion.identity;
        //        }

        //        _diskSpinCoroutine = StartCoroutine(SpinDisks());
        //    }
        //}

        if (result) //Stopped
        {
            StopJukeboxVisuals();
            if (AudioManager.Instance.GetMusicVolume() != 0)
                _mainDiskHandler.SetIndicatorText(JukeboxMainDiskHandler.JukeboxDiskTextType.Stopped);
            else
                MutedFromSettingsIndicator();
        }
        else //Playing
        {
            if (_diskSpinCoroutine != null)
            {
                StopCoroutine(_diskSpinCoroutine);
                _diskSpinCoroutine = null;
                //foreach (Transform rotationT in _diskTransform) rotationT.rotation = Quaternion.identity;
            }

            //_diskSpinCoroutine = StartCoroutine(SpinDisks());
            _mainDiskHandler.ToggleIndicatorHolder(false);
            _mainDiskHandler.StartSpinDisk();
        }
    }

    #region Visuals
    public void StopJukeboxVisuals()
    {
        if (_diskSpinCoroutine != null)
        {
            StopCoroutine(_diskSpinCoroutine);
            _diskSpinCoroutine = null;
        }

        _mainDiskHandler.StopDiskSpin();

        if (OnChangeJukeBoxSong != null) OnChangeJukeBoxSong.Invoke(null);
    }

    public void ClearJukeboxVisuals()
    {
        foreach (TextAutoScroll text in _trackNames) text.SetContent(NoSongName);
        foreach (TextAutoScroll text in _trackCreditsNames) text.SetContent(NoCreditsNames);
        //foreach (Image image in _diskImage) image.sprite = _emptyDisk;

        _mainDiskHandler.ClearDisk();
        MainDiskIndicatorControl();
    }

    public void ToggleJukeboxScreen(bool toggle)
    {
        _jukeboxObject.SetActive(toggle);

        if (toggle)
        {
            JukeboxManager manager = JukeboxManager.Instance;

            SetMuteImage(manager.JukeboxMuted);

            _previousAreaName = AudioManager.Instance.CurrentAreaName;
            AudioManager.Instance.SetCurrentAreaCategoryName("Jukebox");

            if (string.IsNullOrEmpty(manager.TryPlayTrack()))
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
        else
        {
            AudioManager.Instance.SetCurrentAreaCategoryName(_previousAreaName);
            AudioManager.Instance.PlayMusic(_previousAreaName, MusicHandler.MusicSwitchType.CrossFade);
            _mainDiskHandler.StopDiskSpin();
        }
    }

    private void SetSongInfo(MusicTrack track)
    {
        if (track == null)
        {
            _mainDiskHandler.ClearDisk();

            foreach (TextAutoScroll text in _trackNames) text.SetContent(NoSongName);
            foreach (TextAutoScroll text in _trackCreditsNames) text.SetContent(NoCreditsNames);

            if (!JukeboxManager.Instance.JukeboxMuted && _jukeboxObject.activeSelf) MainDiskIndicatorControl();

            return;
        }

        string credits = track.JukeboxInfo.GetArtistNames();

        foreach (TextAutoScroll text in _trackNames) text.SetContent(track.Name);
        foreach (TextAutoScroll text in _trackCreditsNames) text.SetContent(credits);
        //foreach (Image image in _diskImage) image.sprite = track.JukeboxInfo.Disk;
        _mainDiskHandler.SetDisk(track.JukeboxInfo.Disk);

        if (!JukeboxManager.Instance.JukeboxMuted && _jukeboxObject.activeSelf && track.Music != null && _mainDiskHandler.StartSpinDisk())
            MainDiskIndicatorControl();

        _favoriteButtonHandler.Setup(JukeboxManager.Instance.GetTrackFavoriteType(track), track.Id);
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
