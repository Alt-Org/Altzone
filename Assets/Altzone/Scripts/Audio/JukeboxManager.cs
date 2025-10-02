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

        private PlayerData _currentPlayerData = null;
        public PlayerData CurrentPlayerData { get { return _currentPlayerData; } }

        #region Queue
        private List<TrackQueueData> _trackQueue = new();
        public List<TrackQueueData> TrackQueue { get { return _trackQueue; } }

        private int _trackQueuePointer = 0;

        private int _trackQueueLastFreeIndex = 0;

        public int TrackChunkSize = 8;
        //public int TrackChunkBufferSize = 4;

        #endregion

        #region Playback
        private TrackQueueData _currentTrackQueueData;
        public TrackQueueData CurrentTrackQueueData { get { return _currentTrackQueueData; } }

        private Coroutine _trackEndingControlCoroutine;

        private bool _loopLastTrack = true;
        public bool LoopLastTrack { get { return _loopLastTrack; } }

        private bool _playbackPaused = false;

        private bool _jukeboxMuted = false;

        private float _musicElapsedTime = 0f;
        #endregion

        #region Playlist
        public enum PlaylistLoopType
        {
            None,
            LoopPlaylist,
            LoopOne
        }

        [SerializeField] private PlaylistLoopType _loopPlayType = PlaylistLoopType.LoopPlaylist;
        public PlaylistLoopType LoopPlayType {  get { return _loopPlayType; } }

        private bool _isShuffle = false;
        public bool IsShuffle { get { return _isShuffle; } }

        public enum PlaylistType
        {
            Clan,
            Custom,
            All
        }

        private Playlist _currentPlaylist = null;
        public Playlist CurrentPlaylist {  get { return _currentPlaylist; } }

        //private List<Playlist> _allPlaylists = new List<Playlist>();

        private bool _playlistReady = false;
        public bool PlaylistReady { get { return _playlistReady; } }

        private System.DateTime _musicTrackStartTime = System.DateTime.Now;
        #endregion

        #region Playback History
        private string _playbackLastUpdate = "never";
        public string PlaybackLastUpdate { get { return _playbackLastUpdate; } }

        public enum PlaybackHistoryType
        {
            Add,
            Delete,
            Insert,
            Hide,
            Unhide,
            MoveToLast
        }

        private List<PlaybackHistory> _playbackHistory = new();
        public List<PlaybackHistory> PlaybackHistory { get { return _playbackHistory; } }

        private int _localTrackId = 0;

        private float _timeoutSeconds = 10f;
        #endregion

        #region Events & Delegates
        public delegate ChunkPointer GetFreeJukeboxTrackQueueHandler();
        public event GetFreeJukeboxTrackQueueHandler OnGetFreeJukeboxTrackQueueHandler;

        public delegate void OptimizeVisualQueueChunks();
        public event OptimizeVisualQueueChunks OnOptimizeVisualQueueChunks;
        public event OptimizeVisualQueueChunks OnForceOptimizeVisualQueueChunks;

        //public delegate void ReduceQueueHandlerChunkActiveCount(int index);
        //public event ReduceQueueHandlerChunkActiveCount OnReduceQueueHandlerChunkActiveCount;

        public delegate void SetSongInfo(MusicTrack track);
        public event SetSongInfo OnSetSongInfo;

        public delegate void StopJukeboxVisual();
        public event StopJukeboxVisual OnStopJukeboxVisuals;

        public delegate void ClearJukeboxVisual();
        public event ClearJukeboxVisual OnClearJukeboxVisuals;

        public delegate void SetVisibleElapsedTime(float timeElapsed);
        public event SetVisibleElapsedTime OnSetVisibleElapsedTime;

        //public delegate void SetPlayButtonImages(bool value);
        //public event SetPlayButtonImages OnSetPlayButtonImages;

        //public delegate void PlaylistPlay();
        //public event PlaylistPlay OnPlaylistPlay;

        public delegate void PlaylistChange();
        public event PlaylistChange OnQueueChange;

        public delegate void MoveVisualQueueToLast(ChunkPointer chunkPointer, int trackQueueIndex);
        public event MoveVisualQueueToLast OnQueueToLast;

        public delegate JukeboxTrackQueueHandler GetTrackQueueHandler(ChunkPointer chunkPointer);
        public event GetTrackQueueHandler OnGetTrackQueueHandler;
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
            StartCoroutine(Setup());

            //_allPlaylists.Add(new("Kaikki", true, PlaylistType.All, AudioManager.Instance.GetMusicList("Jukebox"))); //TODO: Replace later with what tracks the clan/player owns.
            //_clanPlayList = new("Klaani", PlaylistType.Clan, /*Insert clan playlist fetch here.*/);
            //_allPlaylists.Add(new("Klaani", true, PlaylistType.Clan, new List<MusicTrack>())); //TESTING

            //foreach (Playlist playlist in /*Insert custom playlists here*/)
            //{

            //}

            //_currentPlaylist = _allPlaylists[0]; //TESTING - Replace with last used playlist later.
        }

        private IEnumerator Setup()
        {
            StartCoroutine(GetPlayerData());

            yield return new WaitUntil(() => _currentPlayerData != null);

            _currentPlaylist = new("Klaani", PlaylistType.Clan, TESTCreateServerMusicList()); //TODO: Replace later with playlist from server
            CreateTrackQueue();
            _playlistReady = true;

            if (OnQueueChange != null) OnQueueChange.Invoke();

            //if (_currentPlaylist.Type == PlaylistType.Clan)
            //{
            //    _currentTrackQueueData = _trackQueue[0]; //TODO: Replace 0 with correct music track index from server.
            //    _trackQueuePointer++;
            //}

            _musicTrackStartTime = System.DateTime.Now; //TODO: Replace "System.DateTime.Now" with correct music track start time from server.
            _jukeboxMuted = true;
            PlayNextJukeboxTrack();
            _jukeboxMuted = false;
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

        #region Playlist
        //public List<string> GetPlaylistNames()
        //{
        //    List<string> names = new List<string>();

        //    foreach (Playlist playlist in _allPlaylists)
        //        names.Add(playlist.Name);

        //    return names;
        //}

        //public void PlayPlaylist(bool randomize)
        //{
        //    OnPlaylistPlay?.Invoke();
        //}

        //public void PlayPlaylist(int startIndex)
        //{
        //    if (_trackQueue == null || _trackQueue.Count == 0) CreateTrackQueue();

        //    _trackQueuePointer = startIndex;
        //    PlayNextJukeboxTrack();
        //    OnPlaylistPlay?.Invoke();
        //}

        /// <returns>True if playlist was found.</returns>
        //public bool SetPlaylist(string playlistName)
        //{
        //    if (_currentPlaylist.Name == playlistName) return true;

        //    foreach (Playlist playlist in _allPlaylists)
        //        if (playlist.Name == playlistName)
        //        {
        //            _currentPlaylist = playlist;
        //            OnPlaylistChange?.Invoke(playlist);
        //            return true;
        //        }

        //    return false;
        //}

        public bool AddTrackToPlaylist(string trackName, string playlistName)
        {


            return false;
        }

        public bool RemoveTrackOnPlaylist(string trackName, string playlistName)
        {


            return false;
        }

        public void StartPlaylistPlayback(int startIndex, PlaylistLoopType playType)
        {

        }

        private List<string> TESTCreateServerMusicList() //TODO: Remove when server is ready.
        {
            return new List<string>()
            {
                "käyttäjä1_In Awe",
                "käyttäjä1_What Are You Waiting For",
                "käyttäjä2_Soul",
                "käyttäjä2_Rush Hour",
                "käyttäjä3_Falling Bird",
                "käyttäjä2_Plan",
            };
        }
        #endregion

        #region Playback
        //public void SetLooping(bool looping, bool loopOne) { _loopLastTrack = looping; }

        public string TryPlayTrack()
        {
            if (_jukeboxMuted) return null;

            if (_currentTrackQueueData != null)
                return ContinueTrack(false);
            else
                return PlayNextJukeboxTrack();
        }

        /// <summary>
        /// Plays the current track.
        /// </summary>
        /// <returns>Track name that is playing.</returns>
        public string PlayTrack() { return PlayTrack(_currentTrackQueueData); }

        /// <summary>
        /// Plays the track found in given <c>TrackQueueData</c>.
        /// </summary>
        /// <returns>Track name that is playing.</returns>
        public string PlayTrack(TrackQueueData trackQueueData)
        {
            if (trackQueueData == null || _trackEndingControlCoroutine != null || (_playbackPaused && _currentTrackQueueData != null)) return null;

            string name = "";

            if (_jukeboxMuted)
            {
                name = trackQueueData.MusicTrack.Name;
            }
            else
            {
                name = AudioManager.Instance.PlayMusic("Jukebox", trackQueueData.MusicTrack);

                if (string.IsNullOrEmpty(name)) return null;

                _playbackPaused = false;
            }

            if (OnSetSongInfo != null) OnSetSongInfo.Invoke(trackQueueData.MusicTrack);

            _currentTrackQueueData = trackQueueData;
            _musicElapsedTime = 0f;

            if (_trackEndingControlCoroutine != null) StopCoroutine( _trackEndingControlCoroutine);

            _trackEndingControlCoroutine = StartCoroutine(TrackEndingControl());
            //Debug.LogError(_musicElapsedTime + " | " + _currentTrackQueueData.MusicTrack.Music.length);
            //OnSetPlayButtonImages?.Invoke(true);
            return name;
        }

        /// <returns><c>True</c> if paused.</returns>
        public bool PlaybackToggle(bool muteActivation)
        {
            if (muteActivation)
                _jukeboxMuted = !_jukeboxMuted;
            else
                _playbackPaused = !_playbackPaused;

            if (_trackQueue.Count != 0 && _trackQueuePointer >= _trackQueue.Count) //Start current playlist from beginning.
                _trackQueuePointer = 0;

            if (!_playbackPaused && !_jukeboxMuted)
            {
                if (_currentTrackQueueData != null)
                    ContinueTrack(muteActivation);
                else
                    PlayTrack();
            }
            else
                AudioManager.Instance.PlayFallBackTrack();

            if (_playbackPaused) StopJukebox();

            if (muteActivation)
                return _jukeboxMuted;
            else
                return _playbackPaused;
        }

        public string ContinueTrack(bool muteActivation)
        {
            if (_playbackPaused) _playbackPaused = false;

            if (_currentPlaylist.Type == PlaylistType.Clan || muteActivation)
            {
                float seconds = (float)System.DateTime.Now.Subtract(_musicTrackStartTime).TotalMilliseconds / 1000f;

                while (seconds > _trackQueue[_trackQueuePointer].MusicTrack.Music.length)
                {
                    seconds -= _trackQueue[_trackQueuePointer].MusicTrack.Music.length;
                    _trackQueuePointer++;

                    if (_trackQueuePointer >= _trackQueue.Count) _trackQueuePointer = 0;
                }

                _musicElapsedTime = seconds;
            }

            if (_trackEndingControlCoroutine != null) StopCoroutine(_trackEndingControlCoroutine);

            _trackEndingControlCoroutine = StartCoroutine(TrackEndingControl());
            //OnSetPlayButtonImages?.Invoke(true);

            return AudioManager.Instance.ContinueMusic("Jukebox", _currentTrackQueueData.MusicTrack, _musicElapsedTime);
        }

        public void StopJukebox()
        {
            //if (_trackEndingControlCoroutine != null)
            //{
            //    StopCoroutine(_trackEndingControlCoroutine);
            //    _trackEndingControlCoroutine = null;
            //}

            //OnSetPlayButtonImages?.Invoke(false);
        }

        private IEnumerator TrackEndingControl()
        {
            while (_musicElapsedTime < /*_currentTrackQueueData.MusicTrack.Music.length*/ 5f)
            {
                if (OnSetVisibleElapsedTime != null) OnSetVisibleElapsedTime.Invoke(_musicElapsedTime);
                
                yield return null;
                _musicElapsedTime += Time.deltaTime;
            }

            PlayNextJukeboxTrack();
        }

        private string PlayNextJukeboxTrack()
        {
            if (_trackQueuePointer >= _trackQueue.Count && _loopPlayType == PlaylistLoopType.LoopPlaylist) //Keep playing the current playlist.
                _trackQueuePointer = 0;

            if (_trackQueuePointer < _trackQueue.Count && !_trackQueue[_trackQueuePointer].InUse() && !TryFindValidQueueData())
            {
                Debug.LogError("Next track is null and JukeboxManager failed to find a valid music track!");
                return null;
            }

            string name = null;

            if (_trackEndingControlCoroutine != null)
            {
                StopCoroutine(_trackEndingControlCoroutine);
                _trackEndingControlCoroutine = null;
            }

            if (_loopPlayType == PlaylistLoopType.LoopOne)
            {
                name = PlayTrack(_currentTrackQueueData);
            }
            else if (_trackQueuePointer < _trackQueue.Count) //Play the next track in queue.
            {
                JukeboxTrackQueueHandler queueHandler = null;

                _currentTrackQueueData = GetPreviousTrackQueueData();

                // Move played track to last in JukeboxMusicPlayerHandler if active.
                if (_currentTrackQueueData != null && _currentTrackQueueData.Pointer != null && OnGetTrackQueueHandler != null)
                    queueHandler = OnGetTrackQueueHandler.Invoke(_currentTrackQueueData.Pointer);

                if (queueHandler != null)
                {
                    OnQueueToLast.Invoke(_currentTrackQueueData.Pointer, _currentTrackQueueData.LinearIndex);
                    queueHandler.Clear();
                }
                else if (_currentTrackQueueData != null)
                    AddPlaybackHistory(PlaybackHistoryType.MoveToLast, _currentTrackQueueData);

                // Play next track.
                name = PlayTrack(_trackQueue[_trackQueuePointer]);

                if (name == null)
                {
                    Debug.LogError("JukeboxManager: Next tracks name is null!");
                    return null;
                }

                _musicTrackStartTime = System.DateTime.Now;

                // Hide current tracks visual part in JukeboxMusicPlayerHandler.
                AddPlaybackHistory(PlaybackHistoryType.Hide, _currentTrackQueueData);

                if (OnQueueChange != null) OnQueueChange.Invoke();

                _trackQueuePointer++;
            }
            else //Go back to latest requested music in AudioManager.
            {
                AudioManager manager = AudioManager.Instance;
                _currentTrackQueueData = null;
                StopJukebox();
                if (!string.IsNullOrEmpty(manager.FallbackMusicCategory))
                    manager.PlayMusic(manager.FallbackMusicCategory, manager.FallbackMusicTrack);

                if (OnStopJukeboxVisuals != null)
                {
                    OnStopJukeboxVisuals.Invoke();
                    OnClearJukeboxVisuals.Invoke();
                }
            }

            if (_jukeboxMuted) return null;

            return name;
        }

        /// <summary>
        /// Used when the <c>_trackQueuePointer</c> points to a removed track.
        /// </summary>
        /// <returns>True if method found a valid new index for <c>_trackQueuePointer</c>.</returns>
        private bool TryFindValidQueueData()
        {
            int startIndex = _trackQueuePointer + 1;

            for (int i = startIndex; i < _trackQueue.Count; i++)
                if (_trackQueue[i].InUse())
                {
                    _trackQueuePointer = i;
                    return true;
                }

            for (int i = 0; i < startIndex - 1; i++)
                if (_trackQueue[i].InUse())
                {
                    _trackQueuePointer = i;
                    return true;
                }

            Debug.LogError("JukeboxManager: Could not find a valid TrackQueueData from _trackQueue list!");
            return false;
        }
        #endregion

        #region Playback History
        /// <summary>
        /// Only for Adding and Deleting types.
        /// </summary>
        public void AddPlaybackHistory(PlaybackHistoryType type, TrackQueueData trackQueueHandler)
        {
            AddPlaybackHistory(type, trackQueueHandler, null/*, -1, -1*/);
        }

        //public void AddPlaybackHistory(PlaybackHistoryType type, TrackQueueData trackQueueHandler, int insertChunkIndex, int insertPoolIndex)
        //{
        //    AddPlaybackHistory(type, trackQueueHandler, null, insertChunkIndex, insertPoolIndex);
        //}

        public void AddPlaybackHistory(PlaybackHistoryType type, TrackQueueData trackQueueHandler1, TrackQueueData trackQueueHandler2)
        {
            PlaybackHistory historyData = null;
            //Debug.LogError("operation: " + type.ToString() + ", to music track: " + trackQueueHandler1.MusicTrack.Name);
            // Try finding existing playback history.
            for (int i = 0; i < _playbackHistory.Count; i++)
                if ((_playbackHistory[i].Target1.TrackQueueData.Id.Split("_")[1] == trackQueueHandler1.Id.Split("_")[1]))
                {
                    historyData = _playbackHistory[i];
                    break;
                }

            if (historyData != null && ValidForPlaybackHistoryRemoval(type, historyData._playbackHistoryType)) _playbackHistory.Remove(historyData);

            _playbackHistory.Add(new(type, trackQueueHandler1));
            LogPlaybackLastUpdate();
        }

        private bool ValidForPlaybackHistoryRemoval(PlaybackHistoryType type, PlaybackHistoryType playbackHistoryType)
        {
            switch (type)
            {
                case PlaybackHistoryType.Delete:
                    {
                        switch (playbackHistoryType)
                        {
                            case PlaybackHistoryType.Add: return true;
                            case PlaybackHistoryType.Insert: return true; // check if something breaks!
                            case PlaybackHistoryType.Hide: return true;
                            case PlaybackHistoryType.Unhide: return true;
                            default: return false;
                        }
                    }
                case PlaybackHistoryType.Hide:
                    {
                        return playbackHistoryType == PlaybackHistoryType.Unhide;
                    }
                case PlaybackHistoryType.Unhide:
                    {
                        return playbackHistoryType == PlaybackHistoryType.Hide;
                    }
                default: return false;
            }
        }

        public void ClearPlaybackHistory() { _playbackHistory.Clear(); }

        private void LogPlaybackLastUpdate() { _playbackLastUpdate = System.DateTime.Now.ToString(); }
        #endregion

        #region Queue
        /// <summary>
        /// Used to add music tracks by the local user.
        /// </summary>
        public void QueueTrack(MusicTrack musicTrack)
        {
            if (_trackQueuePointer != 0 && _currentTrackQueueData != null)
                InsertLastToQueueList(musicTrack);
            else if (_currentTrackQueueData != null)
                AddToQueueList(musicTrack);
            else
                PlayTrack(new TrackQueueData(CreateTrackQueueId(musicTrack.Name), 0, null, musicTrack, true));
        }

        private void CreateTrackQueue()
        {
            foreach (TrackQueueData trackQueueData in _currentPlaylist.GetTrackQueueList(_currentPlayerData.Id))
            {
                if (OnGetFreeJukeboxTrackQueueHandler != null) trackQueueData.Pointer = OnGetFreeJukeboxTrackQueueHandler.Invoke();

                AddToQueueList(trackQueueData);
            }

            _localTrackId = _currentPlaylist.TrackQueueDatas.Count;
        }

        private void AddToQueueList(MusicTrack musicTrack) { AddToQueueList(new TrackQueueData(CreateTrackQueueId(musicTrack.Name), _trackQueue.Count, null, musicTrack, true)); }

        private void AddToQueueList(TrackQueueData trackQueueData)
        {
            _trackQueue.Add(trackQueueData);
            AddPlaybackHistory(PlaybackHistoryType.Add, trackQueueData);

            if (OnQueueChange != null) OnQueueChange.Invoke();
        }

        private void InsertLastToQueueList(MusicTrack musicTrack)
        {
            int linearIndex = _trackQueuePointer - 1;
            int skippedAmount = 0;
            TrackQueueData trackQueueData = new(CreateTrackQueueId(musicTrack.Name), linearIndex, null, musicTrack, true);

            _trackQueue.Insert(linearIndex, trackQueueData);
            AddPlaybackHistory(PlaybackHistoryType.Add, trackQueueData);

            if (OnQueueChange != null) OnQueueChange.Invoke();

            _trackQueuePointer++;

            // Update every TrackQueueData and JukeboxTrackQueueHandler linear index that is ahead of the inserted TrackQueueData.
            for (int i = linearIndex + 1; i < _trackQueue.Count; i++)
            {
                _trackQueue[i].LinearIndex = i;

                if (_trackQueue[i].InUse() && OnGetTrackQueueHandler != null)
                    OnGetTrackQueueHandler(_trackQueue[i].Pointer).SetLinearIndex(i - skippedAmount);
                else
                    skippedAmount++;
            }
        }

        public void DeleteFromQueue(int linearIndex) { _trackQueue[linearIndex].Clear(); }

        /// <summary>
        /// Compacts the <c>_trackQueue</c>.
        /// <br/>Note: Called from <c>JukeboxMusicPlayerHandler</c>.
        /// </summary>
        public void OptimizeTrackQueue()
        {
            if (_trackQueuePointer >= _trackQueue.Count) TryFindValidQueueData();

            Queue<int> freeIndexes = new();
            string currentTrackId = _trackQueue[_trackQueuePointer].Id;

            for (int i = 0; i < _trackQueue.Count; i++)
            {
                if (_trackQueue[i].InUse() && freeIndexes.Count != 0)
                {
                    JukeboxTrackQueueHandler handler = OnGetTrackQueueHandler(_trackQueue[i].Pointer);
                    int linearIndex = freeIndexes.Dequeue();
  
                    _trackQueue[linearIndex].SetData(_trackQueue[i], linearIndex);
                    handler.SetLinearIndex(linearIndex);
                    _trackQueue[i].Clear();
                    freeIndexes.Enqueue(i);
                }
                else if (!_trackQueue[i].InUse())
                    freeIndexes.Enqueue(i);
            }

            _trackQueuePointer = _trackQueue.FindIndex((data) => data.Id == currentTrackId);
            //_currentTrackQueueData = GetPreviousTrackQueueData();

            ClearPlaybackHistory();
            LogPlaybackLastUpdate();
        }

        private TrackQueueData GetPreviousTrackQueueData()
        {
            if (_trackQueuePointer - 1 >= 0)
                return _trackQueue[_trackQueuePointer - 1];
            else
            {
                if (_trackQueue[_trackQueue.Count - 1].InUse())
                    return _trackQueue[_trackQueue.Count - 1];
                else
                    for (int i = _trackQueue.Count - 1; i >= 0; i--)
                        if (_trackQueue[i].InUse())
                            return _trackQueue[i];
            }

            Debug.LogError("Failed to find previous TrackQueueData!");
            return null;
        }
        #endregion

        private string CreateTrackQueueId(string musicTrackName)
        {
            // LocalTrackId is supposed to be always unique so that TrackQueueData can be connected to the PlaybackHistory data operations.
            _localTrackId++;
            // PlayerId is used to see witch track can be removed by the local user in clan playlist.
            return $"{_currentPlayerData.Id}_{musicTrackName}_{_localTrackId}"; // PlayerId _ MusicTrackName _ LocalMusicTrackId
        }
    }

    public class TrackQueueData
    {
        public string Id; //Fromat: UserId_TrackName_LocalTrackId
        public int LinearIndex;
        public ChunkPointer Pointer;
        public MusicTrack MusicTrack;
        public bool UserOwned;

        public TrackQueueData(string id, int linearIndex, ChunkPointer pointer, MusicTrack track, bool userOwned) { SetData(id, linearIndex, pointer, track, userOwned); }

        public bool InUse() { return !string.IsNullOrEmpty(Id); }

        public void SetData(TrackQueueData data, int linearIndex) { SetData(data.Id, linearIndex, data.Pointer, data.MusicTrack, data.UserOwned); }

        public void SetData(string id, int linearIndex, ChunkPointer pointer, MusicTrack track, bool userOwned)
        {
            Id = id;
            LinearIndex = linearIndex;
            Pointer = pointer;
            MusicTrack = track;
            UserOwned = userOwned;
        }

        public void Clear()
        {
            Id = "";
            LinearIndex = -1;
            Pointer.Delete();
            Pointer = null;
            MusicTrack = null;
        }
    }

    public class Playlist
    {
        public string Name;
        public JukeboxManager.PlaylistType Type;
        public List<string> TrackQueueDatas; //Fromat: UserId_TrackName

        public Playlist(string name, JukeboxManager.PlaylistType type, List<string> serverMusicTracks)
        {
            Name = name;
            Type = type;
            TrackQueueDatas = serverMusicTracks;
        }

        public List<TrackQueueData> GetTrackQueueList(string localUserId)
        {
            List<TrackQueueData> queueHandlers = new();
            List<MusicTrack> musicTracks = new(AudioManager.Instance.GetMusicList("Jukebox"));

            for (int i = 0; i < TrackQueueDatas.Count; i++)
            {
                string userId = TrackQueueDatas[i].Split('_')[0];
                string serverTrackName = TrackQueueDatas[i].Split('_')[1];

                foreach (MusicTrack track in musicTracks)
                    if (track.Name.ToLower() == serverTrackName.ToLower())
                    {
                        queueHandlers.Add(new(userId + "_" + track.Name, i, null, track, (userId.ToLower() == localUserId.ToLower())));
                        break;
                    }
            }

            return queueHandlers;
        }
    }

    public class PlaybackHistory
    {
        public JukeboxManager.PlaybackHistoryType _playbackHistoryType;

        public PlaybackHistoryDataCollection Target1;
        public PlaybackHistoryDataCollection Target2;

        public PlaybackHistory(JukeboxManager.PlaybackHistoryType type, TrackQueueData trackQueue)
        {
            int chunkIndex = (trackQueue.Pointer != null ? trackQueue.Pointer.ChunkIndex: -1);
            int poolIndex = (trackQueue.Pointer != null ? trackQueue.Pointer.PoolIndex: -1);

            _playbackHistoryType = type;
            Target1 = new(trackQueue);
        }

        /// <summary>
        /// Swap
        /// </summary>
        public PlaybackHistory(JukeboxManager.PlaybackHistoryType type, TrackQueueData trackQueueTarget1, TrackQueueData trackQueueTarget2)
        {
            int chunkIndex1 = (trackQueueTarget1.Pointer != null ? trackQueueTarget1.Pointer.ChunkIndex : -1);
            int poolIndex1 = (trackQueueTarget1.Pointer != null ? trackQueueTarget1.Pointer.PoolIndex : -1);

            int chunkIndex2 = (trackQueueTarget2 != null && trackQueueTarget2.Pointer != null ? trackQueueTarget2.Pointer.ChunkIndex : -1);
            int poolIndex2 = (trackQueueTarget2 != null && trackQueueTarget2.Pointer != null ? trackQueueTarget2.Pointer.PoolIndex : -1);

            _playbackHistoryType = type;
            Target1 = new(trackQueueTarget1/*, chunkIndex1, poolIndex1*/);
            Target2 = new(trackQueueTarget2/*, chunkIndex2, poolIndex2*/);
        }

        public struct PlaybackHistoryDataCollection
        {
            public TrackQueueData TrackQueueData;
            public int LinearIndex;

            public PlaybackHistoryDataCollection(TrackQueueData trackQueueData/*, int chunkIndex, int poolIndex*/)
            {
                TrackQueueData = trackQueueData;
                LinearIndex = trackQueueData.LinearIndex;
            }
        }
    }

    /// <summary>
    /// Used to keep track of and manage items that can change indexes in a chunk list in <c>JukeboxMusicPlayerHandler</c> script.
    /// </summary>
    public class ChunkPointer
    {
        public int ChunkIndex;
        public int PoolIndex;

        public delegate void TrackQueueRemoved(ChunkPointer chunkPointer);
        public event TrackQueueRemoved OnTrackQueueRemoved;

        public ChunkPointer(int chunkIndex, int poolIndex)
        {
            ChunkIndex = chunkIndex;
            PoolIndex = poolIndex;
        }

        public void Set(int chunkIndex, int poolIndex)
        {
            ChunkIndex = chunkIndex;
            PoolIndex = poolIndex;
        }

        public void Delete() { if (OnTrackQueueRemoved != null) OnTrackQueueRemoved.Invoke(this); }
    }
}
