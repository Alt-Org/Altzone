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
        public static JukeboxManager Instance { get; private set; }

        private PlayerData _currentPlayerData = null;
        public PlayerData CurrentPlayerData { get { return _currentPlayerData; } }

        #region Favorite
        private string _favoritesPrefsString = "favoriteTracks";

        private List<MusicTrackFavoriteData> _musicTrackFavorites = new List<MusicTrackFavoriteData>();
        #endregion

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

        public delegate void FavoriteButtonChange(string musicTrackId, MusicTrackFavoriteType favoriteType);
        public event FavoriteButtonChange OnFavoriteButtonChange;
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

            ServerPlaylist playlistData = null;
            bool? timeoutCallback = null;

            StartCoroutine(GetClanPlaylist((data) => timeoutCallback = data, (data) => playlistData = data));

            yield return new WaitUntil(() => (playlistData != null || timeoutCallback != null));

            if (timeoutCallback != null) yield break;

            _currentPlaylist = new("Klaani", PlaylistType.Clan, playlistData);
            //_currentPlaylist = new("Klaani", PlaylistType.Clan, playlistData/*TESTCreateServerMusicList()*/); //Offline testing

            _musicTrackFavorites = GetFavoriteDatas();

            CreateTrackQueueFromCurrentPlaylist();
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

        #region Favorite
        public List<MusicTrackFavoriteData> GetFavoriteDatas() //TODO: Add a check for new tracks that come from jukebox music list.
        {
            string rawData = PlayerPrefs.GetString(_favoritesPrefsString); //Format: TrackId_LikeEnumInt-TrackId_LikeEnumInt-TrackId_LikeEnumInt
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
                favoriteDatas.Add(new(trackData[0], (MusicTrackFavoriteType)(int.Parse(trackData[1]))));
            }

            //Check if there are new tracks to be added.
            foreach (MusicTrack track in musicTracks)
                if (favoriteDatas.FindIndex((data) => track.Id == data.MusicTrackId) == -1)
                    favoriteDatas.Add(new(track.Id, MusicTrackFavoriteType.Neutral));

            return favoriteDatas;
        }

        public void SaveFavoriteDatas(List<MusicTrackFavoriteData> favoriteDatas)
        {
            string sendTarget = "";

            for (int i = 0; i < favoriteDatas.Count; i++)
            {
                sendTarget += favoriteDatas[i].MusicTrackId + "_" + (int)favoriteDatas[i].FavoriteType;

                if (i + 1 < favoriteDatas.Count)
                    sendTarget += "-";
            }

            PlayerPrefs.SetString(_favoritesPrefsString, sendTarget);
        }

        public MusicTrackFavoriteType GetTrackFavoriteType(MusicTrack musicTrack)
        {
            MusicTrackFavoriteData data = _musicTrackFavorites.Find((data) => musicTrack.Id == data.MusicTrackId);

            if (data != null) return data.FavoriteType;

            return MusicTrackFavoriteType.Neutral;
        }

        public void InvokeOnFavoriteButtonChange(string musicTrackId, MusicTrackFavoriteType favoriteType)
        {
            _musicTrackFavorites.Find((data) => musicTrackId == data.MusicTrackId).FavoriteType = favoriteType;

            SaveFavoriteDatas(_musicTrackFavorites);

            if (OnFavoriteButtonChange != null) OnFavoriteButtonChange.Invoke(musicTrackId, favoriteType);
        }

        public MusicTrack GetNotHatedMusicTrack() { return GetNotHatedMusicTrack(_currentTrackQueueData); }

        public MusicTrack GetNotHatedMusicTrack(TrackQueueData trackQueueData)
        {
            if (trackQueueData == null) return null;

            MusicTrackFavoriteData favoriteData = _musicTrackFavorites.Find((data) => trackQueueData.MusicTrack.Id == data.MusicTrackId);

            if (favoriteData != null && favoriteData.FavoriteType == MusicTrackFavoriteType.Dislike)
            {
                List<MusicTrackFavoriteData> likedDatas = _musicTrackFavorites.FindAll((data) => MusicTrackFavoriteType.Like == data.FavoriteType);

                return (GetMusicTrack(likedDatas[Random.Range(0, likedDatas.Count)].MusicTrackId));
            }

            return trackQueueData.MusicTrack;
        }
        #endregion

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

        public void SendPlaylistChangesToServer()
        {
            if (!_serverOperationAvailable || _currentPlaylist == null || _currentPlaylist != null && _currentPlaylist.Type != PlaylistType.Clan) return;

            //SaveTrackQueueToPlaylist();

            StartCoroutine(UpdateClanPlaylist());
        }

        //public bool RemoveTrackOnPlaylist(string trackName, string playlistName)
        //{
            

        //    return false;
        //}

        //public void StartPlaylistPlayback(int startIndex, PlaylistLoopType playType)
        //{

        //}

        private void SaveTrackQueueToPlaylist()
        {
            List<string> packedTrackQueueDatas = new List<string>();

            foreach (TrackQueueData track in _trackQueue)
                if (track.InUse())
                    packedTrackQueueDatas.Add(track.Id);

            _currentPlaylist.PackedTrackQueueDatas = packedTrackQueueDatas;
        }

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

        private IEnumerator UpdateClanPlaylist()
        {
            ServerPlaylist playlistData = null;
            bool? timeout = null;
            bool? callback = null;

            _serverOperationAvailable = false;

            if (_playlistServerFetchCoroutine != null) StopCoroutine(_playlistServerFetchCoroutine);

            StartCoroutine(GetClanPlaylist((data) => timeout = data, (data) => playlistData = data));

            yield return new WaitUntil(() => (playlistData != null || timeout != null));

            if (playlistData == null)
            {
                _serverOperationAvailable = true;
                yield break;
            }

            UpdateLocalPlaylist(playlistData);
            SaveTrackQueueToPlaylist();

            //StartCoroutine(ServerManager.Instance.UpdateJukeboxClanPlaylistToServer(new Playlist("clear", PlaylistType.Custom, new List<string>()), (data) => callback = data)); //Purge clan playlist.
            StartCoroutine(ServerManager.Instance.UpdateJukeboxClanPlaylistToServer(_currentPlaylist, (data) => callback = data));
            StartCoroutine(WaitUntilTimeout(_timeoutSeconds, (data) => timeout = data));

            yield return new WaitUntil(() => (callback != null || timeout != null));

            if (timeout != null || callback != null && !callback.Value)
            {
                Debug.LogError("Failed to update server clan playlist!");
                _serverOperationAvailable = true;
                yield break;
            }

            _serverOperationAvailable = true;

            _playlistServerFetchCoroutine = StartCoroutine(ServerPlaylistFetchLoop());
            Debug.Log("Clan playlist server update successful.");
        }

        private void UpdateLocalPlaylist(ServerPlaylist serverPlaylist)
        {
            List<string> foundInServer = new List<string>();
            List<ServerCompareData> deleteDatas = new List<ServerCompareData>();
            List<ServerCompareData> addDatas = new List<ServerCompareData>();

            //Filter out local users tracks.
            for (int j = 0; j < serverPlaylist.jukeboxSongs.Count; j++)
            {
                string[] datas = serverPlaylist.jukeboxSongs[j].Split('_');

                if (datas[0] == _currentPlayerData.Id) continue;

                foundInServer.Add(serverPlaylist.jukeboxSongs[j]);
            }

            //Gather all possible addable and deletable tracks.
            int i = 0;

            while (i < foundInServer.Count || i < _trackQueue.Count)
            {
                if (i < foundInServer.Count && i < _trackQueue.Count && foundInServer[i] == _trackQueue[i].Id)
                {
                    i++;
                    continue;
                }

                if (i < foundInServer.Count) //Server, look up what might be added.
                {
                    int serverIndex = _trackQueue.FindIndex((data) => foundInServer[i] == data.Id);

                    if (serverIndex == -1) addDatas.Add(new(foundInServer[i], i));
                }

                if (i < _trackQueue.Count) //Local, look up what might be deleted.
                {
                    if (_trackQueue[i].Id.Split('_')[0] == _currentPlayerData.Id || !_trackQueue[i].InUse()) //Skip owned track.
                    {
                        i++;
                        continue;
                    }

                    int localIndex = foundInServer.FindIndex((data) => _trackQueue[i].Id == data);

                    if (localIndex == -1) deleteDatas.Add(new(_trackQueue[i].Id, i));
                }

                i++;
            }

            //Final filtering before actual effecting operations.
            while (i < deleteDatas.Count || i < addDatas.Count)
            {
                if (i < deleteDatas.Count && i < addDatas.Count && deleteDatas[i].Id == addDatas[i].Id)
                {
                    deleteDatas.RemoveAt(i);
                    addDatas.RemoveAt(i);

                    //i++;
                    continue;
                }

                if (i < deleteDatas.Count && addDatas.Count != 0) //Look up what doesn't need to be added.
                {
                    int serverIndex = addDatas.FindIndex((data) => deleteDatas[i].Id == data.Id);

                    if (serverIndex == -1) addDatas.RemoveAt(i);

                    continue;
                }

                if (i < addDatas.Count && deleteDatas.Count != 0) //Look up what doesn't need to be deleted.
                {
                    int localIndex = deleteDatas.FindIndex((data) => addDatas[i].Id == data.Id);

                    if (localIndex == -1) deleteDatas.RemoveAt(i);

                    continue;
                }

                i++;
            }

            //Remove local tracks.
            foreach (ServerCompareData deleteData in deleteDatas)
            {
                AddPlaybackHistory(PlaybackHistoryType.Delete, _trackQueue[deleteData.Index]);
                DeleteFromQueue(deleteData.Index);
            }

            //Add server tracks.
            foreach (ServerCompareData addData in addDatas)
                InsertToQueueList(GetMusicTrack(addData.Id.Split('_')[1]), addData.Index, addData.Id, false);
        }

        public MusicTrack GetMusicTrack(string musicTrackId)
        {
            List<MusicTrack> tracks = AudioManager.Instance.GetMusicList("jukebox");

            foreach (MusicTrack track in tracks)
                if (track.Id == musicTrackId)
                    return track;

            return null;
        }

        /// <summary>
        /// Used to update clan playlist.
        /// </summary>
        /// <returns></returns>
        private IEnumerator ServerPlaylistFetchLoop()
        {
            ServerPlaylist playlistData = null;
            bool? timeout = null;
            float timer = 0f;
 
            while (timer < _playlistServerFetchFrequency)
            {
                yield return null;
                timer += Time.deltaTime;
            }

            StartCoroutine(GetClanPlaylist((data) => timeout = data, (data) => playlistData = data));

            yield return new WaitUntil(() => (playlistData != null || timeout != null));

            if (playlistData == null) yield break;

            UpdateLocalPlaylist(playlistData);

            _playlistServerFetchCoroutine = StartCoroutine(ServerPlaylistFetchLoop());
        }

        private List<string> TESTCreateServerMusicList() //Offline testing.
        {
            return new List<string>()
            {
                "k�ytt�j�1_In Awe",
                "k�ytt�j�1_What Are You Waiting For",
                "k�ytt�j�2_Soul",
                "k�ytt�j�2_Rush Hour",
                "k�ytt�j�3_Falling Bird",
                "k�ytt�j�2_Plan",
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

            //MusicTrack validMusicTrack = GetNotHatedMusicTrack(trackQueueData);

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

            if (_trackQueuePointer >= _trackQueue.Count) _trackQueuePointer = 0;

            if (_currentPlaylist.Type == PlaylistType.Clan || muteActivation)
            {
                float seconds = (float)System.DateTime.Now.Subtract(_musicTrackStartTime).TotalMilliseconds / 1000f;

                while (true)
                {
                    if (!_trackQueue[_trackQueuePointer].InUse()) TryFindValidQueueData();
                    if (seconds < _trackQueue[_trackQueuePointer].MusicTrack.Music.length) break;

                    seconds -= _trackQueue[_trackQueuePointer].MusicTrack.Music.length;

                    _trackQueuePointer++;
                    if (_trackQueuePointer >= _trackQueue.Count) _trackQueuePointer = 0;

                    if (!_trackQueue[_trackQueuePointer].InUse()) TryFindValidQueueData();

                }

                _musicElapsedTime = seconds;
            }

            if (_trackEndingControlCoroutine != null) StopCoroutine(_trackEndingControlCoroutine);

            _trackEndingControlCoroutine = StartCoroutine(TrackEndingControl());
            //OnSetPlayButtonImages?.Invoke(true);

            //MusicTrack musicTrack = GetNotHatedMusicTrack();

            if (OnSetSongInfo != null) OnSetSongInfo.Invoke(_currentTrackQueueData.MusicTrack);

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
            if (_currentTrackQueueData == null || !_currentTrackQueueData.InUse()) yield break;
            Debug.LogError($"Track length: {_currentTrackQueueData.MusicTrack.Music.length}, start time: {_musicElapsedTime}");
            while (true)
            {
                if (_currentTrackQueueData == null || !_currentTrackQueueData.InUse() || _musicElapsedTime >= _currentTrackQueueData.MusicTrack.Music.length) break;

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
        public void AddPlaybackHistory(PlaybackHistoryType type, TrackQueueData trackQueueHandler)
        {
            AddPlaybackHistory(type, trackQueueHandler, null);
        }

        public void AddPlaybackHistory(PlaybackHistoryType type, TrackQueueData trackQueueHandler1, TrackQueueData trackQueueHandler2)
        {
            if (!TryToRemovePlaybackHistory(type, trackQueueHandler1)) _playbackHistory.Add(new(type, trackQueueHandler1));

            LogPlaybackLastUpdate();
        }

        private bool TryToRemovePlaybackHistory(PlaybackHistoryType type, TrackQueueData trackQueueHandler)
        {
            PlaybackHistory historyData = null;

            // Try finding existing playback history.
            for (int i = 0; i < _playbackHistory.Count; i++)
                if ((_playbackHistory[i].Target1.TrackQueueData.Id == trackQueueHandler.Id))
                {
                    historyData = _playbackHistory[i];
                    break;
                }

            if (historyData != null && ValidForPlaybackHistoryRemoval(type, historyData._playbackHistoryType))
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
            if (_trackQueuePointer != 0 && _currentTrackQueueData != null)
                InsertToLastOnQueueList(musicTrack, CreateLocalUserTrackQueueId(musicTrack.Id));
            else if (_currentTrackQueueData != null)
                AddToQueueList(musicTrack);
            else
            {
                AddToQueueList(musicTrack);
                PlayNextJukeboxTrack();
            }
        }

        private void CreateTrackQueueFromCurrentPlaylist()
        {
            foreach (TrackQueueData trackQueueData in _currentPlaylist.GetTrackQueueList(_currentPlayerData.Id))
            {
                if (OnGetFreeJukeboxTrackQueueHandler != null) trackQueueData.Pointer = OnGetFreeJukeboxTrackQueueHandler.Invoke();

                trackQueueData.SetFavoriteData(GetTrackFavoriteType(trackQueueData.MusicTrack));
                AddToQueueList(trackQueueData);
            }

            _localTrackId = _currentPlaylist.PackedTrackQueueDatas.Count;
        }

        private void AddToQueueList(MusicTrack musicTrack)
        {
            AddToQueueList(new TrackQueueData(CreateLocalUserTrackQueueId(musicTrack.Id), _trackQueue.Count, null, musicTrack, true, GetTrackFavoriteType(musicTrack)));
        }

        private void AddToQueueList(TrackQueueData trackQueueData)
        {
            _trackQueue.Add(trackQueueData);
            AddPlaybackHistory(PlaybackHistoryType.Add, trackQueueData);
            SendPlaylistChangesToServer();

            if (OnQueueChange != null) OnQueueChange.Invoke();
        }

        private void InsertToLastOnQueueList(MusicTrack musicTrack, string trackQueueDataId)
        {
            int linearIndex = _trackQueuePointer - 1;

            InsertToQueueList(musicTrack, linearIndex, trackQueueDataId, true);
        }

        private void InsertToQueueList(MusicTrack musicTrack, int insertIndex, string trackQueueDataId, bool addTypeOverride)
        {
            int skippedAmount = 0;
            TrackQueueData trackQueueData = new(trackQueueDataId, insertIndex, null, musicTrack, true, GetTrackFavoriteType(musicTrack));

            _trackQueue.Insert(insertIndex, trackQueueData);

            if (addTypeOverride)
                AddPlaybackHistory(PlaybackHistoryType.Add, trackQueueData);
            else
            {
                AddPlaybackHistory(PlaybackHistoryType.Insert, trackQueueData);
                _trackQueuePointer++;
            }

            SendPlaylistChangesToServer();

            // Update every TrackQueueData and JukeboxTrackQueueHandler linear index that is ahead of the inserted TrackQueueData.
            for (int i = insertIndex + 1; i < _trackQueue.Count; i++)
            {
                _trackQueue[i].LinearIndex = i;

                if (_trackQueue[i].InUse() && OnGetTrackQueueHandler != null)
                    OnGetTrackQueueHandler(_trackQueue[i].Pointer).SetLinearIndex(i - skippedAmount);
                else
                    skippedAmount++;
            }

            if (OnQueueChange != null) OnQueueChange.Invoke();
        }

        public void DeleteFromQueue(int linearIndex)
        {
            _trackQueue[linearIndex].Clear();
            //Debug.LogError("delete----------------");
            if (_serverOperationAvailable) SendPlaylistChangesToServer();
        }

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

        private string CreateLocalUserTrackQueueId(string musicTrackId)
        {
            // LocalTrackId is supposed to be always unique so that TrackQueueData can be connected to the PlaybackHistory data operations.
            _localTrackId++;
            // PlayerId is used to see witch track can be removed by the local user in clan playlist.
            return $"{_currentPlayerData.Id}_{musicTrackId}_{_localTrackId}"; // PlayerId _ MusicTrackName _ LocalMusicTrackId
        }

        private class ServerCompareData
        {
            public string Id;
            public int Index;

            public ServerCompareData(string id, int index)
            {
                Id = id;
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
        public string Id; //Fromat: UserId_MusicTrackId_LocalPlaylistTrackId
        public int LinearIndex;
        public ChunkPointer Pointer;
        public MusicTrack MusicTrack;
        public bool UserOwned;
        public JukeboxManager.MusicTrackFavoriteType FavoriteType;

        public TrackQueueData(string id, int linearIndex, ChunkPointer pointer, MusicTrack track, bool userOwned, JukeboxManager.MusicTrackFavoriteType favoriteType)
        {
            SetData(id, linearIndex, pointer, track, userOwned, favoriteType);
        }

        public bool InUse() { return !string.IsNullOrEmpty(Id); }

        public void SetData(TrackQueueData data, int linearIndex)
        {
            SetData(data.Id, linearIndex, data.Pointer, data.MusicTrack, data.UserOwned, data.FavoriteType);
        }

        public void SetData(string id, int linearIndex, ChunkPointer pointer, MusicTrack track, bool userOwned, JukeboxManager.MusicTrackFavoriteType favoriteType)
        {
            Id = id;
            LinearIndex = linearIndex;
            Pointer = pointer;
            MusicTrack = track;
            UserOwned = userOwned;
            FavoriteType = favoriteType;
        }

        public void SetFavoriteData(JukeboxManager.MusicTrackFavoriteType favoriteType) { FavoriteType = favoriteType; }

        public void Clear()
        {
            Id = "";
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
        public List<string> PackedTrackQueueDatas; //Fromat: UserId_MusicTrackId_LocalPlaylistTrackId

        public Playlist(string name, JukeboxManager.PlaylistType type, List<string> serverMusicTracks)
        {
            Name = name;
            Type = type;
            PackedTrackQueueDatas = serverMusicTracks;
        }

        public Playlist(string name, JukeboxManager.PlaylistType type, ServerPlaylist serverMusicTracks)
        {
            Name = name;
            Type = type;
            PackedTrackQueueDatas = serverMusicTracks.jukeboxSongs.ToList();
        }

        public List<TrackQueueData> GetTrackQueueList(string localUserId)
        {
            List<TrackQueueData> queueHandlers = new();
            List<MusicTrack> musicTracks = new(AudioManager.Instance.GetMusicList("Jukebox"));
            //Debug.LogError(PackedTrackQueueDatas.Count);
            for (int i = 0; i < PackedTrackQueueDatas.Count; i++)
            {
                string[] trackQueuePackedData = PackedTrackQueueDatas[i].Split('_');

                string userId = trackQueuePackedData[0];
                string musicTrackId = trackQueuePackedData[1];
                string localPlaylistTrackId = trackQueuePackedData[2];

                //Debug.LogError("userId: " + userId + ", musicId: " + musicTrackId);
                foreach (MusicTrack track in musicTracks)
                    if (track.Id == musicTrackId)
                    {
                        queueHandlers.Add(new(PackedTrackQueueDatas[i], i, null, track, (userId.ToLower() == localUserId.ToLower()), JukeboxManager.MusicTrackFavoriteType.Neutral));
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

        /// <summary>
        /// Used for delete only!
        /// </summary>
        public PlaybackHistory(TrackQueueData trackQueueData, int chunkIndex, int poolIndex)
        {
            _playbackHistoryType = JukeboxManager.PlaybackHistoryType.Delete;
            Target1 = new(trackQueueData, chunkIndex, poolIndex);
        }

        public PlaybackHistory(JukeboxManager.PlaybackHistoryType type, TrackQueueData trackQueue)
        {
            //int chunkIndex = (trackQueue.Pointer != null ? trackQueue.Pointer.ChunkIndex: -1);
            //int poolIndex = (trackQueue.Pointer != null ? trackQueue.Pointer.PoolIndex: -1);

            _playbackHistoryType = type;
            Target1 = new(trackQueue);
        }

        /// <summary>
        /// Swap
        /// </summary>
        public PlaybackHistory(JukeboxManager.PlaybackHistoryType type, TrackQueueData trackQueueTarget1, TrackQueueData trackQueueTarget2)
        {
            //int chunkIndex1 = (trackQueueTarget1.Pointer != null ? trackQueueTarget1.Pointer.ChunkIndex : -1);
            //int poolIndex1 = (trackQueueTarget1.Pointer != null ? trackQueueTarget1.Pointer.PoolIndex : -1);

            //int chunkIndex2 = (trackQueueTarget2 != null && trackQueueTarget2.Pointer != null ? trackQueueTarget2.Pointer.ChunkIndex : -1);
            //int poolIndex2 = (trackQueueTarget2 != null && trackQueueTarget2.Pointer != null ? trackQueueTarget2.Pointer.PoolIndex : -1);

            _playbackHistoryType = type;
            Target1 = new(trackQueueTarget1/*, chunkIndex1, poolIndex1*/);
            Target2 = new(trackQueueTarget2/*, chunkIndex2, poolIndex2*/);
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
                ChunkIndex = -1;
                PoolIndex = -1;
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
}
