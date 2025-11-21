using System.Collections;
using Altzone.Scripts.Audio;
using Altzone.Scripts.ReferenceSheets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JukeboxTrackButtonHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _trackNameText;
    [SerializeField] private TextAutoScroll _trackNameAutoScroll;
    [SerializeField] private TextMeshProUGUI _trackCreditsNamesText;
    [SerializeField] private TextAutoScroll _trackCreditsNamesAutoScroll;
    [Space]
    [SerializeField] private Image _trackImage;
    [SerializeField] private float _diskRotationSpeed = 100f;
    [SerializeField] private Button _addButton;
    [SerializeField] private FavoriteHandlerBase _favoriteButtonHandler;
    [Space]
    [SerializeField] private Button _previewButton;
    [SerializeField] private Button _infoButton;

    private MusicTrack _musicTrack = null;
    public MusicTrack MusicTrack {  get { return _musicTrack; } }

    private Coroutine _diskSpinCoroutine;

    public delegate void TrackPressed(MusicTrack musicTrack);
    public event TrackPressed OnTrackPressed;

    public delegate void PreviewPressed(JukeboxTrackButtonHandler buttonHandler);
    public event PreviewPressed OnPreviewPressed;

    public delegate void InfoPressed(MusicTrack musicTrack, JukeboxManager.MusicTrackFavoriteType likeType);
    public event InfoPressed OnInfoPressed;

    private void Awake()
    {
        _addButton.onClick.AddListener(() => AddButtonClicked());
        _previewButton.onClick.AddListener(() => PreviewButtonClicked());

        if (_infoButton != null) _infoButton.onClick.AddListener(() => InfoButtonClicked());
    }

    public void AddButtonClicked() { if (_musicTrack != null) OnTrackPressed.Invoke(_musicTrack); }

    public void PreviewButtonClicked() { if (_musicTrack != null) OnPreviewPressed.Invoke(this); }

    public void InfoButtonClicked() { if (_musicTrack != null) OnInfoPressed.Invoke(_musicTrack, _favoriteButtonHandler.MusicTrackLikeType); }

    public void SetTrack(MusicTrack musicTrack, int trackLinearIndex, JukeboxManager.MusicTrackFavoriteType likeType)
    {
        _musicTrack = musicTrack;
        //_trackNameText.text = musicTrack.Name;
        _trackNameAutoScroll.SetContent(musicTrack.Name);
        //_trackCreditsNamesText.text = musicTrack.JukeboxInfo.GetArtistNames();
        _trackCreditsNamesAutoScroll.SetContent(musicTrack.JukeboxInfo.GetArtistNames());
        _trackImage.sprite = musicTrack.JukeboxInfo.Disk;
        gameObject.SetActive(true);

        if (_favoriteButtonHandler != null) _favoriteButtonHandler.Setup(likeType, musicTrack.Id);
    }

    public void Clear() { _musicTrack = null; gameObject.SetActive(false); }

    public void SetVisibility(bool value) { gameObject.SetActive(value); }

    public void StartDiskSpin() { StopDiskSpin(); _diskSpinCoroutine = StartCoroutine(SpinDisk()); }

    public void StopDiskSpin()
    {
        if (_diskSpinCoroutine != null)
        {
            StopCoroutine(_diskSpinCoroutine);
            _diskSpinCoroutine = null;
        }

        _trackImage.transform.rotation = Quaternion.identity;
    }

    private IEnumerator SpinDisk()
    {
        while (true)
        {
            _trackImage.transform.Rotate(Vector3.forward * -_diskRotationSpeed * Time.deltaTime);

            yield return null;
        }
    }
}
