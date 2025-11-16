using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Audio;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine;
using UnityEngine.UI;

public class JukeboxInfoPopupHandler : MonoBehaviour
{
    [SerializeField] private GameObject _webLinkButtonPrefab;
    [SerializeField] private JukeboxTrackButtonHandler _buttonHandler;
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _closeBackgroundButton;

    [SerializeField] private Transform _webLinksContentTransform;
    private List<JukeboxWebLinkHandler> _weblinkButtons = new List<JukeboxWebLinkHandler>();
    private int _weblinkPointer = -1;

    private void Awake()
    {
        _closeButton.onClick.AddListener(() => Close());
        _closeBackgroundButton.onClick.AddListener(() => Close());
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
        if (_buttonHandler.MusicTrack != null && _buttonHandler.MusicTrack.Id != musicTrack.Id) ClearWeblinks();

        List<ArtistInfo> artists = musicTrack.JukeboxInfo.Artists;

        if (artists.Count != 0)
            foreach (ArtistInfo artist in artists)
                GetFreeWeblinkSlot().Set(artist.Artist);

        _buttonHandler.SetTrack(musicTrack, -1, favoriteType);
        gameObject.SetActive(true);
    }

    public void Close() { gameObject.SetActive(false); }
}
