using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Audio;
using Altzone.Scripts.ReferenceSheets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JukeBoxSoulhomeHandler : MonoBehaviour
{
    [SerializeField] private GameObject _jukeboxObject;

    [Header("Disk")]
    [SerializeField] private List<Image> _diskImage;
    [SerializeField] private List<Transform> _diskTransform;
    [SerializeField] private float _diskRotationSpeed = 100f;
    [SerializeField] private Sprite _emptyDisk;

    [Header("TopBarControls")]
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _playListButton;
    [SerializeField] private TMP_Text _pageNameText;
    [Space]
    [SerializeField] private Button _switchMainWindowButton;
    [SerializeField] private Image _switchMainWindowImage;
    [SerializeField] private Sprite _navigationSprite;
    [SerializeField] private Sprite _musicPlayerSprite;

    [Header("MusicControls")]
    [SerializeField] private List<Button> _playButtons;
    [SerializeField] private List<Image> _playButtonImages;
    [SerializeField] private List<Button> _trackGoBackButtons;
    [SerializeField] private List<Button> _trackGoForwardButtons;
    [Space]
    [SerializeField] private Sprite _playSprite;
    [SerializeField] private Sprite _stopSprite;

    [Header("Navigation")]
    [SerializeField] private GameObject _navigationObject;
    [SerializeField] private TMP_InputField _searchField;
    [SerializeField] private Button _tracksFilterButton;
    [Space]
    [SerializeField] private Transform _tracksListContent;
    [SerializeField] private GameObject _jukeboxButtonPrefab;
    [SerializeField] private Button _goToMusicPlayerButton;

    [Header("MusicPlayer")]
    [SerializeField] private GameObject _musicPlayerObject;
    [SerializeField] private TMP_Text _trackName;
    [SerializeField] private Button _shuffleButton;
    [SerializeField] private Button _loopButton;
    [SerializeField] private Button _trackOptionsButton;
    [SerializeField] private TMP_Text _trackPlayTimeText;
    [SerializeField] private Slider _trackPlayTimeSlider;

    [Header("QueueList")]
    [SerializeField] private Transform _queueContent;
    [SerializeField] private GameObject _queueTextPrefab;

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

    private enum JukeboxWindowType
    {
        Navigation,
        MusicPlayer
    }

    private JukeboxWindowType _currentWindowType = JukeboxWindowType.Navigation;

    public delegate void ChangeJukeBoxSong(MusicTrack track);
    public static event ChangeJukeBoxSong OnChangeJukeBoxSong;

    void Start()
    {
        _jukeboxObject.SetActive(false);

        _closeButton.onClick.AddListener(() => ToggleJukeboxScreen(false));
        CreateButtonHandlersChunk();
        CreateQueueHandlersChunk();

        List<MusicTrack> musicTracks = AudioManager.Instance.GetMusicList("Jukebox");

        foreach (MusicTrack track in musicTracks) GetFreeJukeboxTrackButtonHandler().SetTrack(track);

        _trackName.text = NoSongName;

        _switchMainWindowButton.onClick.AddListener(() => SwitchMainWindow());
        _goToMusicPlayerButton.onClick.AddListener(() => SwitchMainWindow());
        foreach (Button button in _playButtons) button.onClick.AddListener(() => PlayStopButtonActivated());
    }

    private void OnEnable()
    {
        JukeboxManager.Instance.OnGetFreeJukeboxTrackQueueHandler += GetFreeJukeboxTrackQueueHandler;
        JukeboxManager.Instance.OnReduceQueueHandlerChunkActiveCount += ReduceQueueHandlerChunkActiveCount;
        JukeboxManager.Instance.OnOptimizeVisualQueueChunks += OptimizeVisualQueueChunksCheck;
        JukeboxManager.Instance.OnSetSongInfo += SetSongInfo;
        JukeboxManager.Instance.OnStopJukeboxVisual += StopJukeboxVisuals;
        JukeboxManager.Instance.OnClearJukeboxVisual += ClearJukeboxVisuals;
        JukeboxManager.Instance.OnSetVisibleElapsedTime += UpdateMusicElapsedTime;

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
        JukeboxManager.Instance.OnSetVisibleElapsedTime -= UpdateMusicElapsedTime;

        StopJukeboxVisuals();
    }

    #region Chunk

    private void CreateButtonHandlersChunk()
    {
        List<JukeboxTrackButtonHandler> jukeboxButtonHandlers = new List<JukeboxTrackButtonHandler>(_trackChunkSize);

        for (int i = 0; i < _trackChunkSize; i++)
        {
            GameObject jukeboxTrackButton = Instantiate(_jukeboxButtonPrefab, _tracksListContent);
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

    private void UpdateMusicElapsedTime(float elapsedTime)
    {
        string minutes = (elapsedTime / 60f).ToString().Split('.')[0];
        string seconds = (elapsedTime % 60).ToString().Split('.')[0];

        _trackPlayTimeText.text = $"{minutes}:{((seconds.Length == 1) ? ("0" + seconds) : seconds)}";
        _trackPlayTimeSlider.value = elapsedTime / JukeboxManager.Instance.CurrentMusicTrack.Music.length;
    }

    private void PlayStopButtonActivated()
    {
        if (JukeboxManager.Instance.CurrentMusicTrack == null) return;

        bool result = JukeboxManager.Instance.PlaybackToggle();

        foreach (Image image in _playButtonImages)
        {
            if (result) //Stopped
            {
                image.sprite = _playSprite;
                StopJukeboxVisuals();
            }
            else //Playing
            {
                image.sprite = _stopSprite;

                if (_diskSpinCoroutine != null)
                {
                    StopCoroutine(_diskSpinCoroutine);
                    _diskSpinCoroutine = null;
                    foreach (Transform rotationT in _diskTransform) rotationT.rotation = Quaternion.identity;
                }

                _diskSpinCoroutine = StartCoroutine(SpinDisk());
            }
        }
    }

    private void SwitchMainWindow()
    {
        switch (_currentWindowType)
        {
            case JukeboxWindowType.Navigation: _navigationObject.SetActive(false); break;
            case JukeboxWindowType.MusicPlayer: _musicPlayerObject.SetActive(false); break;
        }

        _currentWindowType = (_currentWindowType == JukeboxWindowType.Navigation ? JukeboxWindowType.MusicPlayer : JukeboxWindowType.Navigation);

        switch (_currentWindowType)
        {
            case JukeboxWindowType.Navigation: _navigationObject.SetActive(true); _switchMainWindowImage.sprite = _musicPlayerSprite; break;
            case JukeboxWindowType.MusicPlayer: _musicPlayerObject.SetActive(true); _switchMainWindowImage.sprite = _navigationSprite; break;
        }
    }

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
        if (_diskSpinCoroutine != null)
        {
            StopCoroutine(_diskSpinCoroutine);
            _diskSpinCoroutine = null;

            foreach (Transform rotationT in _diskTransform) rotationT.rotation = Quaternion.identity;
        }

        OnChangeJukeBoxSong?.Invoke(null);
    }

    public void ClearJukeboxVisuals()
    {
        _trackName.text = NoSongName;

        foreach (Image image in _diskImage) image.sprite = _emptyDisk;
    }

    public void ToggleJukeboxScreen(bool toggle) { _jukeboxObject.SetActive(toggle); }

    private void SetSongInfo(MusicTrack track)
    {
        if (track == null) return;

        _trackName.text = track.Name;
        foreach (Image image in _diskImage) image.sprite = track.Info.Disk;

        if (_diskSpinCoroutine != null)
        {
            StopCoroutine(_diskSpinCoroutine);
            _diskSpinCoroutine = null;
            foreach (Transform rotationT in _diskTransform) rotationT.rotation = Quaternion.identity;
        }

        if (track.Music != null) _diskSpinCoroutine = StartCoroutine(SpinDisk());

        OnChangeJukeBoxSong?.Invoke(track);
    }

    private IEnumerator SpinDisk()
    {
        while (true)
        {
            foreach (Transform rotationT in _diskTransform) rotationT.Rotate(Vector3.forward * -_diskRotationSpeed * Time.deltaTime);

            yield return null;
        }
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
            destination.SetTrack(target.CurrentTrack);
        else
            destination.Clear();

        _queueHandlerChunks[destination.ChunkIndex].AmountInUse++;

        target.Clear();
        _queueHandlerChunks[target.ChunkIndex].AmountInUse--;
    }
    #endregion
}
