using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine;

namespace Altzone.Scripts.Audio
{
    public class JukeboxManager : AltMonoBehaviour
    {
        public static JukeboxManager Instance { get; private set; }

        #region Queue
        private Queue<TrackQueueData> _trackQueue = new Queue<TrackQueueData>();
        public Queue<TrackQueueData> TrackQueue { get { return _trackQueue; } }

        private string _playbackLastUpdate = "never";
        public string PlaybackLastUpdate { get { return _playbackLastUpdate; } }
        #endregion

        #region Music Track
        private MusicTrack _currentMusicTrack;
        public MusicTrack CurrentMusicTrack { get { return _currentMusicTrack; } }

        private Coroutine _trackEndingControlCoroutine;

        private bool _loopLastTrack = true;
        public bool LoopLastTrack {  get { return _loopLastTrack; } }

        private bool _playbackPaused = true;

        private float _musicElapsedTime = 0f;
        #endregion

        #region Playlist
        public enum PlaylistPlayType
        {
            Normal,
            LoopPlaylist,
            LoopOne
        }

        public enum PlaylistType
        {
            Clan,
            Custom,
            All
        }

        private Playlist _currentPlayList;

        private Playlist _allTracksPlayList;
        private Playlist _clanPlayList;
        private List<Playlist> _customPlaylists = new List<Playlist>();

        private string _customPlaylistName = "";
        #endregion

        #region Playback History
        public enum PlaybackHistoryType
        {
            Add,
            Delete,
            Swap,
            MoveToLast
        }

        private List<PlaybackHistory> _playbackHistory = new();
        public List<PlaybackHistory> PlaybackHistory { get { return _playbackHistory; } }

        private int _localTrackId = 0;

        private float _timeoutSeconds = 10f;
        private PlayerData _currentPlayerData = null;
        #endregion

        #region Events & Delegates
        public delegate JukeboxTrackQueueHandler GetFreeJukeboxTrackQueueHandler();
        public event GetFreeJukeboxTrackQueueHandler OnGetFreeJukeboxTrackQueueHandler;

        public delegate void OptimizeVisualQueueChunks();
        public event OptimizeVisualQueueChunks OnOptimizeVisualQueueChunks;

        public delegate void ReduceQueueHandlerChunkActiveCount(int index);
        public event ReduceQueueHandlerChunkActiveCount OnReduceQueueHandlerChunkActiveCount;

        public delegate void SetSongInfo(MusicTrack track);
        public event SetSongInfo OnSetSongInfo;

        public delegate void StopJukeboxVisual();
        public event StopJukeboxVisual OnStopJukeboxVisual;

        public delegate void ClearJukeboxVisual();
        public event ClearJukeboxVisual OnClearJukeboxVisual;

        public delegate void SetVisibleElapsedTime(float timeElapsed);
        public event SetVisibleElapsedTime OnSetVisibleElapsedTime;

        public delegate void SetPlayButtonImages(bool value);
        public event SetPlayButtonImages OnSetPlayButtonImages;
        #endregion

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
            StartCoroutine(GetPlayerData());
        }

        private IEnumerator GetPlayerData()
        {
            bool? timeout = null;

            StartCoroutine(PlayerDataTransferer("get", null, _timeoutSeconds, data => timeout = data, data => _currentPlayerData = data));
            yield return new WaitUntil(() => (_currentPlayerData != null || timeout != null));

            if (_currentPlayerData == null)
            {
                Debug.LogError("Failed to fetch player data.");
                yield break;
            }
        }

        public void SetLooping(bool looping, bool loopOne) { _loopLastTrack = looping; }

        public string PlayTrack() { return PlayTrack(_currentMusicTrack); }

        public string PlayTrack(MusicTrack musicTrack)
        {
            if (_trackEndingControlCoroutine != null || (_playbackPaused && _currentMusicTrack != null)) return null;

            if (_currentMusicTrack == musicTrack) return ContinueTrack();

            string name = AudioManager.Instance.PlayMusic("Jukebox", musicTrack);

            if (string.IsNullOrEmpty(name)) return null;

            _playbackPaused = false;

            if (OnSetSongInfo != null) OnSetSongInfo.Invoke(musicTrack);

            _currentMusicTrack = musicTrack;
            _musicElapsedTime = 0f;
            _trackEndingControlCoroutine = StartCoroutine(TrackEndingControl());

            OnSetPlayButtonImages?.Invoke(true);

            return name;
        }

