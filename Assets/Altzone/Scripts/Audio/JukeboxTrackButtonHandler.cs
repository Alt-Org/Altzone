using System.Collections;
using Altzone.Scripts.Audio;
using Altzone.Scripts.ReferenceSheets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class JukeboxTrackButtonHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _trackNameText;
    [SerializeField] private Image _trackImage;
    [SerializeField] private float _diskRotationSpeed = 100f;
    [SerializeField] private Button _addButton;
    [SerializeField] private FavoriteButtonHandler _favoriteButtonHandler;
    [SerializeField] private Button _previewButton;

    private int _trackLinearIndex = 0;

    private MusicTrack _musicTrack = null;
    public MusicTrack MusicTrack {  get { return _musicTrack; } }

    private Coroutine _diskSpinCoroutine;

    //public delegate void TrackPressed(int startIndex);
    public delegate void TrackPressed(MusicTrack musicTrack);
    public event TrackPressed OnTrackPressed;

    public delegate void PreviewPressed(JukeboxTrackButtonHandler buttonHandler);
    public event PreviewPressed OnPreviewPressed;

    private void Awake()
    {
        _addButton.onClick.AddListener(() => AddButtonClicked());
        _previewButton.onClick.AddListener(() => PreviewButtonClicked());
    }

    //public bool InUse() { return _currentTrack != null; }

    public void AddButtonClicked() { if (_musicTrack != null) OnTrackPressed.Invoke(_musicTrack); }
    public void PreviewButtonClicked() { if (_musicTrack != null) OnPreviewPressed.Invoke(this); }

    public void SetTrack(MusicTrack musicTrack, int trackLinearIndex, JukeboxManager.MusicTrackFavoriteType likeType)
    {
        _trackLinearIndex = trackLinearIndex;
        _musicTrack = musicTrack;
        _trackNameText.text = musicTrack.Name;
        _trackImage.sprite = musicTrack.Info.Disk;
        gameObject.SetActive(true);
        _favoriteButtonHandler.Setup(likeType, musicTrack.Id);
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
