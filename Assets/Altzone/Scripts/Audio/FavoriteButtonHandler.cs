using Altzone.Scripts.Audio;
using UnityEngine;
using UnityEngine.UI;
using static Altzone.Scripts.Audio.JukeboxManager;

[RequireComponent(typeof(Button))]
public class FavoriteButtonHandler : FavoriteHandlerBase
{
    private Button _changeFavoriteStatusButton;
    [SerializeField] private Sprite _favoriteSprite;
    [SerializeField] private Color _favoriteColor = Color.white;
    [SerializeField] private Sprite _okSprite;
    [SerializeField] private Color _okColor = Color.white;
    [SerializeField] private Sprite _dislikeSprite;

    private void Awake()
    {
        _changeFavoriteStatusButton = GetComponent<Button>();
        _changeFavoriteStatusButton.onClick.AddListener(() => ChangeFavoriteButtonPressed());
    }

    protected override void ChangeFavoriteButtonPressed()
    {
        switch (_musicTrackFavoriteType)
        {
            case MusicTrackFavoriteType.Neutral: _musicTrackFavoriteType = MusicTrackFavoriteType.Like; break;
            case MusicTrackFavoriteType.Like: _musicTrackFavoriteType = MusicTrackFavoriteType.Neutral; break;
            case MusicTrackFavoriteType.Dislike: _musicTrackFavoriteType = MusicTrackFavoriteType.Like; break;
        }

        JukeboxManager.Instance.InvokeOnFavoriteButtonChange(_musicTrackId, _musicTrackFavoriteType);
    }

    protected override void ChangeFavoriteImage(JukeboxManager.MusicTrackFavoriteType type)
    {
        switch (type)
        {
            case JukeboxManager.MusicTrackFavoriteType.Neutral: _favoriteImage.sprite = _okSprite; _favoriteImage.color = _okColor; break;
            case JukeboxManager.MusicTrackFavoriteType.Like: _favoriteImage.sprite = _favoriteSprite; _favoriteImage.color = _favoriteColor; break;
            case JukeboxManager.MusicTrackFavoriteType.Dislike: _favoriteImage.sprite = _dislikeSprite; break;
        }
    }
}