        /// <returns><c>True</c> if paused.</returns>
        public bool PlaybackToggle()
        {
            _playbackPaused = !_playbackPaused;

            if (_playbackPaused)
            {
                StopJukebox();
                AudioManager.Instance.PlayFallBackTrack();
            }
            else
            {
                if (_currentMusicTrack != null)
                    ContinueTrack();
                else
                    PlayTrack();
            }

            return _playbackPaused;
        }

        public string ContinueTrack()
        {
            _trackEndingControlCoroutine = StartCoroutine(TrackEndingControl());
            OnSetPlayButtonImages?.Invoke(true);

            return AudioManager.Instance.ContinueMusic("Jukebox", _currentMusicTrack, _musicElapsedTime);
        }

        public void StopJukebox()
        {
            if (_trackEndingControlCoroutine != null)
            {
                StopCoroutine(_trackEndingControlCoroutine);
                _trackEndingControlCoroutine = null;
            }

            OnSetPlayButtonImages?.Invoke(false);
        }

        private IEnumerator TrackEndingControl()
        {
            while (_musicElapsedTime < _currentMusicTrack.Music.length)
            {
                if (OnSetVisibleElapsedTime != null) OnSetVisibleElapsedTime.Invoke(_musicElapsedTime);

                yield return null;
                _musicElapsedTime += Time.deltaTime;
            }

            PlayNextJukeboxTrack();
        }

        private void PlayNextJukeboxTrack()
        {
            if (_trackEndingControlCoroutine != null)
            {
                StopCoroutine(_trackEndingControlCoroutine);
                _trackEndingControlCoroutine = null;
            }

            if (_trackQueue.Count > 0) //Play the next track in queue.
            {
                TrackQueueData trackQueueHandler = _trackQueue.Peek();
                string name = PlayTrack(trackQueueHandler.MusicTrack);

                if (name == null) return;

                _trackQueue.Dequeue();

                TryAddPlaybackHistory(PlaybackHistoryType.Delete, trackQueueHandler);

                if (OnReduceQueueHandlerChunkActiveCount != null)
                    OnReduceQueueHandlerChunkActiveCount.Invoke(trackQueueHandler.QueueHandler.ChunkIndex);

                trackQueueHandler.QueueHandler.Clear();
            }
            else if (_loopLastTrack) //Keep playing the current track.
            {
                _musicElapsedTime = 0f;
                PlayTrack(_currentMusicTrack);
            }
            else //Go back to latest requested music in AudioManager.
            {
                AudioManager manager = AudioManager.Instance;
                _currentMusicTrack = null;
                StopJukebox();
                if (!string.IsNullOrEmpty(manager.FallbackMusicCategory))
                    manager.PlayMusic(manager.FallbackMusicCategory, manager.FallbackMusicTrack);
                else
                    manager.PlayMusic("Soulhome", "");

                if (OnStopJukeboxVisual != null)
                {
                    OnStopJukeboxVisual.Invoke();
                    OnClearJukeboxVisual.Invoke();
                }
            }

            if (OnOptimizeVisualQueueChunks != null) OnOptimizeVisualQueueChunks.Invoke();
        }

        #region Playback History
        /// <summary>
        /// Only for Adding and Deleting types.
        /// </summary>
        public void TryAddPlaybackHistory(PlaybackHistoryType type, TrackQueueData trackQueueHandler)
        {
            TryAddPlaybackHistory(type, trackQueueHandler, null);
        }

        public void TryAddPlaybackHistory(PlaybackHistoryType type, TrackQueueData trackQueueHandler1, TrackQueueData trackQueueHandler2)
        {
            PlaybackHistory historyData = null;

            foreach (PlaybackHistory data in _playbackHistory)
                if ((data.Target1.Id.Split("_")[1] == trackQueueHandler1.Id.Split("_")[1]) ||
                    (trackQueueHandler2 != null && data.Target2.Id.Split("_")[1] == trackQueueHandler2.Id.Split("_")[1])) // Wrong?
                {
                    historyData = data;
                    break;
                }

            if (historyData != null)
            {
                switch (type)
                {
                    case PlaybackHistoryType.Delete:
                        {
                            if (historyData._playbackHistoryType == PlaybackHistoryType.Add) _playbackHistory.Remove(historyData);

                            break;
                        }
                    case PlaybackHistoryType.Swap:
                        {
                            if (historyData._playbackHistoryType == PlaybackHistoryType.MoveToLast) _playbackHistory.Remove(historyData);

                            break;
                        }
                    case PlaybackHistoryType.MoveToLast:
                        {
                            if (historyData._playbackHistoryType == PlaybackHistoryType.Add ||
                                historyData._playbackHistoryType == PlaybackHistoryType.Swap) _playbackHistory.Remove(historyData);

                            break;
                        }
                }
            }

            if (type != PlaybackHistoryType.Swap)
                _playbackHistory.Add(new(type, trackQueueHandler1));
            else
                _playbackHistory.Add(new(type, trackQueueHandler1, trackQueueHandler2));

            _playbackLastUpdate = System.DateTime.Now.ToString();
        }

