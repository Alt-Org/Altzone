using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Audio;
using Altzone.Scripts.ReferenceSheets;
using TMPro;
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

    private List<Chunk<JukeboxTrackQueueHandler>> _queueHandlerChunks = new();
    public List<Chunk<JukeboxTrackQueueHandler>> QueueHandlerChunks {  get { return _queueHandlerChunks; } }

    private int _queueHandlerChunkPointer = 0;
    private int _queueHandlerPoolPointer = -1;

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
        JukeboxManager.Instance.OnReduceQueueHandlerChunkActiveCount += ReduceQueueHandlerChunkActiveCount;
        JukeboxManager.Instance.OnGetFreeJukeboxTrackQueueHandler += GetFreeJukeboxTrackQueueHandler;
        JukeboxManager.Instance.OnOptimizeVisualQueueChunks += OptimizeVisualQueueChunksCheck;
        JukeboxManager.Instance.OnForceOptimizeVisualQueueChunks += OptimizeVisualQueueChunks;
        JukeboxManager.Instance.OnSetVisibleElapsedTime += UpdateMusicElapsedTime;
        //JukeboxManager.Instance.OnPlaylistChange += PlaylistChange;
        //JukeboxManager.Instance.OnPlaylistPlay += PlaylistChange;
        JukeboxManager.Instance.OnQueueChange += UpdateVisualQueue;
        JukeboxManager.Instance.OnQueueToLast += MoveQueueHandlerToLast;
        JukeboxManager.Instance.OnGetTrackQueueHandler += GetTrackQueueHandler;
    }

    private void OnDisable()
    {
        JukeboxManager.Instance.OnReduceQueueHandlerChunkActiveCount -= ReduceQueueHandlerChunkActiveCount;
        JukeboxManager.Instance.OnGetFreeJukeboxTrackQueueHandler -= GetFreeJukeboxTrackQueueHandler;
        JukeboxManager.Instance.OnOptimizeVisualQueueChunks -= OptimizeVisualQueueChunksCheck;
        JukeboxManager.Instance.OnForceOptimizeVisualQueueChunks -= OptimizeVisualQueueChunks;
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

        for (int i = 0; i < JukeboxManager.Instance.TrackChunkSize; i++)
        {
            GameObject jukeboxTrackButton = Instantiate(_queueTextPrefab, _queueContent);
            JukeboxTrackQueueHandler buttonHandler = jukeboxTrackButton.GetComponent<JukeboxTrackQueueHandler>();
            buttonHandler.Setup(_queueHandlerChunks.Count, i);
            tracksChunk.Add(buttonHandler);

            ChunkPointer chunkPointer = new(_queueHandlerChunks.Count, i);
            chunkPointer.OnTrackQueueRemoved += TrackQueueRemoved;
            //_trackQueueHandlersMap.Add(chunkPointer);
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
        bool done = false;
        string tempMainId = "";
        MusicTrack tempMainTrack = null;

        if (wantedChunk >= _queueHandlerChunks.Count) return GetTrackQueueHandler(GetFreeJukeboxTrackQueueHandler());

        if (!(wantedChunk <= _queueHandlerChunkPointer && wantedPool < _queueHandlerPoolPointer)) _queueHandlerPoolPointer++;

        if (_queueHandlerPoolPointer >= JukeboxManager.Instance.TrackChunkSize)
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
                int tempLinearIndex = (chunkIndex * JukeboxManager.Instance.TrackChunkSize) + poolIndex;

                if (!string.IsNullOrEmpty(tempMainId))
                    _queueHandlerChunks[chunkIndex].Pool[poolIndex].SetTrack(tempMainId, tempMainTrack, tempLinearIndex);
                else
                    _queueHandlerChunks[chunkIndex].Pool[poolIndex].Clear();

                tempMainId = tempInnerId;
                tempMainTrack = tempInnerTrack;

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
            int linearIndex = JukeboxManager.Instance.TrackQueue.Count - 1;

            JukeboxTrackQueueHandler handler = GetTrackQueueHandler(GetFreeJukeboxTrackQueueHandler());

            if (handler != null) handler.SetTrack(tempMainId, tempMainTrack, linearIndex);
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
        chunkPointer.OnTrackQueueRemoved -= TrackQueueRemoved;
        //int index = _trackQueueHandlersMap.FindIndex(
        //    (data) => (data.ChunkIndex == chunkPointer.ChunkIndex && data.PoolIndex == chunkPointer.PoolIndex));

        _queueHandlerChunks[chunkPointer.ChunkIndex].Pool[chunkPointer.PoolIndex].Clear();
        //_trackQueueHandlersMap.RemoveAt(index);
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

        newHandler.SetTrack(oldHandler.Id, oldHandler.MusicTrack, linearIndex);
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

            switch (playbackHistoryData._playbackHistoryType)
            {
                case JukeboxManager.PlaybackHistoryType.Delete:
                    {
                        TrackQueueData queueData = playbackHistoryData.Target1.TrackQueueData;

                        if (trackQueueHandler1 != null)
                            _queueHandlerChunks[queueData.Pointer.ChunkIndex].Pool[trackQueueHandler1.PoolIndex].Clear();
                        else if (queueData.Pointer.ChunkIndex != -1)
                            _queueHandlerChunks[queueData.Pointer.ChunkIndex].Pool[queueData.Pointer.PoolIndex].Clear();
                        else
                            Debug.LogError($"Could not process playback history type: Delete");

                        break;
                    }
                case JukeboxManager.PlaybackHistoryType.Add:
                    {
                        TrackQueueData queueData = playbackHistoryData.Target1.TrackQueueData; //TODO: Change to ref getter if this is not a reference.

                        queueData.Pointer = GetFreeJukeboxTrackQueueHandler();

                        JukeboxTrackQueueHandler handler = GetTrackQueueHandler(queueData.Pointer);

                        handler.SetTrack(queueData.Id, queueData.MusicTrack, playbackHistoryData.Target1.LinearIndex); //TODO: Get "SelfChunkPointerIndex" from playbackhistory.

                        break;
                    }
                case JukeboxManager.PlaybackHistoryType.Insert:
                    {
                        TrackQueueData data = playbackHistoryData.Target1.TrackQueueData;
                        int chunkIndex = Mathf.FloorToInt(data.LinearIndex / JukeboxManager.Instance.TrackChunkSize);
                        int poolIndex = (data.LinearIndex % JukeboxManager.Instance.TrackChunkSize);

                        JukeboxTrackQueueHandler handler = GetInsertedJukeboxTrackQueueHandler(chunkIndex, poolIndex);

                        handler.SetTrack(data.Id, data.MusicTrack, data.LinearIndex);

                        break;
                    }
                case JukeboxManager.PlaybackHistoryType.Hide:
                    {
                        TrackQueueData data = playbackHistoryData.Target1.TrackQueueData;

                        _queueHandlerChunks[data.Pointer.ChunkIndex].Pool[data.Pointer.PoolIndex].SetVisibility(false);

                        break;
                    }
                case JukeboxManager.PlaybackHistoryType.Unhide:
                    {
                        TrackQueueData data = playbackHistoryData.Target1.TrackQueueData;

                        _queueHandlerChunks[data.Pointer.ChunkIndex].Pool[data.Pointer.PoolIndex].SetVisibility(true);

                        break;
                    }
            }
        }

        JukeboxManager.Instance.ClearPlaybackHistory();
        _queueHandlerLastUpdate = JukeboxManager.Instance.PlaybackLastUpdate;

        Debug.Log("Music Player: Visual queue list update done.");
    }

    private TrackQueueData GetEmptyQueueHandler() //TODO: Move getter to JukeboxManager!
    {
        foreach (TrackQueueData data in JukeboxManager.Instance.TrackQueue)
            if (data.Pointer == null)
                return data;

        return null;
    }
    #endregion

    private void UpdateMusicElapsedTime(float elapsedTime)
    {
        //string minutes = (elapsedTime / 60f).ToString().Split('.')[0];
        //string seconds = (elapsedTime % 60).ToString().Split('.')[0];

        //_trackPlayTimeText.text = $"{minutes}:{((seconds.Length == 1) ? ("0" + seconds) : seconds)}";
        _trackPlayTimeSlider.value = elapsedTime / JukeboxManager.Instance.CurrentTrackQueueData.MusicTrack.Music.length;
    }

    #region Optimizations
    /// <summary>
    /// Used when a certain amount of queue handlers have been removed from the active queue.
    /// </summary>
    private void OptimizeVisualQueueChunksCheck()
    {
        //if (_queueUseTimes >= _queueOptimizationThreshold) OptimizeVisualQueueChunks();
    }

    /// <summary>
    /// Optimizes the visible queue by moving any <c>JukeboxTrackQueueHandler</c>'s forward in the
    /// <c>_queueHandlerChunks</c> to keep the <c>_queueHandlerChunks</c> as short as possible.
    /// </summary>
    private void OptimizeVisualQueueChunks()
    {
        Debug.Log("Optimizing jukebox queue visual list...");

        List<int> freeSpaces = new List<int>();
        _queueUseTimes = 0;

        for (int chunkIndex = 0; chunkIndex < _queueHandlerChunks.Count; chunkIndex++) //Chunk
        {
            int freeSpace = 0; //Total amount of free space in the chunk.
            int previousFreeSlot = 0; //Distance to previous free space in the current chunk.

            if (_queueHandlerChunks[chunkIndex].AmountInUse == 0) continue;

            for (int poolIndex = 0; poolIndex < _queueHandlerChunks[chunkIndex].Pool.Count; poolIndex++) //Pool
            {
                if (!_queueHandlerChunks[chunkIndex].Pool[poolIndex].InUse())
                {
                    freeSpace++;
                    previousFreeSlot++;
                }
                else
                {
                    if (chunkIndex != 0 && freeSpaces[chunkIndex - 1] != 0) //Move to a different chunk.
                    {
                        Chunk<JukeboxTrackQueueHandler> destinationChunk = _queueHandlerChunks[chunkIndex - 1];

                        for (int k = 0; k < _queueHandlerChunks[chunkIndex - 1].Pool.Count; k++)
                            if (!_queueHandlerChunks[chunkIndex - 1].Pool[k].InUse())
                                SwapVisualQueueItems(_queueHandlerChunks[chunkIndex].Pool[poolIndex], _queueHandlerChunks[chunkIndex - 1].Pool[k]);

                        freeSpaces[chunkIndex - 1]--;
                        freeSpace++;
                        previousFreeSlot++;
                    }
                    else if (previousFreeSlot != 0) //Move to a different slot in the same pool.
                        SwapVisualQueueItems(_queueHandlerChunks[chunkIndex].Pool[poolIndex], _queueHandlerChunks[chunkIndex].Pool[poolIndex - previousFreeSlot]);
                }
            }

            freeSpaces.Add(freeSpace);
        }

        // Set chunk pointer and pool pointer to closest free space.
        RecalibrateQueuePointers();

        // Optimize TrackQueue in JukeboxManager.
        JukeboxManager.Instance.ReassembleDataQueue();

        _queueHandlerLastUpdate = JukeboxManager.Instance.PlaybackLastUpdate;

        Debug.Log("Jukebox queue visual list optimization done.");
    }

    /// <summary>
    /// Repositions <c>_queueChunkPointer</c> and <c>_queuePoolPointer</c> to the new free tail end on the <c>_queueHandlerChunks</c>.
    /// </summary>
    private void RecalibrateQueuePointers()
    {
        for (int i = _queueHandlerChunks.Count - 1; i >= 0; i--)
        {
            if (_queueHandlerChunks[i].AmountInUse < JukeboxManager.Instance.TrackChunkSize) // Find chunk that has not in use JukeboxTrackQueueHandler's.
            {
                _queueHandlerChunkPointer = i;

                for (int j = _queueHandlerChunks[i].Pool.Count - 1; j >= 0; j--)
                {
                    if (!_queueHandlerChunks[i].Pool[j].InUse())
                        _queueHandlerPoolPointer = j;
                    else
                        break;
                }

                break;
            }
        }

        Debug.LogError($"new chunk pointer: {_queueHandlerChunkPointer}, new pool pointer: {_queueHandlerPoolPointer}");
    }
    #endregion

    private void SwapVisualQueueItems(JukeboxTrackQueueHandler target1, JukeboxTrackQueueHandler target2)
    {
        if (!target1.InUse() && !target2.InUse()) return;

        string tempId = target2.Id;
        MusicTrack tempMusicTrack = target2.MusicTrack;
        int tempLinearIndex = target2.LinearIndex;

        if (target1.MusicTrack != null && target2.MusicTrack == null)
        {
            _queueHandlerChunks[target1.ChunkIndex].AmountInUse--;
            _queueHandlerChunks[target2.ChunkIndex].AmountInUse++;
        }
        else if (target2.MusicTrack != null && target1.MusicTrack == null)
        {
            _queueHandlerChunks[target2.ChunkIndex].AmountInUse--;
            _queueHandlerChunks[target1.ChunkIndex].AmountInUse++;
        }

        target2.SetTrack(target1.Id, target1.MusicTrack, target1.LinearIndex);
        target1.SetTrack(tempId, tempMusicTrack, tempLinearIndex);

        if (target1.MusicTrack == null)
            target1.Clear();
        else
            JukeboxManager.Instance.TrackQueue[target1.LinearIndex].Pointer.Set(target1.ChunkIndex, target1.PoolIndex);

        if (target2.MusicTrack == null)
            target2.Clear();
        else
            JukeboxManager.Instance.TrackQueue[target2.LinearIndex].Pointer.Set(target2.ChunkIndex, target2.PoolIndex);
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
