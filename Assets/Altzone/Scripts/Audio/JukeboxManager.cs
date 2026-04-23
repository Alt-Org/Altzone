using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine;

namespace Altzone.Scripts.Audio
{
    public class JukeboxManager : AltMonoBehaviour
    {
        private const long _trackTimeErrorMargin = 100000000;

        public static JukeboxManager Instance { get; private set; }

        private PlayerData _currentPlayerData = null;
        public PlayerData CurrentPlayerData { get { return _currentPlayerData; } }

        #region Favorite
        private const string _favoritesPrefsString = "favoriteTracks";

        private List<MusicTrackFavoriteData> _musicTrackFavorites = new List<MusicTrackFavoriteData>();
        #endregion

        #region Queue
        private List<TrackQueueData> _trackQueue = new();
        public List<TrackQueueData> TrackQueue { get { return _trackQueue; } }

        public int TrackChunkSize = 8;
        #endregion

        #region Playback
        private TrackQueueData _currentTrackQueueData;
        public TrackQueueData CurrentTrackQueueData { get { return _currentTrackQueueData; } }

        private Coroutine _trackEndingControlCoroutine;

        private bool _loopLastTrack = true;
        public bool LoopLastTrack { get { return _loopLastTrack; } }

        private bool _playbackPaused = false;

        private bool _jukeboxMuted = false;
        public bool JukeboxMuted { get {  return _jukeboxMuted; } }

        private float _musicElapsedTime = 0f;
        #endregion

        #region Playlist
        [Header("Playlist")]
        [SerializeField] private float _playlistServerFetchFrequency = 30f;
        [SerializeField] private float _timeoutSeconds = 10f;

        private Coroutine _playlistServerFetchCoroutine;

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

        public enum MusicTrackFavoriteType
        {
            Neutral,
            Like,
            Dislike
        }

        private bool _serverOperationAvailable = true;

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
        #endregion

        #region Preview
        [SerializeField] private float _previewTime = 10f;

        public enum PreviewLocationType
        {
            Main,
            Secondary
        }

        private bool _trackPreviewActive = false;
        public bool TrackPreviewActive { get { return _trackPreviewActive; } }

        private MusicTrack _currentPreviewMusicTrack = null;
        private JukeboxTrackButtonHandler _currentPreviewMusicTrackHandler = null;
        private Coroutine _currentPreviewTrackCoroutine;
        #endregion

        #region Events & Delegates
        public delegate ChunkPointer GetFreeJukeboxTrackQueueHandler();
        public event GetFreeJukeboxTrackQueueHandler OnGetFreeJukeboxTrackQueueHandler;

        public delegate void OptimizeVisualQueueChunks();
        public event OptimizeVisualQueueChunks OnOptimizeVisualQueueChunks;
        public event OptimizeVisualQueueChunks OnForceOptimizeVisualQueueChunks;

        public delegate void SetSongInfo(MusicTrack track, bool useAnimations = true);
        public event SetSongInfo OnSetSongInfo;

        public delegate void StopJukeboxVisual();
        public event StopJukeboxVisual OnStopJukeboxVisuals;

        public delegate void ClearJukeboxVisual();
        public event ClearJukeboxVisual OnClearJukeboxVisuals;

        public delegate void SetVisibleElapsedTime(float musicTrackLength, float timeElapsed, PreviewLocationType type,
            bool playAnimations = true);
        public event SetVisibleElapsedTime OnSetVisibleElapsedTime;

        public delegate void JukeboxMute(bool value);
        public event JukeboxMute OnJukeboxMute;

        public delegate void PlaylistChange();
        public event PlaylistChange OnQueueChange;

        public delegate void MoveVisualQueueToLast(ChunkPointer chunkPointer, int trackQueueIndex);
        public event MoveVisualQueueToLast OnQueueToLast;

        public delegate JukeboxTrackQueueHandler GetTrackQueueHandler(ChunkPointer chunkPointer);
        public event GetTrackQueueHandler OnGetTrackQueueHandler;

        public delegate void FavoriteButtonChange(string musicTrackId, MusicTrackFavoriteType favoriteType);
        public event FavoriteButtonChange OnFavoriteButtonChange;

        public delegate void PreviewStart();
        public event PreviewStart OnPreviewStart;

        public delegate void PreviewEnd();
        public event PreviewEnd OnPreviewEnd;

        public delegate void ShowTextPopup(string text);
        public event ShowTextPopup OnShowTextPopup;

        public delegate void MusicTrackInfoPressed(MusicTrack musicTrack, JukeboxManager.MusicTrackFavoriteType likeType);
        public event MusicTrackInfoPressed OnMusicTrackInfoPressed;
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

        private void Start() { StartCoroutine(Setup()); }

