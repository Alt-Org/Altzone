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
    //private List<> _weblinkButtons ;

    private void Awake()
    {
        _closeButton.onClick.AddListener(() => Close());
        _closeBackgroundButton.onClick.AddListener(() => Close());
    }

    public void Set(MusicTrack musicTrack, JukeboxManager.MusicTrackFavoriteType favoriteType)
    {
        if (musicTrack.JukeboxInfo.Artists.Count != 0)
        {
            ArtistReference artist = musicTrack.JukeboxInfo.Artists[0].Artist;

        }

        _buttonHandler.SetTrack(musicTrack, -1, favoriteType);

        gameObject.SetActive(true);
    }

    public void Close() { gameObject.SetActive(false); }

    private void ClearWeblinks()
    {

    }
}
