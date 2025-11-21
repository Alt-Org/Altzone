using Altzone.Scripts.Audio;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class FavoriteButtonHandler : FavoriteHandlerBase
{
    private Button _changeFavoriteStatusButton;
    [SerializeField] private Sprite _favoriteSprite;
    [SerializeField] private Sprite _okSprite;
    [SerializeField] private Sprite _dislikeSprite;

    private void Awake()
    {
        _changeFavoriteStatusButton = GetComponent<Button>();
        _changeFavoriteStatusButton.onClick.AddListener(() => ChangeFavoriteButtonPressed());
    }

    protected override void ChangeFavoriteImage(JukeboxManager.MusicTrackFavoriteType type)
    {
        switch (type)
        {
            case JukeboxManager.MusicTrackFavoriteType.Neutral: _favoriteImage.sprite = _okSprite; break;
            case JukeboxManager.MusicTrackFavoriteType.Like: _favoriteImage.sprite = _favoriteSprite; break;
            case JukeboxManager.MusicTrackFavoriteType.Dislike: _favoriteImage.sprite = _dislikeSprite; break;
        }
    }
}