        private IEnumerator Setup()
        {
            StartCoroutine(GetPlayerData());

            yield return new WaitUntil(() => _currentPlayerData != null && AudioManager.Instance);

            ServerPlaylist playlistData = null;
            bool? success = null;

            StartCoroutine(UpdateLocalClanPlaylist((successData) => success = successData, (serverPlaylistData) => playlistData = serverPlaylistData));

            yield return new WaitUntil(() => (success != null));

            if (!success.Value) yield break;

            _currentPlaylist = new Playlist("Klaani", PlaylistType.Clan, playlistData);

            _musicTrackFavorites = GetFavoriteDatas();
            _playlistReady = true;

            OnQueueChange?.Invoke();

            _playlistServerFetchCoroutine = StartCoroutine(ServerPlaylistFetchLoop());
        }

        private IEnumerator GetPlayerData()
        {
            bool? timeout = null;

            StartCoroutine(PlayerDataTransferer("get", null, _timeoutSeconds, data => timeout = data, data => _currentPlayerData = data));
            yield return new WaitUntil(() => (_currentPlayerData != null || timeout != null));

            if (_currentPlayerData == null) Debug.LogError("Failed to fetch player data.");
        }

        #region Favorite
        public List<MusicTrackFavoriteData> GetFavoriteDatas() //TODO: Add a check for new tracks that come from jukebox music list.
        {
            string rawData = PlayerPrefs.GetString(_favoritesPrefsString); //Format: TrackId_LikeEnumInt-TrackId_LikeEnumInt-TrackId...
            List<MusicTrackFavoriteData> favoriteDatas = new List<MusicTrackFavoriteData>();
            List<MusicTrack> musicTracks = AudioManager.Instance.GetMusicList("jukebox");

            if (string.IsNullOrEmpty(rawData)) //First time setup.
            {
                foreach (MusicTrack track in musicTracks)
                    favoriteDatas.Add(new(track.Id, MusicTrackFavoriteType.Neutral));

                return favoriteDatas;
            }

            //Get existing data.
            string[] trackDatas = rawData.Split('-');

            foreach (string rawTrackData in trackDatas)
            {
                string[] trackData = rawTrackData.Split('_');
                favoriteDatas.Add(new MusicTrackFavoriteData(trackData[0], (MusicTrackFavoriteType)(int.Parse(trackData[1]))));
            }

            //Check if there are new tracks to be added.
            foreach (MusicTrack track in musicTracks)
                if (favoriteDatas.FindIndex((data) => track.Id == data.MusicTrackId) == -1)
                    favoriteDatas.Add(new MusicTrackFavoriteData(track.Id, MusicTrackFavoriteType.Neutral));

            return favoriteDatas;
        }

        public void SaveFavoriteDatas(List<MusicTrackFavoriteData> favoriteDatas)
        {
            string sendTarget = "";

            for (int i = 0; i < favoriteDatas.Count; i++)
            {
                sendTarget += favoriteDatas[i].MusicTrackId + "_" + (int)favoriteDatas[i].FavoriteType;

                if (i + 1 < favoriteDatas.Count) sendTarget += "-";
            }

            PlayerPrefs.SetString(_favoritesPrefsString, sendTarget);
        }

        public MusicTrackFavoriteType GetTrackFavoriteType(MusicTrack musicTrack)
        {
            MusicTrackFavoriteData data = _musicTrackFavorites.Find((data) => musicTrack.Id == data.MusicTrackId);

            return data?.FavoriteType ?? MusicTrackFavoriteType.Neutral;
        }

        public void InvokeOnFavoriteButtonChange(string musicTrackId, MusicTrackFavoriteType favoriteType)
        {
            _musicTrackFavorites.Find((data) => musicTrackId == data.MusicTrackId).FavoriteType = favoriteType;

            SaveFavoriteDatas(_musicTrackFavorites);

            OnFavoriteButtonChange?.Invoke(musicTrackId, favoriteType);
        }

        public MusicTrack GetNotHatedMusicTrack() { return GetNotHatedMusicTrack(_currentTrackQueueData); }

        public MusicTrack GetNotHatedMusicTrack(TrackQueueData trackQueueData)
        {
            if (trackQueueData == null) return null;

            MusicTrackFavoriteData favoriteData = _musicTrackFavorites.Find((data) => trackQueueData.MusicTrack.Id == data.MusicTrackId);

            if (favoriteData is not { FavoriteType: MusicTrackFavoriteType.Dislike }) return trackQueueData.MusicTrack;

            List<MusicTrackFavoriteData> likedDatas = _musicTrackFavorites.FindAll((data) => MusicTrackFavoriteType.Like == data.FavoriteType);

            return (GetMusicTrack(likedDatas[Random.Range(0, likedDatas.Count)].MusicTrackId));
        }
        #endregion

