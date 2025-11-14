using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Audio;
using Altzone.Scripts.ReferenceSheets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JukeBoxSoulhomeHandler : MonoBehaviour
{
    [SerializeField] private GameObject _jukeboxObject;

    [Header("Disk")]
    [SerializeField] private List<Image> _diskImage;
    [SerializeField] private List<Transform> _diskTransform;
    [SerializeField] private float _diskRotationSpeed = 100f;
    [SerializeField] private Sprite _emptyDisk;

    //[Header("Multiple locations")]
    [SerializeField] private List<TMP_Text> _trackNames;
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
    [SerializeField] private TextAutoScroll _textAutoScroll;

    [SerializeField] private Button _addMusicInfoButton;
    [SerializeField] private GameObject _addMusicInfoPopup;
    private Coroutine _diskSpinCoroutine;

    public bool JukeBoxOpen { get => _jukeboxObject.activeSelf; }

    private const string NoSongName = "Ei valittua biisiä";

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
    }

    private void OnEnable()
    {
        JukeboxManager.Instance.OnSetSongInfo += SetSongInfo;
        JukeboxManager.Instance.OnStopJukeboxVisuals += StopJukeboxVisuals;
        JukeboxManager.Instance.OnClearJukeboxVisuals += ClearJukeboxVisuals;
        //JukeboxManager.Instance.OnSetPlayButtonImages += SetPlayButtonStates;

        if (JukeboxManager.Instance.CurrentTrackQueueData != null)
            SetSongInfo(JukeboxManager.Instance.CurrentTrackQueueData.MusicTrack);
        else
            foreach (TMP_Text text in _trackNames) text.text = NoSongName;
    }

    private void OnDisable()
    {
        JukeboxManager.Instance.OnSetSongInfo -= SetSongInfo;
        JukeboxManager.Instance.OnStopJukeboxVisuals -= StopJukeboxVisuals;
        JukeboxManager.Instance.OnClearJukeboxVisuals -= ClearJukeboxVisuals;
        //JukeboxManager.Instance.OnSetPlayButtonImages -= SetPlayButtonStates;

        StopJukeboxVisuals();
    }

    private void MuteJukeboxToggle()
    {
        bool result = JukeboxManager.Instance.PlaybackToggle(true);

        if (result) //Stopped
        {
            SetMuteImage(true);
            StopJukeboxVisuals();
        }
        else //Playing
        {
            SetMuteImage(false);

            if (_diskSpinCoroutine != null)
            {
                StopCoroutine(_diskSpinCoroutine);
                _diskSpinCoroutine = null;
                foreach (Transform rotationT in _diskTransform) rotationT.rotation = Quaternion.identity;
            }

            _diskSpinCoroutine = StartCoroutine(SpinDisks());
        }
    }

    private void SetMuteImage(bool onOff) { _soundMuteImage.gameObject.SetActive(onOff); }

    //private void PlaylistChange(int value)
    //{

    //}

    //private void SetPlayButtonStates(bool value)
    //{
    //    foreach (Image image in _playButtonImages)
    //    {
    //        if (!value) //Stopped
    //            image.sprite = _playSprite;
    //        else //Playing
    //            image.sprite = _stopSprite;
    //    }
    //}

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
        }
        else //Playing
        {
            if (_diskSpinCoroutine != null)
            {
                StopCoroutine(_diskSpinCoroutine);
                _diskSpinCoroutine = null;
                foreach (Transform rotationT in _diskTransform) rotationT.rotation = Quaternion.identity;
            }

            _diskSpinCoroutine = StartCoroutine(SpinDisks());
        }
    }

    //private void SwitchMainWindow(JukeboxWindowType type)
    //{
    //    switch (_currentWindowType)
    //    {
    //        case JukeboxWindowType.PlaylistNavigation: _playlistNavigationWindow.SetActive(false); break;
    //        case JukeboxWindowType.MusicPlayer: _musicPlayerWindow.SetActive(false); break;
    //        case JukeboxWindowType.ManagePlaylist: _managePlaylistWindow.SetActive(false); break;
    //    }

    //    _currentWindowType = type;

    //    switch (_currentWindowType)
    //    {
    //        case JukeboxWindowType.PlaylistNavigation: _playlistNavigationWindow.SetActive(true); break;
    //        case JukeboxWindowType.MusicPlayer: _musicPlayerWindow.SetActive(true); break;
    //        case JukeboxWindowType.ManagePlaylist: _managePlaylistWindow.SetActive(true); break;
    //    }

    //    _bottomBarObject.SetActive(!(_currentWindowType == JukeboxWindowType.MusicPlayer));
    //}

    //public string PlayTrack() { return JukeboxManager.Instance.PlayTrack(); }

    public void StopJukeboxVisuals()
    {
        if (_diskSpinCoroutine != null)
        {
            StopCoroutine(_diskSpinCoroutine);
            _diskSpinCoroutine = null;

            foreach (Transform rotationT in _diskTransform) rotationT.rotation = Quaternion.identity;
        }

        OnChangeJukeBoxSong?.Invoke(null);
    }

    public void ClearJukeboxVisuals()
    {
        foreach (TMP_Text text in _trackNames) text.text = NoSongName;
        foreach (Image image in _diskImage) image.sprite = _emptyDisk;
    }

    public void ToggleJukeboxScreen(bool toggle)
    {
        _jukeboxObject.SetActive(toggle);

        if (toggle)
        {
            _previousAreaName = AudioManager.Instance?.CurrentAreaName;
            AudioManager.Instance?.SetCurrentAreaCategoryName("Jukebox");

            JukeboxManager.Instance.TryPlayTrack();
        }
        else
        {
            AudioManager.Instance?.SetCurrentAreaCategoryName(_previousAreaName);

            //bool jukeboxSoulhome = SettingsCarrier.Instance.CanPlayJukeboxInArea(SettingsCarrier.JukeboxPlayArea.Soulhome);

            //if (!jukeboxSoulhome) AudioManager.Instance.PlayFallBackTrack(MusicHandler.MusicSwitchType.CrossFade);

            AudioManager.Instance.PlayMusic(_previousAreaName, MusicHandler.MusicSwitchType.CrossFade);

            //if (JukeboxManager.Instance.CurrentTrackQueueData != null) SetSongInfo(JukeboxManager.Instance.GetNotHatedMusicTrack());
        }
    }

    private void SetSongInfo(MusicTrack track)
    {
        if (track == null) return;

        foreach (TMP_Text text in _trackNames) text.text = track.Name;
        foreach (Image image in _diskImage) image.sprite = track.Info.Disk;

        if (_diskSpinCoroutine != null)
        {
            StopCoroutine(_diskSpinCoroutine);
            _diskSpinCoroutine = null;
            foreach (Transform rotationT in _diskTransform) rotationT.rotation = Quaternion.identity;
        }

        if (_jukeboxObject.activeSelf && track.Music != null) _diskSpinCoroutine = StartCoroutine(SpinDisks());

        _textAutoScroll.ContentChange();
        _favoriteButtonHandler.Setup(JukeboxManager.Instance.GetTrackFavoriteType(track), track.Id);

        OnChangeJukeBoxSong?.Invoke(track);
    }

    private IEnumerator SpinDisks()
    {
        while (true)
        {
            foreach (Transform rotationT in _diskTransform) rotationT.Rotate(Vector3.forward * -_diskRotationSpeed * Time.deltaTime);

            yield return null;
        }
    }
}
