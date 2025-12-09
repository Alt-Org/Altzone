using System.Collections;
using Altzone.Scripts.Audio;
using Altzone.Scripts.ReferenceSheets;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JukeboxTrackButtonHandler : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private TextAutoScroll _trackNameAutoScroll;
    [SerializeField] private TextAutoScroll _trackCreditsNamesAutoScroll;
    [Space]
    [SerializeField] private Image _trackImage;
    [SerializeField] private float _diskRotationSpeed = 100f;
    [SerializeField] private Button _addButton;
    [SerializeField] private FavoriteHandlerBase _favoriteButtonHandler;
    [Space]
    [SerializeField] private Button _previewButton;
    [SerializeField] private Button _infoButton;
    [Space]
    [SerializeField] private float _buttonPressCancelTime = 0.25f;

    private bool _buttonInputCanceled = false;

    private MusicTrack _musicTrack = null;
    public MusicTrack MusicTrack {  get { return _musicTrack; } }

    private Coroutine _diskSpinCoroutine;
    private Coroutine _buttonCancelCoroutine;

    public delegate void TrackPressed(MusicTrack musicTrack);
    public event TrackPressed OnTrackPressed;

    public delegate void PreviewPressed(JukeboxTrackButtonHandler buttonHandler, float previewDuration = -1);
    public event PreviewPressed OnPreviewPressed;

    public delegate void InfoPressed(MusicTrack musicTrack, JukeboxManager.MusicTrackFavoriteType likeType);
    public event InfoPressed OnInfoPressed;

    private void Awake()
    {
        _addButton.onClick.AddListener(() => AddButtonClicked());
        _previewButton.onClick.AddListener(() => PreviewButtonClicked());

        if (_infoButton != null) _infoButton.onClick.AddListener(() => InfoButtonClicked());
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_buttonCancelCoroutine != null)
        {
            StopCoroutine(CancelButtonPress());
            _buttonCancelCoroutine = null;
        }

        _buttonCancelCoroutine = StartCoroutine(CancelButtonPress());
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_buttonCancelCoroutine != null)
        {
            StopCoroutine(CancelButtonPress());
            _buttonCancelCoroutine = null;
        }
    }

    private IEnumerator CancelButtonPress()
    {
        float timer = 0f;

        while (timer < _buttonPressCancelTime)
        {
            yield return null;
            timer += Time.deltaTime;
        }

        _buttonInputCanceled = true;
    }

    public void AddButtonClicked()
    {
        if (!_buttonInputCanceled && _musicTrack != null) OnTrackPressed.Invoke(_musicTrack);

        _buttonInputCanceled = false;
    }

    public void PreviewButtonClicked()
    {
        if (!_buttonInputCanceled && _musicTrack != null) OnPreviewPressed.Invoke(this);

        _buttonInputCanceled = false;
    }

    public void InfoButtonClicked()
    {
        if (!_buttonInputCanceled && _musicTrack != null) OnInfoPressed.Invoke(_musicTrack, _favoriteButtonHandler.MusicTrackLikeType);

        _buttonInputCanceled = false;
    }

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