        public void ClearPlaybackHistory() { _playbackHistory.Clear(); }
        #endregion

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
            TrackQueueData data = new();

            if (OnGetFreeJukeboxTrackQueueHandler != null)
            {
                data.QueueHandler = OnGetFreeJukeboxTrackQueueHandler.Invoke();
                data.QueueHandler.SetTrack(CreateTrackQueueId(), musicTrack);
            }

            data.Id = CreateTrackQueueId();
            data.MusicTrack = musicTrack;
            _trackQueue.Enqueue(data);

            TryAddPlaybackHistory(PlaybackHistoryType.Add, data);
        }

        /// <summary>
        /// Reassembles the <c>_trackQueue</c> from visual jukebox queue. If not done after optimizing <c>_queueHandlerChunks</c>,
        /// the <c>_trackQueue</c> will point to empty <c>_JukeboxTrackQueueHandler</c>s.
        /// </summary>
        public void ReassembleDataQueue(List<Chunk<JukeboxTrackQueueHandler>> queueHandlerChunks)
        {
            _trackQueue.Clear();

            foreach (Chunk<JukeboxTrackQueueHandler> chunk in queueHandlerChunks)
                foreach (JukeboxTrackQueueHandler handler in chunk.Pool)
                    if (handler.InUse())
                    {
                        TrackQueueData data = new(CreateTrackQueueId(), handler, handler.CurrentTrack);
                        data.QueueHandler = handler;
                        data.MusicTrack = handler.CurrentTrack;
                        _trackQueue.Enqueue(data);
                    }

            ClearPlaybackHistory();
            _playbackLastUpdate = System.DateTime.Now.ToString();
        }
        #endregion

        private string CreateTrackQueueId()
        {
            _localTrackId++;
            // PlayerId _ LocalTrackId
            //
            // LocalTrackId is supposed to be always unique so that TrackQueueData can be connected to the PlaybackHistory data operations.
            // PlayerId is used to see witch track can be removed by the local user in clan playlist.
            return $"{_currentPlayerData.Id}_{_localTrackId}";
        }
    }

    public class TrackQueueData
    {
        public string Id;
        public JukeboxTrackQueueHandler QueueHandler;
        public MusicTrack MusicTrack;

        public TrackQueueData() { }

        public TrackQueueData(string id, JukeboxTrackQueueHandler handler, MusicTrack track)
        {
            Id = id;
            QueueHandler = handler;
            MusicTrack = track;
        }
    }

    public class Playlist
    {
        public string Name;
        public List<MusicTrack> MusicTracks;
    }

    public class PlaybackHistory
    {
        public JukeboxManager.PlaybackHistoryType _playbackHistoryType;

        // Swap
        public TrackQueueData Target1 = null;
        public TrackQueueData Target2 = null;

        // For backup if either of the TrackQueueDatas are deleted and become null.
        public int ChunkIndex1 = -1;
        public int PoolIndex1 = -1;
        public int ChunkIndex2 = -1;
        public int PoolIndex2 = -1;

        public PlaybackHistory(JukeboxManager.PlaybackHistoryType type, TrackQueueData trackQueue)
        {
            Target1 = trackQueue;
            _playbackHistoryType = type;

            if (trackQueue != null && trackQueue.QueueHandler != null)
            {
                ChunkIndex1 = trackQueue.QueueHandler.ChunkIndex;
                PoolIndex1 = trackQueue.QueueHandler.PoolIndex;
            }
        }

        /// <summary>
        /// Swap
        /// </summary>
        public PlaybackHistory(JukeboxManager.PlaybackHistoryType type, TrackQueueData trackQueueTarget1, TrackQueueData trackQueueTarget2)
        {
            _playbackHistoryType = type;
            Target1 = trackQueueTarget1;
            Target2 = trackQueueTarget2;

            if (trackQueueTarget1 != null)
            {
                ChunkIndex1 = trackQueueTarget1.QueueHandler.ChunkIndex;
                PoolIndex1 = trackQueueTarget1.QueueHandler.PoolIndex;
            }
            if (trackQueueTarget2 != null)
            {
                ChunkIndex2 = trackQueueTarget2.QueueHandler.ChunkIndex;
                PoolIndex2 = trackQueueTarget2.QueueHandler.PoolIndex;
            }
        }
    }
}
