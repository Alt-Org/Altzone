using Altzone.Scripts.Audio;
using UnityEngine;
using UnityEngine.UI;
using static Altzone.Scripts.Audio.JukeboxManager;

public class BasicFavoriteHandler : FavoriteHandlerBase
{
    [SerializeField] private Toggle _changeFavoriteStatusToggle;

    private bool _toggleActive = false;

    private void Awake()
    {
        if (_changeFavoriteStatusToggle != null)
            _changeFavoriteStatusToggle.onValueChanged.AddListener((value) =>
            {
                if (_toggleActive) return;

                _toggleActive = true;
                ChangeFavoriteButtonPressed();
                _toggleActive = false;

            });
    }

    protected override void ChangeFavoriteButtonPressed()
    {
        switch (_musicTrackFavoriteType)
        {
            case MusicTrackFavoriteType.Neutral: _musicTrackFavoriteType = MusicTrackFavoriteType.Like; /*ChangeFavoriteImage(_musicTrackFavoriteType);*/ break;
            case MusicTrackFavoriteType.Like: _musicTrackFavoriteType = MusicTrackFavoriteType.Neutral; /*ChangeFavoriteImage(_musicTrackFavoriteType); */break;
            case MusicTrackFavoriteType.Dislike: _musicTrackFavoriteType = MusicTrackFavoriteType.Neutral; /*ChangeFavoriteImage(_musicTrackFavoriteType);*/ break;
        }

        JukeboxManager.Instance.InvokeOnFavoriteButtonChange(MusicTrackId, _musicTrackFavoriteType);
    }

    protected override void ChangeFavoriteImage(MusicTrackFavoriteType type)
    {
        _favoriteImage.gameObject.SetActive(type == MusicTrackFavoriteType.Like);
        //Debug.LogError(type == MusicTrackFavoriteType.Like);
        //Debug.LogError(_favoriteImage.gameObject);

        if (_changeFavoriteStatusToggle != null) _changeFavoriteStatusToggle.isOn = type == MusicTrackFavoriteType.Like;
    }
}
