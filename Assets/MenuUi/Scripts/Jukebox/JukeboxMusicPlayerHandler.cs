using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Audio;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine;
using UnityEngine.UI;

public class JukeboxMusicPlayerHandler : MonoBehaviour
{
    [Header("MusicPlayer")]
    //[SerializeField] private Button _shuffleButton;
    //[SerializeField] private Button _loopButton;
    //[SerializeField] private Button _trackOptionsButton;
    //[SerializeField] private TMP_Text _trackPlayTimeText;
    [SerializeField] private Slider _trackPlayTimeSlider;
    [SerializeField] private SliderRubberband _sliderRubberband;
    [SerializeField] private float _sliderRubberbandAnimationTreshold = 0.01f;

    private bool _sliderRubberbandActive = false;
    private float _currentTrackLength = 0f;
    private Coroutine _sliderAnimationCoroutine;

    private List<Chunk<JukeboxTrackQueueHandler>> _queueHandlerChunks = new();
    public List<Chunk<JukeboxTrackQueueHandler>> QueueHandlerChunks {  get { return _queueHandlerChunks; } }

    private int _queueHandlerChunkPointer = 0;
    private int _queueHandlerPoolPointer = -1;

    private int _trackChunkSize = -1;
    //private int _trackChunkBufferSize = -1;

    [SerializeField] private int _queueOptimizationThreshold = 4;
    private int _queueUseTimes = 0;

    [Header("QueueList")]
    [SerializeField] private Transform _queueContent;
    [SerializeField] private GameObject _queueTextPrefab;

    [Header("List Windows")]
    [SerializeField] private GameObject _tracksListObject;
    [SerializeField] private GameObject _queueListObject;
    [Space]
    [SerializeField] private Toggle _tracksToggle;
    [SerializeField] private Toggle _queueToggle;

    private enum ListPageType
    {
        Tracks,
        Queue
    }

    private ListPageType _currentListPage;

    private string _queueHandlerLastUpdate = "never";

    void Awake()
    {
        _trackChunkSize = JukeboxManager.Instance.TrackChunkSize;
        //_trackChunkBufferSize = JukeboxManager.Instance.TrackChunkBufferSize;

        CreateQueueHandlersChunk();

        _tracksToggle.onValueChanged.AddListener((value) => { if (value) ChangeListPage(ListPageType.Tracks); });
        _queueToggle.onValueChanged.AddListener((value) => { if (value) ChangeListPage(ListPageType.Queue); });

    }

    private void Start()
    {
        StartCoroutine(Setup());
    }

    private void OnEnable()
    {
        //JukeboxManager.Instance.OnReduceQueueHandlerChunkActiveCount += ReduceQueueHandlerChunkActiveCount;
        JukeboxManager.Instance.OnGetFreeJukeboxTrackQueueHandler += GetFreeJukeboxTrackQueueHandler;
        //JukeboxManager.Instance.OnOptimizeVisualQueueChunks += OptimizeVisualQueueChunksCheck;
        //JukeboxManager.Instance.OnForceOptimizeVisualQueueChunks += OptimizeVisualQueueChunks;
        JukeboxManager.Instance.OnSetVisibleElapsedTime += UpdateMusicElapsedTime;
        //JukeboxManager.Instance.OnPlaylistChange += PlaylistChange;
        //JukeboxManager.Instance.OnPlaylistPlay += PlaylistChange;
        JukeboxManager.Instance.OnQueueChange += UpdateVisualQueue;
        JukeboxManager.Instance.OnQueueToLast += MoveQueueHandlerToLast;
        JukeboxManager.Instance.OnGetTrackQueueHandler += GetTrackQueueHandler;
    }

