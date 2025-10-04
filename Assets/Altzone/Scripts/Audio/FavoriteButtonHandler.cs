using Altzone.Scripts.Audio;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine;
using UnityEngine.UI;
using static Altzone.Scripts.Audio.JukeboxManager;

[RequireComponent(typeof(Button))]
public class FavoriteButtonHandler : MonoBehaviour
{
    private Button _changeFavoriteStatusButton;
    [SerializeField] private Image _favoriteImage;
    [SerializeField] private Sprite _favoriteSprite;
    [SerializeField] private Sprite _okSprite;
    [SerializeField] private Sprite _dislikeSprite;

    private string _musicTrackId;
    public string MusicTrackId { get { return _musicTrackId; } }

    private MusicTrackFavoriteType _musicTrackFavoriteType = MusicTrackFavoriteType.Neutral;
    public MusicTrackFavoriteType MusicTrackLikeType { get { return _musicTrackFavoriteType; } }

    public delegate void FavoriteStatusChange();
    public event FavoriteStatusChange OnFavoriteStatusChange;

    private void Awake()
    {
        _changeFavoriteStatusButton = GetComponent<Button>();
        _changeFavoriteStatusButton.onClick.AddListener(() => ChangeFavoriteButtonPressed());
    }

    private void Start()
    {
        JukeboxManager.Instance.OnFavoriteButtonChange += OnFavoriteButtonChange;
    }

    private void OnDestroy()
    {
        JukeboxManager.Instance.OnFavoriteButtonChange -= OnFavoriteButtonChange;
    }

    private void OnEnable()
    {
        MusicTrack musicTrack = JukeboxManager.Instance.GetMusicTrack(_musicTrackId);

        if (musicTrack != null) OnFavoriteButtonChange(_musicTrackId, JukeboxManager.Instance.GetTrackFavoriteType(musicTrack));
    }

    public void Setup(MusicTrackFavoriteType type, string musicTrackId)
    {
        _musicTrackFavoriteType = type;
        ChangeFavoriteImage(type);
        _musicTrackId = musicTrackId;
    }

    private void ChangeFavoriteButtonPressed()
    {
        switch (_musicTrackFavoriteType)
        {
            case MusicTrackFavoriteType.Neutral: _musicTrackFavoriteType = MusicTrackFavoriteType.Like; ChangeFavoriteImage(_musicTrackFavoriteType); break;
            case MusicTrackFavoriteType.Like: _musicTrackFavoriteType = MusicTrackFavoriteType.Dislike; ChangeFavoriteImage(_musicTrackFavoriteType); break;
            case MusicTrackFavoriteType.Dislike: _musicTrackFavoriteType = MusicTrackFavoriteType.Neutral; ChangeFavoriteImage(_musicTrackFavoriteType); break;
        }

        JukeboxManager.Instance.InvokeOnFavoriteButtonChange(MusicTrackId, _musicTrackFavoriteType);
    }

    private void ChangeFavoriteImage(MusicTrackFavoriteType type)
    {
        switch (type)
        {
            case MusicTrackFavoriteType.Neutral: _favoriteImage.sprite = _okSprite; break;
            case MusicTrackFavoriteType.Like: _favoriteImage.sprite = _favoriteSprite; break;
            case MusicTrackFavoriteType.Dislike: _favoriteImage.sprite = _dislikeSprite; break;
        }
    }

    private void OnFavoriteButtonChange(string musicTrackId, MusicTrackFavoriteType type)
    {
        if (_musicTrackId == musicTrackId)
        {
            _musicTrackFavoriteType = type;
            ChangeFavoriteImage(type);
        }
    }
}
