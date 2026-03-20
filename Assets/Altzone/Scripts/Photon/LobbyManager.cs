using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.SceneManagement;
using Assert = UnityEngine.Assertions.Assert;
using Random = UnityEngine.Random;

using Photon.Client;
using Photon.Realtime;
using Quantum;

using Prg.Scripts.Common.PubSub;

using Altzone.Scripts.Config;
using Altzone.Scripts.Settings;
using Altzone.Scripts.Common;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.ModelV2;
using Altzone.Scripts.Battle.Photon;
using Altzone.Scripts.Lobby.Wrappers;
using Altzone.Scripts.Window;
using Altzone.Scripts.Audio;
using Altzone.Scripts.AzDebug;
using Altzone.PhotonSerializer;

using Battle.QSimulation.Game;
using PlayerType = Battle.QSimulation.Game.BattleParameters.PlayerType;

namespace Altzone.Scripts.Lobby
{
    public enum LobbyWindowTarget
    {
        None,
        MainMenu,
        LobbyRoom,
        BattleLoad,
        Battle,
        BattleStory,
        Raid
    }

    /// <summary>
    /// Manages local player position and setup in a room.
    /// </summary>
    /// <remarks>
    /// Game settings are saved in player custom properties for each participating player.
    /// </remarks>
    public class LobbyManager : MonoBehaviour, ILobbyCallbacks, IMatchmakingCallbacks, IOnEventCallback, IInRoomCallbacks, IConnectionCallbacks
    {
        private const string BattleID = PhotonBattleRoom.BattleID;

        private const string PlayerPositionKey = PhotonBattleRoom.PlayerPositionKey;
        private const string PlayerCountKey = PhotonBattleRoom.PlayerCountKey;
        private const int PlayerPositionGuest = PhotonBattleRoom.PlayerPositionGuest;

        private const int PlayerPositionSpectator = PhotonBattleRoom.PlayerPositionSpectator;

        private const string TeamAlphaNameKey = PhotonBattleRoom.TeamAlphaNameKey;
        private const string TeamBetaNameKey = PhotonBattleRoom.TeamBetaNameKey;

        /*[Header("Settings"), SerializeField] private WindowDef _mainMenuWindow;
        [SerializeField] private WindowDef _roomWindow;
        [SerializeField] private WindowDef _gameWindow;*/
        [SerializeField] private bool _isCloseRoomOnGameStart;
        //[SerializeField] private SceneDef _raidScene;

        [Header("Team Names")]
        [SerializeField] private string _blueTeamName;
        [SerializeField] private string _redTeamName;

        [Header("Battle Quantum Player")]
        [SerializeField] private RuntimePlayer _player;

        [Header("Battle Quantum Configs")]
        // Quantum Configs
        [SerializeField] private Map _quantumBattleMap;
        [SerializeField] private SimulationConfig _quantumBattleSimulationConfig;
        [SerializeField] private SystemsConfig _quantumBattleSystemsConfig;

        [Header("Battle Quantum Custom Configs")]
        [SerializeField] private BattleQConfig _battleQConfig;

        [Header("Battle Map reference")]
        [SerializeField] private BattleMapReference _battleMapReference;

        private const long STARTDELAY = 2000;
        private const float MatchmakingTimeoutSeconds = 30f;

        private QuantumRunner _runner = null;

        private Coroutine _reserveFreePositionHolder = null;
        private Coroutine _requestPositionChangeHolder = null;
        private Coroutine _matchmakingHolder = null;
        private Coroutine _followLeaderHolder = null;
        private Coroutine _canBattleStartCheckHolder = null;
        private Coroutine _startGameHolder = null;
        // Holder for the client-side StartQuantum coroutine so it can be stopped if needed
        private Coroutine _startQuantumHolder = null;
        private Coroutine _autoJoinHolder = null;
        private Coroutine _verifyPositionsHolder = null;
        // Flag set by OnJoinRoomFailed to signal a join attempt failure to waiting coroutines
        private bool _joinRoomFailed = false;

        private string[] _teammates = null;

        private List<FriendInfo> _friendList;

        [HideInInspector] public ReadOnlyCollection<LobbyRoomInfo> CurrentRooms = null; // Set from LobbyRoomListingController.cs through Instance variable maybe this could be refactored?

        private List<string> _posChangeQueue = new();

        private bool _isStartFinished = false;
        private bool _returnToMainMenuOnMatchmakingRejoin = false;
        // If a master leaves during start, defer returning to the LobbyRoom UI until master switch completes
        private bool _deferReturnToLobbyRoomOnMasterSwitch = false;

        public static LobbyManager Instance { get; private set; }
        public bool IsStartFinished {set => _isStartFinished = value; }
        public static bool IsActive { get => _isActive;}

        private static bool _isActive = false;

        public bool RunnerActive => _runner != null;

        #region Delegates & Events

        public delegate void LobbyWindowChangeRequest(LobbyWindowTarget target, LobbyWindowTarget lobbyWindow = LobbyWindowTarget.None);
        public static event LobbyWindowChangeRequest OnLobbyWindowChangeRequest;

        public delegate void StartTimeSet(long startTime);
        public static event StartTimeSet OnStartTimeSet;

        public delegate void LobbyConnected();
        public static event LobbyConnected LobbyOnConnected;

        public delegate void LobbyConnectedToMaster();
        public static event LobbyConnectedToMaster LobbyOnConnectedToMaster;

        public delegate void LobbyRegionListReceived();
        public static event LobbyRegionListReceived LobbyOnRegionListReceived;

        public delegate void LobbyDisconnected();
        public static event LobbyDisconnected LobbyOnDisconnected;

        public delegate void LobbyJoinedLobby();
        public static event LobbyJoinedLobby LobbyOnJoinedLobby;

        public delegate void LobbyLobbyStatisticsUpdate();
        public static event LobbyLobbyStatisticsUpdate LobbyOnLobbyStatisticsUpdate;

        public delegate void LobbyFriendListUpdate();
        public static event LobbyFriendListUpdate LobbyOnFriendListUpdate;

        public delegate void LobbyLeftLobby();
        public static event LobbyLeftLobby LobbyOnLeftLobby;

        public delegate void LobbyCreatedRoom();
        public static event LobbyCreatedRoom LobbyOnCreatedRoom;

        public delegate void LobbyCreateRoomFailed(short returnCode, string message);
        public static event LobbyCreateRoomFailed LobbyOnCreateRoomFailed;

        public delegate void LobbyRoomListUpdate(List<LobbyRoomInfo> roomList);
        public static event LobbyRoomListUpdate LobbyOnRoomListUpdate;

        public delegate void LobbyJoinedRoom();
        public static event LobbyJoinedRoom LobbyOnJoinedRoom;

        public delegate void LobbyJoinRoomFailed(short returnCode, string message);
        public static event LobbyJoinRoomFailed LobbyOnJoinRoomFailed;

        public delegate void LobbyJoinRandomFailed(short returnCode, string message);
        public static event LobbyJoinRandomFailed LobbyOnJoinRandomFailed;

        public delegate void LobbyLeftRoom();
        public static event LobbyLeftRoom LobbyOnLeftRoom;

        public delegate void LobbyPlayerEnteredRoom(LobbyPlayer newPlayer);
        public static event LobbyPlayerEnteredRoom LobbyOnPlayerEnteredRoom;

        public delegate void LobbyPlayerLeftRoom(LobbyPlayer otherPlayer);
        public static event LobbyPlayerLeftRoom LobbyOnPlayerLeftRoom;

        public delegate void LobbyMasterClientSwitched(LobbyPlayer newMasterClient);
        public static event LobbyMasterClientSwitched LobbyOnMasterClientSwitched;

        public delegate void LobbyRoomPropertiesUpdate(LobbyPhotonHashtable propertiesThatChanged);
        public static event LobbyRoomPropertiesUpdate LobbyOnRoomPropertiesUpdate;

        public delegate void LobbyPlayerPropertiesUpdate(LobbyPlayer targetPlayer, LobbyPhotonHashtable propertiesThatChanged);
        public static event LobbyPlayerPropertiesUpdate LobbyOnPlayerPropertiesUpdate;

        public delegate void LobbyEvent();
        public static event LobbyEvent LobbyOnEvent;

        public delegate void LobbyCustomAuthenticationResponse(Dictionary<string, object> data);
        public static event LobbyCustomAuthenticationResponse LobbyOnCustomAuthenticationResponse;

        public delegate void LobbyCustomAuthenticationFailed(string debugMessage);
        public static event LobbyCustomAuthenticationFailed LobbyOnCustomAuthenticationFailed;

        public delegate void MatchmakingRoomEntered(bool isLeader);
        public static event MatchmakingRoomEntered OnMatchmakingRoomEntered;

        public delegate void RoomLeaderChanged(bool isLeader);
        public static event RoomLeaderChanged OnRoomLeaderChanged;

        public delegate void ClanMemberDisconnected();
        public static event ClanMemberDisconnected OnClanMemberDisconnected;

        public delegate void FailedToStartMatchmakingGame();
        public static event FailedToStartMatchmakingGame OnFailedToStartMatchmakingGame;

        public delegate void GameCountdownUpdate(int secondsRemaining);
        public static event GameCountdownUpdate OnGameCountdownUpdate;

        public delegate void GameStartCancelled();
        public static event GameStartCancelled OnGameStartCancelled;

        public delegate void MatchmakingStopped();
        public static event MatchmakingStopped OnMatchmakingStopped;

        public delegate void KickedOutOfTheRoom(GetKickedEvent.ReasonType reason);
        public static event KickedOutOfTheRoom OnKickedOutOfTheRoom;

        #endregion

        // Public helper to request a lobby window change from outside LobbyManager.
        public static void RequestLobbyWindowChange(LobbyWindowTarget target, LobbyWindowTarget lobbyWindow = LobbyWindowTarget.None)
        {
            OnLobbyWindowChangeRequest?.Invoke(target, lobbyWindow);
        }

        private static long GetRoomCreationTimestamp(LobbyRoomInfo r)
        {
            try
            {
                if (r == null) return long.MaxValue;
                if (r.CustomProperties == null) return long.MaxValue;
                if (!r.CustomProperties.ContainsKey(PhotonBattleRoom.BattleID)) return long.MaxValue;
                var bidObj = r.CustomProperties[PhotonBattleRoom.BattleID];
                if (bidObj == null) return long.MaxValue;
                string bid = bidObj.ToString();
                int idx = bid.LastIndexOf('_');
                if (idx < 0 || idx + 1 >= bid.Length) return long.MaxValue;
                string ts = bid.Substring(idx + 1);
                if (long.TryParse(ts, out long v)) return v;
            }
            catch { }
            return long.MaxValue;
        }

        // Helper to safely raise Photon events only when connected to the GameServer and ready.
        private bool SafeRaiseEvent(byte eventCode, object content, RaiseEventArgs raiseEventArgs, SendOptions sendOptions)
        {
            try
            {
                var client = PhotonRealtimeClient.Client;
                if (client != null && client.Server == ServerConnection.GameServer && client.IsConnectedAndReady && PhotonRealtimeClient.InRoom)
                {
                    return client.OpRaiseEvent(eventCode, content, raiseEventArgs, sendOptions);
                }
                Debug.Log($"Skipping OpRaiseEvent {eventCode}: Server={client?.Server}, IsConnectedAndReady={client?.IsConnectedAndReady}, InRoom={PhotonRealtimeClient.InRoom}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to OpRaiseEvent {eventCode}: {ex.Message}");
            }
            return false;
        }