    private void OnDisable()
    {
        //JukeboxManager.Instance.OnReduceQueueHandlerChunkActiveCount -= ReduceQueueHandlerChunkActiveCount;
        JukeboxManager.Instance.OnGetFreeJukeboxTrackQueueHandler -= GetFreeJukeboxTrackQueueHandler;
        //JukeboxManager.Instance.OnOptimizeVisualQueueChunks -= OptimizeVisualQueueChunksCheck;
        //JukeboxManager.Instance.OnForceOptimizeVisualQueueChunks -= OptimizeVisualQueueChunks;
        JukeboxManager.Instance.OnSetVisibleElapsedTime -= UpdateMusicElapsedTime;
        //JukeboxManager.Instance.OnPlaylistChange -= PlaylistChange;
        //JukeboxManager.Instance.OnPlaylistPlay -= PlaylistChange;
        JukeboxManager.Instance.OnQueueChange -= UpdateVisualQueue;
        JukeboxManager.Instance.OnQueueToLast -= MoveQueueHandlerToLast;
        JukeboxManager.Instance.OnGetTrackQueueHandler -= GetTrackQueueHandler;
    }

    private IEnumerator Setup()
    {
        yield return new WaitUntil(() => JukeboxManager.Instance.PlaylistReady);

        if (_queueHandlerLastUpdate != JukeboxManager.Instance.PlaybackLastUpdate) UpdateVisualQueue();
    }

    #region Chunk
    private void CreateQueueHandlersChunk()
    {
        Chunk<JukeboxTrackQueueHandler> tracksChunk = new Chunk<JukeboxTrackQueueHandler>();

        for (int i = 0; i < _trackChunkSize; i++)
        {
            GameObject jukeboxTrackButton = Instantiate(_queueTextPrefab, _queueContent);
            JukeboxTrackQueueHandler buttonHandler = jukeboxTrackButton.GetComponent<JukeboxTrackQueueHandler>();
            buttonHandler.Setup(_queueHandlerChunks.Count, i);
            buttonHandler.OnDeleteEvent += TrackRemoved;
            tracksChunk.Add(buttonHandler);

            ChunkPointer chunkPointer = new(_queueHandlerChunks.Count, i);
            chunkPointer.OnTrackQueueRemoved += TrackQueueRemoved;
        }

        tracksChunk.AmountInUse = 0;
        _queueHandlerChunks.Add(tracksChunk);
    }

    /// <returns><c>ChunkPointer</c> to last free <c>JukeboxTrackQueueHandler</c> on the <c>_queueHandlerChunks</c> list.</returns>
    private ChunkPointer GetFreeJukeboxTrackQueueHandler()
    {
        _queueHandlerPoolPointer++;

        if (_queueHandlerPoolPointer >= JukeboxManager.Instance.TrackChunkSize)
        {
            CreateQueueHandlersChunk();
            _queueHandlerChunkPointer++;
            _queueHandlerPoolPointer = 0;
        }

        _queueHandlerChunks[_queueHandlerChunkPointer].AmountInUse++;

        return new(_queueHandlerChunkPointer, _queueHandlerPoolPointer);
    }

