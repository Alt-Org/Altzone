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
    [SerializeField] private Button _addButton;
    [SerializeField] private FavoriteButtonHandler _favoriteButtonHandler;

    private int _trackLinearIndex = 0;

    private MusicTrack _currentTrack = null;
    public MusicTrack CurrentTrack {  get { return _currentTrack; } }


    //public delegate void TrackPressed(int startIndex);
    public delegate void TrackPressed(MusicTrack musicTrack);
    public event TrackPressed OnTrackPressed;

    private void Awake()
    {
        _addButton.onClick.AddListener(() => AddButtonClicked());
    }

    //public bool InUse() { return _currentTrack != null; }

    public void AddButtonClicked() { if (_currentTrack != null) OnTrackPressed.Invoke(_currentTrack); }

    public void SetTrack(MusicTrack musicTrack, int trackLinearIndex, JukeboxManager.MusicTrackFavoriteType likeType)
    {
        _trackLinearIndex = trackLinearIndex;
        _currentTrack = musicTrack;
        _trackNameText.text = musicTrack.Name;
        _trackImage.sprite = musicTrack.Info.Disk;
        gameObject.SetActive(true);
        _favoriteButtonHandler.Setup(likeType, musicTrack.Id);
    }

    public void Clear() { _currentTrack = null; gameObject.SetActive(false); }

    public void SetVisibility(bool value) { gameObject.SetActive(value); }
}