        #region Playlist
        private IEnumerator GetClanPlaylist(System.Action<bool?> timeoutCallback, System.Action<ServerPlaylist> playlistData)
        {
            bool? timeout = null;
            ServerPlaylist serverPlaylist = null;

            StartCoroutine(ServerManager.Instance.GetJukeboxClanPlaylist((data) => serverPlaylist = data));
            Coroutine timeoutCoroutine = StartCoroutine(WaitUntilTimeout(_timeoutSeconds, (timeoutData) => timeout = timeoutData));

            yield return new WaitUntil(() => (serverPlaylist != null || timeout != null));

            if (timeout != null)
            {
                Debug.LogError("Failed to fetch jukebox playlist from server!");
                timeoutCallback(false);
                yield break;
            }

            StopCoroutine(timeoutCoroutine);
            playlistData(serverPlaylist);
        }

        private void UpdateLocalClanPlaylist() { StartCoroutine(UpdateLocalClanPlaylist(null, null)); }

        private IEnumerator UpdateLocalClanPlaylist(System.Action<bool> successCallback, System.Action<ServerPlaylist> serverPlaylistCallback)
        {
            ServerPlaylist serverPlaylistData = null;
            bool? timeout = null;

            _serverOperationAvailable = false;

            if (_playlistServerFetchCoroutine != null)
            {
                StopCoroutine(_playlistServerFetchCoroutine);
                _playlistServerFetchCoroutine = null;
            }

            StartCoroutine(GetClanPlaylist((data) => timeout = data, (data) => serverPlaylistData = data));

            yield return new WaitUntil(() => (serverPlaylistData != null || timeout != null));

            if (serverPlaylistData == null)
            {
                _serverOperationAvailable = true;
                successCallback?.Invoke(false);

                yield break;
            }

            UpdateTrackQueue(serverPlaylistData);
            StartCoroutine(PlayServerTrack(serverPlaylistData.currentSong));

            _serverOperationAvailable = true;

            _playlistServerFetchCoroutine = StartCoroutine(ServerPlaylistFetchLoop());

            serverPlaylistCallback?.Invoke(serverPlaylistData);
            successCallback?.Invoke(true);
        }

        private IEnumerator DeleteTrackFromServer(System.Action<bool> successCallback, string trackUniqueId, string trackName)
        {
            bool? timeout = null;
            bool? callback = null;

            _serverOperationAvailable = false;

            StartCoroutine(ServerManager.Instance.DeleteJukeboxClanMusicTrack((data) => callback = data, trackUniqueId));
            StartCoroutine(WaitUntilTimeout(_timeoutSeconds, (data) => timeout = data));

            yield return new WaitUntil(() => (callback != null || timeout != null));

            if (timeout != null || callback != null && !callback.Value)
            {
                Debug.LogError("Failed to delete music track from clan playlist!");
                _serverOperationAvailable = true;
                successCallback?.Invoke(false);

                yield break;
            }

            _serverOperationAvailable = true;
            OnShowTextPopup?.Invoke($"Kappale: {trackName}, poistettu.");
            successCallback?.Invoke(true);
        }

        private IEnumerator AddTrackToServer(System.Action<bool> done, MusicTrack musicTrack)
        {
            bool? timeout = null;
            bool? callback = null;

            _serverOperationAvailable = false;

            StartCoroutine(ServerManager.Instance.AddJukeboxClanMusicTrack((data) => callback = data, musicTrack));
            StartCoroutine(WaitUntilTimeout(_timeoutSeconds, (data) => timeout = data));

            yield return new WaitUntil(() => (callback != null || timeout != null));

            if (timeout != null || callback != null && !callback.Value)
            {
                Debug.LogWarning("Failed to add music track to clan playlist or max amount of tracks has been added!");
                OnShowTextPopup?.Invoke($"Enimmäismäärä kappaleita lisätty!");
                _serverOperationAvailable = true;
                done(false);

                yield break;
            }

            OnShowTextPopup?.Invoke($"Kappale: {musicTrack.Name}, lisätty.");

            _serverOperationAvailable = true;
            done(true);
        }