    /// <summary>
    /// <c>OptimizeVisualQueueChunks</c> method should be always be executed after setting data to returns
    /// <c>JukeboxTrackQueueHandler</c> due to insert desyncing the JukeboxManager's <c>_trackQueue</c> lists data!
    /// </summary>
    private JukeboxTrackQueueHandler GetInsertedJukeboxTrackQueueHandler(int wantedChunk, int wantedPool)
    {
        JukeboxManager jukeboxManager = JukeboxManager.Instance;
        bool done = false;
        string tempMainId = "";
        MusicTrack tempMainTrack = null;
        bool tempMainUserOwned = false;

        if (wantedChunk >= _queueHandlerChunks.Count) return GetTrackQueueHandler(GetFreeJukeboxTrackQueueHandler());

        if (!(wantedChunk <= _queueHandlerChunkPointer && wantedPool < _queueHandlerPoolPointer)) _queueHandlerPoolPointer++;

        if (_queueHandlerPoolPointer >= jukeboxManager.TrackChunkSize)
        {
            CreateQueueHandlersChunk();
            _queueHandlerChunkPointer++;
            _queueHandlerPoolPointer = 0;
        }

        //Push back all the InUse JukeboxTrackQueueHandlers to make the wanted space
        for (int chunkIndex = wantedChunk; wantedChunk < _queueHandlerChunks.Count; chunkIndex++)
        {
            for (int poolIndex = wantedPool; poolIndex < _queueHandlerChunks[chunkIndex].Pool.Count; poolIndex++)
            {
                string tempInnerId = _queueHandlerChunks[chunkIndex].Pool[poolIndex].Id;
                MusicTrack tempInnerTrack = _queueHandlerChunks[chunkIndex].Pool[poolIndex].MusicTrack;
                bool tempInnerUserOwned = _queueHandlerChunks[chunkIndex].Pool[poolIndex].UserOwned;
                int tempLinearIndex = (chunkIndex * jukeboxManager.TrackChunkSize) + poolIndex;

                if (!string.IsNullOrEmpty(tempMainId))
                {
                    _queueHandlerChunks[chunkIndex].Pool[poolIndex].SetTrack(tempMainId, tempMainTrack, tempLinearIndex,
                        tempMainUserOwned, jukeboxManager.GetTrackFavoriteType(tempMainTrack));
                    _queueHandlerChunks[chunkIndex].Pool[poolIndex].SetVisibility(true);
                }
                else
                    _queueHandlerChunks[chunkIndex].Pool[poolIndex].Clear();

                tempMainId = tempInnerId;
                tempMainTrack = tempInnerTrack;
                tempMainUserOwned = tempInnerUserOwned;

                if (string.IsNullOrEmpty(tempMainId))
                {
                    done = true;
                    break;
                }
            }

            if (done) break;
        }

        if (!string.IsNullOrEmpty(tempMainId))
        {
            int linearIndex = jukeboxManager.TrackQueue.Count - 1;

            JukeboxTrackQueueHandler handler = GetTrackQueueHandler(GetFreeJukeboxTrackQueueHandler());

            if (handler != null)
            {
                handler.SetTrack(tempMainId, tempMainTrack, linearIndex, tempMainUserOwned, jukeboxManager.GetTrackFavoriteType(tempMainTrack));
                handler.SetVisibility(true);
            }
        }

        return _queueHandlerChunks[wantedChunk].Pool[wantedPool];
    }

    private void ReduceQueueHandlerChunkActiveCount(int chunkIndex)
    {
        _queueHandlerChunks[chunkIndex].AmountInUse--;
        _queueUseTimes++;
    }

    private void TrackQueueRemoved(ChunkPointer chunkPointer)
    {
        _queueHandlerChunks[chunkPointer.ChunkIndex].Pool[chunkPointer.PoolIndex].Clear();
    }

    public JukeboxTrackQueueHandler GetTrackQueueHandler(ChunkPointer chunkPointer)
    {
        if (chunkPointer == null || chunkPointer.ChunkIndex == -1 || _queueHandlerChunks.Count <= chunkPointer.ChunkIndex ||
            _queueHandlerChunks[chunkPointer.ChunkIndex].Pool.Count <= chunkPointer.PoolIndex) return null;

        return _queueHandlerChunks[chunkPointer.ChunkIndex].Pool[chunkPointer.PoolIndex];
    }

    private void MoveQueueHandlerToLast(ChunkPointer chunkPointer, int linearIndex)
    {
        JukeboxTrackQueueHandler oldHandler = GetTrackQueueHandler(chunkPointer);
        JukeboxTrackQueueHandler newHandler = GetTrackQueueHandler(GetFreeJukeboxTrackQueueHandler());
        //Debug.LogError("move, chunk: " + chunkPointer.ChunkIndex + ", pool" + chunkPointer.PoolIndex);
        newHandler.SetTrack(oldHandler.Id, oldHandler.MusicTrack, linearIndex, oldHandler.UserOwned, JukeboxManager.Instance.GetTrackFavoriteType(oldHandler.MusicTrack));
        newHandler.SetVisibility(true);
        ReduceQueueHandlerChunkActiveCount(oldHandler.ChunkIndex);
        chunkPointer.Set(newHandler.ChunkIndex, newHandler.PoolIndex);
        oldHandler.Clear();
    }
    #endregion

