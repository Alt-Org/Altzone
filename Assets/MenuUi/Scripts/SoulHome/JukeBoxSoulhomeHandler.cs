using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JukeBoxSoulhomeHandler : MonoBehaviour
{
    //public static JukeBoxSoulhomeHandler Instance;

    [Header("Disk")]
    [SerializeField] private Image _diskImage;
    [SerializeField] private Transform _diskTransform;
    [SerializeField] private float _diskRotationSpeed = 100f;

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
    private List<Chunk<JukeboxTrackButtonHandler>> _trackVisualSelectionChunks = new List<Chunk<JukeboxTrackButtonHandler>>();
    private int _selectionChunkPointer = 0;
    private int _selectionPoolPointer = -1;

    private List<Chunk<JukeboxTrackQueueHandler>> _trackVisualQueueChunks = new List<Chunk<JukeboxTrackQueueHandler>>();
    private int _queueChunkPointer = 0;
    private int _queuePoolPointer = -1;

    private Queue<JukeboxTrackQueueHandler> _trackQueue = new Queue<JukeboxTrackQueueHandler>();
    [SerializeField] private int _queueOptimizationThreshold = 4;
    private int _queueUseTimes = 0;

    private MusicTrack _currentMusicTrack;

    private Coroutine _trackEndingControlCoroutine;

    //private void Awake()
    //{
    //    if (Instance == null)
    //        Instance = this;
    //}

    void Start()
    {
        _backButton.onClick.AddListener(() => ToggleJukeboxScreen(false));
        CreateJukeboxButtonHandlersChunk();
        CreateJukeboxQueueHandlersChunk();

        List<MusicTrack> musicTracks = AudioManager.Instance.GetMusicList("Jukebox");

        foreach (MusicTrack track in musicTracks) GetFreeJukeboxTrackButtonHandler().SetTrack(track);
    }

    private void OnEnable()
    {
        //JukeboxController.OnChangeJukeBoxSong += SetSongInfo;
        //JukeboxController.OnChangeJukeBoxQueue += UpdateQueueText;
    }

    private void OnDisable()
    {
        //JukeboxController.OnChangeJukeBoxSong -= SetSongInfo;
        //JukeboxController.OnChangeJukeBoxQueue -= UpdateQueueText;
    }

    #region Chunk

    private void CreateJukeboxButtonHandlersChunk()
    {
        List<JukeboxTrackButtonHandler> jukeboxButtonHandlers = new List<JukeboxTrackButtonHandler>(_trackChunkSize);

        for (int i = 0; i < _trackChunkSize; i++)
        {
            GameObject jukeboxTrackButton = Instantiate(_jukeboxButtonPrefab, _trackListContent);
            JukeboxTrackButtonHandler buttonHandler = jukeboxTrackButton.GetComponent<JukeboxTrackButtonHandler>();
            buttonHandler.OnTrackPressed += QueueTrack;
            buttonHandler.Clear();
            jukeboxButtonHandlers.Add(buttonHandler);
        }

        Chunk<JukeboxTrackButtonHandler> tracksChunk = new Chunk<JukeboxTrackButtonHandler>();
        tracksChunk.Pool = jukeboxButtonHandlers;
        tracksChunk.AmountInUse = 0;

        _trackVisualSelectionChunks.Add(tracksChunk);
    }

    private void CreateJukeboxQueueHandlersChunk()
    {
        List<JukeboxTrackQueueHandler> jukeboxButtonHandlers = new List<JukeboxTrackQueueHandler>(_trackChunkSize);

        for (int i = 0; i < _trackChunkSize; i++)
        {
            GameObject jukeboxTrackButton = Instantiate(_queueTextPrefab, _queueContent);
            JukeboxTrackQueueHandler buttonHandler = jukeboxTrackButton.GetComponent<JukeboxTrackQueueHandler>();
            buttonHandler.Setup(_trackVisualQueueChunks.Count, i);
            jukeboxButtonHandlers.Add(buttonHandler);
        }

        Chunk<JukeboxTrackQueueHandler> tracksChunk = new Chunk<JukeboxTrackQueueHandler>();
        tracksChunk.Pool = jukeboxButtonHandlers;
        tracksChunk.AmountInUse = 0;

        _trackVisualQueueChunks.Add(tracksChunk);
    }

    private JukeboxTrackButtonHandler GetFreeJukeboxTrackButtonHandler()
    {
        _selectionPoolPointer++;

        if (_selectionPoolPointer >= _trackChunkSize)
        {
            CreateJukeboxButtonHandlersChunk();
            _selectionChunkPointer++;
            _selectionPoolPointer = 0;
        }

        _trackVisualSelectionChunks[_selectionChunkPointer].AmountInUse++;

        return _trackVisualSelectionChunks[_selectionChunkPointer].Pool[_selectionPoolPointer];
    }

    private JukeboxTrackQueueHandler GetFreeJukeboxTrackQueueHandler()
    {
        _queuePoolPointer++;

        if (_queuePoolPointer >= _trackChunkSize)
        {
            CreateJukeboxQueueHandlersChunk();
            _queueChunkPointer++;
            _queuePoolPointer = 0;
        }

        _trackVisualQueueChunks[_queueChunkPointer].AmountInUse++;

        return _trackVisualQueueChunks[_queueChunkPointer].Pool[_queuePoolPointer];
    }

    #endregion

    public void ToggleJukeboxScreen(bool toggle) { _jukeboxObject.SetActive(toggle); }

    private string PlayTrack(MusicTrack musicTrack)
    {
        string name = AudioManager.Instance.PlayMusic("Jukebox", musicTrack);

        if (string.IsNullOrEmpty(name)) return null;

        _currentMusicTrack = musicTrack;
        SetSongInfo(musicTrack);

        _trackEndingControlCoroutine = StartCoroutine(TrackEndingControl());

        return name;
    }

    public void StopJukebox()
    {
        _diskImage.sprite = null;

        if (_diskSpinCoroutine != null)
        {
            StopCoroutine(_diskSpinCoroutine);
            _diskSpinCoroutine = null;
            _diskTransform.rotation = Quaternion.identity;
        }

        if (_trackEndingControlCoroutine != null)
        {
            StopCoroutine(_trackEndingControlCoroutine);
            _trackEndingControlCoroutine = null;
        }
    }

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
    }

    private IEnumerator SpinDisk()
    {
        while (true)
        {
            _diskTransform.Rotate(Vector3.forward * -_diskRotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator TrackEndingControl()
    {
        float timer = 0f;

        while (timer < _currentMusicTrack.Music.length)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        PlayNextJukeboxTrack();
    }

    private void PlayNextJukeboxTrack()
    {
        if (_trackQueue.Count > 0)
        {
            JukeboxTrackQueueHandler trackQueueHandler = _trackQueue.Peek();
            string name = PlayTrack(trackQueueHandler.CurrentTrack);

            if (name == null) return;

            _trackQueue.Dequeue();
            _trackVisualQueueChunks[trackQueueHandler.ChunkIndex].AmountInUse--;
            trackQueueHandler.Clear();

            _queueUseTimes++;
        }
        else
            _trackEndingControlCoroutine = StartCoroutine(TrackEndingControl());

        if (_queueUseTimes >= _queueOptimizationThreshold) OptimizeVisualQueueChunks();
    }

    #region Queue
    public void QueueTrack(MusicTrack musicTrack)
    {
        if (_currentMusicTrack != null)
            AddToQueue(musicTrack);
        else
            PlayTrack(musicTrack);
    }

    private void AddToQueue(MusicTrack musicTrack)
    {
        JukeboxTrackQueueHandler trackQueueHandler = GetFreeJukeboxTrackQueueHandler();
        trackQueueHandler.SetTrack(musicTrack);
        _trackQueue.Enqueue(trackQueueHandler);
    }

    private void OptimizeVisualQueueChunks()
    {
        List<int> freeSpaces = new List<int>();

        Debug.Log("Optimizing jukebox queue visual list...");
        _queueUseTimes = 0;

        for (int i = 0; i < _trackVisualQueueChunks.Count; i++)
        {
            int freeSpace = 0;

            //if (_trackVisualQueueChunks[i].AmountInUse >= _trackChunkSize) //Full
            //    freeSpaces.Add(freeSpace);
            //else
            //{
                int previousFreeSlot = 0;

                for (int j = 0; j < _trackVisualQueueChunks[i].Pool.Count; j++)
                {
                    if (!_trackVisualQueueChunks[i].Pool[j].InUse())
                    {
                        freeSpace++;
                        previousFreeSlot++;
                    }
                    else
                    {
                        if (i != 0 && freeSpaces[i - 1] != 0) //Move to a different chunk.
                        {
                            Chunk<JukeboxTrackQueueHandler> destinationChunk = _trackVisualQueueChunks[i - 1];

                            for (int k = 0; k < _trackVisualQueueChunks[i - 1].Pool.Count; k++)
                                if (!_trackVisualQueueChunks[i - 1].Pool[k].InUse())
                                    MoveVisualQueueItem(_trackVisualQueueChunks[i].Pool[j], _trackVisualQueueChunks[i - 1].Pool[k]);

                            freeSpaces[i - 1]--;
                            freeSpace++;
                            previousFreeSlot++;
                        }
                        else if (previousFreeSlot != 0) //Move to a different slot in the same pool.
                            MoveVisualQueueItem(_trackVisualQueueChunks[i].Pool[j], _trackVisualQueueChunks[i].Pool[j - previousFreeSlot]);
                    }
                }

                freeSpaces.Add(freeSpace);
            //}
        }

        // Set chunk pointer and pool pointer to closest free space.
        RecalibrateQueuePointers();

        // Reassemble _trackQueue.
        ReassebleDataQueue();

        Debug.Log("Jukebox queue visual list optimization done.");
    }

    /// <summary>
    /// Repositions <c>_queueChunkPointer</c> and <c>_queuePoolPointer</c> to the new free tail end on the <c>_trackVisualQueueChunks</c>.
    /// </summary>
    private void RecalibrateQueuePointers()
    {
        for (int i = _trackVisualQueueChunks.Count - 1; i >= 0; i--)
        {
            //Debug.LogError(i + ", chunk || inuse: " + _trackVisualQueueChunks[i].AmountInUse);
            if (_trackVisualQueueChunks[i].AmountInUse > 0)
            {
                if (_trackVisualQueueChunks[i].AmountInUse >= _trackChunkSize)
                    _queueChunkPointer = i;
                else if ((i + 1) < _trackVisualQueueChunks.Count)
                    _queueChunkPointer = i + 1;

                for (int j = _trackVisualQueueChunks[i].Pool.Count - 1; j >= 0; j--)
                {
                    //Debug.LogError(_trackVisualQueueChunks[i].Pool[j].InUse());
                    if (!_trackVisualQueueChunks[i].Pool[j].InUse())
                        _queuePoolPointer = j - 1;
                }

                break;
            }
        }
    }

    /// <summary>
    /// Reassebles the <c>_trackQueue</c>. If not done after optimizing <c>_trackVisualQueueChunks</c>,
    /// the <c>_trackQueue</c> will point to empty <c>_JukeboxTrackQueueHandler</c>s.
    /// </summary>
    private void ReassebleDataQueue()
    {
        _trackQueue.Clear();

        foreach (Chunk<JukeboxTrackQueueHandler> chunk in _trackVisualQueueChunks)
            foreach (JukeboxTrackQueueHandler handler in chunk.Pool)
                if (handler.InUse())
                    _trackQueue.Enqueue(handler);
    }

    private void MoveVisualQueueItem(JukeboxTrackQueueHandler target, JukeboxTrackQueueHandler destination)
    {
        destination.SetTrack(target.CurrentTrack);
        _trackVisualQueueChunks[destination.ChunkIndex].AmountInUse++;

        target.Clear();
        _trackVisualQueueChunks[target.ChunkIndex].AmountInUse--;
    }
    #endregion

    //public void ExitMainMenuMode()
    //{
    //    _isMainMenuMode = false;
    //}

    private class Chunk<T>
    {
        public int AmountInUse = 0;
        public List<T> Pool = new List<T>();
    }
}
