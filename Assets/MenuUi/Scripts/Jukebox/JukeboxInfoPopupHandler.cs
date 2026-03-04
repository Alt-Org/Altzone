using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Audio;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine;
using UnityEngine.UI;

public class JukeboxInfoPopupHandler : MonoBehaviour
{
    [Header("Close buttons")]
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _closeBackgroundButton;
    [Header("Track info")]
    [SerializeField] private TextAutoScroll _trackNameAutoScroll;
    [SerializeField] private TextAutoScroll _trackCreditsNamesAutoScroll;
    [Header("Disk image")]
    [SerializeField] private JukeboxSecondaryDiskHandler _secondaryDiskHandler;
    [Header("Preview")]
    [SerializeField] private JukeboxPreviewHandler _previewHandler;
    [Header("Favorite")]
    [SerializeField] private FavoriteHandlerBase _favoriteHandler;
    [Header("Add to playlist")]
    [SerializeField] private Button _addButton;
    [Header("Weblinks")]
    [SerializeField] private GameObject _webLinkButtonPrefab;
    [SerializeField] private Transform _webLinksContentTransform;

    private MusicTrack _musicTrack = null;
    public MusicTrack MusicTrack { get { return _musicTrack; } }

    private List<JukeboxWebLinkHandler> _weblinkButtons = new List<JukeboxWebLinkHandler>();
    private int _weblinkPointer = -1;

    public delegate void TrackPressed(MusicTrack musicTrack);
    public event TrackPressed OnTrackPressed;

    public delegate void PreviewPressed(JukeboxTrackButtonHandler buttonHandler, float previewDuration = -1);
    public event PreviewPressed OnPreviewPressed;

    private void Awake()
    {
        _closeButton.onClick.AddListener(() => Close());
        _closeBackgroundButton.onClick.AddListener(() => Close());
        _addButton.onClick.AddListener(() => AddTrack());

        _previewHandler.OnStartPreview += JukeboxManager.Instance.PlayPreview;
        OnTrackPressed += JukeboxManager.Instance.QueueTrack;
    }

    private void OnEnable()
    {
        if (JukeboxManager.Instance.TrackPreviewActive) JukeboxManager.Instance.StopMusicPreview();
    }

    private void OnDisable()
    {
        if (JukeboxManager.Instance.TrackPreviewActive) JukeboxManager.Instance.StopMusicPreview();
    }

    #region Weblink
    private JukeboxWebLinkHandler GetFreeWeblinkSlot()
    {
        _weblinkPointer++;

        if (_weblinkPointer >= _weblinkButtons.Count)
        {
            JukeboxWebLinkHandler handler = Instantiate(_webLinkButtonPrefab, _webLinksContentTransform).GetComponent<JukeboxWebLinkHandler>();
            _weblinkButtons.Add(handler);
        }

        return _weblinkButtons[_weblinkPointer];
    }

    private void ClearWeblinks()
    {
        while (_weblinkPointer >= 0)
        {
            _weblinkButtons[_weblinkPointer].Clear();
            _weblinkPointer--;
        }
    }
    #endregion

    public void Set(MusicTrack musicTrack, JukeboxManager.MusicTrackFavoriteType favoriteType)
    {
        if (_musicTrack == null || _musicTrack.Id != musicTrack.Id) ClearWeblinks();

        _favoriteHandler.Setup(JukeboxManager.Instance.GetTrackFavoriteType(musicTrack), musicTrack.Id);

        List<ArtistInfo> artists = musicTrack.JukeboxInfo.Artists;

        if (artists.Count != 0 && (_musicTrack == null || _musicTrack.Id != musicTrack.Id))
            foreach (ArtistInfo artist in artists)
                if (artist.Artist != null && !string.IsNullOrEmpty(artist.Artist.WebsiteAddress))
                    GetFreeWeblinkSlot().Set(artist.Artist);

        _musicTrack = musicTrack;
        _trackNameAutoScroll.SetContent(musicTrack.Name);
        _trackCreditsNamesAutoScroll.SetContent(musicTrack.JukeboxInfo.GetArtistNames());
        _secondaryDiskHandler.SetDisk(musicTrack.JukeboxInfo.Disk);
        _previewHandler.SetMusicTrack(musicTrack);
        _favoriteHandler.Setup(favoriteType, musicTrack.Id);
        gameObject.SetActive(true);
    }

    private void AddTrack() { JukeboxManager.Instance.QueueTrack(_musicTrack); }

    public void Close() { gameObject.SetActive(false); }
}
