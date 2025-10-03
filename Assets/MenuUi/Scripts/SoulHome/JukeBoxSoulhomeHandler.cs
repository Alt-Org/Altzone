using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Audio;
using Altzone.Scripts.ReferenceSheets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JukeBoxSoulhomeHandler : MonoBehaviour
{
    [Header("Disk")]
    [SerializeField] private Image _diskImage;
    [SerializeField] private Transform _diskTransform;
    [SerializeField] private float _diskRotationSpeed = 100f;
    [SerializeField] private Sprite _emptyDisk;

    [Header("Other")]
    [SerializeField] private GameObject _jukeboxObject;
    [SerializeField] private Button _backButton;
    [SerializeField] private TextMeshProUGUI _songName;

    [Header("Songlist")]
    [SerializeField] private Transform _trackListContent;
    [SerializeField] private GameObject _jukeboxButtonPrefab;

    [Header("Queuelist")]
    [SerializeField] private Transform _queueContent;
    [SerializeField] private GameObject _queueTextPrefab;

    private bool _isMainMenuMode = false;
    private Coroutine _diskSpinCoroutine;

    public bool JukeBoxOpen { get => _jukeboxObject.activeSelf; }

    [SerializeField] private int _trackChunkSize = 8;
    private List<Chunk<JukeboxTrackButtonHandler>> _buttonHandlerChunks = new List<Chunk<JukeboxTrackButtonHandler>>(); //Visible
    private int _buttonHandlerChunkPointer = 0;
    private int _buttonHandlerPoolPointer = -1;

    private List<Chunk<JukeboxTrackQueueHandler>> _queueHandlerChunks = new List<Chunk<JukeboxTrackQueueHandler>>(); //Visible
    private int _queueHandlerChunkPointer = 0;
    private int _queueHandlerPoolPointer = -1;

    
    [SerializeField] private int _queueOptimizationThreshold = 4;
    private int _queueUseTimes = 0;

    private const string NoSongName = "Ei valittua biisiä";

    public delegate void ChangeJukeBoxSong(MusicTrack track);
    public static event ChangeJukeBoxSong OnChangeJukeBoxSong;

    void Start()
    {
        _backButton.onClick.AddListener(() => ToggleJukeboxScreen(false));
        CreateButtonHandlersChunk();
        CreateQueueHandlersChunk();

        List<MusicTrack> musicTracks = AudioManager.Instance.GetMusicList("Jukebox");

        foreach (MusicTrack track in musicTracks) GetFreeJukeboxTrackButtonHandler().SetTrack(track);

        _songName.text = NoSongName;
    }

    private void OnEnable()
    {
        JukeboxManager.Instance.OnGetFreeJukeboxTrackQueueHandler += GetFreeJukeboxTrackQueueHandler;
        JukeboxManager.Instance.OnReduceQueueHandlerChunkActiveCount += ReduceQueueHandlerChunkActiveCount;
        JukeboxManager.Instance.OnOptimizeVisualQueueChunks += OptimizeVisualQueueChunksCheck;
        JukeboxManager.Instance.OnSetSongInfo += SetSongInfo;
        JukeboxManager.Instance.OnStopJukeboxVisual += StopJukeboxVisuals;

        if (JukeboxManager.Instance.CurrentMusicTrack != null)
            SetSongInfo(JukeboxManager.Instance.CurrentMusicTrack);
    }

    private void OnDisable()
    {
        JukeboxManager.Instance.OnGetFreeJukeboxTrackQueueHandler -= GetFreeJukeboxTrackQueueHandler;
        JukeboxManager.Instance.OnReduceQueueHandlerChunkActiveCount -= ReduceQueueHandlerChunkActiveCount;
        JukeboxManager.Instance.OnOptimizeVisualQueueChunks -= OptimizeVisualQueueChunksCheck;
        JukeboxManager.Instance.OnSetSongInfo -= SetSongInfo;
        JukeboxManager.Instance.OnStopJukeboxVisual -= StopJukeboxVisuals;

        StopJukeboxVisuals();
    }

    #region Chunk

    private void CreateButtonHandlersChunk()
    {
        List<JukeboxTrackButtonHandler> jukeboxButtonHandlers = new List<JukeboxTrackButtonHandler>(_trackChunkSize);

        for (int i = 0; i < _trackChunkSize; i++)
        {
            GameObject jukeboxTrackButton = Instantiate(_jukeboxButtonPrefab, _trackListContent);
            JukeboxTrackButtonHandler buttonHandler = jukeboxTrackButton.GetComponent<JukeboxTrackButtonHandler>();
            buttonHandler.OnTrackPressed += JukeboxManager.Instance.QueueTrack;
            buttonHandler.Clear();
            jukeboxButtonHandlers.Add(buttonHandler);
        }

        Chunk<JukeboxTrackButtonHandler> tracksChunk = new Chunk<JukeboxTrackButtonHandler>();
        tracksChunk.Pool = jukeboxButtonHandlers;
        tracksChunk.AmountInUse = 0;

        _buttonHandlerChunks.Add(tracksChunk);
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

    #endregion

    private void ReduceQueueHandlerChunkActiveCount(int index)
    {
        _queueHandlerChunks[index].AmountInUse--;
        _queueUseTimes++;
    }

    public string PlayTrack()
    {
        string name = JukeboxManager.Instance.PlayTrack();

        if (string.IsNullOrEmpty(name)) return null;

        return name;
    }

    public void StopJukeboxVisuals()
    {
        _songName.text = NoSongName;
        _diskImage.sprite = _emptyDisk;

        if (_diskSpinCoroutine != null)
        {
            StopCoroutine(_diskSpinCoroutine);
            _diskSpinCoroutine = null;
            _diskTransform.rotation = Quaternion.identity;
        }

        //JukeboxManager.Instance.StopJukebox();
        OnChangeJukeBoxSong?.Invoke(null);
    }

    public void ToggleJukeboxScreen(bool toggle) { _jukeboxObject.SetActive(toggle); }

    private void SetSongInfo(MusicTrack track)
    {
        if (track == null) return;

        _songName.text = track.Name;
        _diskImage.sprite = track.Info.Disk;

        if (_diskSpinCoroutine != null)
        {
            StopCoroutine(_diskSpinCoroutine);
            _diskSpinCoroutine = null;
            _diskTransform.rotation = Quaternion.identity;
        }

        if (track.Music != null) _diskSpinCoroutine = StartCoroutine(SpinDisk());

        OnChangeJukeBoxSong?.Invoke(track);
    }

    private IEnumerator SpinDisk()
    {
        while (true)
        {
            _diskTransform.Rotate(Vector3.forward * -_diskRotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    #region Optimization
    private void OptimizeVisualQueueChunksCheck()
    {
        if (_queueUseTimes >= _queueOptimizationThreshold) OptimizeVisualQueueChunks();

        gameObject.GetComponent<DailyTaskProgressListener>().UpdateProgress("1");
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
        destination.SetTrack(target.CurrentTrack);
        _queueHandlerChunks[destination.ChunkIndex].AmountInUse++;

        target.Clear();
        _queueHandlerChunks[target.ChunkIndex].AmountInUse--;
    }
    #endregion
}