    private void ChangeListPage(ListPageType type)
    {
        switch (_currentListPage)
        {
            case ListPageType.Tracks: _tracksListObject.SetActive(false); break;
            case ListPageType.Queue: _queueListObject.SetActive(false); break;
        }

        switch (type)
        {
            case ListPageType.Tracks: _tracksListObject.SetActive(true); break;
            case ListPageType.Queue: _queueListObject.SetActive(true); break;
        }

        _currentListPage = type;
    }

    //private void PlaylistChange()
    //{
    //    List<TrackQueueData> trackQueue = JukeboxManager.Instance.TrackQueue;
    //    int newChunksStart = -1;
    //    Debug.LogError("asdasd");
    //    if (_queueHandlerChunks.Count < trackQueue.Count)
    //    {
    //        int createAmount = (int)System.Math.Ceiling((float)((trackQueue.Count - (_queueHandlerChunks.Count * _trackChunkSize)) / _trackChunkSize));

    //        newChunksStart = _queueHandlerChunks.Count;

    //        for (int i = 0; i < createAmount; i++) CreateQueueHandlersChunk();
    //    }

    //    for (int i = 0; i < _queueHandlerChunks.Count; i++)
    //    {
    //        if (newChunksStart != -1 && newChunksStart <= i) break;

    //        if (trackQueue.Count <= (i * _trackChunkSize)) // Clear all the extra JukeboxTrackQueueHandler's.
    //            foreach (JukeboxTrackQueueHandler handler in _queueHandlerChunks[i].Pool)
    //                handler.Clear();

    //        for (int j = 0; j < _queueHandlerChunks[i].Pool.Count; j++)
    //        {
    //            int playlistIndex = (i * _trackChunkSize) + j;

    //            if (playlistIndex < trackQueue.Count)
    //            {
    //                TrackQueueData queueData = trackQueue[playlistIndex];
    //                _queueHandlerChunks[i].Pool[j].SetTrack(queueData.Id, queueData.MusicTrack);
    //            }
    //            else
    //                _queueHandlerChunks[i].Pool[j].Clear();
    //        }
    //    }
    //}

