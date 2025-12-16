using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Audio;
using Altzone.Scripts.ReferenceSheets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JukeboxPlaylistNavigationHandler : MonoBehaviour
{
    [Header("Navigation")]
    [SerializeField] private TMP_InputField _searchField;
    //[SerializeField] private Button _openTrackFiltersPopupButton;
    [Space]
    [SerializeField] private Transform _tracksListContent;
    [SerializeField] private GameObject _jukeboxButtonPrefab;
    //[Space]
    //[SerializeField] private Button _playPlaylistNormal;
    //[SerializeField] private Button _playPlaylistShuffle;
    [Space]
    [SerializeField] private int _trackChunkSize = 8;

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


    private List<Chunk<JukeboxTrackButtonHandler>> _buttonHandlersOriginalOrder;
    private List<Chunk<JukeboxTrackButtonHandler>> _buttonHandlerChunks = new List<Chunk<JukeboxTrackButtonHandler>>(); //Visible
    private int _buttonHandlerChunkPointer = 0;
    private int _buttonHandlerPoolPointer = -1;

    //private string _currentPlaylistName = "";

    private List<Chunk<bool>> _hiddenTrackHandlers = new List<Chunk<bool>>();
    private int _previousSearchLength = 0;

    public delegate void InfoPressed(MusicTrack musicTrack, JukeboxManager.MusicTrackFavoriteType likeType);
    public event InfoPressed OnInfoPressed;

    private void Start()
    {
        CreateButtonHandlersChunk();
        FillSelectionButtonList();

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

    #region Chunk
    private void CreateButtonHandlersChunk()
    {
        Chunk<JukeboxTrackButtonHandler> tracksChunk = new Chunk<JukeboxTrackButtonHandler>();

        for (int i = 0; i < _trackChunkSize; i++)
        {
            GameObject jukeboxTrackButton = Instantiate(_jukeboxButtonPrefab, _tracksListContent);
            JukeboxTrackButtonHandler buttonHandler = jukeboxTrackButton.GetComponent<JukeboxTrackButtonHandler>();
            //buttonHandler.OnTrackPressed += JukeboxManager.Instance.PlayPlaylist;
            buttonHandler.OnTrackPressed += JukeboxManager.Instance.QueueTrack;
            buttonHandler.OnPreviewPressed += JukeboxManager.Instance.PlayPreview;
            buttonHandler.OnInfoPressed += OpenMusicTrackInfoPopup;
            buttonHandler.Clear();
            tracksChunk.Add(buttonHandler);
        }

        tracksChunk.AmountInUse = 0;
        _buttonHandlerChunks.Add(tracksChunk);
    }

    private JukeboxTrackButtonHandler GetFreeJukeboxTrackButtonHandler()
    {
        _buttonHandlerPoolPointer++;

        if (_buttonHandlerPoolPointer >= _trackChunkSize)
        {
            CreateButtonHandlersChunk();
            _buttonHandlerChunkPointer++;
            _buttonHandlerPoolPointer = 0;
        }

        _buttonHandlerChunks[_buttonHandlerChunkPointer].AmountInUse++;

        return _buttonHandlerChunks[_buttonHandlerChunkPointer].Pool[_buttonHandlerPoolPointer];
    }
    #endregion

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
        /*if (string.IsNullOrEmpty(_currentPlaylistName))*/ ClearButtonHandlers();
        List<MusicTrack> musicTracks = AudioManager.Instance.GetMusicList("Jukebox"); //TODO: Replace with what tracks the clan actually owns when possible.

        for (int i = 0; i < musicTracks.Count; i++) GetFreeJukeboxTrackButtonHandler().SetTrack(musicTracks[i], i, JukeboxManager.Instance.GetTrackFavoriteType(musicTracks[i]));

        for (int i = 0; i < _buttonHandlerChunks.Count; i++)
        {
            Chunk<bool> chunk = new();

            for (int j = 0; j < _trackChunkSize; j++) chunk.Pool.Add(false);

            _hiddenTrackHandlers.Add(chunk);
        }
    }

    private void ClearButtonHandlers()
    {
        foreach (var chunk in _buttonHandlerChunks)
        {
            chunk.AmountInUse = 0;

            foreach (var handler in chunk.Pool)
                handler.Clear();
        }

        foreach (var chunk in _hiddenTrackHandlers)
        {
            chunk.AmountInUse = 0;

            for (int i = 0; i < chunk.Pool.Count; i++)
                chunk.Pool[i] = false;
        }

        _buttonHandlerChunkPointer = 0;
        _buttonHandlerPoolPointer = 0;
    }

    private void OpenMusicTrackInfoPopup(MusicTrack musicTrack, JukeboxManager.MusicTrackFavoriteType likeType)
    {
        OnInfoPressed.Invoke(musicTrack, likeType);
    }

    #region Filtering
    private void SearchFieldChange(string value)
    {
        if (string.IsNullOrEmpty(value)) //Set all track button handlers visible that have a music track.
            foreach (Chunk<JukeboxTrackButtonHandler> chunk in _buttonHandlerChunks)
                foreach (JukeboxTrackButtonHandler handler in chunk.Pool)
                    if (handler.MusicTrack != null)
                        handler.SetVisibility(true);

        int textDirection = (_previousSearchLength < value.Length) ? 1 : -1; //1: Forward, -1: Backward.

        _previousSearchLength = value.Length;

        for (int i = 0; i < _buttonHandlerChunks.Count; i++)
        {
            Chunk<JukeboxTrackButtonHandler> chunk = _buttonHandlerChunks[i];
            Chunk<bool> hiddenTrackChunkData = _hiddenTrackHandlers[i];

            if ((textDirection == 1 && hiddenTrackChunkData.AmountInUse == _trackChunkSize) ||
                (textDirection == -1 && hiddenTrackChunkData.AmountInUse == 0)) continue;

            for (int j = 0; j < _trackChunkSize; j++)
            {
                JukeboxTrackButtonHandler handler = chunk.Pool[j];

                if ((handler.MusicTrack == null || handler.MusicTrack.Music == null) ||
                    ((textDirection == 1 && hiddenTrackChunkData.Pool[j]) ||
                    (textDirection == -1 && !hiddenTrackChunkData.Pool[j]))) continue;

                bool visible = handler.MusicTrack.Name.Contains(value, System.StringComparison.CurrentCultureIgnoreCase);

                if ((textDirection == 1 && visible) || (textDirection == -1 && !visible)) continue;

                if (hiddenTrackChunkData.Pool[j] != !visible) hiddenTrackChunkData.AmountInUse += textDirection;

                hiddenTrackChunkData.Pool[j] = !visible;
                handler.SetVisibility(visible);
            }
        }
    }

    #endregion
}
