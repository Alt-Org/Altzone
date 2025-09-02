using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JukeboxMusicPlayerHandler : MonoBehaviour
{
    [Header("MusicPlayer")]
    [SerializeField] private GameObject _musicPlayerObject;
    [SerializeField] private Button _shuffleButton;
    [SerializeField] private Button _loopButton;
    [SerializeField] private Button _trackOptionsButton;
    [SerializeField] private TMP_Text _trackPlayTimeText;
    [SerializeField] private Slider _trackPlayTimeSlider;
    [Space]
    [SerializeField] private int _trackChunkSize = 8;

    private List<Chunk<JukeboxTrackQueueHandler>> _queueHandlerChunks = new List<Chunk<JukeboxTrackQueueHandler>>(); //Visible
    private int _queueHandlerChunkPointer = 0;
    private int _queueHandlerPoolPointer = -1;

    [SerializeField] private int _queueOptimizationThreshold = 4;
    private int _queueUseTimes = 0;

    [Header("QueueList")]
    [SerializeField] private Transform _queueContent;
    [SerializeField] private GameObject _queueTextPrefab;

    private string _queueHandlerLastUpdate = "never";

    void Awake()
    {
        CreateQueueHandlersChunk();
    }

    private void OnEnable()
    {
        JukeboxManager.Instance.OnReduceQueueHandlerChunkActiveCount += ReduceQueueHandlerChunkActiveCount;
        JukeboxManager.Instance.OnGetFreeJukeboxTrackQueueHandler += GetFreeJukeboxTrackQueueHandler;
        JukeboxManager.Instance.OnOptimizeVisualQueueChunks += OptimizeVisualQueueChunksCheck;
        JukeboxManager.Instance.OnSetVisibleElapsedTime += UpdateMusicElapsedTime;

        if (_queueHandlerLastUpdate != JukeboxManager.Instance.PlaybackLastUpdate) UpdateVisualQueue();
    }

    private void OnDisable()
    {
        JukeboxManager.Instance.OnReduceQueueHandlerChunkActiveCount -= ReduceQueueHandlerChunkActiveCount;
        JukeboxManager.Instance.OnGetFreeJukeboxTrackQueueHandler -= GetFreeJukeboxTrackQueueHandler;
        JukeboxManager.Instance.OnOptimizeVisualQueueChunks -= OptimizeVisualQueueChunksCheck;
        JukeboxManager.Instance.OnSetVisibleElapsedTime -= UpdateMusicElapsedTime;
    }

    private void UpdateVisualQueue()
    {
        if (_queueHandlerLastUpdate == "never")
            foreach (TrackQueueData data in JukeboxManager.Instance.TrackQueue)
            {
                data.QueueHandler = GetFreeJukeboxTrackQueueHandler();
                data.QueueHandler.SetTrack(data.Id, data.MusicTrack);
            }
        else
            foreach (PlaybackHistory playbackHistoryData in JukeboxManager.Instance.PlaybackHistory)
            {
                JukeboxTrackQueueHandler trackQueueHandler1 = playbackHistoryData.Target1.QueueHandler;

                switch (playbackHistoryData._playbackHistoryType)
                {
                    case JukeboxManager.PlaybackHistoryType.Delete:
                        {
                            if (trackQueueHandler1 != null)
                                _queueHandlerChunks[trackQueueHandler1.ChunkIndex].Pool[trackQueueHandler1.PoolIndex].Clear();
                            else if (playbackHistoryData.ChunkIndex1 != -1)
                                _queueHandlerChunks[playbackHistoryData.ChunkIndex1].Pool[playbackHistoryData.PoolIndex1].Clear();
                            else
                                Debug.LogError($"Could not process delete playback history type: {playbackHistoryData._playbackHistoryType.ToString()}");

                            break;
                        }
                    //case JukeboxManager.PlaybackHistoryType.Swap:
                    //    {
                    //        JukeboxTrackQueueHandler trackQueueHandler2 = null;

                    //        foreach (TrackQueueData trackQueueData in JukeboxManager.Instance.TrackQueue)
                    //            if (playbackHistoryData.Target2 != null && playbackHistoryData.Target2.Id == trackQueueHandler2.Id)
                    //            {

                    //                break;
                    //            }

                    //        break;
                    //    }
                    case JukeboxManager.PlaybackHistoryType.Add:
                        {
                            TrackQueueData queueData = GetEmptyQueueHandler();

                            JukeboxTrackQueueHandler queueHandler = GetFreeJukeboxTrackQueueHandler();
                            queueHandler.SetTrack(queueData.Id, queueData.MusicTrack);

                            queueData.QueueHandler = queueHandler;
                            break;
                        }
                    //case JukeboxManager.PlaybackHistoryType.MoveToLast:
                    //    {

                    //        break;
                    //    }
                }
            }

        JukeboxManager.Instance.ClearPlaybackHistory();
        _queueHandlerLastUpdate = JukeboxManager.Instance.PlaybackLastUpdate;
    }

    private TrackQueueData GetEmptyQueueHandler()
    {
        foreach (TrackQueueData data in JukeboxManager.Instance.TrackQueue)
            if (data.QueueHandler == null)
                return data;

        return null;
    }

    private void ReduceQueueHandlerChunkActiveCount(int index)
    {
        _queueHandlerChunks[index].AmountInUse--;
        _queueUseTimes++;
    }

    private void CreateQueueHandlersChunk()
    {
        List<JukeboxTrackQueueHandler> jukeboxButtonHandlers = new List<JukeboxTrackQueueHandler>(_trackChunkSize);

        for (int i = 0; i < _trackChunkSize; i++)
        {
            GameObject jukeboxTrackButton = Instantiate(_queueTextPrefab, _queueContent);
            JukeboxTrackQueueHandler buttonHandler = jukeboxTrackButton.GetComponent<JukeboxTrackQueueHandler>();
            buttonHandler.Setup(_queueHandlerChunks.Count, i);
            jukeboxButtonHandlers.Add(buttonHandler);
        }

        Chunk<JukeboxTrackQueueHandler> tracksChunk = new Chunk<JukeboxTrackQueueHandler>();
        tracksChunk.Pool = jukeboxButtonHandlers;
        tracksChunk.AmountInUse = 0;

        _queueHandlerChunks.Add(tracksChunk);
    }

    private JukeboxTrackQueueHandler GetFreeJukeboxTrackQueueHandler()
    {
        _queueHandlerPoolPointer++;

        if (_queueHandlerPoolPointer >= _trackChunkSize)
        {
            CreateQueueHandlersChunk();
            _queueHandlerChunkPointer++;
            _queueHandlerPoolPointer = 0;
        }

        _queueHandlerChunks[_queueHandlerChunkPointer].AmountInUse++;

        return _queueHandlerChunks[_queueHandlerChunkPointer].Pool[_queueHandlerPoolPointer];
    }

    private void UpdateMusicElapsedTime(float elapsedTime)
    {
        string minutes = (elapsedTime / 60f).ToString().Split('.')[0];
        string seconds = (elapsedTime % 60).ToString().Split('.')[0];

        _trackPlayTimeText.text = $"{minutes}:{((seconds.Length == 1) ? ("0" + seconds) : seconds)}";
        _trackPlayTimeSlider.value = elapsedTime / JukeboxManager.Instance.CurrentMusicTrack.Music.length;
    }

    #region Optimization
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
        List<int> freeSpaces = new List<int>();

        Debug.Log("Optimizing jukebox queue visual list...");
        _queueUseTimes = 0;

        for (int i = 0; i < _queueHandlerChunks.Count; i++)
        {
            int freeSpace = 0; //Total amount of free space in the chunk.
            int previousFreeSlot = 0; //Distance to previous free space in the current chunk.

            for (int j = 0; j < _queueHandlerChunks[i].Pool.Count; j++)
            {
                if (!_queueHandlerChunks[i].Pool[j].InUse())
                {
                    freeSpace++;
                    previousFreeSlot++;
                }
                else
                {
                    if (i != 0 && freeSpaces[i - 1] != 0) //Move to a different chunk.
                    {
                        Chunk<JukeboxTrackQueueHandler> destinationChunk = _queueHandlerChunks[i - 1];

                        for (int k = 0; k < _queueHandlerChunks[i - 1].Pool.Count; k++)
                            if (!_queueHandlerChunks[i - 1].Pool[k].InUse())
                                MoveVisualQueueItem(_queueHandlerChunks[i].Pool[j], _queueHandlerChunks[i - 1].Pool[k]);

                        freeSpaces[i - 1]--;
                        freeSpace++;
                        previousFreeSlot++;
                    }
                    else if (previousFreeSlot != 0) //Move to a different slot in the same pool.
                        MoveVisualQueueItem(_queueHandlerChunks[i].Pool[j], _queueHandlerChunks[i].Pool[j - previousFreeSlot]);
                }
            }

            freeSpaces.Add(freeSpace);
        }

        // Set chunk pointer and pool pointer to closest free space.
        RecalibrateQueuePointers();

        // Reassemble _trackQueue.
        JukeboxManager.Instance.ReassembleDataQueue(_queueHandlerChunks);

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
            if (_queueHandlerChunks[i].AmountInUse > 0)
            {
                int nextIndex = i + 1;

                if (_queueHandlerChunks[i].AmountInUse >= _trackChunkSize)
                    _queueHandlerChunkPointer = i;
                else if (nextIndex < _queueHandlerChunks.Count)
                    _queueHandlerChunkPointer = nextIndex;

                for (int j = _queueHandlerChunks[i].Pool.Count - 1; j >= 0; j--)
                    if (!_queueHandlerChunks[i].Pool[j].InUse())
                        _queueHandlerPoolPointer = j - 1;

                break;
            }
        }
    }

    private void MoveVisualQueueItem(JukeboxTrackQueueHandler target, JukeboxTrackQueueHandler destination)
    {
        if (target.CurrentTrack != null)
            destination.SetTrack(target.Id, target.CurrentTrack);
        else
            destination.Clear();

        _queueHandlerChunks[destination.ChunkIndex].AmountInUse++;

        target.Clear();
        _queueHandlerChunks[target.ChunkIndex].AmountInUse--;
    }
    #endregion
}