    #region Playback History
    private void UpdateVisualQueue()
    {
        Debug.Log("Music Player: Updating visual queue list...");

        foreach (PlaybackHistory playbackHistoryData in JukeboxManager.Instance.PlaybackHistory)
        {
            JukeboxTrackQueueHandler trackQueueHandler1 = GetTrackQueueHandler(playbackHistoryData.Target1.TrackQueueData.Pointer);
            TrackQueueData queueData = playbackHistoryData.Target1.TrackQueueData;

            switch (playbackHistoryData._playbackHistoryType)
            {
                case JukeboxManager.PlaybackHistoryType.Delete:
                    {
                        if (playbackHistoryData.Target1.ChunkIndex == -1)
                        {
                            Debug.LogError("ChunkPointer is null!");
                            break;
                        }
                        //Debug.LogError("delete history");
                        _queueHandlerChunks[playbackHistoryData.Target1.ChunkIndex].AmountInUse--;

                        //if (trackQueueHandler1 != null)
                        //    _queueHandlerChunks[queueData.Pointer.ChunkIndex].Pool[trackQueueHandler1.PoolIndex].Clear();
                        //else if (queueData.Pointer.ChunkIndex != -1)
                            _queueHandlerChunks[playbackHistoryData.Target1.ChunkIndex].Pool[playbackHistoryData.Target1.PoolIndex].Clear();

                        break;
                    }
                case JukeboxManager.PlaybackHistoryType.Add:
                    {
                        queueData.Pointer = GetFreeJukeboxTrackQueueHandler();
                        //Debug.LogError("add history");
                        JukeboxTrackQueueHandler handler = GetTrackQueueHandler(queueData.Pointer);

                        handler.SetTrack(queueData.ServerSongData.id, queueData.MusicTrack, playbackHistoryData.Target1.LinearIndex, queueData.UserOwned, JukeboxManager.Instance.GetTrackFavoriteType(queueData.MusicTrack));
                        handler.SetVisibility(true);

                        break;
                    }
                case JukeboxManager.PlaybackHistoryType.Insert:
                    {
                        int chunkIndex = Mathf.FloorToInt(queueData.LinearIndex / JukeboxManager.Instance.TrackChunkSize);
                        int poolIndex = (queueData.LinearIndex % JukeboxManager.Instance.TrackChunkSize);
                        //Debug.LogError("insert history");
                        JukeboxTrackQueueHandler handler = GetInsertedJukeboxTrackQueueHandler(chunkIndex, poolIndex);

                        queueData.Pointer = new(chunkIndex, poolIndex);

                        handler.SetTrack(queueData.ServerSongData.id, queueData.MusicTrack, queueData.LinearIndex, queueData.UserOwned, JukeboxManager.Instance.GetTrackFavoriteType(queueData.MusicTrack));
                        handler.SetVisibility(true);

                        break;
                    }
                case JukeboxManager.PlaybackHistoryType.Hide:
                    {
                        //Debug.LogError(queueData.Pointer);
                        _queueHandlerChunks[queueData.Pointer.ChunkIndex].Pool[queueData.Pointer.PoolIndex].SetVisibility(false);

                        break;
                    }
                case JukeboxManager.PlaybackHistoryType.Unhide:
                    {
                        _queueHandlerChunks[queueData.Pointer.ChunkIndex].Pool[queueData.Pointer.PoolIndex].SetVisibility(true);

                        break;
                    }
                case JukeboxManager.PlaybackHistoryType.MoveToLast:
                    {
                        //Debug.LogError("Move to last history: " + queueData.Pointer.ChunkIndex + ", " + queueData.Pointer.PoolIndex);
                        MoveQueueHandlerToLast(queueData.Pointer, queueData.LinearIndex);

                        break;
                    }
            }
        }

        JukeboxManager.Instance.ClearPlaybackHistory();
        _queueHandlerLastUpdate = JukeboxManager.Instance.PlaybackLastUpdate;

        Debug.Log("Music Player: Visual queue list update done.");

        OptimizeVisualQueueChunksCheck();
    }

    private void TrackRemoved(int chunkIndex, int poolIndex, int linearIndex)
    {
        JukeboxManager.Instance.DeleteFromQueue(linearIndex, true);
        _queueHandlerChunks[chunkIndex].Pool[poolIndex].Clear();
        ReduceQueueHandlerChunkActiveCount(chunkIndex);
        OptimizeVisualQueueChunksCheck();
    }

    //private TrackQueueData GetEmptyQueueHandler() //TODO: Move getter to JukeboxManager!
    //{
    //    foreach (TrackQueueData data in JukeboxManager.Instance.TrackQueue)
    //        if (data.Pointer == null)
    //            return data;

    //    return null;
    //}
    #endregion