        private void UpdateTrackQueue(ServerPlaylist serverPlaylist)
        {
            List<ServerCompareData> deleteDatas = new List<ServerCompareData>();
            List<ServerCompareData> addDatas = new List<ServerCompareData>();
            List<ServerSong> serverSongs = serverPlaylist.songQueue;

            //Gather all possible addable and deletable tracks.
            int i = 0;

            while (i < serverSongs.Count || i < _trackQueue.Count)
            {
                if (i < serverSongs.Count && i < _trackQueue.Count && _trackQueue[i].InUse() && serverSongs[i].id == _trackQueue[i].ServerSongData.id) //Skip.
                {
                    i++;
                    continue;
                }

                if (i < serverSongs.Count) //Server, look up what will be added.
                {
                    int serverIndex = _trackQueue.FindIndex((data) => data.ServerSongData != null && serverSongs[i].id == data.ServerSongData.id);

                    if (serverIndex == -1) addDatas.Add(new ServerCompareData(serverSongs[i], i));
                }

                if (i < _trackQueue.Count && _trackQueue[i].InUse()) //Local, look up what will be deleted.
                {
                    int localIndex = serverSongs.FindIndex((data) => _trackQueue[i].ServerSongData.id == data.id);

                    if (localIndex == -1) deleteDatas.Add(new ServerCompareData(_trackQueue[i].ServerSongData, -1));
                }

                i++;
            }

            //Remove local tracks.
            foreach (ServerCompareData deleteData in deleteDatas)
            {
                TrackQueueData data = _trackQueue.Find((data) => (data.ServerSongData != null && deleteData.ServerSongData.songId == data.ServerSongData.songId));

                if (data.Pointer != null)
                {
                    AddPlaybackHistory(PlaybackHistoryType.Delete, data);
                    OnQueueChange?.Invoke();
                }

                DeleteFromQueue(data.ServerSongData.id);
            }

            //Add server tracks.
            foreach (ServerCompareData addData in addDatas)
            {
                ServerSong temp = addData.ServerSongData;

                AddToQueueList(new TrackQueueData(temp, GetMusicTrack(temp.songId), _currentPlayerData.Id == temp.playerId));
            }

            OnQueueChange?.Invoke();
        }

        public MusicTrack GetMusicTrack(string musicTrackId)
        {
            List<MusicTrack> tracks = AudioManager.Instance.GetMusicList("jukebox");

            return tracks.Find(track => track.Id == musicTrackId);
        }

        /// <summary>
        /// Used to update clan playlist.
        /// </summary>
        /// <returns></returns>
        private IEnumerator ServerPlaylistFetchLoop()
        {
            bool? success = null;
            float timer = 0f;

            while (timer < _playlistServerFetchFrequency)
            {
                yield return null;
                timer += Time.deltaTime;
            }

            StartCoroutine(UpdateLocalClanPlaylist((successData) => success = successData, null));

            yield return new WaitUntil(() => (success != null));

            if (success != null && !success.Value) yield break;

            _playlistServerFetchCoroutine = StartCoroutine(ServerPlaylistFetchLoop());
        }
        #endregion

        #region Playback
        //public void SetLooping(bool looping, bool loopOne) { _loopLastTrack = looping; }

