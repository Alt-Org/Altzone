using Altzone.Scripts.ReferenceSheets;
using UnityEngine;
using UnityEngine.UI;
using static Altzone.Scripts.Audio.JukeboxManager;

namespace Altzone.Scripts.Audio
{
    public class FavoriteHandlerBase : MonoBehaviour
    {
        [SerializeField] protected Image _favoriteImage;

        protected string _musicTrackId;
        public string MusicTrackId { get { return _musicTrackId; } }

        protected MusicTrackFavoriteType _musicTrackFavoriteType = MusicTrackFavoriteType.Neutral;
        public MusicTrackFavoriteType MusicTrackLikeType { get { return _musicTrackFavoriteType; } }

        public delegate void FavoriteStatusChange();
        public event FavoriteStatusChange OnFavoriteStatusChange;

        protected void Start()
        {
            JukeboxManager.Instance.OnFavoriteButtonChange += OnFavoriteButtonChange;
        }

        protected void OnDestroy()
        {
            JukeboxManager.Instance.OnFavoriteButtonChange -= OnFavoriteButtonChange;
        }

        protected void OnEnable()
        {
            JukeboxManager manager = JukeboxManager.Instance;
            MusicTrack musicTrack = manager.GetMusicTrack(_musicTrackId);

            if (musicTrack != null) OnFavoriteButtonChange(_musicTrackId, manager.GetTrackFavoriteType(musicTrack));
        }

        public void Setup(MusicTrackFavoriteType type, string musicTrackId)
        {
            _musicTrackFavoriteType = type;
            ChangeFavoriteImage(type);
            _musicTrackId = musicTrackId;
        }

        protected virtual void ChangeFavoriteButtonPressed()
        {
            switch (_musicTrackFavoriteType)
            {
                case MusicTrackFavoriteType.Neutral: _musicTrackFavoriteType = MusicTrackFavoriteType.Like; /*ChangeFavoriteImage(_musicTrackFavoriteType);*/ break;
                case MusicTrackFavoriteType.Like: _musicTrackFavoriteType = MusicTrackFavoriteType.Dislike; /*ChangeFavoriteImage(_musicTrackFavoriteType);*/ break;
                case MusicTrackFavoriteType.Dislike: _musicTrackFavoriteType = MusicTrackFavoriteType.Neutral; /*ChangeFavoriteImage(_musicTrackFavoriteType);*/ break;
            }

            JukeboxManager.Instance.InvokeOnFavoriteButtonChange(_musicTrackId, _musicTrackFavoriteType);
        }

        protected void OnFavoriteButtonChange(string musicTrackId, MusicTrackFavoriteType type)
        {
            if (_musicTrackId != musicTrackId) return;

            _musicTrackFavoriteType = type;
            ChangeFavoriteImage(type);
        }

        protected virtual void ChangeFavoriteImage(MusicTrackFavoriteType type) { }
    }
}