    private void UpdateMusicElapsedTime(float musicTrackLength, float elapsedTime)
    {
        JukeboxManager manager = JukeboxManager.Instance;
        //string minutes = (elapsedTime / 60f).ToString().Split('.')[0];
        //string seconds = (elapsedTime % 60).ToString().Split('.')[0];

        //_trackPlayTimeText.text = $"{minutes}:{((seconds.Length == 1) ? ("0" + seconds) : seconds)}";

        if (_sliderRubberbandActive && _currentTrackLength != musicTrackLength)
        {
            Debug.LogError(_currentTrackLength + " | " + musicTrackLength);
            if (_sliderAnimationCoroutine != null)
            {
                StopCoroutine(_sliderAnimationCoroutine);
                _sliderAnimationCoroutine = null;
            }
            _sliderRubberbandActive = false;
        }

        if (!_sliderRubberbandActive && Mathf.Abs(_trackPlayTimeSlider.value - (elapsedTime / musicTrackLength)) > _sliderRubberbandAnimationTreshold)
        {
            Debug.LogError(_sliderRubberbandActive + "" + Mathf.Abs(_trackPlayTimeSlider.value - (elapsedTime / musicTrackLength)) +" > " + _sliderRubberbandAnimationTreshold);
            _currentTrackLength = musicTrackLength;
            _sliderRubberbandActive = true;
            _sliderAnimationCoroutine = StartCoroutine(_sliderRubberband.StartRubberband(elapsedTime, musicTrackLength, (data) => _sliderRubberbandActive = !data));
        }

        if (!_sliderRubberbandActive) _trackPlayTimeSlider.value = elapsedTime / musicTrackLength;
    }

    #region Optimizations
    /// <summary>
    /// Used when a certain amount of queue handlers have been removed from the active queue.
    /// </summary>
    private void OptimizeVisualQueueChunksCheck()
    {
        if (_queueUseTimes >= _queueOptimizationThreshold) OptimizeVisualQueueChunks();
    }

    /// <summary>
    /// Optimizes the visible queue by moving any <c>JukeboxTrackQueueHandler</c>'s forward in the
    /// <c>_queueHandlerChunks</c> to keep the <c>_queueHandlerChunks</c> as short as possible.
    /// </summary>
    private void OptimizeVisualQueueChunks()
    {
        Debug.Log("Music Player: Optimizing jukebox queue visual list...");

        List<int> freeSpaces = new(); //Free space in previous chunks.
        _queueUseTimes = 0;

        for (int chunkIndex = 0; chunkIndex < _queueHandlerChunks.Count; chunkIndex++) //Chunk
        {
            int freeSpace = 0; //Total amount of free space in the chunk.
            int previousFreeSlot = 0; //Distance to previous free space in the current chunk.

            if (_queueHandlerChunks[chunkIndex].AmountInUse == 0)
                freeSpace = _trackChunkSize;
            else
                for (int poolIndex = 0; poolIndex < _queueHandlerChunks[chunkIndex].Pool.Count; poolIndex++) //Pool
                {
                    JukeboxTrackQueueHandler handler = _queueHandlerChunks[chunkIndex].Pool[poolIndex]; //TODO: Null for some reason!?

                    if (!handler.InUse())
                    {
                        freeSpace++;
                        previousFreeSlot++;
                    }
                    else
                    {
                        if (chunkIndex != 0 && freeSpaces[chunkIndex - 1] != 0) //Move to a different chunk.
                        {
                            Chunk<JukeboxTrackQueueHandler> destinationChunk = _queueHandlerChunks[chunkIndex - 1];

                            for (int destinationPoolIndex = 0; destinationPoolIndex < _queueHandlerChunks[chunkIndex - 1].Pool.Count; destinationPoolIndex++)
                                if (!_queueHandlerChunks[chunkIndex - 1].Pool[destinationPoolIndex].InUse())
                                {
                                    MoveToDestinationVisualQueueItem(handler, _queueHandlerChunks[chunkIndex - 1].Pool[destinationPoolIndex]);
                                    break;
                                }

                            freeSpaces[chunkIndex - 1]--;
                            freeSpace++;
                            previousFreeSlot++;
                        }
                        else if (previousFreeSlot != 0) //Move to a different slot in the same pool.
                            MoveToDestinationVisualQueueItem(handler, _queueHandlerChunks[chunkIndex].Pool[poolIndex - previousFreeSlot]);
                    }
                }

            freeSpaces.Add(freeSpace);
        }

        // Set chunk pointer and pool pointer to closest free space.
        RecalibrateQueuePointers();

        // Optimize TrackQueue in JukeboxManager.
        JukeboxManager.Instance.OptimizeTrackQueue();

        _queueHandlerLastUpdate = JukeboxManager.Instance.PlaybackLastUpdate;

        //Debug.LogError("After:-------------------------------------------------------------------------------");
        //for (int i = 0; i < _queueHandlerChunks.Count; i++)
        //    for (int j = 0; j < _queueHandlerChunks[i].Pool.Count; j++)
        //        Debug.LogError("chunk: " + i + ", pool: " + j + ", Id: " + _queueHandlerChunks[i].Pool[j].Id + ", linearIndex: " + _queueHandlerChunks[i].Pool[j].LinearIndex);

        Debug.Log("Music Player: Jukebox queue visual list optimization done.");
    }