        private IEnumerator PlayServerTrack(ServerCurrentSong serverCurrentSong)
        {
            yield return new WaitUntil(() => _currentPlaylist != null);

            if (serverCurrentSong == null)
            {
                if (!_trackPreviewActive)
                {
                    OnStopJukeboxVisuals?.Invoke();
                    OnClearJukeboxVisuals?.Invoke();

                    AudioManager.Instance.PlayFallBackTrack(MusicHandler.MusicSwitchType.CrossFade);
                }

                _currentTrackQueueData = null;

                yield break;
            }

            System.DateTime gmtTime = System.DateTimeOffset.FromUnixTimeMilliseconds(serverCurrentSong.startedAt).DateTime;
            System.DateTime localTime = gmtTime.ToLocalTime();

            bool timeCloseEnough = Mathf.Abs(localTime.Ticks - _musicTrackStartTime.Ticks) <= _trackTimeErrorMargin;

            _musicTrackStartTime = localTime;

            if (_currentTrackQueueData is { ServerSongData: not null } && serverCurrentSong.songId == _currentTrackQueueData.ServerSongData.songId)
            {
                ContinueTrack(false, !timeCloseEnough);
            }
            else
            {
                MusicTrack musicTrack = GetMusicTrack(serverCurrentSong.songId);

                if (musicTrack == null)
                {
                    Debug.LogError("Could not find jukebox music track by song id: " + serverCurrentSong.songId);
                    yield break;
                }

                _currentTrackQueueData = new TrackQueueData(new ServerSong(serverCurrentSong), musicTrack);

                ContinueTrack(false, !timeCloseEnough);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string TryPlayTrack(bool useAnimations = true)
        {
            if (_jukeboxMuted) return null;

            if (_currentTrackQueueData != null)
                return ContinueTrack(false, false, useAnimations);
            if (_currentPlaylist != null && _currentPlaylist.Type != PlaylistType.Clan)
                return PlayNextJukeboxTrack();

            OnSetSongInfo?.Invoke(null, useAnimations);

            return null;
        }

        /// <summary>
        /// Plays the current track.
        /// </summary>
        /// <returns>Track name that is playing.</returns>
        public string PlayTrack() { return PlayTrack(_currentTrackQueueData, false); }

        /// <summary>
        /// Plays the track found in given <c>TrackQueueData</c>.
        /// </summary>
        /// <returns>Track name that is playing.</returns>
        public string PlayTrack(TrackQueueData trackQueueData, bool forcePlay)
        {
            if (!forcePlay && PlayTrackBlockingCheck(trackQueueData) || _trackPreviewActive) return null;

            string trackName = "";

            _currentTrackQueueData = trackQueueData;

            if (_jukeboxMuted)
            {
                trackName = trackQueueData.MusicTrack.Name;
            }
            else
            {
                trackName = AudioManager.Instance.PlayMusic("Jukebox", trackQueueData.MusicTrack);

                if (string.IsNullOrEmpty(trackName))
                {
                    Debug.LogWarning("Failed to play jukebox music track!");
                    return null;
                }

                _playbackPaused = false;
            }

            OnSetSongInfo?.Invoke(trackQueueData.MusicTrack);

            _musicElapsedTime = 0f;

            if (_trackEndingControlCoroutine != null) StopCoroutine( _trackEndingControlCoroutine);

            _trackEndingControlCoroutine = StartCoroutine(TrackEndingControl());

            return trackName;
        }

        private bool PlayTrackBlockingCheck(TrackQueueData trackQueueData)
        {
            return trackQueueData == null || _trackEndingControlCoroutine != null || (_playbackPaused && _currentTrackQueueData != null);
        }

        /// <returns><c>True</c> if paused.</returns>
        public bool PlaybackToggle(bool muteActivation)
        {
            if (muteActivation)
            {
                _jukeboxMuted = !_jukeboxMuted;

                if (_jukeboxMuted && _trackPreviewActive) StopMusicPreview();
            }
            else
                _playbackPaused = !_playbackPaused;

            if (!_playbackPaused && !_jukeboxMuted)
            {
                if (_currentTrackQueueData != null)
                    ContinueTrack(muteActivation);
                else
                    PlayTrack();
            }
            else
                AudioManager.Instance.PlayFallBackTrack(MusicHandler.MusicSwitchType.CrossFade);

            if (_playbackPaused) StopJukebox();

            return (muteActivation ? _jukeboxMuted : _playbackPaused);
        }

        public string ContinueTrack(bool muteActivation, bool forcePlay = false, bool playAnimations = true)
        {
            if (_trackPreviewActive || _currentPlaylist == null) return null;

            if (_playbackPaused) _playbackPaused = false;

            if (_currentPlaylist.Type == PlaylistType.Clan || muteActivation)
            {
                if (_currentTrackQueueData == null || !_currentTrackQueueData.InUse())
                {
                    OnStopJukeboxVisuals?.Invoke();
                    OnClearJukeboxVisuals?.Invoke();

                    return null;
                }

                _musicElapsedTime = (float)System.DateTime.Now.Subtract(_musicTrackStartTime).TotalMilliseconds / 1000f;
            }

            if (_trackEndingControlCoroutine != null) StopCoroutine(_trackEndingControlCoroutine);

            _trackEndingControlCoroutine = StartCoroutine(TrackEndingControl(playAnimations));

            if (_currentTrackQueueData == null) return null;

            OnSetSongInfo?.Invoke(_currentTrackQueueData.MusicTrack);

            if (!_jukeboxMuted)
                return AudioManager.Instance.ContinueMusic("Jukebox", _currentTrackQueueData.MusicTrack,
                    MusicHandler.MusicSwitchType.CrossFade, _musicElapsedTime, forcePlay);

            return null;
        }

        public void StopJukebox()
        {

        }

        private IEnumerator TrackEndingControl(bool playAnimations = true)
        {
            if (_currentTrackQueueData == null || !_currentTrackQueueData.InUse()) yield break;

            while (true)
            {
                if (_currentTrackQueueData == null || !_currentTrackQueueData.InUse() ||
                    _musicElapsedTime >= _currentTrackQueueData.MusicTrack.Music.length) break;

                if (!_trackPreviewActive && OnSetVisibleElapsedTime != null)
                    OnSetVisibleElapsedTime.Invoke(_currentTrackQueueData.MusicTrack.Music.length, _musicElapsedTime,
                        PreviewLocationType.Main, playAnimations);

                yield return null;
                _musicElapsedTime += Time.deltaTime;
            }

            yield return new WaitUntil(() => _serverOperationAvailable);

            if (_currentPlaylist.Type != PlaylistType.Clan)
                PlayNextJukeboxTrack();
            else
                UpdateLocalClanPlaylist();
        }

        private string PlayNextJukeboxTrack()
        {
            string trackName = null;

            if (_trackEndingControlCoroutine != null)
            {
                StopCoroutine(_trackEndingControlCoroutine);
                _trackEndingControlCoroutine = null;
            }

            if (_loopPlayType == PlaylistLoopType.LoopOne)
            {
                trackName = PlayTrack(_currentTrackQueueData, false);
            }
            else if (_trackQueue.Count != 0) //Play the next track in queue.
            {
                // Play next track.
                trackName = PlayTrack(_trackQueue[0], false);

                if (trackName == null) return null;

                // Hide current tracks visual part in JukeboxMusicPlayerHandler.
                if (_currentTrackQueueData != null)
                {
                    AddPlaybackHistory(PlaybackHistoryType.Hide, _currentTrackQueueData);
                    OnQueueChange?.Invoke();
                }
            }
            else //Go back to latest requested music in AudioManager.
            {
                AudioManager manager = AudioManager.Instance;
                _currentTrackQueueData = null;
                StopJukebox();

                if (!string.IsNullOrEmpty(manager.FallbackMusicCategory))
                    manager.PlayMusic(manager.FallbackMusicCategory, manager.FallbackMusicTrack);

                OnStopJukeboxVisuals?.Invoke();
                OnClearJukeboxVisuals?.Invoke();
            }

            return _jukeboxMuted ? null : trackName;
        }
        #endregion

        #region Playback History
        public void AddPlaybackHistory(PlaybackHistoryType type, TrackQueueData trackQueueHandler)
        {
            AddPlaybackHistory(type, trackQueueHandler, null);
        }

        public void AddPlaybackHistory(PlaybackHistoryType type, TrackQueueData trackQueueHandler1, TrackQueueData trackQueueHandler2)
        {
            if (trackQueueHandler1 == null)
            {
                Debug.LogError(type + ", operation failed due to trackQueueHandler1 being null!");
                return;
            }

            if (!TryToRemovePlaybackHistory(type, trackQueueHandler1)) _playbackHistory.Add(new(type, trackQueueHandler1));

            LogPlaybackLastUpdate();
        }

        private bool TryToRemovePlaybackHistory(PlaybackHistoryType type, TrackQueueData trackQueueHandler)
        {
            PlaybackHistory historyData = null;

            // Try finding existing playback history.
            for (int i = 0; i < _playbackHistory.Count; i++)
                if ((_playbackHistory[i].Target1.TrackQueueData.ServerSongData.id == trackQueueHandler.ServerSongData.id))
                {
                    historyData = _playbackHistory[i];
                    break;
                }

            if (historyData != null && ValidForPlaybackHistoryRemoval(type, historyData.PlaybackHistoryType))
            {
                _playbackHistory.Remove(historyData);
                return true;
            }

            return false;
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
            StartCoroutine(AddToServerSongQueue(musicTrack, false));
        }

        private IEnumerator AddToServerSongQueue(MusicTrack musicTrack, bool dontSendToServer)
        {
            if (!_serverOperationAvailable) yield break;

            bool? done = null;

            StartCoroutine(AddTrackToServer((data) => done = data, musicTrack));

            yield return new WaitUntil(() => done != null);

            UpdateLocalClanPlaylist();
        }

        private void AddToQueueList(TrackQueueData trackQueueData)
        {
            trackQueueData.LinearIndex = _trackQueue.Count;
            _trackQueue.Add(trackQueueData);
            AddPlaybackHistory(PlaybackHistoryType.Add, trackQueueData);
            OnQueueChange?.Invoke();
        }

        private IEnumerator InsertToLastOnQueueList(MusicTrack musicTrack/*, string trackQueueDataId*/)
        {
            bool? done = null;

            StartCoroutine(AddTrackToServer((data) => done = data, musicTrack));

            yield return new WaitUntil(() => done != null);

            UpdateLocalClanPlaylist();
        }

        public void DeleteFromQueue(int linearIndex, bool updateServer)
        {
            TrackQueueData data = _trackQueue[linearIndex];

            if (updateServer) StartCoroutine(DeleteTrackFromServer(null, data.ServerSongData.id, data.MusicTrack.Name));

            _trackQueue[linearIndex].Clear();
        }

        public void DeleteFromQueue(string uniqueId)
        {
            _trackQueue.Find((data) => data.ServerSongData != null && uniqueId == data.ServerSongData.id)?.Clear();
        }

        /// <summary>
        /// Compacts the <c>_trackQueue</c>.
        /// <br/>Note: Called from <c>JukeboxMusicPlayerHandler</c>.
        /// </summary>
        public void OptimizeTrackQueue()
        {
            Queue<int> freeIndexes = new();

            for (int i = 0; i < _trackQueue.Count; i++)
            {
                if (_trackQueue[i].InUse() && freeIndexes.Count != 0)
                {
                    JukeboxTrackQueueHandler handler = OnGetTrackQueueHandler?.Invoke(_trackQueue[i].Pointer);
                    int linearIndex = freeIndexes.Dequeue();

                    if (!handler)
                    {
                        Debug.LogError("Failed to get JukeboxTrackQueueHandler.");
                        return;
                    }

                    _trackQueue[linearIndex].SetData(_trackQueue[i], linearIndex);
                    handler.SetLinearIndex(linearIndex);
                    _trackQueue[i].Clear();
                    freeIndexes.Enqueue(i);
                }
                else if (!_trackQueue[i].InUse())
                    freeIndexes.Enqueue(i);
            }

            ClearPlaybackHistory();
            LogPlaybackLastUpdate();
        }
        #endregion

        #region Preview
        public bool PlayPreview(JukeboxTrackButtonHandler buttonHandler, PreviewLocationType type, float previewDuration = -1f)
        {
            if (!PlayPreview(buttonHandler.MusicTrack, type, previewDuration)) return false;

            buttonHandler.StartDiskSpin();
            _currentPreviewMusicTrackHandler = buttonHandler;

            return true;
        }

        public bool PlayPreview(MusicTrack musicTrack, PreviewLocationType type, float previewDuration = -1f)
        {
            if (_currentPreviewMusicTrackHandler)
            {
                _currentPreviewMusicTrackHandler.StopDiskSpin();
                _currentPreviewMusicTrackHandler = null;
            }

            if (_currentPreviewTrackCoroutine != null)
            {
                StopCoroutine(_currentPreviewTrackCoroutine);
                _currentPreviewTrackCoroutine = null;
            }

            string trackName = AudioManager.Instance.ContinueMusic("Jukebox", musicTrack,
                MusicHandler.MusicSwitchType.CrossFade, 0f, true);

            if (trackName == null)
            {
                Debug.LogWarning("Failed to start music preview playback.");
                return false;
            }

            OnJukeboxMute?.Invoke(false);

            _jukeboxMuted = false;
            _trackPreviewActive = true;

            OnSetSongInfo?.Invoke(musicTrack);
            OnPreviewStart?.Invoke();

            if (previewDuration == -1f) previewDuration = _previewTime;

            _currentPreviewMusicTrack = musicTrack;
            _currentPreviewTrackCoroutine = StartCoroutine(MusicPreviewPlaybackControl(previewDuration, type));

            return true;
        }

        private IEnumerator MusicPreviewPlaybackControl(float previewTime, PreviewLocationType type)
        {
            float timer = 0f;

            while (timer < previewTime)
            {
                if (OnSetVisibleElapsedTime != null) OnSetVisibleElapsedTime.Invoke(_currentPreviewMusicTrack.Music.length, timer, type);

                yield return null;
                timer += Time.deltaTime;
            }

            StopMusicPreview();
        }

        public void StopMusicPreview()
        {
            if (_currentPreviewTrackCoroutine != null)
            {
                StopCoroutine(_currentPreviewTrackCoroutine);
                _currentPreviewTrackCoroutine = null;
            }

            if (_currentPreviewMusicTrackHandler)
            {
                _currentPreviewMusicTrackHandler.StopDiskSpin();
                _currentPreviewMusicTrackHandler = null;
            }

            _currentPreviewMusicTrack = null;

            OnPreviewEnd?.Invoke();

            _trackPreviewActive = false;

            if (_currentTrackQueueData == null)
            {
                AudioManager.Instance.PlayFallBackTrack(MusicHandler.MusicSwitchType.CrossFade);

                OnStopJukeboxVisuals?.Invoke();
                OnClearJukeboxVisuals?.Invoke();

                return;
            }

            if (ContinueTrack(false, true) == null)
            {
                Debug.LogWarning("Failed to continue track playback.");
                return;
            }

            OnSetSongInfo?.Invoke(_currentTrackQueueData.MusicTrack);

        }
        #endregion

        public void InvokeOnMusicTrackInfoPressed(MusicTrack musicTrack, MusicTrackFavoriteType likeType)
        {
            OnMusicTrackInfoPressed?.Invoke(musicTrack, likeType);
        }

        private class ServerCompareData
        {
            public ServerSong ServerSongData;
            public int Index;

            public ServerCompareData(ServerSong serverSong, int index)
            {
                ServerSongData = serverSong;
                Index = index;
            }
        }

        public class MusicTrackFavoriteData
        {
            public string MusicTrackId;
            public MusicTrackFavoriteType FavoriteType;

            public MusicTrackFavoriteData(string musicTrackId, MusicTrackFavoriteType likeType)
            {
                MusicTrackId = musicTrackId;
                FavoriteType = likeType;
            }
        }
    }

    public class TrackQueueData
    {
        public ServerSong ServerSongData;
        public int LinearIndex;
        public ChunkPointer Pointer;
        public MusicTrack MusicTrack;
        public bool UserOwned;
        public JukeboxManager.MusicTrackFavoriteType FavoriteType;

        public TrackQueueData(ServerSong serverSong, MusicTrack musicTrack)
        {
            SetData(serverSong, -1, null, musicTrack, false, JukeboxManager.MusicTrackFavoriteType.Neutral);
        }

        public TrackQueueData(ServerSong serverSong, MusicTrack musicTrack, bool userOwned)
        {
            SetData(serverSong, -1, null, musicTrack, userOwned, JukeboxManager.MusicTrackFavoriteType.Neutral);
        }

        public TrackQueueData(ServerSong serverSong, int linearIndex, ChunkPointer pointer, MusicTrack track, bool userOwned, JukeboxManager.MusicTrackFavoriteType favoriteType)
        {
            SetData(serverSong, linearIndex, pointer, track, userOwned, favoriteType);
        }

        public bool InUse() { return ServerSongData != null; }

        public void SetData(TrackQueueData data, int linearIndex)
        {
            SetData(data.ServerSongData, linearIndex, data.Pointer, data.MusicTrack, data.UserOwned, data.FavoriteType);
        }

        public void SetData(ServerSong serverSong, int linearIndex, ChunkPointer pointer, MusicTrack track, bool userOwned, JukeboxManager.MusicTrackFavoriteType favoriteType)
        {
            ServerSongData = serverSong;
            LinearIndex = linearIndex;
            Pointer = pointer;
            MusicTrack = track;
            UserOwned = userOwned;
            FavoriteType = favoriteType;
        }

        public void SetFavoriteData(JukeboxManager.MusicTrackFavoriteType favoriteType) { FavoriteType = favoriteType; }

        public void Clear()
        {
            ServerSongData = null;
            LinearIndex = -1;

            if (Pointer != null) Pointer.Delete();

            Pointer = null;
            MusicTrack = null;
        }
    }

    public class Playlist
    {
        public string Name;
        public JukeboxManager.PlaylistType Type;
        public List<ServerSong> PackedTrackQueueDatas; //Format: UserId_MusicTrackId_LocalPlaylistTrackId

        public Playlist(string name, JukeboxManager.PlaylistType type, List<ServerSong> serverMusicTracks)
        {
            Name = name;
            Type = type;
            PackedTrackQueueDatas = serverMusicTracks;
        }

        public Playlist(string name, JukeboxManager.PlaylistType type, ServerPlaylist serverMusicTracks)
        {
            Name = name;
            Type = type;
            PackedTrackQueueDatas = serverMusicTracks.songQueue.ToList();
        }
    }

    public class PlaybackHistory
    {
        public JukeboxManager.PlaybackHistoryType PlaybackHistoryType {get; private set;}

        public PlaybackHistoryDataCollection Target1;
        public PlaybackHistoryDataCollection Target2;

        /// <summary>
        /// Used for delete only!
        /// </summary>
        public PlaybackHistory(TrackQueueData trackQueueData, int chunkIndex, int poolIndex)
        {
            PlaybackHistoryType = JukeboxManager.PlaybackHistoryType.Delete;
            Target1 = new(trackQueueData, chunkIndex, poolIndex);
        }

        public PlaybackHistory(JukeboxManager.PlaybackHistoryType type, TrackQueueData trackQueue)
        {
            PlaybackHistoryType = type;
            Target1 = new PlaybackHistoryDataCollection(trackQueue);
        }

        /// <summary>
        /// Swap
        /// </summary>
        public PlaybackHistory(JukeboxManager.PlaybackHistoryType type, TrackQueueData trackQueueTarget1, TrackQueueData trackQueueTarget2)
        {
            PlaybackHistoryType = type;
            Target1 = new PlaybackHistoryDataCollection(trackQueueTarget1);
            Target2 = new PlaybackHistoryDataCollection(trackQueueTarget2);
        }

        public struct PlaybackHistoryDataCollection
        {
            public TrackQueueData TrackQueueData;
            public int LinearIndex;

            // Delete
            public int ChunkIndex;
            public int PoolIndex;

            public PlaybackHistoryDataCollection(TrackQueueData trackQueueData)
            {
                TrackQueueData = trackQueueData;
                LinearIndex = trackQueueData.LinearIndex;
                ChunkIndex = trackQueueData.Pointer?.ChunkIndex ?? -1;
                PoolIndex = trackQueueData.Pointer?.PoolIndex ?? -1;
            }

            // Delete
            public PlaybackHistoryDataCollection(TrackQueueData trackQueueData, int chunkIndex, int poolIndex)
            {
                TrackQueueData = trackQueueData;
                LinearIndex = trackQueueData.LinearIndex;
                ChunkIndex = chunkIndex;
                PoolIndex = poolIndex;
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

    public class PersonalizedMusicTrack
    {
        private MusicTrack _track;
        private JukeboxManager.MusicTrackFavoriteType _favoriteType;

        public MusicTrack Track { get { return _track; } }
        public JukeboxManager.MusicTrackFavoriteType FavoriteType  { get { return _favoriteType; } }

        public PersonalizedMusicTrack(MusicTrack track, JukeboxManager.MusicTrackFavoriteType favoriteType)
        {
            _track = track;
            _favoriteType = favoriteType;
        }
    }
}
