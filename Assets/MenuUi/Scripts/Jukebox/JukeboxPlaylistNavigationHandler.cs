using System.Collections.Generic;
using Altzone.Scripts.Audio;
using Altzone.Scripts.ReferenceSheets;
using TMPro;
using UnityEngine;

public class JukeboxPlaylistNavigationHandler : MonoBehaviour
{
    [Header("Navigation")]
    [SerializeField] private TMP_InputField _searchField;
    //[SerializeField] private Button _openTrackFiltersPopupButton;
    [Space]
    //[SerializeField] private Transform _tracksListContent;
    //[SerializeField] private GameObject _jukeboxButtonPrefab;
    //[Space]
    //[SerializeField] private Button _playPlaylistNormal;
    //[SerializeField] private Button _playPlaylistShuffle;
    //[Space]
    //[SerializeField] private int _trackChunkSize = 8;
    [SerializeField] private SmartVerticalObjectList _smartList;

    //[Header("Filters Popup")]
    //[SerializeField] private GameObject _filtersPopupObject;
    //[Space]
    //[SerializeField] private Button _closeFiltersPopupButton;
    //[SerializeField] private Button _resetFiltersButton;
    //[Space]
    //[SerializeField] private Toggle _favoriteFilterToggle;
    //[SerializeField] private TMP_Dropdown _nameOrderFilterDropdown;
    //[SerializeField] private TMP_Dropdown _genreFilterDropdown;
    //[SerializeField] private TMP_Dropdown _albumFilterDropdown;
    //[SerializeField] private TMP_Dropdown _artistFilterDropdown;


    //private List<Chunk<JukeboxTrackButtonHandler>> _buttonHandlersOriginalOrder;
    //private List<Chunk<JukeboxTrackButtonHandler>> _buttonHandlerChunks = new List<Chunk<JukeboxTrackButtonHandler>>(); //Visible
    //private int _buttonHandlerChunkPointer = 0;
    //private int _buttonHandlerPoolPointer = -1;

    //private string _currentPlaylistName = "";

    private List<Chunk<bool>> _hiddenTrackHandlers = new List<Chunk<bool>>();
    private int _previousSearchLength = 0;

    private List<PersonalizedMusicTrack> _personalizedMusicTracks = new();

    // public delegate void InfoPressed(MusicTrack musicTrack, JukeboxManager.MusicTrackFavoriteType likeType);
    // public event InfoPressed OnInfoPressed;

    private void Start()
    {
        FillSelectionButtonList();
        _smartList.OnNewDataRequested += UpdateButtonHandlerData;

        #region Filters
        _searchField.onValueChanged.AddListener((value) => SearchFieldChange(value));

        //_openTrackFiltersPopupButton.onClick.AddListener(() => _filtersPopupObject.SetActive(true));
        //_closeFiltersPopupButton.onClick.AddListener(() => _filtersPopupObject.SetActive(false));
        #endregion
    }

    private void OnEnable()
    {
        //JukeboxManager.Instance.OnPlaylistChange += SetPlaylist;

        //if (string.IsNullOrEmpty(_currentPlaylistName)) StartCoroutine(GetCurrentPlaylist());
    }

    private void OnDisable()
    {
        //JukeboxManager.Instance.OnPlaylistChange -= SetPlaylist;

    }

    #region Playlist

    //private void SetPlaylist(Playlist playlist)
    //{
    //    _currentPlaylistName = playlist.Name;
    //    FillContentList(playlist.MusicTracks);
    //}

    //private IEnumerator GetCurrentPlaylist()
    //{
    //    JukeboxManager manager = JukeboxManager.Instance;

    //    yield return new WaitUntil(() => manager.CurrentPlaylist != null);

    //    //_currentPlaylistName = manager.CurrentPlaylist.Name;

    //}
    #endregion

    private void FillSelectionButtonList()
    {
        List<MusicTrack> musicTracks = AudioManager.Instance.GetMusicList("Jukebox"); //TODO: Replace with what tracks the clan actually owns when possible.

        foreach (MusicTrack musicTrack in musicTracks)
        {
            PersonalizedMusicTrack pMusicTrack = new (musicTrack, JukeboxManager.Instance.GetTrackFavoriteType(musicTrack));

            _personalizedMusicTracks.Add(pMusicTrack);
        }

        _smartList.Setup<PersonalizedMusicTrack>(_personalizedMusicTracks);
    }

    private void UpdateButtonHandlerData(int targetIndex)
    {
        _smartList.UpdateContent<PersonalizedMusicTrack>(targetIndex, _personalizedMusicTracks[targetIndex]);
    }

    #region Filtering
    private void SearchFieldChange(string value) //TODO: Out Of Order! Make compatible with Smart List.
    {
        return;

        // if (string.IsNullOrEmpty(value)) //Set all track button handlers visible that have a music track.
        //     foreach (Chunk<JukeboxTrackButtonHandler> chunk in _buttonHandlerChunks)
        //         foreach (JukeboxTrackButtonHandler handler in chunk.Pool)
        //             if (handler.MusicTrack != null)
        //                 handler.SetVisibility(true);
        //
        // int textDirection = (_previousSearchLength < value.Length) ? 1 : -1; //1: Forward, -1: Backward.
        //
        // _previousSearchLength = value.Length;

        // for (int i = 0; i < _buttonHandlerChunks.Count; i++)
        // {
        //     Chunk<JukeboxTrackButtonHandler> chunk = _buttonHandlerChunks[i];
        //     Chunk<bool> hiddenTrackChunkData = _hiddenTrackHandlers[i];
        //
        //     if ((textDirection == 1 && hiddenTrackChunkData.AmountInUse == _trackChunkSize) ||
        //         (textDirection == -1 && hiddenTrackChunkData.AmountInUse == 0)) continue;
        //
        //     for (int j = 0; j < _trackChunkSize; j++)
        //     {
        //         JukeboxTrackButtonHandler handler = chunk.Pool[j];
        //
        //         if ((handler.MusicTrack == null || handler.MusicTrack.Music == null) ||
        //             ((textDirection == 1 && hiddenTrackChunkData.Pool[j]) ||
        //             (textDirection == -1 && !hiddenTrackChunkData.Pool[j]))) continue;
        //
        //         bool visible = handler.MusicTrack.Name.Contains(value, System.StringComparison.CurrentCultureIgnoreCase);
        //
        //         if ((textDirection == 1 && visible) || (textDirection == -1 && !visible)) continue;
        //
        //         if (hiddenTrackChunkData.Pool[j] != !visible) hiddenTrackChunkData.AmountInUse += textDirection;
        //
        //         hiddenTrackChunkData.Pool[j] = !visible;
        //         handler.SetVisibility(visible);
        //     }
        // }
    }
    #endregion
}