    /// <summary>
    /// Repositions <c>_queueChunkPointer</c> and <c>_queuePoolPointer</c> to the new free tail end on the <c>_queueHandlerChunks</c>.
    /// </summary>
    private void RecalibrateQueuePointers()
    {
        for (int i = 0; i < _queueHandlerChunks.Count; i++)
        {
            //Debug.LogError($"in use: {_queueHandlerChunks[i].AmountInUse}, max size: {JukeboxManager.Instance.TrackChunkSize}");
            if (_queueHandlerChunks[i].AmountInUse < JukeboxManager.Instance.TrackChunkSize) // Find chunk that has not in use JukeboxTrackQueueHandler's.
            {
                _queueHandlerChunkPointer = i;

                for (int j = 0; j < _queueHandlerChunks[i].Pool.Count; j++)
                {
                    //Debug.LogError(_queueHandlerChunks[i].Pool[j].Id);
                    if (!_queueHandlerChunks[i].Pool[j].InUse())
                    {
                        _queueHandlerPoolPointer = j;
                        break;
                    }
                }

                break;
            }
        }

        //Debug.LogError($"new chunk pointer: {_queueHandlerChunkPointer}, new pool pointer: {_queueHandlerPoolPointer}");
    }
    #endregion

    private void MoveToDestinationVisualQueueItem(JukeboxTrackQueueHandler target, JukeboxTrackQueueHandler destination)
    {
        string tempId = target.Id;
        MusicTrack tempMusicTrack = target.MusicTrack;
        int tempLinearIndex = target.LinearIndex;
        bool tempUserOwned = target.UserOwned;
        bool tempVisibility = target.GetVisibility();

        destination.SetVisibility(tempVisibility);
        destination.SetTrack(tempId, tempMusicTrack, tempLinearIndex, tempUserOwned, JukeboxManager.Instance.GetTrackFavoriteType(tempMusicTrack));

        JukeboxManager.Instance.TrackQueue[destination.LinearIndex].Pointer.Set(destination.ChunkIndex, destination.PoolIndex);
        
        _queueHandlerChunks[target.ChunkIndex].AmountInUse--;
        _queueHandlerChunks[destination.ChunkIndex].AmountInUse++;

        target.Clear();
    }

    //private void MoveVisualQueueItemToLast(JukeboxTrackQueueHandler target)
    //{
    //    JukeboxTrackQueueHandler handler = GetFreeJukeboxTrackQueueHandler();

    //    if (target.CurrentTrack != null)
    //        _queueHandlerChunks[handler.ChunkIndex].Pool[handler.PoolIndex].SetTrack(target.Id, target.CurrentTrack);

    //    _queueHandlerChunks[handler.ChunkIndex].AmountInUse++;

    //    _queueHandlerChunks[target.ChunkIndex].AmountInUse--;
    //    _queueHandlerChunks[target.ChunkIndex].Pool[target.PoolIndex].Clear();
    //}
}