        private IEnumerator AutoJoinLargestMatchmakingRoom()
        {
            try
            {
                // Do not auto-join other rooms when current room is a Custom game.
                try
                {
                    if (PhotonRealtimeClient.CurrentRoom != null && PhotonRealtimeClient.CurrentRoom.CustomProperties != null
                        && PhotonRealtimeClient.CurrentRoom.CustomProperties.ContainsKey(PhotonBattleRoom.GameTypeKey))
                    {
                        if ((GameType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey) == GameType.Custom)
                        {
                            Debug.Log("AutoJoinLargestMatchmakingRoom: current room is Custom game mode, aborting auto-join.");
                            yield break;
                        }
                    }
                }
                catch { }
                string originalRoom = PhotonRealtimeClient.CurrentRoom?.Name ?? string.Empty;
                int originalCount = PhotonRealtimeClient.CurrentRoom?.PlayerCount ?? 0;
                int attempts = 0;

                while (attempts < 8)
                {
                    attempts++;

                    // If we have a cached room list, check if current room is already the best; if so, do nothing.
                    if (CurrentRooms != null && CurrentRooms.Count > 0)
                    {
                        LobbyRoomInfo bestRoom = null;
                        foreach (var room in CurrentRooms)
                        {
                            try
                            {
                                if (!room.CustomProperties.ContainsKey(PhotonBattleRoom.IsMatchmakingKey)) continue;
                                if (!(room.CustomProperties[PhotonBattleRoom.IsMatchmakingKey] is bool isMm) || !isMm) continue;
                            }
                            catch { continue; }

                            if (room.PlayerCount >= room.MaxPlayers) continue;

                            if (bestRoom == null)
                            {
                                bestRoom = room;
                            }
                            else
                            {
                                if (room.PlayerCount > bestRoom.PlayerCount)
                                {
                                    bestRoom = room;
                                }
                                else if (room.PlayerCount == bestRoom.PlayerCount)
                                {
                                    // Tie-breaker: choose randomly to distribute players across equal rooms.
                                    if (UnityEngine.Random.value > 0.5f)
                                    {
                                        bestRoom = room;
                                    }
                                }
                            }
                        }

                        if (bestRoom != null)
                        {
                            // If original room is already the best (or as large as best), don't leave it.
                            if (!string.IsNullOrEmpty(originalRoom) && (originalRoom == bestRoom.Name || originalCount >= bestRoom.PlayerCount))
                            {
                                yield break;
                            }

                            // We have a better room available; remember target and leave to lobby to join it.
                            string targetRoom = bestRoom.Name;
                            Debug.Log($"AutoJoinLargestMatchmakingRoom: better room found {targetRoom}, leaving current room to join.");
                            if (PhotonRealtimeClient.InRoom) PhotonRealtimeClient.LeaveRoom();
                            yield return new WaitUntil(() => PhotonRealtimeClient.InLobby || !PhotonRealtimeClient.InRoom);

                            // Attempt to join the target room directly (room list may be cleared on leave)
                            if (!string.IsNullOrEmpty(targetRoom) && PhotonRealtimeClient.InLobby)
                            {
                                    Debug.Log($"AutoJoinLargestMatchmakingRoom: attempting join {targetRoom}");
                                    PhotonRealtimeClient.JoinRoom(targetRoom);
                                float joinStart = Time.time;
                                while (!PhotonRealtimeClient.InRoom && Time.time - joinStart < 5f)
                                {
                                    yield return null;
                                }

                                if (PhotonRealtimeClient.InRoom)
                                {
                                    yield break;
                                }
                            }
                        }
                    }

                    // If we don't have a cached room list, leave to lobby and wait for room list update, then decide.
                    if (PhotonRealtimeClient.InRoom)
                    {
                        PhotonRealtimeClient.LeaveRoom();
                        yield return new WaitUntil(() => PhotonRealtimeClient.InLobby || !PhotonRealtimeClient.InRoom);
                    }

                    // Wait until room list is populated
                    float start = Time.time;
                    while ((CurrentRooms == null || CurrentRooms.Count == 0) && Time.time - start < 5f)
                    {
                        yield return null;
                    }

                    // Try to find best room from the fresh list and join it
                    if (CurrentRooms != null && CurrentRooms.Count > 0)
                    {
                        LobbyRoomInfo bestRoom = null;
                        foreach (var room in CurrentRooms)
                        {
                            try
                            {
                                if (!room.CustomProperties.ContainsKey(PhotonBattleRoom.IsMatchmakingKey)) continue;
                                if (!(room.CustomProperties[PhotonBattleRoom.IsMatchmakingKey] is bool isMm) || !isMm) continue;
                            }
                            catch { continue; }

                            if (room.PlayerCount >= room.MaxPlayers) continue;
                            if (room.Name == originalRoom) continue;

                            if (bestRoom == null)
                            {
                                bestRoom = room;
                            }
                            else
                            {
                                if (room.PlayerCount > bestRoom.PlayerCount)
                                {
                                    bestRoom = room;
                                }
                                else if (room.PlayerCount == bestRoom.PlayerCount)
                                {
                                    // Tie-breaker: choose randomly to distribute players across equal rooms.
                                    if (UnityEngine.Random.value > 0.5f)
                                    {
                                        bestRoom = room;
                                    }
                                }
                            }
                        }

                        if (bestRoom != null)
                        {
                            PhotonRealtimeClient.JoinRoom(bestRoom.Name);
                            float joinStart = Time.time;
                            while (!PhotonRealtimeClient.InRoom && Time.time - joinStart < 5f)
                            {
                                yield return null;
                            }

                            if (PhotonRealtimeClient.InRoom)
                            {
                                yield break;
                            }
                        }
                    }

                    // If couldn't find/join a better room, rejoin original room if possible
                    if (!string.IsNullOrEmpty(originalRoom) && PhotonRealtimeClient.InLobby)
                    {
                        Debug.Log($"AutoJoinLargestMatchmakingRoom: rejoining original room {originalRoom}");
                        PhotonRealtimeClient.JoinRoom(originalRoom);
                        float rejoinStart = Time.time;
                        while (!PhotonRealtimeClient.InRoom && Time.time - rejoinStart < 3f)
                        {
                            yield return null;
                        }
                    }

                    // Wait before next attempt
                    yield return new WaitForSeconds(1f + attempts);
                }
            }
            finally
            {
                _autoJoinHolder = null;
                try
                {
                    if (!PhotonRealtimeClient.InRoom)
                    {
                        Debug.Log("AutoJoinLargestMatchmakingRoom: failed to join any matchmaking room; returning to MainMenu.");
                        OnLobbyWindowChangeRequest?.Invoke(LobbyWindowTarget.MainMenu);
                    }
                }
                catch { }
            }
        }


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                _isActive = false;
                if (!_isActive) Activate();
            }
        }

        public void OnEnable()
        {
            if (!_isActive) Activate();
        }

        public void OnDisable()
        {
            PhotonRealtimeClient.Client.RemoveCallbackTarget(this);
            this.Unsubscribe();
        }

        private void OnApplicationQuit()
        {
            if (PhotonRealtimeClient.Client.InRoom)
            {
                PhotonRealtimeClient.LeaveRoom();
            }
            else if (PhotonRealtimeClient.InLobby)
            {
                PhotonRealtimeClient.LeaveLobby();
            }
        }
        public void Activate()
        {
            if (_isActive) { Debug.LogWarning("LobbyManager is already active."); return; }
            _isActive = true;
            PhotonRealtimeClient.Client.AddCallbackTarget(this);
            PhotonRealtimeClient.Client.StateChanged += OnStateChange;
            this.Subscribe<ReserveFreePositionEvent>(OnReserveFreePositionEvent);
            this.Subscribe<PlayerPosEvent>(OnPlayerPosEvent);
            this.Subscribe<BotToggleEvent>(OnBotToggleEvent);
            this.Subscribe<BotFillToggleEvent>(OnBotFillToggleEvent);
            this.Subscribe<StartRoomEvent>(OnStartRoomEvent);
            this.Subscribe<StartPlayingEvent>(OnStartPlayingEvent);
            this.Subscribe<StartRaidTestEvent>(OnStartRaidTestEvent);
            this.Subscribe<StartMatchmakingEvent>(OnStartMatchmakingEvent);
            this.Subscribe<StopMatchmakingEvent>(OnStopMatchmakingEvent);
            this.Subscribe<GetKickedEvent>(OnGetKickedEvent);
            StartCoroutine(Service());

            GameConfig gameConfig = GameConfig.Get();
            PlayerSettings playerSettings = gameConfig.PlayerSettings;
            string photonRegion = string.IsNullOrEmpty(playerSettings.PhotonRegion) ? null : playerSettings.PhotonRegion;
            StartCoroutine(StartLobby(playerSettings.PlayerGuid, playerSettings.PhotonRegion));
        }

        private IEnumerator Service()
        {
            while (true)
            {
                PhotonRealtimeClient.Client?.Service();
                //Debug.LogWarning(".");
                yield return new WaitForSeconds(0.05f);
            }
        }

        private IEnumerator StartLobby(string playerGuid, string photonRegion)
        {
            ClientState networkClientState = PhotonRealtimeClient.NetworkClientState;
            Debug.Log($"{networkClientState}");
            var delay = new WaitForSeconds(0.1f);
            while (!PhotonRealtimeClient.Client.InLobby)
            {
                if (networkClientState != PhotonRealtimeClient.NetworkClientState)
                {
                    // Even with delay we must reduce NetworkClientState logging to only when it changes to avoid flooding (on slower connections).
                    networkClientState = PhotonRealtimeClient.NetworkClientState;
                    Debug.Log($"{networkClientState}");
                }
                if (PhotonRealtimeClient.Client.InRoom)
                {
                    PhotonRealtimeClient.LeaveRoom();
                }
                else if (PhotonRealtimeClient.CanConnect)
                {
                    DataStore store = Storefront.Get();
                    PlayerData playerData = null;
                    store.GetPlayerData(playerGuid, p => playerData = p);
                    yield return new WaitUntil(() => playerData != null);
                    PhotonRealtimeClient.Client.UserId = playerData.Id;
                    PhotonRealtimeClient.Connect(playerData.Name, photonRegion);
                }
                else if (PhotonRealtimeClient.CanJoinLobby)
                {
                    PhotonRealtimeClient.JoinLobbyWithWrapper(null);
                }
                yield return delay;
            }
        }

        private void OnStateChange(ClientState arg1, ClientState arg2)
        {
            Debug.Log(arg1 + " -> " + arg2);
        }

        private void OnReserveFreePositionEvent(ReserveFreePositionEvent data)
        {
            if (_reserveFreePositionHolder == null)
            {
                _reserveFreePositionHolder = StartCoroutine(ReserveFreePosition(true));
            }
        }

        private IEnumerator ReserveFreePosition(bool setToPlayerProperties = false)
        {
            try
            {
                // Loop until player correctly reserves slot
                int freePosition;
                bool success = false;
                do
                {
                        // Getting first free position from the room and creating the photon hashtables for setting property
                        freePosition = PhotonLobbyRoom.GetFirstFreePlayerPos();
                        if (!PhotonLobbyRoom.IsValidPlayerPos(freePosition))
                        {
                            this.Publish<GetKickedEvent>(new(GetKickedEvent.ReasonType.FullRoom));
                            yield break;
                        }

                        string positionKey = PhotonBattleRoom.GetPositionKey(freePosition);

                        // Try atomic reservation locally first (compare-and-swap on room property)
                        var positionProps = new LobbyPhotonHashtable(new Dictionary<object, object> { { positionKey, PhotonRealtimeClient.LocalLobbyPlayer.UserId } });
                        var expectedProps = new LobbyPhotonHashtable(new Dictionary<object, object> { { positionKey, "" } });

                        bool sent = false;
                        try
                        {
                            sent = PhotonRealtimeClient.LobbyCurrentRoom.SetCustomProperties(positionProps, expectedProps);
                        }
                        catch { sent = false; }

                        if (sent)
                        {
                            // Wait until property is confirmed set to our user id or someone else grabbed it
                            float timeout = Time.time + 1f;
                            while (Time.time < timeout)
                            {
                                string positionValue = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(positionKey, "");
                                if (positionValue == PhotonRealtimeClient.LocalLobbyPlayer.UserId)
                                {
                                    success = true;
                                    // If requested, also set the local player's PlayerPositionKey now that room reservation succeeded
                                    if (setToPlayerProperties)
                                    {
                                        try
                                        {
                                            PhotonRealtimeClient.LocalPlayer.SetCustomProperty(PlayerPositionKey, freePosition);
                                        }
                                        catch { }
                                    }
                                    break;
                                }
                                else if (!PhotonBattleRoom.CheckIfPositionIsFree(freePosition))
                                {
                                    // somebody else reserved it
                                    break;
                                }
                                yield return null;
                            }
                        }
                        else
                        {
                            // Fallback: ask master to assign the position
                            StartCoroutine(RequestPositionChange(freePosition));
                            string positionValue = "";
                            yield return new WaitUntil(() =>
                            {
                                positionValue = PhotonRealtimeClient.CurrentRoom.GetCustomProperty(positionKey, "");
                                return positionValue != "";
                            });

                            // Checking if local player is the one in the slot or if there was a conflict
                            success = positionValue == PhotonRealtimeClient.LocalLobbyPlayer.UserId;

                            if (success && setToPlayerProperties)
                            {
                                try
                                {
                                    PhotonRealtimeClient.LocalPlayer.SetCustomProperty(PlayerPositionKey, freePosition);
                                }
                                catch { }
                            }
                        }

                        if (success) break;
                        yield return null;
                } while (!success);

                // Setting to player properties
                //if (setToPlayerProperties) PhotonRealtimeClient.LocalPlayer.SetCustomProperty(PhotonBattleRoom.PlayerPositionKey, freePosition);
            }
            finally
            {
                _reserveFreePositionHolder = null;
            }
        }

        private bool CheckIfAllPlayersInPosition()
        {
            bool pos1Set = false;
            bool pos2Set = false;
            bool pos3Set = false;
            bool pos4Set = false;
            foreach (var player in PhotonRealtimeClient.GetCurrentRoomPlayers())
            {
                if (!player.HasCustomProperty(PlayerPositionKey) || !player.HasCustomProperty(PhotonBattleRoom.PlayerCharacterIdsKey) || !player.HasCustomProperty(PhotonBattleRoom.PlayerStatsKey))
                {
                    return false;
                }
                var playerPosition = player.GetCustomProperty(PlayerPositionKey, 0);
                switch (playerPosition)
                {
                    case 1:
                        if(pos1Set) return false;
                        if(!PhotonRealtimeClient.CurrentRoom.GetCustomProperty(PhotonBattleRoom.PlayerPositionKey1, "").Equals(player.UserId)) return false;
                        pos1Set = true;
                        break;
                    case 2:
                        if (pos2Set) return false;
                        if (!PhotonRealtimeClient.CurrentRoom.GetCustomProperty(PhotonBattleRoom.PlayerPositionKey2, "").Equals(player.UserId)) return false;
                        pos2Set = true;
                        break;
                    case 3:
                        if (pos3Set) return false;
                        if (!PhotonRealtimeClient.CurrentRoom.GetCustomProperty(PhotonBattleRoom.PlayerPositionKey3, "").Equals(player.UserId)) return false;
                        pos3Set = true;
                        break;
                    case 4:
                        if (pos4Set) return false;
                        if (!PhotonRealtimeClient.CurrentRoom.GetCustomProperty(PhotonBattleRoom.PlayerPositionKey4, "").Equals(player.UserId)) return false;
                        pos4Set = true;
                        break;
                    default: return false;

                }
            }
            return pos1Set && pos2Set && pos3Set && pos4Set;
        }

        private IEnumerator CheckIfBattleCanStart()
        {
            try
            {
                yield break;
                yield return new WaitForSeconds(0.5f);
                yield return new WaitUntil(() => _posChangeQueue.Count == 0 && !_playerPosChangeInProgress);
                Room room = PhotonRealtimeClient.CurrentRoom;
                if (PhotonRealtimeClient.LocalPlayer.IsMasterClient)
                {
                    while (room.PlayerCount >= room.MaxPlayers)
                    {
                        if (CheckIfAllPlayersInPosition())
                        {
                            GameType gameType = (GameType)room.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey);
                            if (gameType == GameType.Custom) OnStartPlayingEvent(new());
                        }
                        yield return null;
                    }
                }
            }
            finally
            {
                _canBattleStartCheckHolder = null;
            }
        }

        private void OnPlayerPosEvent(PlayerPosEvent data)
        {
            if (_requestPositionChangeHolder == null)
            {
                _requestPositionChangeHolder = StartCoroutine(RequestPositionChange(data.PlayerPosition));
            }
        }

        private void OnBotToggleEvent(BotToggleEvent data)
        {
            StartCoroutine(SetBot(data.PlayerPosition, data.BotActive));
        }

        private void OnBotFillToggleEvent(BotFillToggleEvent data)
        {
            StartCoroutine(SetBotFill(data.BotFillActive));
        }

        private IEnumerator RequestPositionChange(int position)
        {
            try
            {
                // Saving the previous position to a variable
                int oldPosition = PhotonRealtimeClient.LocalPlayer.GetCustomProperty(PlayerPositionKey, -1);
                int currentPosition = oldPosition;

                do
                {
                    // Checking if the new position is free before raising event to master client
                    if (PhotonBattleRoom.CheckIfPositionIsFree(position) == false)
                    {
                        if(PhotonBattleRoom.CheckIfPositionHasBot(position)) Debug.LogWarning($"Failed to reserve the position {position} because there is a bot in the slot.");
                        else Debug.LogWarning($"Failed to reserve the position {position}. This likely because somebody already is in this position.");
                        yield break;
                    }

                    // Raising event to master client
                    SafeRaiseEvent(
                        PhotonRealtimeClient.PhotonEvent.PlayerPositionChangeRequested,
                        position,
                        new RaiseEventArgs { Receivers = ReceiverGroup.MasterClient },
                        SendOptions.SendReliable
                    );

                    // Giving position time to update, loop will send request again if position didn't update within this time.
                    yield return new WaitForSeconds(0.5f);

                    // Getting the current position
                    currentPosition = PhotonRealtimeClient.LocalPlayer.GetCustomProperty(PlayerPositionKey, -1);

                } while (currentPosition == oldPosition); // Checking if the position has changed. If not sending event again.
            }
            finally
            {
                _requestPositionChangeHolder = null;
            }
        }

        private void OnStartRoomEvent(StartRoomEvent data)
        {
            Debug.Log($"onEvent {data}");
            StartCoroutine(OnStartRoom());
        }

        private IEnumerator OnStartRoom()
        {
            float startTime =Time.time;
            yield return new WaitUntil(() => PhotonRealtimeClient.Client.InRoom || Time.time > startTime+10);
            if (!PhotonRealtimeClient.Client.InRoom)
            {
                Debug.LogWarning("Failed to join a room in time.");
                PhotonRealtimeClient.LeaveRoom();
                yield break;
            }
            if(PhotonRealtimeClient.LocalPlayer.IsMasterClient) PhotonRealtimeClient.CurrentRoom.SetCustomProperties(new PhotonHashtable { { BattleID, PhotonRealtimeClient.CurrentRoom.Name.Replace(' ', '_') + "_" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString() } });
            //WindowManager.Get().ShowWindow(_roomWindow);
            OnLobbyWindowChangeRequest?.Invoke(LobbyWindowTarget.LobbyRoom);
        }

        private void OnStartPlayingEvent(StartPlayingEvent data)
        {
            Debug.Log($"onEvent {data}");
            if (_startGameHolder != null)
            {
                StopCoroutine(_startGameHolder);
                _startGameHolder = null;
            }
            _startGameHolder = StartCoroutine(StartTheGameplay(_isCloseRoomOnGameStart, _blueTeamName, _redTeamName));
        }

        private void OnStartRaidTestEvent(StartRaidTestEvent data)
        {
            Debug.Log($"onEvent {data}");
            StartCoroutine(StartTheRaidTestRoom());
        }

        private void OnStartMatchmakingEvent(StartMatchmakingEvent data)
        {
            Debug.Log($"onEvent {data}");

            if (!PhotonRealtimeClient.InRoom) return;

            // Starting matchmaking coroutine
            if (_matchmakingHolder == null)
            {
                _matchmakingHolder = StartCoroutine(StartMatchmaking(data.SelectedGameType));
            }
        }

        private void OnStopMatchmakingEvent(StopMatchmakingEvent data)
        {
            Debug.Log($"onEvent {data}");

            // Only send RoomChangeRequested if we're connected to the Game server, ready and in a room.
            // OpRaiseEvent is not allowed on the MasterServer/NameServer connections and requires the client to be ready.
            if (PhotonRealtimeClient.Client != null && PhotonRealtimeClient.Client.Server == ServerConnection.GameServer && PhotonRealtimeClient.Client.IsConnectedAndReady && PhotonRealtimeClient.InRoom)
            {
                SafeRaiseEvent(
                    PhotonRealtimeClient.PhotonEvent.RoomChangeRequested,
                    PhotonRealtimeClient.LocalPlayer.UserId,
                    new RaiseEventArgs { Receivers = ReceiverGroup.Others },
                    SendOptions.SendReliable
                );
            }
            else
            {
                Debug.Log($"Skipping RoomChangeRequested broadcast: Server={PhotonRealtimeClient.Client?.Server}, IsConnectedAndReady={PhotonRealtimeClient.Client?.IsConnectedAndReady}, InRoom={PhotonRealtimeClient.InRoom}");
            }

            StartCoroutine(LeaveMatchmaking());
        }
        #region Matchmaking
        /// <summary>
        /// Stops any active matchmaking or follow leader coroutines.
        /// Call this before leaving a room when switching game types.
        /// </summary>
        public void StopMatchmakingCoroutines()
        {
            if (_matchmakingHolder != null)
            {
                StopCoroutine(_matchmakingHolder);
                _matchmakingHolder = null;
            }

            if (_followLeaderHolder != null)
            {
                StopCoroutine(_followLeaderHolder);
                _followLeaderHolder = null;
            }

            if (_startGameHolder != null)
            {
                StopCoroutine(_startGameHolder);
                _startGameHolder = null;
            }
            if (_autoJoinHolder != null)
            {
                StopCoroutine(_autoJoinHolder);
                _autoJoinHolder = null;
            }
        }

        private IEnumerator StartMatchmaking(GameType gameType, bool broadcastRoomChange = true)
        {
            bool keepHolder = false;
            try
            {
            // Closing the room so that no others can join
            PhotonRealtimeClient.CurrentRoom.IsOpen = false;

            // Saving custom properties from the room to the variables
            string clanName = PhotonRealtimeClient.CurrentRoom.GetCustomProperty(PhotonBattleRoom.ClanNameKey, "");
            int soulhomeRank = PhotonRealtimeClient.CurrentRoom.GetCustomProperty(PhotonBattleRoom.SoulhomeRank, 0);

            string positionValue1 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty(PhotonBattleRoom.PlayerPositionKey1, "");
            string positionValue2 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty(PhotonBattleRoom.PlayerPositionKey2, "");
            string positionValue3 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty(PhotonBattleRoom.PlayerPositionKey3, "");
            string positionValue4 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty(PhotonBattleRoom.PlayerPositionKey4, "");

            // Saving other player's userids to enter the new game room together with master client
            List<string> expectedUsers = new();
            foreach (var player in PhotonRealtimeClient.CurrentRoom.Players)
            {
                if (player.Value.UserId != PhotonRealtimeClient.LocalPlayer.UserId)
                {
                    expectedUsers.Add(player.Value.UserId);
                }

                // Saving clan name and soulhome rank to player's custom properties in case the matchmaking leader leaves
                if (!string.IsNullOrEmpty(clanName))
                {
                    player.Value.SetCustomProperty(PhotonBattleRoom.ClanNameKey, clanName);
                    player.Value.SetCustomProperty(PhotonBattleRoom.SoulhomeRank, soulhomeRank);
                }
            }
            _teammates = expectedUsers.ToArray();

            // Sending other players in the room the room change request, setting own leader id key as own userid to indicate being the leader
            PhotonRealtimeClient.LocalPlayer.SetCustomProperty(PhotonBattleRoom.LeaderIdKey, PhotonRealtimeClient.LocalPlayer.UserId);

            if (broadcastRoomChange && PhotonRealtimeClient.Client != null && PhotonRealtimeClient.Client.Server == ServerConnection.GameServer && PhotonRealtimeClient.Client.IsConnectedAndReady && PhotonRealtimeClient.InRoom)
            {
                SafeRaiseEvent(
                    PhotonRealtimeClient.PhotonEvent.RoomChangeRequested,
                    PhotonRealtimeClient.LocalPlayer.UserId,
                    new RaiseEventArgs { Receivers = ReceiverGroup.Others },
                    SendOptions.SendReliable
                );
            }
            else if (broadcastRoomChange)
            {
                Debug.Log($"Skipping RoomChangeRequested broadcast (StartMatchmaking): Server={PhotonRealtimeClient.Client?.Server}, IsConnectedAndReady={PhotonRealtimeClient.Client?.IsConnectedAndReady}, InRoom={PhotonRealtimeClient.InRoom}");
            }

            // Nulling room list and leaving room so that client can get room list
            CurrentRooms = null;
            PhotonRealtimeClient.LeaveRoom();

            // Waiting until in lobby and that current room list has rooms
            yield return new WaitUntil(() => PhotonRealtimeClient.InLobby && CurrentRooms != null);

            // Searching for suitable room and attempting to join each candidate.
            // If a JoinRoom attempt fails (room filled/closed) we continue searching other rooms.
            bool roomFound = false;
            bool joinedExistingRoom = false;
            // Use a shorter per-room timeout for Random2v2 to reduce delays when iterating many candidates.
            float joinAttemptTimeout = gameType == GameType.Random2v2 ? 2f : 5f; // seconds to wait for a join to succeed before trying next room

            if (CurrentRooms != null && CurrentRooms.Count > 0)
            {
                // Sort candidates by descending player count (prefer fuller rooms) and deterministic tie-breaker
                var roomsList = CurrentRooms.OrderByDescending(r => r.PlayerCount).ThenBy(r => r.Name).ToList();

                foreach (LobbyRoomInfo room in roomsList)
                {
                    // Checking if the room has a game type and matchmaking key in the first place
                    if (!room.CustomProperties.ContainsKey(PhotonBattleRoom.GameTypeKey) || !room.CustomProperties.ContainsKey(PhotonBattleRoom.IsMatchmakingKey))
                    {
                        continue;
                    }

                    // Checking that the game type matches and that the room is a matchmaking room
                    if ((GameType)room.CustomProperties[PhotonBattleRoom.GameTypeKey] != gameType || (bool)room.CustomProperties[PhotonBattleRoom.IsMatchmakingKey] == false)
                    {
                        continue;
                    }

                    // Decide if we should attempt to join this room
                    bool shouldTryJoin = false;
                    switch (gameType)
                    {
                        case GameType.Clan2v2:
                            if ((string)room.CustomProperties[PhotonBattleRoom.ClanNameKey] != clanName && room.MaxPlayers - room.PlayerCount >= _teammates.Length + 1)
                            {
                                shouldTryJoin = true;
                            }
                            break;
                        case GameType.Random2v2:
                            if (room.MaxPlayers - room.PlayerCount >= _teammates.Length + 1)
                            {
                                shouldTryJoin = true;
                            }
                            break;
                    }

                    if (!shouldTryJoin) continue;

                    // Attempt join and wait until success, explicit failure, or timeout
                    _joinRoomFailed = false;
                    PhotonRealtimeClient.JoinRoom(room.Name, _teammates);
                    float joinStart = Time.time;
                    yield return new WaitUntil(() => PhotonRealtimeClient.InRoom || _joinRoomFailed || Time.time > joinStart + joinAttemptTimeout);

                    if (PhotonRealtimeClient.InRoom)
                    {
                        roomFound = true;
                        joinedExistingRoom = true;
                        break;
                    }
                    else
                    {
                        Debug.LogWarning($"JoinRoom failed or timed out for '{room.Name}', trying next candidate.");
                        // try next room
                    }
                }
            }

            // If no existing room was successfully joined, request server-side join-or-create
            if (!joinedExistingRoom)
            {
                switch (gameType)
                {
                    case GameType.Clan2v2:
                        PhotonRealtimeClient.JoinRandomOrCreateClan2v2Room(clanName, soulhomeRank, _teammates, true);
                        break;
                    case GameType.Random2v2:
                        PhotonRealtimeClient.JoinRandomOrCreateRandom2v2Room(_teammates, true);
                        break;
                }
            }

            // Waiting until client is in room
            yield return new WaitUntil(() => PhotonRealtimeClient.InRoom);

            // Reconcile near-simultaneous matchmaking room creations:
            // If there are other matchmaking rooms for the same game type, try to move into a single shared room.
            try
            {
                if (PhotonRealtimeClient.CurrentRoom != null && PhotonRealtimeClient.CurrentRoom.CustomProperties != null
                    && PhotonRealtimeClient.CurrentRoom.CustomProperties.ContainsKey(PhotonBattleRoom.IsMatchmakingKey)
                    && PhotonRealtimeClient.CurrentRoom.CustomProperties[PhotonBattleRoom.IsMatchmakingKey] is bool curIsMm && curIsMm)
                {
                    string myRoom = PhotonRealtimeClient.CurrentRoom.Name;
                    int myCount = PhotonRealtimeClient.CurrentRoom.PlayerCount;
                    int reconcileAttempts = 0;
                    while (reconcileAttempts < 3)
                    {
                        reconcileAttempts++;
                        if (CurrentRooms == null || CurrentRooms.Count == 0)
                        {
                            float waitStart = Time.time;
                            while ((CurrentRooms == null || CurrentRooms.Count == 0) && Time.time - waitStart < 1f)
                            {
                                yield return null;
                            }
                            if (CurrentRooms == null || CurrentRooms.Count == 0) break;
                        }

                        // Count total matchmaking rooms that have at least one player (including our own)
                        int totalMatchmakingRoomsWithPlayers = 0;
                        foreach (var r in CurrentRooms)
                        {
                            try
                            {
                                if (!r.CustomProperties.ContainsKey(PhotonBattleRoom.IsMatchmakingKey)) continue;
                                if (!(r.CustomProperties[PhotonBattleRoom.IsMatchmakingKey] is bool isMm) || !isMm) continue;
                            }
                            catch { continue; }

                            if (r.PlayerCount > 0) totalMatchmakingRoomsWithPlayers++;
                        }

                        // If there are fewer than 2 rooms with players, no need to consolidate
                        if (totalMatchmakingRoomsWithPlayers < 2) break;

                        // Select deterministic target (highest player count, lexicographic name tie-breaker)
                        LobbyRoomInfo target = null;
                        foreach (var r in CurrentRooms)
                        {
                            try
                            {
                                if (!r.CustomProperties.ContainsKey(PhotonBattleRoom.IsMatchmakingKey)) continue;
                                if (!(r.CustomProperties[PhotonBattleRoom.IsMatchmakingKey] is bool isMm) || !isMm) continue;
                            }
                            catch { continue; }

                            if (r.Name == myRoom) continue;
                            if (r.MaxPlayers - r.PlayerCount <= 0) continue;

                            if (target == null)
                            {
                                target = r;
                            }
                            else
                            {
                                if (r.PlayerCount > target.PlayerCount) target = r;
                                else if (r.PlayerCount == target.PlayerCount && string.CompareOrdinal(r.Name, target.Name) < 0) target = r;
                            }
                        }

                        if (target == null) break;

                        // Always try to move into the chosen target when multiple non-empty matchmaking rooms exist
                        string targetName = target.Name;
                        PhotonRealtimeClient.LeaveRoom();
                        yield return new WaitUntil(() => PhotonRealtimeClient.InLobby || !PhotonRealtimeClient.InRoom);
                        if (PhotonRealtimeClient.InLobby)
                        {
                            PhotonRealtimeClient.JoinRoom(targetName);
                            float joinStart = Time.time;
                            while (!PhotonRealtimeClient.InRoom && Time.time - joinStart < 5f)
                            {
                                yield return null;
                            }

                            if (PhotonRealtimeClient.InRoom && PhotonRealtimeClient.CurrentRoom?.Name == targetName)
                            {
                                // moved into shared room
                                break;
                            }
                            else
                            {
                                // try rejoining original room if still desired
                                if (!PhotonRealtimeClient.InRoom && !string.IsNullOrEmpty(myRoom))
                                {
                                    PhotonRealtimeClient.JoinRoom(myRoom);
                                    float rejoinStart = Time.time;
                                    while (!PhotonRealtimeClient.InRoom && Time.time - rejoinStart < 3f)
                                    {
                                        yield return null;
                                    }
                                    if (PhotonRealtimeClient.InRoom) myCount = PhotonRealtimeClient.CurrentRoom.PlayerCount;
                                }
                            }
                        }
                    }
                }
            }
            finally { }

            // If room was found setting room properties
            if (roomFound)
            {
                switch (gameType)
                {
                    case GameType.Clan2v2:
                        // Setting clan name as opponent clan
                        PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.ClanOpponentNameKey, clanName);

                        // Setting own and teammate positions from old room to position keys 3 and 4
                        PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PlayerPositionKey3, positionValue1);
                        PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PlayerPositionKey4, positionValue2);
                        break;

                    case GameType.Random2v2:
                        if (_teammates.Length == 0) // If queuing solo
                        {
                            StartCoroutine(ReserveFreePosition());
                        }
                        else // Queuing with a teammate TODO: untested code, when queueing with teammate is possible test this and fix any issues
                        {
                            // Checking if position is free and if so setting userid from old room to that position
                            if (PhotonBattleRoom.CheckIfPositionIsFree(PhotonBattleRoom.PlayerPosition3))
                            {
                                PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PlayerPositionKey3, positionValue3);
                            }
                            else // If position is not free
                            {
                                // Moving the player at the position to the first free position (should be either 1 or 2 since room max players is 4)
                                int freePosition = PhotonLobbyRoom.GetFirstFreePlayerPos();
                                if (!PhotonLobbyRoom.IsValidPlayerPos(freePosition)) yield break;
                                string newRoomPositionValue3 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey3);
                                PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.GetPositionKey(freePosition), newRoomPositionValue3);
                            }

                            if (PhotonBattleRoom.CheckIfPositionIsFree(PhotonBattleRoom.PlayerPosition4))
                            {
                                PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PlayerPositionKey4, positionValue4);
                            }
                            else
                            {
                                int freePosition = PhotonLobbyRoom.GetFirstFreePlayerPos();
                                if (!PhotonLobbyRoom.IsValidPlayerPos(freePosition)) yield break;
                                string newRoomPositionValue4 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey4);
                                PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.GetPositionKey(freePosition), newRoomPositionValue4);
                            }
                        }
                        break;
                }
            }
            else if (!roomFound) // Initializing new created room properties
            {
                // Setting player positions from the old room
                PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PlayerPositionKey1, positionValue1);
                PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PlayerPositionKey2, positionValue2);
                PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PlayerPositionKey3, positionValue3);
                PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PlayerPositionKey4, positionValue4);
            }

            // Stopping coroutine if not a master client
            if (!PhotonRealtimeClient.LocalPlayer.IsMasterClient) yield break;

            _matchmakingHolder = StartCoroutine(WaitForMatchmakingPlayers());
            keepHolder = true;
        }
        finally
        {
            if (!keepHolder) _matchmakingHolder = null;
        }

        }

        private IEnumerator WaitForMatchmakingPlayers()
        {
            try
            {
                if (!PhotonRealtimeClient.LocalPlayer.IsMasterClient) yield break;

                bool gameStarting = false;
                do
                {
                // Checking every 0,5s if we can start gameplay
                bool canStartGameplay = false;
                float waitStartTime = Time.time;
                bool botBackfillApplied = false;
                do
                {
                    yield return new WaitForSeconds(0.5f);

                    // Check if matchmaking timeout expired and fill remaining slots with bots (Random2v2 only)
                    GameType currentGameType = (GameType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey);
                    if (!botBackfillApplied && currentGameType == GameType.Random2v2 && Time.time - waitStartTime >= MatchmakingTimeoutSeconds)
                    {
                        Debug.Log($"Matchmaking timeout ({MatchmakingTimeoutSeconds}s) reached for Random2v2. Filling remaining slots with bots.");

                        int[] positions = {
                            PhotonBattleRoom.PlayerPosition1, PhotonBattleRoom.PlayerPosition2,
                            PhotonBattleRoom.PlayerPosition3, PhotonBattleRoom.PlayerPosition4
                        };
                        foreach (int pos in positions)
                        {
                            if (PhotonBattleRoom.CheckIfPositionIsFree(pos))
                            {
                                string posKey = PhotonBattleRoom.GetPositionKey(pos);
                                PhotonRealtimeClient.CurrentRoom.SetCustomProperty(posKey, "Bot");
                            }
                        }

                        PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.BotFillKey, true);
                        botBackfillApplied = true;
                    }

                    if (!botBackfillApplied)
                    {
                        // Normal path: wait for real players to fill the room
                        if (PhotonRealtimeClient.CurrentRoom.PlayerCount != PhotonRealtimeClient.CurrentRoom.MaxPlayers) continue;
                    }

                    // At this point either the room is full or we've applied bot backfill.
                    // Proceed to mapping player -> room position keys even if some position slots are not yet set.
                    canStartGameplay = true;

                } while (!canStartGameplay);


                // Updating player positions from room to player properties, and waiting that they have been synced
                string positionValue1 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey1);
                string positionValue2 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey2);
                string positionValue3 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey3);
                string positionValue4 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey4);

                foreach (var player in PhotonRealtimeClient.CurrentRoom.Players)
                {
                    int position = PhotonBattleRoom.PlayerPositionGuest;

                    if (player.Value.UserId == positionValue1) position = PhotonBattleRoom.PlayerPosition1;
                    else if (player.Value.UserId == positionValue2) position = PhotonBattleRoom.PlayerPosition2;
                    else if (player.Value.UserId == positionValue3) position = PhotonBattleRoom.PlayerPosition3;
                    else if (player.Value.UserId == positionValue4) position = PhotonBattleRoom.PlayerPosition4;
                    else
                    {
                        // If player isn't in any position, getting the first free player position.
                        // This method checks for duplicate and missing players
                        position = PhotonLobbyRoom.GetFirstFreePlayerPos(new(player.Value)); // TODO: if Clan2v2 ensure that player ends on the correct side
                        if (!PhotonLobbyRoom.IsValidPlayerPos(position)) continue;
                        string positionKey = PhotonBattleRoom.GetPositionKey(position);

                        // Setting position to room and waiting until it's synced
                        PhotonRealtimeClient.CurrentRoom.SetCustomProperty(positionKey, player.Value.UserId);
                        yield return new WaitUntil(() => PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(positionKey) == player.Value.UserId);
                    }

                    // Setting position to player properties and waiting until it's synced
                    player.Value.SetCustomProperty(PhotonBattleRoom.PlayerPositionKey, position);
                    yield return new WaitUntil(() => player.Value.GetCustomProperty<int>(PhotonBattleRoom.PlayerPositionKey) == position);
                }

                // Checking that the clan names are in order
                GameType roomGameType = (GameType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey);
                if (roomGameType == GameType.Clan2v2)
                {
                    string primaryClan = string.Empty;
                    string opponentClan = string.Empty;

                    foreach (var player in PhotonRealtimeClient.CurrentRoom.Players)
                    {
                        int playerPos = player.Value.GetCustomProperty<int>(PhotonBattleRoom.PlayerPositionKey);

                        if (playerPos == PhotonBattleRoom.PlayerPosition1)
                        {
                            primaryClan = player.Value.GetCustomProperty(PhotonBattleRoom.ClanNameKey, string.Empty);
                        }
                        else if (playerPos == PhotonBattleRoom.PlayerPosition3)
                        {
                            opponentClan = player.Value.GetCustomProperty(PhotonBattleRoom.ClanNameKey, string.Empty);
                        }
                    }
                    if (PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.ClanNameKey) != primaryClan)
                    {
                        PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.ClanNameKey, primaryClan);
                    }

                    if (PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.ClanOpponentNameKey) != opponentClan)
                    {
                        PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.ClanOpponentNameKey, opponentClan);
                    }

                    _blueTeamName = primaryClan;
                    _redTeamName = opponentClan;
                }

                // For Random2v2 ensure team names are set (they aren't set by the Clan2v2 block above)
                if (roomGameType == GameType.Random2v2)
                {
                    if (string.IsNullOrWhiteSpace(_blueTeamName)) _blueTeamName = "Team Alpha";
                    if (string.IsNullOrWhiteSpace(_redTeamName)) _redTeamName = "Team Beta";
                }

                // Set BattleID for matchmaking rooms (StartRoomEvent is not published for matchmaking rooms)
                if (!PhotonRealtimeClient.CurrentRoom.CustomProperties.ContainsKey(PhotonBattleRoom.BattleID)
                    || string.IsNullOrEmpty(PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.BattleID)))
                {
                    PhotonRealtimeClient.CurrentRoom.SetCustomProperties(new PhotonHashtable
                    {
                        { PhotonBattleRoom.BattleID, PhotonRealtimeClient.CurrentRoom.Name.Replace(' ', '_') + "_" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString() }
                    });
                    yield return null;
                }

                // Starting gameplay coroutine if all positions are filled (real players + bots), else we loop again
                int botCount = PhotonBattleRoom.GetBotCount();
                if (PhotonRealtimeClient.CurrentRoom.PlayerCount + botCount >= PhotonRealtimeClient.CurrentRoom.MaxPlayers)
                {
                    if (_startGameHolder != null)
                    {
                        StopCoroutine(_startGameHolder);
                        _startGameHolder = null;
                    }
                    _startGameHolder = StartCoroutine(StartTheGameplay(_isCloseRoomOnGameStart, _blueTeamName, _redTeamName));
                    gameStarting = true;
                }

                } while (!gameStarting);
            }
            finally
            {
                _matchmakingHolder = null;
            }
        }

        private IEnumerator FollowLeaderToNewRoom(string leaderUserId)
        {
            try
            {
                // Don't follow leader away from this room if we're in a Custom game.
                try
                {
                    if (PhotonRealtimeClient.CurrentRoom != null && PhotonRealtimeClient.CurrentRoom.CustomProperties != null
                        && PhotonRealtimeClient.CurrentRoom.CustomProperties.ContainsKey(PhotonBattleRoom.GameTypeKey))
                    {
                        if ((GameType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey) == GameType.Custom)
                        {
                            Debug.Log("FollowLeaderToNewRoom: current room is Custom, will not leave.");
                            yield break;
                        }
                    }
                }
                catch { }

                string oldRoomName = PhotonRealtimeClient.CurrentRoom?.Name ?? string.Empty;

                // Leave current room and wait until in lobby
                if (PhotonRealtimeClient.InRoom) PhotonRealtimeClient.LeaveRoom();
                yield return new WaitUntil(() => PhotonRealtimeClient.InLobby);

                // Try to find leader via friends list; fallback to joining the matchmaking room with most players
                bool newRoomJoined = false;
                int attempts = 0;
                do
                {
                    attempts++;
                    _friendList = null;
                    // Only call OpFindFriends when the client is connected and the leader is not this client.
                    if (PhotonRealtimeClient.Client != null && PhotonRealtimeClient.Client.IsConnectedAndReady &&
                        !string.IsNullOrEmpty(leaderUserId) && PhotonRealtimeClient.LocalPlayer != null && leaderUserId != PhotonRealtimeClient.LocalPlayer.UserId)
                    {
                        PhotonRealtimeClient.Client.OpFindFriends(new string[1] { leaderUserId });
                        yield return new WaitUntil(() => _friendList != null );
                    }
                    else
                    {
                        // Skip friends lookup and continue with fallback to room list.
                        _friendList = new List<FriendInfo>();
                    }

                    foreach (FriendInfo friend in _friendList)
                    {
                        if (friend.UserId == leaderUserId && friend.IsInRoom && friend.Room != oldRoomName)
                        {
                            PhotonRealtimeClient.JoinRoom(friend.Room);
                            newRoomJoined = true;
                            break;
                        }
                    }

                    if (!newRoomJoined)
                    {
                        // Fallback: join the matchmaking room with the most players (excluding the old room)
                        yield return new WaitUntil(() => CurrentRooms != null);
                        LobbyRoomInfo bestRoom = null;
                        foreach (var room in CurrentRooms)
                        {
                            try
                            {
                                if (!room.CustomProperties.ContainsKey(PhotonBattleRoom.IsMatchmakingKey)) continue;
                                if (!(room.CustomProperties[PhotonBattleRoom.IsMatchmakingKey] is bool isMm) || !isMm) continue;
                            }
                            catch { continue; }

                            if (room.Name == oldRoomName) continue;
                            if (bestRoom == null)
                            {
                                bestRoom = room;
                            }
                            else
                            {
                                if (room.PlayerCount > bestRoom.PlayerCount)
                                {
                                    bestRoom = room;
                                }
                                else if (room.PlayerCount == bestRoom.PlayerCount)
                                {
                                    // Tie-breaker: choose randomly to distribute players across equal rooms.
                                    if (UnityEngine.Random.value > 0.5f)
                                    {
                                        bestRoom = room;
                                    }
                                }
                            }
                        }

                        if (bestRoom != null)
                        {
                            PhotonRealtimeClient.JoinRoom(bestRoom.Name);
                            newRoomJoined = true;
                            break;
                        }
                    }

                    // Small delay to avoid tight loop; give state time to update
                    yield return new WaitForSeconds(0.5f);
                } while (!newRoomJoined && attempts < 10);
            }
            finally
            {
                _followLeaderHolder = null;
            }
        }

        private IEnumerator LeaveMatchmaking()
        {
            // Safely read the matchmaking room game type; PhotonExtensions handles null room now.
            GameType matchmakingRoomGameType = GameType.Random2v2;
            if (PhotonRealtimeClient.CurrentRoom != null)
            {
                try
                {
                    matchmakingRoomGameType = (GameType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to read matchmaking room game type: {ex.Message}");
                }
            }

            if (_matchmakingHolder != null)
            {
                StopCoroutine(_matchmakingHolder);
                _matchmakingHolder = null;
            }

            OnMatchmakingStopped?.Invoke();

            // If we're currently in a room, leave it and wait for lobby. Otherwise wait until we are in lobby.
            if (PhotonRealtimeClient.InRoom)
            {
                PhotonRealtimeClient.LeaveRoom();
                yield return new WaitUntil(() => PhotonRealtimeClient.InLobby);
            }
            else
            {
                if (!PhotonRealtimeClient.InLobby)
                {
                    yield return new WaitUntil(() => PhotonRealtimeClient.InLobby);
                }
            }

            // Creating back the non-matchmaking room which the teammates can join (only for Clan2v2)
            switch (matchmakingRoomGameType)
            {
                case GameType.Clan2v2:
                {
                    string clanName = PhotonRealtimeClient.LocalLobbyPlayer?.GetCustomProperty(PhotonBattleRoom.ClanNameKey, "");
                    int soulhomeRank = PhotonRealtimeClient.LocalLobbyPlayer?.GetCustomProperty(PhotonBattleRoom.SoulhomeRank, 0) ?? 0;
                    PhotonRealtimeClient.CreateClan2v2LobbyRoom(clanName, soulhomeRank, _teammates);
                    break;
                }
            }
        }
        #endregion

        private IEnumerator StartTheGameplay(bool isCloseRoom, string blueTeamName, string redTeamName)
        {
            try
            {
                // TODO: Select random characters if some are not selected
                //if (!PhotonBattleRoom.IsValidAllSelectedCharacters())
                //{
                //    StartingGameFailed();
                //    throw new UnityException("can't start game, everyone needs to have 3 defence characters selected");
                //}
                //Debug.Log($"startTheGameplay {gameWindow}");
                if (!PhotonRealtimeClient.LocalPlayer.IsMasterClient)
                {
                    StartingGameFailed();
                    throw new UnityException("only master client can start the game");
                }
                Player player = PhotonRealtimeClient.LocalPlayer;
                int masterPosition = player.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
                if (!PhotonLobbyRoom.IsValidPlayerPos(masterPosition))
                {
                    StartingGameFailed();
                    throw new UnityException($"master client does not have valid player position: {masterPosition}");
                }

                // Checking that every player has a player position key and if not waiting until they have one (every one who joins room will reserve position for themselves)
                Room room = PhotonRealtimeClient.CurrentRoom;
                List<Player> players = room.Players.Values.ToList();
                List<int> missingPlayers = new();

                foreach (Player roomPlayer in players)
                {
                    if (!roomPlayer.HasCustomProperty(PlayerPositionKey)) missingPlayers.Add(roomPlayer.ActorNumber);
                }

                foreach (int actorNumber in missingPlayers) // Wait until every player has a custom property PlayerPositionKey
                {
                    yield return new WaitUntil(() =>
                    {
                        Player playerMissingPosition = room.GetPlayer(actorNumber);
                        if (playerMissingPosition == null) return true;
                        return playerMissingPosition.HasCustomProperty(PlayerPositionKey);
                    });
                }

                // Checking player positions before starting gameplay
                players = room.Players.Values.ToList();
                string[] playerUserIds = new string[4] { "", "", "", "" };
                string[] playerUserNames = new string[4] { "", "", "", "" };
                PlayerType[] playerTypes = new PlayerType[4] { PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None, };

                int playerCount = 0;
                StartGameData data = null;
                foreach (Player roomPlayer in players)
                {
                    int playerPos = roomPlayer.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
                    if (!PhotonLobbyRoom.IsValidPlayerPos(playerPos))
                    {
                        // If player position is not valid we get new position for them, this method checks for duplicate and missing player positions
                        int newPos = PhotonLobbyRoom.GetFirstFreePlayerPos(new(roomPlayer));
                        if (!PhotonLobbyRoom.IsValidPlayerPos(newPos)) continue;

                        // Setting the new position to player and room properties and waiting until it's synced
                        roomPlayer.SetCustomProperty(PlayerPositionKey, newPos);
                        yield return new WaitUntil(() => roomPlayer.GetCustomProperty<int>(PlayerPositionKey) == newPos);

                        string positionKey = PhotonBattleRoom.GetPositionKey(newPos);
                        room.SetCustomProperty(positionKey, roomPlayer.UserId);
                        yield return new WaitUntil(() => room.GetCustomProperty<string>(positionKey) == roomPlayer.UserId);

                        playerPos = newPos;
                    }
                    playerTypes[playerPos-1] = PlayerType.Player;
                    playerUserIds[playerPos - 1] = roomPlayer.UserId;
                    playerUserNames[playerPos - 1] = roomPlayer.NickName;
                    playerCount += 1;
                }

                int j = 1;
                foreach(PlayerType type in playerTypes)
                {
                    if(type == PlayerType.None)
                    {
                        string positionKey = PhotonBattleRoom.GetPositionKey(j);
                        string positionValue = PhotonRealtimeClient.LobbyCurrentRoom?.GetCustomProperty<string>(positionKey);
                        if (positionValue == "Bot") playerTypes[j-1] = PlayerType.Bot;
                        playerUserNames[j - 1] = "Bot";
                    }
                    j++;
                }

                // Getting starting emotion from current room custom properties
                Emotion startingEmotion = (Emotion)room.GetCustomProperty(PhotonBattleRoom.StartingEmotionKey, (int)Emotion.Blank);

                // If starting emotion is blank getting a random starting emotion
                if (startingEmotion == Emotion.Blank)
                {
                    startingEmotion = (Emotion)UnityEngine.Random.Range(0, 4);
                }

                // Getting map id from room custom properties
                string mapId = room.GetCustomProperty(PhotonBattleRoom.MapKey, string.Empty);

                // If there is no map id getting a random map
                if (mapId == string.Empty)
                {
                    int mapIndex = UnityEngine.Random.Range(0, _battleMapReference.Maps.Count);
                    mapId = _battleMapReference.Maps[mapIndex].MapId;
                }

                if (player.IsMasterClient)
                {
                    // Ensure team names are not empty — use defaults for matchmaking/random games
                    if (string.IsNullOrWhiteSpace(blueTeamName)) blueTeamName = "Team Alpha";
                    if (string.IsNullOrWhiteSpace(redTeamName)) redTeamName = "Team Beta";
                    //room.CustomProperties.Add(TeamAlphaNameKey, blueTeamName);
                    //room.CustomProperties.Add(TeamBetaNameKey, redTeamName);
                    //room.CustomProperties.Add(PlayerCountKey, realPlayerCount);
                    room.SetCustomProperties(new PhotonHashtable
                    {
                        { BattleID, room.GetCustomProperty<string>(PhotonBattleRoom.BattleID)},
                        { TeamAlphaNameKey, blueTeamName },
                        { TeamBetaNameKey, redTeamName },
                        { PlayerCountKey, playerCount }
                    });

                    yield return null;
                    if (isCloseRoom && PhotonRealtimeClient.CurrentRoom.IsOpen)
                    {
                        PhotonRealtimeClient.CloseRoom(false);
                        yield return null;
                    }

                    if(PhotonBattleRoom.IsBotFillActive())
                    for (int i=0; i < playerTypes.Length; i++)
                    {
                            if (playerTypes[i] == PlayerType.None) { playerTypes[i] = PlayerType.Bot;}
                    }

                    data = new()
                    {
                        StartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        PlayerSlotUserIds = playerUserIds,
                        PlayerSlotUserNames = playerUserNames,
                        PlayerSlotTypes = playerTypes,
                        ProjectileInitialEmotion = startingEmotion,
                        MapId = mapId,
                        PlayerCount = playerCount,
                        Seed = Random.Range(int.MinValue, int.MaxValue),
                    };

                }
                // Countdown from 5 to 0 before starting the game
                for (int i = 5; i >= 0; i--)
                {
                    SafeRaiseEvent(
                        PhotonRealtimeClient.PhotonEvent.GameCountdown,
                        i,
                        new RaiseEventArgs { Receivers = ReceiverGroup.All },
                        SendOptions.SendReliable);
                    if (i > 0) yield return new WaitForSeconds(1f);
                }

                // Validate that all expected real players are still present before raising StartGame.
                if (PhotonRealtimeClient.InMatchmakingRoom)
                {
                    bool missingPlayer = false;
                    foreach (string uid in data.PlayerSlotUserIds)
                    {
                        if (string.IsNullOrEmpty(uid) || uid == "Bot") continue;
                        bool present = PhotonRealtimeClient.CurrentRoom?.Players?.Values?.Any(p => p.UserId == uid) ?? false;
                        if (!present)
                        {
                            missingPlayer = true;
                            Debug.LogWarning($"Aborting StartGame: player {uid} missing from room.");
                            break;
                        }
                    }

                    if (missingPlayer)
                    {
                        SafeRaiseEvent(
                            PhotonRealtimeClient.PhotonEvent.CancelGameStart,
                            null,
                            new RaiseEventArgs { Receivers = ReceiverGroup.All },
                            SendOptions.SendReliable
                        );

                        if (_startGameHolder != null)
                        {
                            StopCoroutine(_startGameHolder);
                            _startGameHolder = null;
                        }

                        if (_matchmakingHolder != null)
                        {
                            StopCoroutine(_matchmakingHolder);
                            _matchmakingHolder = null;
                        }
                        _matchmakingHolder = StartCoroutine(WaitForMatchmakingPlayers());
                        yield break;
                    }
                }

                if (!SafeRaiseEvent(PhotonRealtimeClient.PhotonEvent.StartGame, StartGameData.Serialize(data), new RaiseEventArgs{Receivers = ReceiverGroup.All}, SendOptions.SendReliable))
                {
                    Debug.LogError("Unable to start game.");
                    StartingGameFailed();
                    yield break;
                }
                Debug.Log("Starting Game");
                //WindowManager.Get().ShowWindow(gameWindow);
            }
            finally
            {
                _startGameHolder = null;
            }
        }

        private void StartingGameFailed()
        {
            // Clear BattleID so room is not left in 'starting' state after a failed start
            try
            {
                if (PhotonRealtimeClient.CurrentRoom != null && PhotonRealtimeClient.CurrentRoom.CustomProperties.ContainsKey(PhotonBattleRoom.BattleID))
                {
                    PhotonRealtimeClient.CurrentRoom.SetCustomProperties(new PhotonHashtable { { PhotonBattleRoom.BattleID, "" } });
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to clear BattleID in StartingGameFailed: {ex.Message}");
            }

            if (!PhotonRealtimeClient.CurrentRoom.IsOpen) PhotonRealtimeClient.OpenRoom();

            if (PhotonRealtimeClient.InMatchmakingRoom)
            {
                OnFailedToStartMatchmakingGame?.Invoke();
                GameType gameType = (GameType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey);
                OnStopMatchmakingEvent(new(gameType));
            }
        }

        private IEnumerator StartQuantum(StartGameData data)
        {
            Debug.Log("Starting Quantum");
            string battleID = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(BattleID);

            try
            {
                // Getting the index of own user id from the player slot user id array to determine which player slot is for local player.
                string userId = PhotonRealtimeClient.LocalPlayer.UserId;
                int slotIndex = Array.IndexOf(data.PlayerSlotUserIds, userId);
                if (slotIndex < 0 || slotIndex >= RuntimePlayer.PlayerSlots.Length)
                {
                    Debug.LogError($"Player userId '{userId}' not found in StartGameData.PlayerSlotUserIds: [{string.Join(", ", data.PlayerSlotUserIds)}]. Cannot start Quantum.");
                    OnLobbyWindowChangeRequest?.Invoke(LobbyWindowTarget.MainMenu);
                    yield break;
                }
                BattlePlayerSlot playerSlot = RuntimePlayer.PlayerSlots[slotIndex];

                // Setting map to variable
                BattleMap battleMap = _battleMapReference.GetBattleMap(data.MapId);
                if (battleMap == null)
                {
                    Debug.LogError($"BattleMap with id '{data.MapId}' not found. Cannot start Quantum.");
                    OnLobbyWindowChangeRequest?.Invoke(LobbyWindowTarget.MainMenu);
                    yield break;
                }
                Map map = battleMap.Map;
                if (map != null) _quantumBattleMap = map;

                bool AreAllExpectedPlayersPresent()
                {
                    if (data == null || data.PlayerSlotUserIds == null) return true;
                    foreach (string uid in data.PlayerSlotUserIds)
                    {
                        if (string.IsNullOrEmpty(uid) || uid == "Bot") continue;
                        bool present = PhotonRealtimeClient.CurrentRoom?.Players?.Values?.Any(p => p.UserId == uid) ?? false;
                        if (!present) return false;
                    }
                    return true;
                }

                // If any expected player already left, abort starting Quantum
                if (!AreAllExpectedPlayersPresent())
                {
                    Debug.LogWarning("Aborting StartQuantum: one or more expected players missing from the room.");
                    SafeRaiseEvent(
                        PhotonRealtimeClient.PhotonEvent.CancelGameStart,
                        null,
                        new RaiseEventArgs { Receivers = ReceiverGroup.All },
                        SendOptions.SendReliable
                    );

                    if (PhotonRealtimeClient.LocalPlayer.IsMasterClient)
                    {
                        if (_matchmakingHolder != null)
                        {
                            StopCoroutine(_matchmakingHolder);
                            _matchmakingHolder = null;
                        }
                        _matchmakingHolder = StartCoroutine(WaitForMatchmakingPlayers());
                    }
                    yield break;
                }

                if (QuantumRunner.Default != null)
                {
                    Debug.Log($"QuantumRunner is already running: {QuantumRunner.Default.Id}");
                    yield break;
                }

                RuntimeConfig config = new()
                {
                    // quantum
                    Map              = _quantumBattleMap,
                    SimulationConfig = _quantumBattleSimulationConfig,
                    SystemsConfig    = _quantumBattleSystemsConfig,
                    Seed             = data.Seed,

                    // battle
                    BattleConfig     = _battleQConfig,
                    BattleParameters = new()
                    {
                        PlayerNames = data.PlayerSlotUserNames,
                        PlayerSlotTypes = data.PlayerSlotTypes,
                        PlayerSlotUserIDs = data.PlayerSlotUserIds,
                        PlayerCount = data.PlayerCount,
                        ProjectileInitialEmotion = (BattleEmotionState)data.ProjectileInitialEmotion
                    }
                };

                SessionRunner.Arguments sessionRunnerArguments = new()
                {
                    RunnerFactory             = QuantumRunnerUnityFactory.DefaultFactory,
                    GameParameters            = QuantumRunnerUnityFactory.CreateGameParameters,
                    ClientId                  = ServerManager.Instance.Player._id,
                    RuntimeConfig             = config,
                    SessionConfig             = QuantumDeterministicSessionConfigAsset.Global.Config,
                    GameMode                  = Photon.Deterministic.DeterministicGameMode.Multiplayer,
                    PlayerCount               = PhotonRealtimeClient.CurrentRoom.MaxPlayers,
                    StartGameTimeoutInSeconds = 10,
                    Communicator              = new QuantumNetworkCommunicator(PhotonRealtimeClient.Client)
                };
                long sendTime = data.StartTime;

                // Start Battle Countdown (request UI to show countdown)
                OnLobbyWindowChangeRequest?.Invoke(LobbyWindowTarget.BattleLoad);
                _isStartFinished = false;

                // Wait for UI to subscribe to OnStartTimeSet (timeout to avoid hanging)
                const float onStartSubscribeTimeout = 5f;
                float subscribeStart = Time.time;
                while (OnStartTimeSet == null && Time.time - subscribeStart < onStartSubscribeTimeout)
                {
                    yield return null;
                }
                if (OnStartTimeSet == null)
                {
                    Debug.LogWarning("StartQuantum: OnStartTimeSet has no subscribers after timeout; proceeding without countdown UI.");
                }

                if (sendTime == 0) sendTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                long timeToStart = (sendTime + STARTDELAY) - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                long startTime = sendTime + timeToStart;

                // Notify listeners about start time (if any)
                if (OnStartTimeSet != null)
                {
                    try
                    {
                        OnStartTimeSet?.Invoke(sendTime);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"StartQuantum: OnStartTimeSet invocation threw: {ex.Message}");
                    }
                }

                // Wait for UI to finish the start animation / loading sequence. Use a timeout to avoid permanent blocking.
                const float startFinishTimeout = 15f;
                float startWaitStart = Time.time;
                while (!_isStartFinished && Time.time - startWaitStart < startFinishTimeout)
                {
                    yield return null;
                }
                if (!_isStartFinished)
                {
                    Debug.LogWarning("StartQuantum: timed out waiting for start animation to finish; proceeding.");
                    _isStartFinished = true;
                }

                // Final check before loading Battle scene / starting runner
                if (!AreAllExpectedPlayersPresent())
                {
                    Debug.LogWarning("Aborting StartQuantum before runner start: expected player missing.");
                    SafeRaiseEvent(
                        PhotonRealtimeClient.PhotonEvent.CancelGameStart,
                        null,
                        new RaiseEventArgs { Receivers = ReceiverGroup.All },
                        SendOptions.SendReliable
                    );

                    if (PhotonRealtimeClient.LocalPlayer.IsMasterClient)
                    {
                        if (_matchmakingHolder != null)
                        {
                            StopCoroutine(_matchmakingHolder);
                            _matchmakingHolder = null;
                        }
                        _matchmakingHolder = StartCoroutine(WaitForMatchmakingPlayers());
                    }
                    yield break;
                }

                AudioManager.Instance.StopMusic();

                // Move to Battle and start Runner
                OnLobbyWindowChangeRequest?.Invoke(LobbyWindowTarget.Battle);

                // Wait for scene to load, but timeout to avoid blocking forever
                const float sceneLoadTimeout = 30f;
                float sceneLoadStart = Time.time;
                while (SceneManager.GetActiveScene().name != _quantumBattleMap.Scene && Time.time - sceneLoadStart < sceneLoadTimeout)
                {
                    yield return null;
                }
                if (SceneManager.GetActiveScene().name != _quantumBattleMap.Scene)
                {
                    Debug.LogError($"StartQuantum: Scene '{_quantumBattleMap.Scene}' failed to load within timeout.");
                    StartingGameFailed();
                    yield break;
                }

                DebugLogFileHandler.ContextEnter(DebugLogFileHandler.ContextID.Battle);
                DebugLogFileHandler.FileOpen(battleID, (int)playerSlot);

                // Always load current player characters before AddPlayer.
                // In the Custom room flow, SetPlayerQuantumCharacters is called by RoomSetupManager,
                // but in the Matchmaking flow it was never called — leaving _player.Characters stale.
                // Loading here ensures all game types have correct, up-to-date character data
                // with pre-resolved entity prototypes (critical for Quantum determinism).
                {
                    string playerGuid = GameConfig.Get().PlayerSettings.PlayerGuid;
                    PlayerData playerData = null;
                    Storefront.Get().GetPlayerData(playerGuid, p => playerData = p);
                    yield return new WaitUntil(() => playerData != null);

                    List<CustomCharacter> selectedCharacters = new();
                    var battleCharacters = playerData.CurrentBattleCharacters;
                    for (int i = 0; i < battleCharacters.Count; i++)
                    {
                        selectedCharacters.Add(battleCharacters[i]);
                    }
                    SetPlayerQuantumCharacters(selectedCharacters);
                }

                int retryCount=0;
                do
                {
                    Task<bool> task = StartRunner(sessionRunnerArguments);

                    yield return new WaitUntil(() => task.IsCompleted);
                    if (task.Result)
                    {
                        _player.PlayerSlot = playerSlot;
                        _player.UserID = userId;
                        _runner?.Game.AddPlayer(_player);
                        break;
                    }
                    else
                    {
                        retryCount++;
                        if (retryCount == 5) { OnLobbyWindowChangeRequest?.Invoke(LobbyWindowTarget.MainMenu); break; }
                    }
                } while (true);
            }
            finally
            {
                _startQuantumHolder = null;
            }
        }

        private async Task<bool> StartRunner(SessionRunner.Arguments sessionRunnerArguments)
        {
            string pluginDisconnectReason = null;
            IDisposable pluginDisconnectListener = QuantumCallback.SubscribeManual<CallbackPluginDisconnect>(m => pluginDisconnectReason = m.Reason);

            _runner = null;
            try
            {
                _runner = (QuantumRunner)await SessionRunner.StartAsync(sessionRunnerArguments);
            }
            catch (Exception ex)
            {
                pluginDisconnectListener.Dispose();
                Debug.LogException(ex);
                return false;
            }
            return true;
        }

        public static void ExitQuantum(bool winningTeam, float gameLengthSec)
        {
            CloseRunner();
            DataCarrier.AddData(DataCarrier.BattleWinner,winningTeam);
            OnLobbyWindowChangeRequest?.Invoke(LobbyWindowTarget.BattleStory);
        }

        public static void CloseRunner()
        {
            AudioManager.Instance.StopMusic();
            QuantumRunner.ShutdownAll();
            DebugLogFileHandler.ContextEnter(DebugLogFileHandler.ContextID.MenuUI);
        }

        public static void ExitBattleStory()
        {
            OnLobbyWindowChangeRequest?.Invoke(LobbyWindowTarget.MainMenu, LobbyWindowTarget.BattleStory);
        }

        private static IEnumerator StartTheRaidTestRoom()
        {
            //Debug.Log($"RAID TEST {raidScene}");
            if (!PhotonRealtimeClient.LocalPlayer.IsMasterClient)
            {
                throw new UnityException("only master client can start this game");
            }
            yield return null;
            /*if (!raidScene.IsNetworkScene)
            {
                throw new UnityException($"scene {raidScene} IsNetworkScene = false");
            }*/
            //PhotonRealtimeClient.LoadLevel(raidScene.SceneName);
            //StartCoroutine(StartTheRaidTestRoom(_raidScene));
            OnLobbyWindowChangeRequest?.Invoke(LobbyWindowTarget.Raid);
        }

        private bool _playerPosChangeInProgress = false;

        private IEnumerator SetPlayer(Player player, int playerPosition)
        {
            if (_posChangeQueue.Contains(player.UserId)) yield break;

            _posChangeQueue.Add(player.UserId);

            yield return new WaitUntil(() => !_playerPosChangeInProgress);

            _playerPosChangeInProgress = true;
            // Checking if any of the players in the room are already in the position (value is anything else than empty string) and if so return.
            if (PhotonBattleRoom.CheckIfPositionIsFree(playerPosition) == false)
            {
                Debug.LogWarning("Requested position is not free.");
                _posChangeQueue.Remove(player.UserId);
                _playerPosChangeInProgress = false;
                yield break;
            }

            Assert.IsTrue(PhotonLobbyRoom.IsValidGameplayPosOrGuest(playerPosition));

            // Initializing hash tables for setting the new position as taken
            string newPositionKey = PhotonBattleRoom.GetPositionKey(playerPosition);

            if (!player.HasCustomProperty(PlayerPositionKey))
            {
                Debug.Log($"setPlayer {PlayerPositionKey}={playerPosition}");
                var position = new LobbyPhotonHashtable(new Dictionary<object, object> { { newPositionKey, player.UserId } });
                var eValue = new LobbyPhotonHashtable(new Dictionary<object, object> { { newPositionKey, "" } }); // Expecting the new position to be empty
                if (PhotonRealtimeClient.LobbyCurrentRoom.SetCustomProperties(position, eValue))
                {
                    float timeout = Time.time + 1f;
                    bool success = false;
                    while (Time.time < timeout)
                    {
                        string positionValue = PhotonRealtimeClient.LobbyCurrentRoom.GetCustomProperty<string>(newPositionKey, "");
                        if (positionValue == player.UserId)
                        {
                            success = true;
                            break;
                        }
                        else if (!PhotonBattleRoom.CheckIfPositionIsFree(playerPosition))
                        {
                            break;
                        }
                        yield return new WaitForSeconds(0.1f);
                    }

                    if (success)
                    {
                        player.SetCustomProperty(PlayerPositionKey, playerPosition);
                    }
                }

                _posChangeQueue.Remove(player.UserId);
                _playerPosChangeInProgress = false;
                yield break;
            }

            // Initializing hash tables for setting the previous position empty
            int curValue = player.GetCustomProperty<int>(PlayerPositionKey);
            string previousPositionKey = PhotonBattleRoom.GetPositionKey(curValue);

            var newPosition = new LobbyPhotonHashtable(new Dictionary<object, object> { { newPositionKey, player.UserId } });
            var expectedValue = new LobbyPhotonHashtable(new Dictionary<object, object> { { newPositionKey, "" } }); // Expecting the new position to be empty

            // Setting new position as taken
            if (PhotonRealtimeClient.LobbyCurrentRoom.SetCustomProperties(newPosition, expectedValue))
            {
                float timeout = Time.time + 1f;
                bool success = false;
                while (Time.time < timeout)
                {
                    // Checking if the position is set to the player user id
                    if (PhotonRealtimeClient.LobbyCurrentRoom.GetCustomProperty<string>(newPositionKey) == player.UserId)
                    {
                        success = true;
                        break;
                    }
                    else if (!PhotonBattleRoom.CheckIfPositionIsFree(playerPosition))
                    {
                        Debug.LogWarning($"Failed to reserve the position {playerPosition}. This likely because somebody already is in this position.");
                        break;
                    }
                    yield return new WaitForSeconds(0.1f);
                }

                if (success)
                {
                    // Setting new position to player's custom properties
                    player.SetCustomProperty(PlayerPositionKey, playerPosition);

                    var emptyPosition = new LobbyPhotonHashtable(new Dictionary<object, object> { { previousPositionKey, "" } });
                    expectedValue = new LobbyPhotonHashtable(new Dictionary<object, object> { { previousPositionKey, player.UserId } }); // Expected to have the player's id in the previous position

                    // Setting previous position empty
                    if (!PhotonRealtimeClient.LobbyCurrentRoom.SetCustomProperties(emptyPosition, expectedValue))
                    {
                        Debug.LogWarning($"Failed to free the position {curValue}. This likely because the player doesn't reserve it.");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Failed to reserve the position {playerPosition}. This likely because somebody already is in this position.");
            }
            _posChangeQueue.Remove(player.UserId);
            _playerPosChangeInProgress = false;
            yield break;
        }

        private IEnumerator SetBot(int playerPosition, bool active)
        {
            yield return new WaitUntil(() => !_playerPosChangeInProgress);

            _playerPosChangeInProgress = true;
            // Checking if any of the players in the room are already in the position (value is anything else than empty string) and if so return.
            if (PhotonBattleRoom.CheckIfPositionIsFree(playerPosition) == false && active)
            {
                Debug.LogWarning("Requested position is not free.");
                _playerPosChangeInProgress = false;
                yield break;
            }
            else if (!PhotonBattleRoom.CheckIfPositionIsFree(playerPosition) == false && !active)
            {
                Debug.LogWarning("Requested is already empty.");
                _playerPosChangeInProgress = false;
                yield break;
            }

            Assert.IsTrue(PhotonLobbyRoom.IsValidGameplayPosOrGuest(playerPosition));

            // Initializing hash tables for setting the new position as taken
            string newPositionKey = PhotonBattleRoom.GetPositionKey(playerPosition);

            LobbyPhotonHashtable newPosition;
            LobbyPhotonHashtable expectedValue;
            if (active)
            {
                newPosition = new LobbyPhotonHashtable(new Dictionary<object, object> { { newPositionKey, "Bot" } });
                expectedValue = new LobbyPhotonHashtable(new Dictionary<object, object> { { newPositionKey, "" } }); // Expecting the new position to be empty

                if (PhotonRealtimeClient.LobbyCurrentRoom.SetCustomProperties(newPosition, expectedValue))
                {
                    float timeout = Time.time + 1f;
                    bool success = false;
                    while (Time.time < timeout)
                    {
                        // Checking if the position is set to have a Bot
                        if (PhotonRealtimeClient.LobbyCurrentRoom.GetCustomProperty<string>(newPositionKey) == "Bot")
                        {
                            success = true;
                            break;
                        }
                        else if (!PhotonBattleRoom.CheckIfPositionIsFree(playerPosition))
                        {
                            Debug.LogWarning($"Failed to reserve the position {playerPosition}. This likely because somebody already is in this position.");
                            break;
                        }
                        yield return new WaitForSeconds(0.1f);
                    }

                    if (success)
                    {
                        Debug.Log($"Set Bot to position {playerPosition}");
                        Room room = PhotonRealtimeClient.CurrentRoom;
                        int playerCount = room.PlayerCount;
                        int botCount = PhotonBattleRoom.GetBotCount();
                        if (playerCount + botCount >= room.MaxPlayers) PhotonRealtimeClient.CloseRoom();
                    }
                }
                else
                {
                    Debug.LogWarning($"Failed to reserve the position {playerPosition}. This likely because somebody already is in this position.");
                }
            }
            else
            {
                newPosition = new LobbyPhotonHashtable(new Dictionary<object, object> { { newPositionKey, "" } });
                expectedValue = new LobbyPhotonHashtable(new Dictionary<object, object> { { newPositionKey, "Bot" } }); // Expecting the position to have a bot

                if (PhotonRealtimeClient.LobbyCurrentRoom.SetCustomProperties(newPosition, expectedValue))
                {
                    float timeout = Time.time + 1f;
                    bool success = false;
                    while (Time.time < timeout)
                    {
                        // Checking if the position is set to have a Bot
                        if (PhotonRealtimeClient.LobbyCurrentRoom.GetCustomProperty<string>(newPositionKey) == "")
                        {
                            success = true;
                            break;
                        }
                        else if (PhotonBattleRoom.CheckIfPositionIsFree(playerPosition))
                        {
                            Debug.LogWarning($"Slot is free? Wait? How did you end up here?");
                            success = true;
                            break;
                        }
                        yield return new WaitForSeconds(0.1f);
                    }

                    if (success)
                    {
                        Debug.Log($"Freed position {playerPosition}");
                        Room room = PhotonRealtimeClient.CurrentRoom;
                        if (!room.IsOpen) PhotonRealtimeClient.OpenRoom();
                    }
                }
                else
                {
                    Debug.LogWarning($"Failed to reserve the position {playerPosition}. This likely because somebody already is in this position.");
                }
            }

            _playerPosChangeInProgress = false;
            yield break;
        }

        private IEnumerator SetBotFill(bool active)
        {
            LobbyPhotonHashtable newValue;
            if (active)
            {
                newValue = new LobbyPhotonHashtable(new Dictionary<object, object> { { PhotonBattleRoom.BotFillKey, true } });

                if (PhotonRealtimeClient.LobbyCurrentRoom.SetCustomProperties(newValue))
                {
                    float timeout = Time.time + 1f;
                    bool success = false;
                    while (Time.time < timeout)
                    {
                        // Checking if the position is set to have a Bot
                        if (PhotonRealtimeClient.LobbyCurrentRoom.GetCustomProperty<bool>(PhotonBattleRoom.BotFillKey) == true)
                        {
                            success = true;
                            break;
                        }
                        yield return new WaitForSeconds(0.1f);
                    }

                    if (success)
                    {
                        Debug.Log($"Set BotFill to {active}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Failed to activate bot fill. Something borke really bad.");
                }
            }
            else
            {
                newValue = new LobbyPhotonHashtable(new Dictionary<object, object> { { PhotonBattleRoom.BotFillKey, false } });

                if (PhotonRealtimeClient.LobbyCurrentRoom.SetCustomProperties(newValue))
                {
                    float timeout = Time.time + 1f;
                    bool success = false;
                    while (Time.time < timeout)
                    {
                        // Checking if the position is set to have a Bot
                        if (PhotonRealtimeClient.LobbyCurrentRoom.GetCustomProperty<bool>(PhotonBattleRoom.BotFillKey) == false)
                        {
                            success = true;
                            break;
                        }
                        yield return new WaitForSeconds(0.1f);
                    }

                    if (success)
                    {
                        Debug.Log($"Set BotFill to {active}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Failed to activate bot fill. Something borke really bad.");
                }
            }

            yield break;
        }

        private IEnumerator VerifyRoomPositionsLoop()
        {
            try
            {
                while (PhotonRealtimeClient.InRoom && PhotonRealtimeClient.LocalPlayer.IsMasterClient)
                {
                    try
                    {
                        Room room = PhotonRealtimeClient.CurrentRoom;
                        if (room != null)
                        {
                            var existingUserIds = new HashSet<string>(room.Players.Values.Select(p => p.UserId));
                            string[] posKeys = {
                                PhotonBattleRoom.PlayerPositionKey1,
                                PhotonBattleRoom.PlayerPositionKey2,
                                PhotonBattleRoom.PlayerPositionKey3,
                                PhotonBattleRoom.PlayerPositionKey4
                            };

                            foreach (var key in posKeys)
                            {
                                string val = room.GetCustomProperty<string>(key, "");
                                if (string.IsNullOrEmpty(val)) continue;
                                if (val == "Bot") continue;
                                if (!existingUserIds.Contains(val))
                                {
                                    var emptyPosition = new LobbyPhotonHashtable(new Dictionary<object, object> { { key, "" } });
                                    var expectedValue = new LobbyPhotonHashtable(new Dictionary<object, object> { { key, val } });
                                    try
                                    {
                                        PhotonRealtimeClient.LobbyCurrentRoom.SetCustomProperties(emptyPosition, expectedValue);
                                        Debug.Log($"VerifyRoomPositionsLoop: cleared stale position {key} (value {val}).");
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.LogWarning($"VerifyRoomPositionsLoop: failed to clear {key}: {ex.Message}");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"VerifyRoomPositionsLoop: unexpected error: {ex.Message}");
                    }
                    yield return new WaitForSeconds(2f);
                }
            }
            finally
            {
                _verifyPositionsHolder = null;
            }
        }

        public void SetPlayerQuantumCharacters(List<CustomCharacter> characters)
        {
            Assert.IsTrue(
                characters.Count == RuntimePlayer.CharacterCount,
                string.Format("Invalid number of Characters (not {0})", RuntimePlayer.CharacterCount)
            );

            CustomCharacter character;
            for (int i = 0; i < RuntimePlayer.CharacterCount; i++) {
                character = characters[i];

                if(character == null || character.Id is CharacterID.None)
                {
                    character = new(PlayerCharacterPrototypes.GetCharacter("-1",true).BaseCharacter);
                }

                var stats = new BattlePlayerStats()
                {
                    Attack        = BaseCharacter.GetStatValueFP(StatType.Attack, character.Attack),
                    Defence       = BaseCharacter.GetStatValueFP(StatType.Defence, character.Defence),
                    CharacterSize = BaseCharacter.GetStatValueFP(StatType.CharacterSize, character.CharacterSize),
                    Speed         = BaseCharacter.GetStatValueFP(StatType.Speed, character.Speed)
                };

                // Pre-resolve the entity prototype in View code so Simulation never calls into the View layer.
                // This is critical for Quantum determinism — calling BattleAltzoneLink from Simulation causes checksum errors.
                int characterId = (int)character.Id;
                PlayerCharacterPrototype protoInfo = PlayerCharacterPrototypes.GetCharacter(characterId.ToString());
                AssetRef<EntityPrototype> prototype = protoInfo != null ? protoInfo.BattleEntityPrototype : default;

                _player.Characters[i] = new BattleCharacterBase()
                {
                    Id            = characterId,
                    Class         = (int)character.CharacterClassType,
                    Stats         = stats,
                    Prototype     = prototype,
                };
            }
        }

        private void StopHolderCoroutines()
        {
            if (_reserveFreePositionHolder != null)
            {
                StopCoroutine(_reserveFreePositionHolder);
                _reserveFreePositionHolder = null;
            }

            if (_requestPositionChangeHolder != null)
            {
                StopCoroutine(_requestPositionChangeHolder);
                _requestPositionChangeHolder = null;
            }

            if (_matchmakingHolder != null)
            {
                StopCoroutine(_matchmakingHolder);
                _matchmakingHolder = null;
                _teammates = null;
            }

            if (_autoJoinHolder != null)
            {
                StopCoroutine(_autoJoinHolder);
                _autoJoinHolder = null;
            }

            if (_followLeaderHolder != null)
            {
                StopCoroutine(_followLeaderHolder);
                _followLeaderHolder = null;
            }
        }

        private void OnGetKickedEvent(GetKickedEvent data)
        {
            PhotonRealtimeClient.LeaveRoom();
            OnKickedOutOfTheRoom?.Invoke(data.Reason);
        }

        public void OnDisconnected(DisconnectCause cause)
        {
            // Stopping any coroutines which are stored in holder variables
            StopHolderCoroutines();

            Debug.Log($"OnDisconnected {cause}");
            if (cause != DisconnectCause.ApplicationQuit)
            {
                GameConfig gameConfig = GameConfig.Get();
                PlayerSettings playerSettings = gameConfig.PlayerSettings;
                string photonRegion = string.IsNullOrEmpty(playerSettings.PhotonRegion) ? null : playerSettings.PhotonRegion;
                StartCoroutine(StartLobby(playerSettings.PlayerGuid, playerSettings.PhotonRegion));
            }
            LobbyOnDisconnected?.Invoke();
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log($"OnPlayerLeftRoom {otherPlayer.GetDebugLabel()}");

            if (PhotonRealtimeClient.Client.State == ClientState.Leaving) return;

            // If a game start countdown or start flow is in progress, cancel it.
            bool startCountdownInProgress = _startGameHolder != null || _startQuantumHolder != null;
            if (!startCountdownInProgress && PhotonRealtimeClient.CurrentRoom != null)
            {
                try
                {
                    startCountdownInProgress = PhotonRealtimeClient.CurrentRoom.CustomProperties.ContainsKey(PhotonBattleRoom.BattleID);
                }
                catch { }
            }

            if (startCountdownInProgress)
            {
                // Read current game type if available
                GameType currentRoomGameType = GameType.Random2v2;
                try
                {
                    if (PhotonRealtimeClient.CurrentRoom != null)
                    {
                        currentRoomGameType = (GameType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey);
                    }
                }
                catch { }

                // If this is a Custom game, avoid auto-leave/follow behavior triggered by countdown cancel.
                bool isCustomRoom = false;
                try
                {
                    if (PhotonRealtimeClient.CurrentRoom != null && PhotonRealtimeClient.CurrentRoom.CustomProperties != null
                        && PhotonRealtimeClient.CurrentRoom.CustomProperties.ContainsKey(PhotonBattleRoom.GameTypeKey))
                    {
                        isCustomRoom = (GameType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey) == GameType.Custom;
                    }
                }
                catch { }

                if (PhotonRealtimeClient.LocalPlayer.IsMasterClient)
                {
                    // Broadcast CancelGameStart with requeue instruction and the game type (skip broadcasting for Custom games)
                    if (!isCustomRoom)
                    {
                        // Cancel the start but do not force clients to leave; master will requeue silently.
                        SafeRaiseEvent(
                            PhotonRealtimeClient.PhotonEvent.CancelGameStart,
                            new object[] { false, (int)currentRoomGameType },
                            new RaiseEventArgs { Receivers = ReceiverGroup.All },
                            SendOptions.SendReliable
                        );
                    }

                    if (_startGameHolder != null)
                    {
                        StopCoroutine(_startGameHolder);
                        _startGameHolder = null;
                    }

                    // Master should restart matchmaking in the current room (stay and wait/auto-fill) instead of leaving.
                    if (PhotonRealtimeClient.InMatchmakingRoom)
                    {
                        if (!isCustomRoom)
                        {
                            if (_matchmakingHolder != null)
                            {
                                StopCoroutine(_matchmakingHolder);
                                _matchmakingHolder = null;
                            }
                            // Restart waiting loop in the same room so teammates remain together.
                            _matchmakingHolder = StartCoroutine(WaitForMatchmakingPlayers());
                        }
                        else
                        {
                            // In Custom games, do not initiate matchmaking or notify other clients to follow.
                            StartingGameFailed();
                        }
                    }
                    else
                    {
                        StartingGameFailed();
                    }
                }
                else
                {
                    // Stop any client-side start coroutine and notify UI
                    if (_startQuantumHolder != null)
                    {
                        StopCoroutine(_startQuantumHolder);
                        _startQuantumHolder = null;
                    }

                    OnGameStartCancelled?.Invoke();

                    try
                    {
                        StopMatchmakingCoroutines();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Failed to stop matchmaking coroutines: {ex.Message}");
                    }

                    // Ensure non-master clients re-reserve a free position when a start is cancelled.
                    if (!PhotonRealtimeClient.LocalPlayer.IsMasterClient && PhotonRealtimeClient.InMatchmakingRoom)
                    {
                        try
                        {
                            if (!PhotonRealtimeClient.LocalPlayer.HasCustomProperty(PlayerPositionKey))
                            {
                                if (_reserveFreePositionHolder == null)
                                {
                                    _reserveFreePositionHolder = StartCoroutine(ReserveFreePosition(true));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"CancelGameStart: failed to reserve position: {ex.Message}");
                        }
                    }

                    // Non-master clients should leave the previous room, return to main menu and attempt to requeue
                    if (PhotonRealtimeClient.InRoom)
                    {
                        // If the player who left was the master, defer returning to the LobbyRoom
                        // until the master switch completes to avoid race conditions.
                        try
                        {
                            if (otherPlayer != null && otherPlayer.IsMasterClient)
                            {
                                _deferReturnToLobbyRoomOnMasterSwitch = true;
                            }
                            else
                            {
                                // Do not force non-master clients to leave; just return to the room UI and remain queued.
                                OnLobbyWindowChangeRequest?.Invoke(LobbyWindowTarget.LobbyRoom);
                            }
                        }
                        catch
                        {
                            OnLobbyWindowChangeRequest?.Invoke(LobbyWindowTarget.LobbyRoom);
                        }
                    }
                }
            }

            // Clearing the player position in the room if player is master client
            if (PhotonRealtimeClient.LocalPlayer.IsMasterClient)
            {
                int otherPlayerPosition = otherPlayer.GetCustomProperty<int>(PlayerPositionKey);
                if (!PhotonLobbyRoom.IsValidPlayerPos(otherPlayerPosition)) return;
                string positionKey = PhotonBattleRoom.GetPositionKey(otherPlayerPosition);

                var emptyPosition = new LobbyPhotonHashtable(new Dictionary<object, object> { { positionKey, "" } });
                var expectedValue = new LobbyPhotonHashtable(new Dictionary<object, object> { { positionKey, otherPlayer.UserId } });

                PhotonRealtimeClient.LobbyCurrentRoom.SetCustomProperties(emptyPosition, expectedValue);
                if(_posChangeQueue.Contains(otherPlayer.UserId)) _posChangeQueue.Remove(otherPlayer.UserId);
            }

            if (PhotonRealtimeClient.InMatchmakingRoom && _followLeaderHolder == null)
            {
                // If the game type is clan 2v2 and the player who left was a teammate we leave the room,
                // since you can't play the game mode without 2 person team from the same clan
                GameType roomGameType = (GameType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey);
                if (roomGameType == GameType.Clan2v2)
                {
                    string ownClan = PhotonRealtimeClient.LocalPlayer.GetCustomProperty<string>(PhotonBattleRoom.ClanNameKey);
                    string otherPlayerClan = otherPlayer.GetCustomProperty<string>(PhotonBattleRoom.ClanNameKey);

                    if (ownClan == otherPlayerClan)
                    {
                        _teammates = null;
                        StartCoroutine(LeaveMatchmaking());
                        OnClanMemberDisconnected?.Invoke();
                    }
                    return;
                }

                // Checking if the other player who left was local player's leader
                string matchmakingLeaderId = PhotonRealtimeClient.LocalPlayer.GetCustomProperty(PhotonBattleRoom.LeaderIdKey, string.Empty);
                if (matchmakingLeaderId == otherPlayer.UserId)
                {
                    // Clear leader id; new master will be set in OnMasterClientSwitched
                    try
                    {
                        PhotonRealtimeClient.LocalPlayer.RemoveCustomProperty(PhotonBattleRoom.LeaderIdKey);
                    }
                    catch { }
                    OnRoomLeaderChanged?.Invoke(false);
                }
            }

            Room room = PhotonRealtimeClient.CurrentRoom;
            int playerCount = room.PlayerCount;
            int botCount = PhotonBattleRoom.GetBotCount();

            try
            {
                if (PhotonRealtimeClient.LocalPlayer.IsMasterClient && playerCount + botCount < room.MaxPlayers && !PhotonRealtimeClient.CurrentRoom.IsOpen)
                {
                    PhotonRealtimeClient.OpenRoom();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"OnPlayerLeftRoom: failed to open room: {ex.Message}");
            }

            // Master: ensure any stale player position keys are cleared when any player leaves
            if (PhotonRealtimeClient.LocalPlayer.IsMasterClient)
            {
                try
                {
                    if (room != null)
                    {
                        var existingUserIds = new HashSet<string>(room.Players.Values.Select(p => p.UserId));
                        string[] posKeys = {
                            PhotonBattleRoom.PlayerPositionKey1,
                            PhotonBattleRoom.PlayerPositionKey2,
                            PhotonBattleRoom.PlayerPositionKey3,
                            PhotonBattleRoom.PlayerPositionKey4
                        };

                        foreach (var key in posKeys)
                        {
                            string val = room.GetCustomProperty<string>(key, "");
                            if (string.IsNullOrEmpty(val)) continue;
                            if (val == "Bot") continue;
                            if (!existingUserIds.Contains(val))
                            {
                                var emptyPosition = new LobbyPhotonHashtable(new Dictionary<object, object> { { key, "" } });
                                var expectedValue = new LobbyPhotonHashtable(new Dictionary<object, object> { { key, val } });
                                try
                                {
                                    PhotonRealtimeClient.LobbyCurrentRoom.SetCustomProperties(emptyPosition, expectedValue);
                                    Debug.Log($"Cleared stale position {key} (value {val}) on player leave.");
                                }
                                catch (Exception ex2)
                                {
                                    Debug.LogWarning($"Failed to clear stale position {key}: {ex2.Message}");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"OnPlayerLeftRoom: failed to clean stale positions: {ex.Message}");
                }
            }

            LobbyOnPlayerLeftRoom?.Invoke(new(otherPlayer));

            // Ensure master continues matchmaking wait loop so countdowns can be restarted
            if (PhotonRealtimeClient.InMatchmakingRoom && PhotonRealtimeClient.LocalPlayer.IsMasterClient && _matchmakingHolder == null)
            {
                _matchmakingHolder = StartCoroutine(WaitForMatchmakingPlayers());
            }
        }

        public void OnJoinedRoom()
        {
            // Enable: PhotonNetwork.CloseConnection needs to to work across all clients - to kick off invalid players!
            PhotonRealtimeClient.EnableCloseConnection = true;

            // Getting info if room is matchmaking room or not
            if (PhotonRealtimeClient.InMatchmakingRoom)
            {
                // If we previously got a CancelGameStart and now rejoined matchmaking, leave and return to main menu
                if (_returnToMainMenuOnMatchmakingRejoin)
                {
                    _returnToMainMenuOnMatchmakingRejoin = false;
                    StopMatchmakingCoroutines();
                    PhotonRealtimeClient.LeaveRoom();
                    OnLobbyWindowChangeRequest?.Invoke(LobbyWindowTarget.MainMenu);
                    return;
                }

                // Ensure returning master does not reclaim leadership: clear LeaderIdKey if it points to own id but this client is not master.
                try
                {
                    if (PhotonRealtimeClient.LocalPlayer.HasCustomProperty(PhotonBattleRoom.LeaderIdKey))
                    {
                        string leaderProp = PhotonRealtimeClient.LocalPlayer.GetCustomProperty<string>(PhotonBattleRoom.LeaderIdKey, string.Empty);
                        if (!PhotonRealtimeClient.LocalPlayer.IsMasterClient && leaderProp == PhotonRealtimeClient.LocalPlayer.UserId)
                        {
                            PhotonRealtimeClient.LocalPlayer.RemoveCustomProperty(PhotonBattleRoom.LeaderIdKey);
                        }
                    }
                }
                catch { }

                bool isLeader = PhotonRealtimeClient.LocalPlayer.UserId == PhotonRealtimeClient.LocalPlayer.GetCustomProperty<string>(PhotonBattleRoom.LeaderIdKey);
                OnMatchmakingRoomEntered?.Invoke(isLeader);

                // Start auto-join only for non-master clients that are effectively alone in their matchmaking room.
                // Do not auto-join if the current room is a Custom game.
                bool inCustomRoom = false;
                try
                {
                    if (PhotonRealtimeClient.CurrentRoom != null && PhotonRealtimeClient.CurrentRoom.CustomProperties != null
                        && PhotonRealtimeClient.CurrentRoom.CustomProperties.ContainsKey(PhotonBattleRoom.GameTypeKey))
                    {
                        inCustomRoom = (GameType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey) == GameType.Custom;
                    }
                }
                catch { }

                if (!PhotonRealtimeClient.LocalPlayer.IsMasterClient && PhotonRealtimeClient.CurrentRoom != null && PhotonRealtimeClient.CurrentRoom.PlayerCount <= 1 && !inCustomRoom)
                {
                    if (_autoJoinHolder == null)
                    {
                        _autoJoinHolder = StartCoroutine(AutoJoinLargestMatchmakingRoom());
                    }
                }
                // If we're master client, start periodic verification of room position keys to clear stale reservations
                if (PhotonRealtimeClient.LocalPlayer.IsMasterClient && _verifyPositionsHolder == null)
                {
                    _verifyPositionsHolder = StartCoroutine(VerifyRoomPositionsLoop());
                }
            }
            else
            {
                LobbyOnJoinedRoom?.Invoke();
            }
        }

        private IEnumerator LeaveAndAutoRequeue(GameType gameType)
        {
            try
            {
                Debug.Log($"LeaveAndAutoRequeue: preparing to leave and requeue for {gameType}");

                // Stop any existing matchmaking/holder coroutines to avoid conflicts
                try { StopMatchmakingCoroutines(); } catch { }
                try { StopHolderCoroutines(); } catch { }

                // Leave current room and wait until in lobby
                if (PhotonRealtimeClient.InRoom) PhotonRealtimeClient.LeaveRoom();
                yield return new WaitUntil(() => PhotonRealtimeClient.InLobby);

                // Show main menu
                OnLobbyWindowChangeRequest?.Invoke(LobbyWindowTarget.MainMenu);

                // Slightly longer delay to let UI and network state settle
                yield return new WaitForSeconds(0.5f);

                // If local player is master, attempt to start matchmaking flow (master may already have started it)
                if (PhotonRealtimeClient.LocalPlayer.IsMasterClient)
                {
                    if (_matchmakingHolder != null)
                    {
                        StopCoroutine(_matchmakingHolder);
                        _matchmakingHolder = null;
                    }
                    _matchmakingHolder = StartCoroutine(StartMatchmaking(gameType));
                }
                else
                {
                    // Non-master: try to auto-join the largest available matchmaking room (skip for Custom game type)
                    Debug.Log($"LeaveAndAutoRequeue: non-master starting auto-join for {gameType}");
                    if (gameType != GameType.Custom)
                    {
                        if (_autoJoinHolder != null)
                        {
                            StopCoroutine(_autoJoinHolder);
                            _autoJoinHolder = null;
                        }
                        _autoJoinHolder = StartCoroutine(AutoJoinLargestMatchmakingRoom());
                    }
                    else
                    {
                        Debug.Log("LeaveAndAutoRequeue: skipping auto-join for Custom game type.");
                    }
                }
            }
            finally { }
        }

        public void OnLeftRoom() // IMatchmakingCallbacks
        {
            // Clearing player position key from own custom properties
            if (PhotonRealtimeClient.LocalPlayer.HasCustomProperty(PlayerPositionKey)) PhotonRealtimeClient.LocalPlayer.RemoveCustomProperty(PlayerPositionKey);

            // If position change coroutine is running stopping it
            if (_requestPositionChangeHolder != null)
            {
                StopCoroutine(_requestPositionChangeHolder);
                _requestPositionChangeHolder = null;
            }

            Debug.Log($"OnLeftRoom {PhotonRealtimeClient.LocalPlayer.GetDebugLabel()}");
            StartCoroutine(Service());
            // Stop verify loop when leaving room
            if (_verifyPositionsHolder != null)
            {
                StopCoroutine(_verifyPositionsHolder);
                _verifyPositionsHolder = null;
            }
            LobbyOnLeftRoom?.Invoke();
            // Goto lobby if we left (in)voluntarily any room
            // - typically master client kicked us off before starting a new game as we did not qualify to participate.
            // - can not use GoBack() because we do not know the reason for player leaving the room.
            // UPDATE 17.11.2023 - Since Lobby-scene has been moved to the main menu we will now load the main menu instead.
            //WindowManager.Get().ShowWindow(_mainMenuWindow);
        }

        public void OnCreatedRoom()
        {
            Debug.Log($"Created room {PhotonRealtimeClient.Client.CurrentRoom.Name}");
            StartCoroutine(Service());

            if (_matchmakingHolder == null)
            {
                LobbyOnCreatedRoom?.Invoke();
            }
            else
            {
                OnMatchmakingRoomEntered?.Invoke(true);
            }
        }
        public void OnJoinedLobby() { StartCoroutine(Service()); LobbyOnJoinedLobby?.Invoke(); }

        public void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            List<LobbyRoomInfo> lobbyRoomList = new();
            foreach (RoomInfo roomInfo in roomList)
            {
                lobbyRoomList.Add(new(roomInfo));
            }
            LobbyOnRoomListUpdate?.Invoke(lobbyRoomList);
        }
        public void OnLeftLobby() { LobbyOnLeftLobby?.Invoke(); }
        public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics) { LobbyOnLobbyStatisticsUpdate?.Invoke(); }
        public void OnFriendListUpdate(List<FriendInfo> friendList) {
            _friendList = friendList;
            LobbyOnFriendListUpdate?.Invoke();
        }
        public void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"CreateRoomFailed {returnCode} {message}");
            LobbyOnCreateRoomFailed?.Invoke(returnCode, message);
        }
        public void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"JoinRoomFailed {returnCode} {message}");
            // Signal to any waiting matchmaking coroutine that a join attempt failed
            _joinRoomFailed = true;
            LobbyOnJoinRoomFailed?.Invoke(returnCode, message);
        }
        public void OnJoinRandomFailed(short returnCode, string message) { LobbyOnJoinRandomFailed?.Invoke(returnCode, message); }

        public void OnEvent(EventData photonEvent)
        {
            if(photonEvent.Code != 103) Debug.Log($"Received PhotonEvent {photonEvent.Code}");

            switch (photonEvent.Code)
            {
                case PhotonRealtimeClient.PhotonEvent.CancelGameStart:
                {
                    Debug.Log("Received CancelGameStart");
                    // Parse optional requeue instruction: [bool requeue, int gameType]
                    bool requeueInstruction = false;
                    GameType requeueGameType = GameType.Random2v2;
                    try
                    {
                        if (photonEvent.CustomData is object[] arr && arr.Length > 0)
                        {
                            if (arr[0] is bool b) requeueInstruction = b;
                            if (arr.Length > 1 && arr[1] is int gi) requeueGameType = (GameType)gi;
                        }
                        else if (photonEvent.CustomData is PhotonHashtable pht)
                        {
                            if (pht.ContainsKey("requeue")) requeueInstruction = (bool)pht["requeue"];
                            if (pht.ContainsKey("gameType")) requeueGameType = (GameType)(int)pht["gameType"];
                        }
                    }
                    catch { }
                    // Ensure any local start coroutine is stopped
                    if (_startGameHolder != null)
                    {
                        StopCoroutine(_startGameHolder);
                        _startGameHolder = null;
                    }
                    if (_startQuantumHolder != null)
                    {
                        StopCoroutine(_startQuantumHolder);
                        _startQuantumHolder = null;
                    }

                    // Clear any return-to-main flag; keep players in-room and restore pre-countdown UI.
                    _returnToMainMenuOnMatchmakingRejoin = false;
                    OnGameStartCancelled?.Invoke();
                    // Also signal countdown listeners with a sentinel value so older UI code can cancel
                    OnGameCountdownUpdate?.Invoke(-1);

                    try
                    {
                        StopMatchmakingCoroutines();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Failed to stop matchmaking coroutines: {ex.Message}");
                    }

                    // Non-master clients: either return to LobbyRoom or leave and requeue if instructed
                        if (!PhotonRealtimeClient.LocalPlayer.IsMasterClient)
                        {
                            // If this is a Custom game, do not honour requeue instructions that force clients to leave.
                            bool isCustomRoom = false;
                            try
                            {
                                if (PhotonRealtimeClient.CurrentRoom != null && PhotonRealtimeClient.CurrentRoom.CustomProperties != null
                                    && PhotonRealtimeClient.CurrentRoom.CustomProperties.ContainsKey(PhotonBattleRoom.GameTypeKey))
                                {
                                    isCustomRoom = (GameType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey) == GameType.Custom;
                                }
                            }
                            catch { }

                            if (requeueInstruction)
                            {
                                if (!isCustomRoom)
                                {
                                    StartCoroutine(LeaveAndAutoRequeue(requeueGameType));
                                }
                                else
                                {
                                    Debug.Log("CancelGameStart: requeue requested but current room is Custom; staying in room.");
                                }
                            }
                            else
                            {
                                OnLobbyWindowChangeRequest?.Invoke(LobbyWindowTarget.LobbyRoom);
                            }
                        }

                        // Clear BattleID so cancelled start does not leave stale room state
                        try
                        {
                            if (PhotonRealtimeClient.CurrentRoom != null && PhotonRealtimeClient.CurrentRoom.CustomProperties.ContainsKey(PhotonBattleRoom.BattleID))
                            {
                                PhotonRealtimeClient.CurrentRoom.SetCustomProperties(new PhotonHashtable { { PhotonBattleRoom.BattleID, "" } });
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"Failed to clear BattleID on CancelGameStart: {ex.Message}");
                        }

                        break;
                    }
                case PhotonRealtimeClient.PhotonEvent.GameCountdown:
                    int countdown = (int)photonEvent.CustomData;
                    OnGameCountdownUpdate?.Invoke(countdown);
                    break;
                case PhotonRealtimeClient.PhotonEvent.StartGame:
                    // ByteArraySlice.Buffer may contain extra unused bytes beyond the actual data,
                    // so we must copy only the valid portion (Offset to Offset+Count) to avoid corrupt deserialization.
                    byte[] byteArray;
                    if (photonEvent.CustomData is byte[] directBytes)
                    {
                        byteArray = directBytes;
                    }
                    else if (photonEvent.CustomData is ByteArraySlice slice)
                    {
                        byteArray = new byte[slice.Count];
                        System.Buffer.BlockCopy(slice.Buffer, slice.Offset, byteArray, 0, slice.Count);
                    }
                    else
                    {
                        Debug.LogError($"StartGame event received with unexpected data type: {photonEvent.CustomData?.GetType()}");
                        break;
                    }
                    var startData = StartGameData.Deserialize(byteArray);

                    // Defensive check: if in matchmaking and any expected real player is missing, abort start.
                    if (PhotonRealtimeClient.InMatchmakingRoom)
                    {
                        bool missing = false;
                        foreach (string uid in startData.PlayerSlotUserIds)
                        {
                            if (string.IsNullOrEmpty(uid) || uid == "Bot") continue;
                            bool present = PhotonRealtimeClient.CurrentRoom?.Players?.Values?.Any(p => p.UserId == uid) ?? false;
                            if (!present)
                            {
                                missing = true;
                                Debug.LogWarning($"Received StartGame but player {uid} missing; aborting start.");
                                break;
                            }
                        }

                        if (missing)
                        {
                            OnGameStartCancelled?.Invoke();

                            if (PhotonRealtimeClient.LocalLobbyPlayer.IsMasterClient)
                            {
                                if (_matchmakingHolder != null)
                                {
                                    StopCoroutine(_matchmakingHolder);
                                    _matchmakingHolder = null;
                                }
                                _matchmakingHolder = StartCoroutine(WaitForMatchmakingPlayers());
                            }

                            break;
                        }
                    }

                    // Start the client-side StartQuantum coroutine and keep a holder so we can stop it if needed
                    if (_startQuantumHolder != null)
                    {
                        StopCoroutine(_startQuantumHolder);
                        _startQuantumHolder = null;
                    }
                    _startQuantumHolder = StartCoroutine(StartQuantum(startData));
                    break;
                case PhotonRealtimeClient.PhotonEvent.PlayerPositionChangeRequested:
                    int position = (int)photonEvent.CustomData;
                    Player player = PhotonRealtimeClient.CurrentRoom.GetPlayer(photonEvent.Sender);
                    if (player != null)
                    {
                        if (!_posChangeQueue.Contains(player.UserId)) StartCoroutine(SetPlayer(player, position));
                        else Debug.LogError($"Player {photonEvent.Sender} pos change already queued.");
                    }
                    else Debug.LogError($"Player {photonEvent.Sender} not found in room");

                    if (_canBattleStartCheckHolder == null) _canBattleStartCheckHolder = StartCoroutine(CheckIfBattleCanStart());
                    break;

                case PhotonRealtimeClient.PhotonEvent.RoomChangeRequested:
                {
                    string leaderUserId = (string)photonEvent.CustomData;
                    string matchmakingLeaderId = string.Empty;

                    // If room is not a matchmaking room the person sending the event is the leader.
                    if (!PhotonRealtimeClient.InMatchmakingRoom)
                    {
                        PhotonRealtimeClient.LocalPlayer.SetCustomProperty(PhotonBattleRoom.LeaderIdKey, leaderUserId);
                        matchmakingLeaderId = leaderUserId;
                    }

                    if (matchmakingLeaderId == string.Empty)
                    {
                        matchmakingLeaderId = PhotonRealtimeClient.LocalPlayer.GetCustomProperty(PhotonBattleRoom.LeaderIdKey, string.Empty);
                    }

                    // Do not follow leader to another room in Custom game mode.
                    bool isCustomRoom = false;
                    try
                    {
                        if (PhotonRealtimeClient.CurrentRoom != null && PhotonRealtimeClient.CurrentRoom.CustomProperties != null
                            && PhotonRealtimeClient.CurrentRoom.CustomProperties.ContainsKey(PhotonBattleRoom.GameTypeKey))
                        {
                            isCustomRoom = (GameType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey) == GameType.Custom;
                        }
                    }
                    catch { }

                    if (!isCustomRoom && _followLeaderHolder == null && leaderUserId == matchmakingLeaderId)
                    {
                        _followLeaderHolder = StartCoroutine(FollowLeaderToNewRoom(leaderUserId));
                    }
                    else if (isCustomRoom)
                    {
                        Debug.Log("RoomChangeRequested ignored: current room is Custom mode.");
                    }
                    break;
                }
            }
            LobbyOnEvent?.Invoke();
        }

        public void OnConnected() { LobbyOnConnected?.Invoke(); }
        public void OnConnectedToMaster() {
            LobbyOnConnectedToMaster?.Invoke();
            GameConfig gameConfig = GameConfig.Get();
            PlayerSettings playerSettings = gameConfig.PlayerSettings;
            string photonRegion = string.IsNullOrEmpty(playerSettings.PhotonRegion) ? null : playerSettings.PhotonRegion;
            StartCoroutine(StartLobby(playerSettings.PlayerGuid, playerSettings.PhotonRegion));
        }
        public void OnRegionListReceived(RegionHandler regionHandler) { LobbyOnRegionListReceived?.Invoke(); }
        public void OnCustomAuthenticationResponse(Dictionary<string, object> data) { LobbyOnCustomAuthenticationResponse?.Invoke(data); }
        public void OnCustomAuthenticationFailed(string debugMessage) { LobbyOnCustomAuthenticationFailed?.Invoke(debugMessage); }

        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            Room room = PhotonRealtimeClient.CurrentRoom;
            int playerCount = room.PlayerCount;
            int botCount = PhotonBattleRoom.GetBotCount();
            if (PhotonRealtimeClient.LocalPlayer.IsMasterClient)
            {
                if (playerCount + botCount == room.MaxPlayers && room.IsOpen) PhotonRealtimeClient.CloseRoom();

                if (_canBattleStartCheckHolder == null) _canBattleStartCheckHolder = StartCoroutine(CheckIfBattleCanStart());

                // Ensure master continues matchmaking loop so countdowns can be restarted when new players join
                if (PhotonRealtimeClient.InMatchmakingRoom && _matchmakingHolder == null)
                {
                    _matchmakingHolder = StartCoroutine(WaitForMatchmakingPlayers());
                }
            }
            if (playerCount + botCount <= room.MaxPlayers) LobbyOnPlayerEnteredRoom?.Invoke(new(newPlayer));
        }
        public void OnRoomPropertiesUpdate(PhotonHashtable propertiesThatChanged) { LobbyOnRoomPropertiesUpdate?.Invoke(new(propertiesThatChanged)); }
        public void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonHashtable changedProps) { LobbyOnPlayerPropertiesUpdate?.Invoke(new(targetPlayer),new(changedProps)); }
        public void OnMasterClientSwitched(Player newMasterClient) {
            LobbyOnMasterClientSwitched?.Invoke(new(newMasterClient));

            // Cancel any in-progress countdown locally when master changes (previous master might have started it)
            if (_startGameHolder != null)
            {
                StopCoroutine(_startGameHolder);
                _startGameHolder = null;
            }
            OnGameStartCancelled?.Invoke();

            // Ensure any stale BattleID is cleared when master changes so new master does not see a hanging start
            try
            {
                if (PhotonRealtimeClient.CurrentRoom != null && PhotonRealtimeClient.CurrentRoom.CustomProperties.ContainsKey(PhotonBattleRoom.BattleID))
                {
                    PhotonRealtimeClient.CurrentRoom.SetCustomProperties(new PhotonHashtable { { PhotonBattleRoom.BattleID, "" } });
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to clear BattleID on master switch: {ex.Message}");
            }

            // If we are in a matchmaking room, new master should continue matchmaking; others stay and wait
            if (PhotonRealtimeClient.InMatchmakingRoom)
            {
                // Update local player's known leader id to the current master so returning/disconnected players don't reclaim leadership.
                try
                {
                    PhotonRealtimeClient.LocalPlayer.SetCustomProperty(PhotonBattleRoom.LeaderIdKey, newMasterClient.UserId);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to set leader id on master switch: {ex.Message}");
                }

                if (PhotonRealtimeClient.LocalLobbyPlayer.IsMasterClient)
                {
                    if (_matchmakingHolder != null)
                    {
                        StopCoroutine(_matchmakingHolder);
                        _matchmakingHolder = null;
                    }
                    _matchmakingHolder = StartCoroutine(WaitForMatchmakingPlayers());
                }

                // If we deferred returning to the LobbyRoom because the master left,
                // perform the UI return now that master switch has completed (for non-master clients).
                try
                {
                    if (_deferReturnToLobbyRoomOnMasterSwitch)
                    {
                        if (!PhotonRealtimeClient.LocalLobbyPlayer.IsMasterClient && PhotonRealtimeClient.InRoom)
                        {
                            _deferReturnToLobbyRoomOnMasterSwitch = false;
                            OnLobbyWindowChangeRequest?.Invoke(LobbyWindowTarget.MainMenu);
                        }
                        else
                        {
                            _deferReturnToLobbyRoomOnMasterSwitch = false;
                        }
                    }
                }
                catch { _deferReturnToLobbyRoomOnMasterSwitch = false; }
            }

            // Start or stop verify loop depending on whether we are the new master
            try
            {
                if (PhotonRealtimeClient.LocalPlayer.IsMasterClient)
                {
                    if (_verifyPositionsHolder == null) _verifyPositionsHolder = StartCoroutine(VerifyRoomPositionsLoop());
                }
                else
                {
                    if (_verifyPositionsHolder != null)
                    {
                        StopCoroutine(_verifyPositionsHolder);
                        _verifyPositionsHolder = null;
                    }
                }
            }
            catch { }

            // New master should also clean up any stale player position keys left in room properties
            try
            {
                var room = PhotonRealtimeClient.CurrentRoom;
                if (room != null)
                {
                    var existingUserIds = new HashSet<string>(room.Players.Values.Select(p => p.UserId));
                    string[] posKeys = {
                        PhotonBattleRoom.PlayerPositionKey1,
                        PhotonBattleRoom.PlayerPositionKey2,
                        PhotonBattleRoom.PlayerPositionKey3,
                        PhotonBattleRoom.PlayerPositionKey4
                    };

                    foreach (var key in posKeys)
                    {
                        string val = room.GetCustomProperty<string>(key, "");
                        if (string.IsNullOrEmpty(val)) continue;
                        if (val == "Bot") continue;
                        if (!existingUserIds.Contains(val))
                        {
                            var emptyPosition = new LobbyPhotonHashtable(new Dictionary<object, object> { { key, "" } });
                            var expectedValue = new LobbyPhotonHashtable(new Dictionary<object, object> { { key, val } });
                            try
                            {
                                PhotonRealtimeClient.LobbyCurrentRoom.SetCustomProperties(emptyPosition, expectedValue);
                                Debug.Log($"Cleared stale position {key} (value {val}) on master switch.");
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning($"Failed to clear stale position {key}: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"OnMasterClientSwitched: failed to clean stale positions: {ex.Message}");
            }
        }

        public class ReserveFreePositionEvent
        {
        }

        public class PlayerPosEvent
        {
            public readonly int PlayerPosition;

            public PlayerPosEvent(int playerPosition)
            {
                PlayerPosition = playerPosition;
            }

            public override string ToString()
            {
                return $"{nameof(PlayerPosition)}: {PlayerPosition}";
            }
        }

        public class BotToggleEvent
        {
            public readonly int PlayerPosition;
            public readonly bool BotActive;

            public BotToggleEvent(int playerPosition, bool value)
            {
                PlayerPosition = playerPosition;
                BotActive = value;
            }

            public override string ToString()
            {
                return $"{nameof(PlayerPosition)}: {PlayerPosition}, {nameof(BotActive)}: {BotActive}";
            }
        }

        public class BotFillToggleEvent
        {
            public readonly bool BotFillActive;

            public BotFillToggleEvent( bool value)
            {
                BotFillActive = value;
            }

            public override string ToString()
            {
                return $"{nameof(BotFillActive)}: {BotFillActive}";
            }
        }

        public class StartRoomEvent
        {
        }

        public class StartPlayingEvent
        {
        }

        public class StartRaidTestEvent
        {
        }

        public class StartMatchmakingEvent
        {
            public readonly GameType SelectedGameType;

            public StartMatchmakingEvent(GameType gameType)
            {
                SelectedGameType = gameType;
            }

            public override string ToString()
            {
                return $"{nameof(SelectedGameType)}: {SelectedGameType}";
            }
        }

        public class StopMatchmakingEvent
        {
            public readonly GameType SelectedGameType;

            public StopMatchmakingEvent(GameType gameType)
            {
                SelectedGameType = gameType;
            }

            public override string ToString()
            {
                return $"{nameof(SelectedGameType)}: {SelectedGameType}";
            }
        }

        public class GetKickedEvent
        {
            public enum ReasonType
            {
                FullRoom,
                RoomLeader
            }

            public readonly ReasonType Reason;

            public GetKickedEvent(ReasonType reasonType)
            {
                Reason = reasonType;
            }

            public override string ToString()
            {
                return $"{nameof(Reason)}: {Enum.GetName(typeof(ReasonType), Reason)}";
            }
        }
    }


    public class StartGameData
    {
        public long StartTime { get; set; }
        public string[] PlayerSlotUserIds { get; set; }
        public string[] PlayerSlotUserNames { get; set; }
        public PlayerType[] PlayerSlotTypes { get; set; }
        public Emotion ProjectileInitialEmotion { get; set; }
        public string MapId { get; set; }
        public int PlayerCount { get; set; }
        public int Seed { get; set; }

        public static byte[] Serialize(StartGameData data)
        {
            var b = data;
            byte[] bytes = new byte[0];
            Serializer.Serialize(b.StartTime, ref bytes);
            Serializer.Serialize(b.PlayerSlotUserIds, ref bytes);
            Serializer.Serialize(b.PlayerSlotUserNames, ref bytes);
            Serializer.Serialize(b.PlayerSlotTypes.Cast<int>().ToArray(), ref bytes);
            Serializer.Serialize((int)b.ProjectileInitialEmotion, ref bytes);
            Serializer.Serialize(b.MapId, ref bytes);
            Serializer.Serialize(b.PlayerCount, ref bytes);
            Serializer.Serialize(b.Seed, ref bytes);

            return bytes;
        }

        public static StartGameData Deserialize(byte[] data)
        {
            var result = new StartGameData();
            int offset = 0;
            result.StartTime = Serializer.DeserializeLong(data, ref offset);
            result.PlayerSlotUserIds = Serializer.DeserializeStringArray(data, ref offset);
            result.PlayerSlotUserNames = Serializer.DeserializeStringArray(data, ref offset);
            result.PlayerSlotTypes = Serializer.DeserializeIntArray(data, ref offset).Cast<PlayerType>().ToArray();
            result.ProjectileInitialEmotion = (Emotion)Serializer.DeserializeInt(data, ref offset);
            result.MapId = Serializer.DeserializeString(data, ref offset);
            result.PlayerCount = Serializer.DeserializeInt(data, ref offset);
            result.Seed = Serializer.DeserializeInt(data, ref offset);

            return result;
        }

        public override string ToString()
        {
            return $"Start time: {StartTime}" +
                 $"\nPlayerSlotUserIds: {string.Join(", ", PlayerSlotUserIds)}" +
                 $"\nPlayerSlotUserNames: {string.Join(", ", PlayerSlotUserNames)}" +
                 $"\nPlayerSlotTypes: {string.Join(", ",PlayerSlotTypes)}" +
                 $"\nProjectileInitialEmotion: {ProjectileInitialEmotion}" +
                 $"\nMapId: {MapId}" +
                 $"\nPlayerCount: {PlayerCount}" +
                 $"\nSeed: {Seed}";
        }
    }
}
