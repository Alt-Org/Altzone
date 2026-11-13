using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Altzone.PhotonSerializer;
using Altzone.Scripts.Audio;
using Altzone.Scripts.AzDebug;
using Altzone.Scripts.Battle.Photon;
using Altzone.Scripts.Common;
using Altzone.Scripts.Config;
using Altzone.Scripts.Lobby.Wrappers;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.ModelV2;
using Altzone.Scripts.MQTT;
using Altzone.Scripts.Settings;
using Altzone.Scripts.Window;
using Battle.QSimulation.Game;
using Photon.Client;
using Photon.Realtime;
using Prg.Scripts.Common.PubSub;
using Quantum;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assert = UnityEngine.Assertions.Assert;
using PlayerType = Battle.QSimulation.Game.BattleParameters.PlayerType;
using Random = UnityEngine.Random;

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
        #region Constants & Serialized Fields

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

        #endregion

        #region Private Fields

        private const long STARTDELAY = 2000;
        private const float MatchmakingTimeoutSeconds = 10f;

        private QuantumRunner _runner = null;
        private BattleRoomInfo _roomInfo = null;

        private Coroutine _reserveFreePositionHolder = null;
        private Coroutine _requestPositionChangeHolder = null;
        private Coroutine _matchmakingHolder = null;
        private MatchmakingType _currentMatchmakingGameType = MatchmakingType.Random2v2;
        private GameType _currentGameType = GameType.BattlePingPong;
        private Coroutine _startGameHolder = null;
        // Holder for the client-side StartQuantum coroutine so it can be stopped if needed
        private Coroutine _startQuantumHolder = null;
        // Holder for the background Photon Service coroutine (single instance)
        private Coroutine _serviceHolder = null;
        private Coroutine _verifyPositionsHolder = null;
        private Coroutine _canBattleStartCheckHolder = null;

        private string _lastAutoInviteRoomName = string.Empty;
        private float _lastAutoInviteJoinTime = -100f;
        private string _pendingInRoomInviteRoomName = string.Empty;
        private string _pendingAcceptedInRoomInviteRoomName = string.Empty;
        private float _pendingAcceptedInRoomInviteStartTime = -100f;
        private readonly Dictionary<string, float> _declinedInRoomInviteUntil = new();
        private const float InRoomInvitePromptThrottleSeconds = 5f;
        private const float InRoomInviteDeclineCooldownSeconds = 30f;
        private const float InRoomInviteValiditySeconds = 60f;
        private const float InRoomInviteJoinTimeoutSeconds = 12f;
        // Photon player-property updates are asynchronous. Do not tell the matchmaking backend
        // that this client is ready until the server-assigned slot has been acknowledged locally.
        private const float ServerPhotonPositionReadyTimeoutSeconds = 5f;

        private List<FriendInfo> _friendList;

        [HideInInspector] public ReadOnlyCollection<LobbyRoomInfo> CurrentRooms = null; // Set from LobbyRoomListingController.cs through Instance variable maybe this could be refactored?

        private List<string> _posChangeQueue = new();

        private bool _isStartFinished = false;
        // Tracks whether a game start countdown is active (GameCountdown > 0)
        private bool _countdownActive = false;
        // Timestamp when a GameCountdown > 0 was last observed locally (Time.time)
        private float _lastCountdownStartTime = -100f;
        // True after StartGame has been received for the current room.
        private bool _matchHasStartedInCurrentRoom = false;
        // Debounce UI/event spam so StartMatchmaking cannot re-enter in the same transition window.
        private float _lastStartMatchmakingAcceptedTime = -100f;
        private bool _serverMatchmakingActive;
        private bool _serverRoomStartInFlight;
        private Coroutine _serverBotReplacementHolder;
        private bool _serverPhotonReadyInFlight;
        private bool _serverMatchStartHandled;
        private bool _serverMatchStartConfirmedByBackend;
        private bool _serverInviteJoinInFlight;
        private string _pendingServerInviteRoomId = string.Empty;
        private MQTTMatchRoomData _serverRoomData;
        private MqttMatchSet _serverMatchData;

        public static LobbyManager Instance { get; private set; }
        public bool IsStartFinished {set => _isStartFinished = value; }
        private static float? _matchmakingStartTime;
        private static int? _matchmakingStoppedElapsedSeconds;

        public static int? MatchmakingElapsedSeconds => _matchmakingStoppedElapsedSeconds
            ?? (_matchmakingStartTime is float startTime ? (int)(Time.time - startTime) : (int?)null);

        private static void FreezeMatchmakingTimer()
        {
            _matchmakingStoppedElapsedSeconds = MatchmakingElapsedSeconds;
        }

        private static void NotifyMatchmakingRoomEntered(bool isLeader)
        {
            _matchmakingStoppedElapsedSeconds = null;
            _matchmakingStartTime = Time.time;
            OnMatchmakingRoomEntered?.Invoke(isLeader);
        }

        private static void NotifyMatchmakingStopped()
        {
            _matchmakingStartTime = null;
            _matchmakingStoppedElapsedSeconds = null;
            OnMatchmakingStopped?.Invoke();
        }

        private static void NotifyGameStartCancelled()
        {
            _matchmakingStoppedElapsedSeconds = null;
            if (_matchmakingStartTime.HasValue)
                _matchmakingStartTime = Time.time;

            OnGameStartCancelled?.Invoke();
        }

        public static bool IsServerMatchmakingActive => Instance != null && Instance._serverMatchmakingActive;
        public static int ServerMatchmakingPlayerCount => Instance?._serverRoomData?.players?.Count ?? 0;
        public static bool IsLocalServerMatchmakingOwner => Instance?._serverRoomData?.ownerPlayerId == ServerManager.Instance?.Player?._id;
        public static string[] ServerMatchmakingPlayerNames => Instance?._serverRoomData?.players?
            .Where(player => player != null && !string.IsNullOrWhiteSpace(player.name))
            .Select(player => player.name)
            .ToArray() ?? Array.Empty<string>();
        public static bool IsActive { get => _isActive;}

        private static bool _isActive = false;
        private static bool _gamePlayedOut = false;
        private static bool _battleStartUiReady = false;

        public bool RunnerActive => _runner != null;

        public BattleRoomInfo RoomInfo { get => _roomInfo;}

        #endregion

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

        public delegate void FriendLobbyJoinFailed(string roomName, short returnCode, string message);
        public static event FriendLobbyJoinFailed OnFriendLobbyJoinFailed;

        #endregion

        #region Public API & Helpers

        // Public helper to request a lobby window change from outside LobbyManager.
        public static void RequestLobbyWindowChange(LobbyWindowTarget target, LobbyWindowTarget lobbyWindow = LobbyWindowTarget.None)
        {
            OnLobbyWindowChangeRequest?.Invoke(target, lobbyWindow);
        }

        public static void NotifyGamePlayedOut()
        {
            _gamePlayedOut = true;

            // The LobbyManager survives scene changes. Mark the local queue as finished as soon as
            // the battle ends so menu UI created for the results flow cannot restore the mini
            // matchmaking panel from stale state. Keep the server match data until its finished
            // event arrives so the result-reporting flow can still correlate and clear it.
            if (Instance != null && Instance._serverMatchmakingActive)
            {
                Instance._serverMatchmakingActive = false;
                NotifyMatchmakingStopped();
            }
        }

        public static void NotifyBattleStartUiReady()
        {
            _battleStartUiReady = true;
        }

        private bool IsGameStartTransitionActive()
        {
            // BattleID can remain set after match end, so rely on transient runtime flags instead.
            return !_gamePlayedOut && (_countdownActive || _startGameHolder != null || _startQuantumHolder != null);
        }

        private void QueueCustomBattleStartCheck()
        {
            if (_canBattleStartCheckHolder != null) return;
            if (!PhotonRealtimeClient.InRoom || PhotonRealtimeClient.CurrentRoom == null) return;
            if (PhotonRealtimeClient.LocalPlayer == null || !PhotonRealtimeClient.LocalPlayer.IsMasterClient) return;

            MatchmakingType gameType;
            try
            {
                gameType = (MatchmakingType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.MatchmakingKey);
            }
            catch
            {
                return;
            }

            if (gameType != MatchmakingType.Custom) return;
            _canBattleStartCheckHolder = StartCoroutine(CheckIfBattleCanStart());
        }

        private IEnumerator CheckIfBattleCanStart()
        {
            try
            {
                yield return new WaitUntil(() => _posChangeQueue.Count == 0 && !_playerPosChangeInProgress);
                if (!PhotonRealtimeClient.InRoom || PhotonRealtimeClient.CurrentRoom == null) yield break;
                if (PhotonRealtimeClient.LocalPlayer == null || !PhotonRealtimeClient.LocalPlayer.IsMasterClient) yield break;
                if (_startGameHolder != null || _startQuantumHolder != null) yield break;

                Room room = PhotonRealtimeClient.CurrentRoom;
                //if (room.PlayerCount != room.MaxPlayers) yield break;
                yield return null;
                if (CheckIfAllPlayersInPosition())
                {
                    if (CheckIfAllPlayersAreReady())
                    {
                        MatchmakingType gameType = (MatchmakingType)room.GetCustomProperty<int>(PhotonBattleRoom.MatchmakingKey);
                        if (gameType == MatchmakingType.Custom)
                        {
                            OnStartPlayingEvent(new StartPlayingEvent());
                        }
                    }
                }
            }
            finally
            {
                _canBattleStartCheckHolder = null;
            }
        }

        public bool JoinFriendLobbyRoom(string roomName)
        {
            if (string.IsNullOrWhiteSpace(roomName)) return false;

            if (roomName == _pendingServerInviteRoomId)
            {
                if (_serverInviteJoinInFlight) return false;

                _pendingServerInviteRoomId = string.Empty;
                StartCoroutine(JoinServerMatchmakingRoom(roomName));
                return true;
            }

            if (!PhotonRealtimeClient.InLobby || PhotonRealtimeClient.InRoom) return false;

            _pendingAcceptedInRoomInviteRoomName = roomName;
            Debug.Log($"JoinFriendLobbyRoom: joining server-invited room '{roomName}'.");
            if (PhotonRealtimeClient.JoinRoom(roomName)) return true;

            _pendingAcceptedInRoomInviteRoomName = string.Empty;
            OnFriendLobbyJoinFailed?.Invoke(roomName, -1, "join operation could not be started");
            return false;
        }

        public void DeclineInRoomInvite(string roomName)
        {
            if (string.IsNullOrEmpty(roomName)) return;

            if (roomName == _pendingServerInviteRoomId)
            {
                _pendingServerInviteRoomId = string.Empty;
                return;
            }

            _declinedInRoomInviteUntil[roomName] = Time.time + InRoomInviteDeclineCooldownSeconds;
            if (_pendingInRoomInviteRoomName == roomName)
            {
                _pendingInRoomInviteRoomName = string.Empty;
            }
            if (_pendingAcceptedInRoomInviteRoomName == roomName)
            {
                _pendingAcceptedInRoomInviteRoomName = string.Empty;
                _pendingAcceptedInRoomInviteStartTime = -100f;
            }
            Debug.Log($"DeclineInRoomInvite: declined room '{roomName}'.");
        }

        private bool IsInRoomInviteDeclinedRecently(string roomName)
        {
            if (!_declinedInRoomInviteUntil.TryGetValue(roomName, out float until)) return false;

            if (Time.time > until)
            {
                _declinedInRoomInviteUntil.Remove(roomName);
                return false;
            }

            return true;
        }

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

        private bool CanMutateRoomPropertiesNow(string context = null, bool logWhenNotReady = false)
        {
            var client = PhotonRealtimeClient.Client;
            bool ready = client != null
                && client.Server == ServerConnection.GameServer
                && client.IsConnectedAndReady
                && PhotonRealtimeClient.InRoom
                && PhotonRealtimeClient.CurrentRoom != null
                && client.State != ClientState.Leaving
                && client.State != ClientState.DisconnectingFromGameServer
                && client.State != ClientState.DisconnectingFromMasterServer;

            if (!ready && logWhenNotReady)
            {
                string source = string.IsNullOrEmpty(context) ? "RoomProperties" : context;
                Debug.LogWarning($"{source}: skipping room property write - client not ready (State={client?.State}, Server={client?.Server}, Ready={client?.IsConnectedAndReady}, InRoom={PhotonRealtimeClient.InRoom})");
            }

            return ready;
        }

        private int GetFirstFreePositionWithoutVerification()
        {
            if (PhotonRealtimeClient.CurrentRoom == null) return PlayerPositionGuest;

            int maxPlayers = PhotonRealtimeClient.CurrentRoom.MaxPlayers;
            int[] candidatePositions = maxPlayers >= 4
                ? new[] { PhotonBattleRoom.PlayerPosition1, PhotonBattleRoom.PlayerPosition2, PhotonBattleRoom.PlayerPosition3, PhotonBattleRoom.PlayerPosition4 }
                : new[] { PhotonBattleRoom.PlayerPosition1, PhotonBattleRoom.PlayerPosition3 };

            foreach (int position in candidatePositions)
            {
                string value = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.GetPositionKey(position), string.Empty);
                if (string.IsNullOrEmpty(value)) return position;
            }

            return PlayerPositionGuest;
        }

        private int GetReservedRoomPositionForUser(string userId)
        {
            if (string.IsNullOrEmpty(userId) || PhotonRealtimeClient.CurrentRoom == null) return PlayerPositionGuest;

            int maxPlayers = PhotonRealtimeClient.CurrentRoom.MaxPlayers;
            int[] candidatePositions = maxPlayers >= 4
                ? new[] { PhotonBattleRoom.PlayerPosition1, PhotonBattleRoom.PlayerPosition2, PhotonBattleRoom.PlayerPosition3, PhotonBattleRoom.PlayerPosition4 }
                : new[] { PhotonBattleRoom.PlayerPosition1, PhotonBattleRoom.PlayerPosition2 };

            foreach (int position in candidatePositions)
            {
                string value = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.GetPositionKey(position), string.Empty);
                if (string.Equals(value, userId, StringComparison.Ordinal)) return position;
            }

            return PlayerPositionGuest;
        }

        private void ClearStaleHumanPositionReservations(string context)
        {
            if (!CanMutateRoomPropertiesNow()) return;

            try
            {
                Room room = PhotonRealtimeClient.CurrentRoom;
                if (room == null) return;

                var existingUserIds = new HashSet<string>(room.Players.Values.Select(p => p.UserId));
                string[] posKeys = {
                    PhotonBattleRoom.PlayerPositionKey1,
                    PhotonBattleRoom.PlayerPositionKey2,
                    PhotonBattleRoom.PlayerPositionKey3,
                    PhotonBattleRoom.PlayerPositionKey4
                };

                foreach (string key in posKeys)
                {
                    string val = room.GetCustomProperty<string>(key, string.Empty);
                    if (string.IsNullOrEmpty(val) || val == "Bot") continue;
                    if (existingUserIds.Contains(val)) continue;

                    var emptyPosition = new LobbyPhotonHashtable(new Dictionary<object, object> { { key, "" } });
                    var expectedValue = new LobbyPhotonHashtable(new Dictionary<object, object> { { key, val } });
                    if (PhotonRealtimeClient.LobbyCurrentRoom != null && PhotonRealtimeClient.LobbyCurrentRoom.SetCustomProperties(emptyPosition, expectedValue))
                    {
                        Debug.Log($"{context}: cleared stale position {key} (value {val}).");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"{context}: failed to clear stale reservations: {ex.Message}");
            }
        }

        private string GetPlayerName(string userId)
        {
            if (string.IsNullOrEmpty(userId) || PhotonRealtimeClient.LobbyCurrentRoom == null) return string.Empty;

            var player = PhotonRealtimeClient.LobbyCurrentRoom.Players.Values.FirstOrDefault(p => p.UserId == userId);
            if (player == null || string.IsNullOrEmpty(player.UserId)) return string.Empty;
            else return player.NickName;
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
            }

            if (_canBattleStartCheckHolder != null)
            {
                StopCoroutine(_canBattleStartCheckHolder);
                _canBattleStartCheckHolder = null;
            }
        }

        #endregion

        #region Unity Lifecycle & Activation

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
            UnsubscribeServerMatchmakingEvents();
            try
            {
                if (PhotonRealtimeClient.Client != null)
                {
                    PhotonRealtimeClient.RemoveCallbackTarget(this);
                    try { PhotonRealtimeClient.Client.StateChanged -= OnStateChange; } catch (Exception ex) { Debug.LogWarning($"OnDisable: failed to unsubscribe StateChanged: {ex.Message}"); }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"OnDisable: unsubscribe failed: {ex.Message}");
            }
            this.Unsubscribe();
            _isActive = false;
            if (_serviceHolder != null)
            {
                try
                {
                    StopCoroutine(_serviceHolder);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"OnDisable: failed to stop service coroutine: {ex.Message}");
                }
                _serviceHolder = null;
            }
        }

        private void OnApplicationQuit()
        {
            if (PhotonRealtimeClient.Client != null)
            {
                try
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
                catch (Exception ex)
                {
                    Debug.LogWarning($"OnApplicationQuit: leave failed: {ex.Message}");
                }
            }
        }
        public void Activate()
        {
            if (_isActive) { Debug.LogWarning("LobbyManager is already active."); return; }
            _isActive = true;
            PhotonRealtimeClient.AddCallbackTarget(this);
            if (PhotonRealtimeClient.Client != null)
            {
                try { PhotonRealtimeClient.Client.StateChanged += OnStateChange; } catch (Exception ex) { Debug.LogWarning($"Activate: failed to subscribe StateChanged: {ex.Message}"); }
            }
            this.Subscribe<ReserveFreePositionEvent>(OnReserveFreePositionEvent);
            this.Subscribe<PlayerPosEvent>(OnPlayerPosEvent);
            this.Subscribe<BotToggleEvent>(OnBotToggleEvent);
            this.Subscribe<BotFillToggleEvent>(OnBotFillToggleEvent);
            this.Subscribe<ReadyToggleEvent>(OnReadyToggleEvent);
            this.Subscribe<StartRoomEvent>(OnStartRoomEvent);
            this.Subscribe<StartPlayingEvent>(OnStartPlayingEvent);
            this.Subscribe<StartRaidTestEvent>(OnStartRaidTestEvent);
            this.Subscribe<StartMatchmakingEvent>(OnStartMatchmakingEvent);
            this.Subscribe<StopMatchmakingEvent>(OnStopMatchmakingEvent);
            this.Subscribe<GetKickedEvent>(OnGetKickedEvent);
            SubscribeServerMatchmakingEvents();
            if (_serviceHolder == null) _serviceHolder = StartCoroutine(Service());

            GameConfig gameConfig = GameConfig.Get();
            PlayerSettings playerSettings = gameConfig.PlayerSettings;
            string photonRegion = string.IsNullOrEmpty(playerSettings.PhotonRegion) ? null : playerSettings.PhotonRegion;
            StartCoroutine(StartLobby(playerSettings.PlayerGuid, playerSettings.PhotonRegion));
        }

        private void SubscribeServerMatchmakingEvents()
        {
            MQTTManager.OnMatchmakingRoomDataUpdated -= OnServerMatchmakingRoomUpdated;
            MQTTManager.OnMatchmakingInviteReceived -= OnServerMatchmakingInviteReceived;
            MQTTManager.OnMatchmakingClanInviteReceived -= OnServerMatchmakingInviteReceived;
            MQTTManager.OnMatchmakingMatchFound -= OnServerMatchFound;
            MQTTManager.OnMatchmakingMatchStarted -= OnServerMatchStarted;
            MQTTManager.OnMatchmakingMatchFinished -= OnServerMatchFinished;

            MQTTManager.OnMatchmakingRoomDataUpdated += OnServerMatchmakingRoomUpdated;
            MQTTManager.OnMatchmakingInviteReceived += OnServerMatchmakingInviteReceived;
            MQTTManager.OnMatchmakingClanInviteReceived += OnServerMatchmakingInviteReceived;
            MQTTManager.OnMatchmakingMatchFound += OnServerMatchFound;
            MQTTManager.OnMatchmakingMatchStarted += OnServerMatchStarted;
            MQTTManager.OnMatchmakingMatchFinished += OnServerMatchFinished;
        }

        private void UnsubscribeServerMatchmakingEvents()
        {
            MQTTManager.OnMatchmakingRoomDataUpdated -= OnServerMatchmakingRoomUpdated;
            MQTTManager.OnMatchmakingInviteReceived -= OnServerMatchmakingInviteReceived;
            MQTTManager.OnMatchmakingClanInviteReceived -= OnServerMatchmakingInviteReceived;
            MQTTManager.OnMatchmakingMatchFound -= OnServerMatchFound;
            MQTTManager.OnMatchmakingMatchStarted -= OnServerMatchStarted;
            MQTTManager.OnMatchmakingMatchFinished -= OnServerMatchFinished;
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

        #endregion




        #region Matchmaking
        private IEnumerator StartServerMatchmaking(MatchmakingType matchmakingType, GameType gameType, bool isPremadeInRoom)
        {
            _currentMatchmakingGameType = matchmakingType;
            _currentGameType = gameType;
            _serverMatchmakingActive = true;
            _serverRoomStartInFlight = false;
            _serverPhotonReadyInFlight = false;
            _serverMatchStartHandled = false;
            _serverMatchStartConfirmedByBackend = false;
            _serverMatchData = null;

            string automaticInvitePlayerId = null;
            try
            {
                if (PhotonRealtimeClient.CurrentRoom != null)
                {
                    automaticInvitePlayerId = PhotonRealtimeClient.CurrentRoom.Players.Values
                        .Where(player => player != null
                            && !string.IsNullOrWhiteSpace(player.UserId)
                            && player.UserId != PhotonRealtimeClient.LocalPlayer.UserId)
                        .Select(player => player.UserId)
                        .FirstOrDefault();

                    PhotonRealtimeClient.CurrentRoom.IsOpen = false;
                    PhotonRealtimeClient.LeaveRoom();
                }

                if (!PhotonRealtimeClient.InLobby)
                {
                    yield return new WaitUntil(() => PhotonRealtimeClient.InLobby);
                }

                int teamSize = gameType == GameType.BattleFlipperGame ? 1 : 2;
                bool joinedExistingRoom = false;
                MatchmakingRoomResponse matchmakingRoom = null;

                if (matchmakingType == MatchmakingType.Clan2v2 && !isPremadeInRoom)
                {
                    yield return StartCoroutine(TryJoinOpenClanMatchmakingRoom(
                        gameType,
                        roomIdBefore: null,
                        (success, room) =>
                        {
                            joinedExistingRoom = success;
                            matchmakingRoom = room;
                        }));
                }

                if (!joinedExistingRoom)
                {
                    bool createSucceeded = false;
                    yield return StartCoroutine(ServerManager.Instance.MatchmakingCreateRoom(
                        matchmakingType,
                        gameType,
                        (success, room) =>
                        {
                            createSucceeded = success;
                            matchmakingRoom = room;
                        },
                        teamSize: teamSize,
                        allowBots: matchmakingType is MatchmakingType.Random2v2
                            or MatchmakingType.Clan2v2
                            or MatchmakingType.Custom,
                        automaticInvitePlayerId: isPremadeInRoom ? automaticInvitePlayerId : null));

                    if (!createSucceeded || matchmakingRoom == null)
                    {
                        Debug.LogWarning("Server matchmaking room creation failed.");
                        ResetServerMatchmakingState();
                        OnFailedToStartMatchmakingGame?.Invoke();
                        yield break;
                    }

                    // Two clan members can create rooms at almost the same instant. The owner of
                    // the newer room moves to the older one so both clients converge automatically.
                    if (matchmakingType == MatchmakingType.Clan2v2 && !isPremadeInRoom
                        && (matchmakingRoom.status == InviteStatus.OPEN
                            || (matchmakingRoom.status == InviteStatus.READY
                                && matchmakingRoom.bots != null
                                && matchmakingRoom.bots.Count > 0)))
                    {
                        string createdRoomId = matchmakingRoom.id;
                        for (int attempt = 0; attempt < 4 && !joinedExistingRoom; attempt++)
                        {
                            yield return new WaitForSeconds(0.25f);
                            MatchmakingRoomResponse joinedRoom = null;
                            yield return StartCoroutine(TryJoinOpenClanMatchmakingRoom(
                                gameType,
                                createdRoomId,
                                (success, room) =>
                                {
                                    joinedExistingRoom = success;
                                    joinedRoom = room;
                                }));

                            if (joinedExistingRoom) matchmakingRoom = joinedRoom;
                        }
                    }
                }

                if (matchmakingRoom == null)
                {
                    Debug.LogWarning("Server clan matchmaking room entry failed.");
                    ResetServerMatchmakingState();
                    OnFailedToStartMatchmakingGame?.Invoke();
                    yield break;
                }

                _serverRoomData = new MQTTMatchRoomData
                {
                    id = matchmakingRoom.id,
                    matchType = matchmakingRoom.matchType,
                    gameType = matchmakingRoom.gameType,
                    status = matchmakingRoom.status,
                    ownerPlayerId = matchmakingRoom.ownerPlayerId,
                    clanId = matchmakingRoom.clanId,
                    teamSize = matchmakingRoom.teamSize,
                    allowBots = matchmakingRoom.allowBots,
                    createdAt = matchmakingRoom.createdAt,
                    updatedAt = matchmakingRoom.updatedAt,
                    readyAt = matchmakingRoom.readyAt,
                    players = matchmakingRoom.players?
                        .Select(playerId => new MQTTMatchPlayers { playerId = playerId })
                        .ToList() ?? new List<MQTTMatchPlayers>(),
                    bots = matchmakingRoom.bots ?? new List<MQTTBots>()
                };

                bool isOwner = matchmakingRoom.ownerPlayerId == ServerManager.Instance.Player?._id;
                NotifyMatchmakingRoomEntered(isOwner);
                OnRoomLeaderChanged?.Invoke(isOwner);

                if (isOwner && matchmakingRoom.status == InviteStatus.READY)
                {
                    bool hasReplaceableBot = matchmakingRoom.matchType is MatchType.RANDOM or MatchType.CLAN
                        && matchmakingRoom.bots != null
                        && matchmakingRoom.bots.Count > 0;

                    if (hasReplaceableBot)
                    {
                        if (_serverBotReplacementHolder == null)
                        {
                            _serverBotReplacementHolder = StartCoroutine(
                                WaitForServerBotReplacement(matchmakingRoom.id));
                        }
                    }
                    else
                    {
                        yield return StartCoroutine(StartServerRoomIfReady(matchmakingRoom.id));
                    }
                }
            }
            finally
            {
                _matchmakingHolder = null;
            }
        }

        private IEnumerator TryJoinOpenClanMatchmakingRoom(
            GameType gameType,
            string roomIdBefore,
            Action<bool, MatchmakingRoomResponse> callback)
        {
            bool listSucceeded = false;
            List<MatchmakingRoomResponse> rooms = null;
            yield return StartCoroutine(ServerManager.Instance.MatchmakingGetRooms((success, result) =>
            {
                listSucceeded = success;
                rooms = result;
            }));

            string playerId = ServerManager.Instance.Player?._id;
            string clanId = ServerManager.Instance.Player?.clan_id;
            MatchmakingRoomResponse roomToJoin = listSucceeded
                ? rooms?
                    .Where(room => room != null
                        && room.matchType == MatchType.CLAN
                        && room.gameType == (int)gameType
                        && (room.status == InviteStatus.OPEN
                            || (room.status == InviteStatus.READY
                                && room.bots != null
                                && room.bots.Count > 0))
                        && room.teamSize == 2
                        && room.players != null
                        && room.players.Count < room.teamSize
                        && !room.players.Contains(playerId)
                        && room.clanId == clanId
                        && (string.IsNullOrWhiteSpace(roomIdBefore)
                            || string.CompareOrdinal(room.id, roomIdBefore) < 0))
                    .OrderBy(room => room.id, StringComparer.Ordinal)
                    .FirstOrDefault()
                : null;

            if (roomToJoin == null)
            {
                callback?.Invoke(false, null);
                yield break;
            }

            if (ServerManager.Instance.HasActiveMatchmakingRoom)
            {
                bool leftCurrentRoom = false;
                yield return StartCoroutine(ServerManager.Instance.MatchmakingLeaveRoom(success => leftCurrentRoom = success));
                if (!leftCurrentRoom)
                {
                    callback?.Invoke(false, null);
                    yield break;
                }
            }

            bool joinSucceeded = false;
            MatchmakingRoomResponse joinedRoom = null;
            yield return StartCoroutine(ServerManager.Instance.MatchmakingJoinRoom(roomToJoin.id, (success, room) =>
            {
                joinSucceeded = success;
                joinedRoom = room;
            }));
            callback?.Invoke(joinSucceeded, joinedRoom);
        }

        private IEnumerator JoinServerMatchmakingRoom(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId) || _serverInviteJoinInFlight) yield break;

            _serverInviteJoinInFlight = true;
            try
            {
                if (PhotonRealtimeClient.InRoom)
                {
                    PhotonRealtimeClient.LeaveRoom();
                    yield return new WaitUntil(() => PhotonRealtimeClient.InLobby);
                }

                bool success = false;
                MatchmakingRoomResponse joinedRoom = null;
                yield return StartCoroutine(ServerManager.Instance.MatchmakingJoinRoom(roomId, (joined, room) =>
                {
                    success = joined;
                    joinedRoom = room;
                }));

                if (!success || joinedRoom == null)
                {
                    OnFriendLobbyJoinFailed?.Invoke(roomId, -1, "server matchmaking room join failed");
                    yield break;
                }

                _serverMatchmakingActive = true;
                _currentMatchmakingGameType = (MatchmakingType)joinedRoom.matchType;
                _currentGameType = (GameType)joinedRoom.gameType;
                NotifyMatchmakingRoomEntered(joinedRoom.ownerPlayerId == ServerManager.Instance.Player?._id);
            }
            finally
            {
                _serverInviteJoinInFlight = false;
            }
        }

        private IEnumerator StartServerRoomIfReady(string roomId)
        {
            if (_serverRoomStartInFlight || string.IsNullOrWhiteSpace(roomId)) yield break;
            _serverRoomStartInFlight = true;
            try
            {
                bool success = false;
                MatchmakingRoomResponse room = null;
                yield return StartCoroutine(ServerManager.Instance.MatchmakingStartRoom(roomId, (started, response) =>
                {
                    success = started;
                    room = response;
                }));

                if (!success)
                {
                    // The backend can enqueue the room successfully and still return an error if a
                    // follow-up queue task fails. Re-read the authoritative state before reporting
                    // a false failure or attempting a duplicate start.
                    bool readSucceeded = false;
                    MatchmakingRoomResponse currentRoom = null;
                    yield return StartCoroutine(ServerManager.Instance.MatchmakingGetRoom(roomId, (read, response) =>
                    {
                        readSucceeded = read;
                        currentRoom = response;
                    }));

                    if (readSucceeded
                        && currentRoom != null
                        && (currentRoom.status == InviteStatus.QUEUED || currentRoom.status == InviteStatus.MATCHED))
                    {
                        success = true;
                        room = currentRoom;
                        Debug.LogWarning($"Room start returned an error, but backend room '{roomId}' is already {currentRoom.status}; continuing.");
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to start server matchmaking room '{roomId}'.");
                        OnFailedToStartMatchmakingGame?.Invoke();
                    }
                }

                if (success && !string.IsNullOrWhiteSpace(room?.matchId))
                {
                    ServerManager.Instance.SetActiveMatchmakingMatch(room.matchId);
                }
            }
            finally
            {
                _serverRoomStartInFlight = false;
            }
        }

        private IEnumerator WaitForServerBotReplacement(string roomId)
        {
            try
            {
                float waitStart = Time.time;
                while (_serverMatchmakingActive
                    && ServerManager.Instance.ActiveMatchmakingRoomId == roomId
                    && Time.time - waitStart < MatchmakingTimeoutSeconds)
                {
                    if (_serverRoomData == null
                        || _serverRoomData.id != roomId
                        || _serverRoomData.status != InviteStatus.READY)
                    {
                        yield break;
                    }

                    if (_serverRoomData.bots == null || _serverRoomData.bots.Count == 0)
                    {
                        yield return StartCoroutine(StartServerRoomIfReady(roomId));
                        yield break;
                    }

                    yield return new WaitForSeconds(0.25f);
                }

                if (!_serverMatchmakingActive
                    || ServerManager.Instance.ActiveMatchmakingRoomId != roomId)
                {
                    yield break;
                }

                bool readSucceeded = false;
                MatchmakingRoomResponse currentRoom = null;
                yield return StartCoroutine(ServerManager.Instance.MatchmakingGetRoom(roomId, (success, room) =>
                {
                    readSucceeded = success;
                    currentRoom = room;
                }));

                if (readSucceeded && currentRoom?.status == InviteStatus.READY)
                {
                    yield return StartCoroutine(StartServerRoomIfReady(roomId));
                }
            }
            finally
            {
                _serverBotReplacementHolder = null;
            }
        }

        private void OnServerMatchmakingRoomUpdated(MQTTMatchRoomData room)
        {
            if (room == null || string.IsNullOrWhiteSpace(room.id)) return;
            if (!string.IsNullOrWhiteSpace(ServerManager.Instance.ActiveMatchmakingRoomId)
                && room.id != ServerManager.Instance.ActiveMatchmakingRoomId) return;

            _serverMatchmakingActive = true;
            _serverRoomData = room;
            bool isOwner = room.ownerPlayerId == ServerManager.Instance.Player?._id;
            NotifyMatchmakingRoomEntered(isOwner);
            OnRoomLeaderChanged?.Invoke(isOwner);

            if (isOwner && room.status == InviteStatus.READY && !_serverRoomStartInFlight)
            {
                bool hasReplaceableBot = room.matchType is MatchType.RANDOM or MatchType.CLAN
                    && room.bots != null
                    && room.bots.Count > 0;

                if (hasReplaceableBot)
                {
                    if (_serverBotReplacementHolder == null)
                    {
                        _serverBotReplacementHolder = StartCoroutine(
                            WaitForServerBotReplacement(room.id));
                    }
                }
                else
                {
                    if (_serverBotReplacementHolder != null)
                    {
                        StopCoroutine(_serverBotReplacementHolder);
                        _serverBotReplacementHolder = null;
                    }
                    StartCoroutine(StartServerRoomIfReady(room.id));
                }
            }
            else if (room.status == InviteStatus.CANCELLED)
            {
                ResetServerMatchmakingState();
                NotifyMatchmakingStopped();
            }
        }

        private void OnServerMatchmakingInviteReceived(MQTTMatchInvite invite)
        {
            if (invite == null || string.IsNullOrWhiteSpace(invite.id)) return;

            if (IsAcceptedPremadeServerInvite(invite))
            {
                _pendingServerInviteRoomId = string.Empty;
                Debug.Log($"Automatically joining matchmaking room '{invite.id}' with the already accepted premade teammate.");
                StartCoroutine(JoinServerMatchmakingRoom(invite.id));
                return;
            }

            _pendingServerInviteRoomId = invite.id;
        }

        private bool IsAcceptedPremadeServerInvite(MQTTMatchInvite invite)
        {
            if (!PhotonRealtimeClient.InRoom || PhotonRealtimeClient.CurrentRoom == null) return false;

            string localPlayerId = ServerManager.Instance.Player?._id
                ?? PhotonRealtimeClient.LocalPlayer?.UserId
                ?? string.Empty;
            string ownerPlayerId = invite.ownerPlayer?.playerId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(localPlayerId)
                || string.IsNullOrWhiteSpace(ownerPlayerId)
                || string.Equals(localPlayerId, ownerPlayerId, StringComparison.Ordinal)) return false;

            try
            {
                var room = PhotonRealtimeClient.CurrentRoom;
                bool isPremade = room.GetCustomProperty<bool>(PhotonBattleRoom.PremadeModeKey, false);
                int inviteState = room.GetCustomProperty<int>(
                    PhotonBattleRoom.PremadeInviteStateKey,
                    PhotonBattleRoom.PremadeInviteStateNone);
                string leaderPlayerId = room.GetCustomProperty<string>(PhotonBattleRoom.PremadeLeaderUserIdKey, string.Empty);
                string invitedPlayerId = room.GetCustomProperty<string>(PhotonBattleRoom.PremadeInvitedUserIdKey, string.Empty);
                string premadePlayerId1 = room.GetCustomProperty<string>(PhotonBattleRoom.PremadeUserId1Key, string.Empty);
                string premadePlayerId2 = room.GetCustomProperty<string>(PhotonBattleRoom.PremadeUserId2Key, string.Empty);
                MatchmakingType targetMatchmakingType = (MatchmakingType)room.GetCustomProperty<int>(
                    PhotonBattleRoom.PremadeTargetMatchmakingTypeKey,
                    (int)MatchmakingType.None);
                GameType targetGameType = (GameType)room.GetCustomProperty<int>(
                    PhotonBattleRoom.PremadeTargetGameTypeKey,
                    (int)GameType.BattlePingPong);

                bool localPlayerIsAcceptedTeammate = string.Equals(invitedPlayerId, localPlayerId, StringComparison.Ordinal)
                    && string.Equals(premadePlayerId2, localPlayerId, StringComparison.Ordinal);
                bool ownerIsPremadeLeader = string.Equals(leaderPlayerId, ownerPlayerId, StringComparison.Ordinal)
                    && string.Equals(premadePlayerId1, ownerPlayerId, StringComparison.Ordinal);

                return isPremade
                    && inviteState == PhotonBattleRoom.PremadeInviteStateAccepted
                    && localPlayerIsAcceptedTeammate
                    && ownerIsPremadeLeader
                    && targetMatchmakingType == (MatchmakingType)invite.matchType
                    && targetGameType == (GameType)invite.gameType;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Could not verify automatic premade matchmaking join: {ex.Message}");
                return false;
            }
        }

        private void OnServerMatchFound(MqttMatchSet match)
        {
            if (match == null || string.IsNullOrWhiteSpace(match.id)) return;

            ServerManager serverManager = ServerManager.Instance;
            if (!_serverMatchmakingActive || serverManager == null || !serverManager.HasActiveMatchmakingRoom)
            {
                Debug.LogWarning($"Ignoring backend match '{match.id}' because the player has not entered a matchmaking room.");
                return;
            }

            string localPlayerId = serverManager.Player?._id;
            bool localPlayerIsInMatch = !string.IsNullOrWhiteSpace(localPlayerId)
                && match.teams != null
                && match.teams.Any(team => team?.players != null
                    && team.players.Any(player => player?.playerId == localPlayerId));
            if (!localPlayerIsInMatch)
            {
                Debug.LogWarning($"Ignoring backend match '{match.id}' because it does not include the local player.");
                return;
            }

            if (_serverMatchData != null && _serverMatchData.id == match.id && _serverPhotonReadyInFlight) return;

            FreezeMatchmakingTimer();
            _serverMatchData = match;
            _serverMatchStartConfirmedByBackend = false;
            _currentMatchmakingGameType = (MatchmakingType)match.matchType;
            _currentGameType = (GameType)match.gameType;
            serverManager.SetActiveMatchmakingMatch(match.id);
            StartCoroutine(JoinServerPhotonRoom(match));
        }

        private IEnumerator JoinServerPhotonRoom(MqttMatchSet match)
        {
            if (MQTTManager.Instance != null)
            {
                Task subscription = MQTTManager.Instance.SubscribeToActiveMatch(match.id);
                yield return new WaitUntil(() => subscription.IsCompleted);
                if (subscription.IsFaulted) Debug.LogException(subscription.Exception);
            }

            if (PhotonRealtimeClient.InRoom && PhotonRealtimeClient.CurrentRoom?.Name != match.id)
            {
                PhotonRealtimeClient.LeaveRoom();
            }

            if (!PhotonRealtimeClient.InLobby && !PhotonRealtimeClient.InRoom)
            {
                yield return new WaitUntil(() => PhotonRealtimeClient.InLobby || PhotonRealtimeClient.InRoom);
            }

            if (PhotonRealtimeClient.InRoom && PhotonRealtimeClient.CurrentRoom?.Name == match.id)
            {
                yield break;
            }

            if (!PhotonRealtimeClient.JoinOrCreateServerMatchRoom(match))
            {
                Debug.LogWarning($"Unable to join backend-assigned Photon room '{match.id}'.");
                OnFailedToStartMatchmakingGame?.Invoke();
            }
        }

        private void OnServerMatchStarted(MqttMatchSet match)
        {
            if (match == null || _serverMatchData == null || match.id != _serverMatchData.id) return;
            _serverMatchData = match;
            _serverMatchStartConfirmedByBackend = true;
            PublishServerMatchStartToPhoton();
            TryStartServerMatchGameplay();
        }

        private void PublishServerMatchStartToPhoton()
        {
            if (!_serverMatchStartConfirmedByBackend
                || !IsCurrentServerMatchRoom()
                || PhotonRealtimeClient.CurrentRoom == null) return;

            try
            {
                if (!PhotonRealtimeClient.CurrentRoom.GetCustomProperty<bool>(
                    PhotonBattleRoom.ServerMatchStartedKey,
                    false))
                {
                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(
                        PhotonBattleRoom.ServerMatchStartedKey,
                        true);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Could not publish the backend-confirmed match start to Photon: {ex.Message}");
            }
        }

        private void TryStartServerMatchGameplay()
        {
            if (_serverMatchStartHandled
                || !IsCurrentServerMatchRoom()
                || PhotonRealtimeClient.LocalPlayer == null
                || !PhotonRealtimeClient.LocalPlayer.IsMasterClient) return;

            _serverMatchStartHandled = true;
            OnStartPlayingEvent(new StartPlayingEvent());
        }

        private void OnServerMatchFinished(MqttMatchSet match)
        {
            if (match == null || _serverMatchData == null || match.id != _serverMatchData.id) return;
            _serverMatchData = match;
            ServerManager.Instance.ClearActiveMatchmakingState(match.id);
            ResetServerMatchmakingState();
        }

        private IEnumerator ConfirmServerPhotonReady()
        {
            if (_serverPhotonReadyInFlight || _serverMatchData == null) yield break;
            _serverPhotonReadyInFlight = true;

            string matchId = _serverMatchData.id;
            int expectedPosition = ApplyServerMatchPosition();
            float positionDeadline = Time.realtimeSinceStartup + ServerPhotonPositionReadyTimeoutSeconds;

            while (_serverMatchmakingActive
                && _serverMatchData != null
                && _serverMatchData.id == matchId
                && PhotonRealtimeClient.InRoom
                && PhotonRealtimeClient.CurrentRoom?.Name == matchId)
            {
                int localPosition = PhotonRealtimeClient.LocalPlayer?.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest)
                    ?? PlayerPositionGuest;
                if (PhotonLobbyRoom.IsValidPlayerPos(expectedPosition) && localPosition == expectedPosition)
                {
                    break;
                }

                if (Time.realtimeSinceStartup >= positionDeadline)
                {
                    _serverPhotonReadyInFlight = false;
                    Debug.LogWarning($"Photon did not acknowledge server match position {expectedPosition} for match '{matchId}' in time; readiness was not confirmed.");
                    OnFailedToStartMatchmakingGame?.Invoke();
                    yield break;
                }

                // The room creator can receive OnJoinedRoom before its initial room properties
                // are visible locally. Retry resolving the assigned slot, but do not confirm ready
                // until the corresponding player property has also arrived from Photon.
                if (!PhotonLobbyRoom.IsValidPlayerPos(expectedPosition))
                {
                    expectedPosition = ApplyServerMatchPosition();
                }
                yield return null;
            }

            if (!_serverMatchmakingActive
                || _serverMatchData == null
                || _serverMatchData.id != matchId
                || !PhotonRealtimeClient.InRoom
                || PhotonRealtimeClient.CurrentRoom?.Name != matchId)
            {
                _serverPhotonReadyInFlight = false;
                yield break;
            }

            bool success = false;
            MqttMatchSet confirmedMatch = null;
            yield return StartCoroutine(ServerManager.Instance.MatchmakingConfirmPhotonReady(
                matchId,
                (confirmed, match) =>
                {
                    success = confirmed;
                    confirmedMatch = match;
                }));
            if (!success)
            {
                _serverPhotonReadyInFlight = false;
                Debug.LogWarning($"Backend did not accept Photon readiness for match '{matchId}'.");
                OnFailedToStartMatchmakingGame?.Invoke();
                yield break;
            }

            if (confirmedMatch != null)
            {
                _serverMatchData = confirmedMatch;
                if (!string.IsNullOrWhiteSpace(confirmedMatch.battleStartedAt))
                {
                    _serverMatchStartConfirmedByBackend = true;
                    PublishServerMatchStartToPhoton();
                    TryStartServerMatchGameplay();
                }
            }
        }

        private int ApplyServerMatchPosition()
        {
            string localUserId = PhotonRealtimeClient.LocalPlayer?.UserId;
            if (string.IsNullOrWhiteSpace(localUserId) || PhotonRealtimeClient.CurrentRoom == null) return PlayerPositionGuest;

            for (int position = PhotonBattleRoom.PlayerPosition1; position <= PhotonBattleRoom.PlayerPosition4; position++)
            {
                string reservedUserId = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.GetPositionKey(position), string.Empty);
                if (reservedUserId != localUserId) continue;
                PhotonRealtimeClient.LocalPlayer.SetCustomProperty(PlayerPositionKey, position);
                return position;
            }

            return PlayerPositionGuest;
        }

        private bool IsServerAuthoritativeMatchmakingActive()
        {
            if (_serverMatchmakingActive) return true;

            ServerManager serverManager = ServerManager.Instance;
            return serverManager != null
                && (serverManager.HasActiveMatchmakingRoom
                    || !string.IsNullOrEmpty(serverManager.ActiveMatchmakingMatchId));
        }

        private bool IsCurrentServerMatchRoom()
        {
            return IsServerAuthoritativeMatchmakingActive()
                && _serverMatchData != null
                && PhotonRealtimeClient.CurrentRoom?.Name == _serverMatchData.id;
        }



        private IEnumerator StopServerMatchmaking()
        {
            bool left = false;
            yield return StartCoroutine(ServerManager.Instance.MatchmakingLeaveRoom(success => left = success));
            if (!left) Debug.LogWarning("Backend matchmaking leave failed or the match was already assigned.");

            if (PhotonRealtimeClient.InRoom) PhotonRealtimeClient.LeaveRoom();
            ResetServerMatchmakingState();
            NotifyMatchmakingStopped();
        }

        private void ResetServerMatchmakingState()
        {
            if (_serverBotReplacementHolder != null)
            {
                StopCoroutine(_serverBotReplacementHolder);
                _serverBotReplacementHolder = null;
            }
            _serverMatchmakingActive = false;
            _serverRoomStartInFlight = false;
            _serverPhotonReadyInFlight = false;
            _serverMatchStartHandled = false;
            _serverMatchStartConfirmedByBackend = false;
            _pendingServerInviteRoomId = string.Empty;
            _serverRoomData = null;
            _serverMatchData = null;
        }

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

            if (_canBattleStartCheckHolder != null)
            {
                StopCoroutine(_canBattleStartCheckHolder);
                _canBattleStartCheckHolder = null;
            }
        }

        private bool RoomHasSameSideCapacityForPremade(LobbyRoomInfo room)
        {
            if (room == null || room.CustomProperties == null) return false;

            string positionValue1 = room.CustomProperties.ContainsKey(PhotonBattleRoom.PlayerPositionKey1)
                ? room.CustomProperties[PhotonBattleRoom.PlayerPositionKey1]?.ToString()
                : string.Empty;
            string positionValue2 = room.CustomProperties.ContainsKey(PhotonBattleRoom.PlayerPositionKey2)
                ? room.CustomProperties[PhotonBattleRoom.PlayerPositionKey2]?.ToString()
                : string.Empty;
            string positionValue3 = room.CustomProperties.ContainsKey(PhotonBattleRoom.PlayerPositionKey3)
                ? room.CustomProperties[PhotonBattleRoom.PlayerPositionKey3]?.ToString()
                : string.Empty;
            string positionValue4 = room.CustomProperties.ContainsKey(PhotonBattleRoom.PlayerPositionKey4)
                ? room.CustomProperties[PhotonBattleRoom.PlayerPositionKey4]?.ToString()
                : string.Empty;

            int alphaFree = (string.IsNullOrEmpty(positionValue1) ? 1 : 0) + (string.IsNullOrEmpty(positionValue2) ? 1 : 0);
            int betaFree = (string.IsNullOrEmpty(positionValue3) ? 1 : 0) + (string.IsNullOrEmpty(positionValue4) ? 1 : 0);
            return alphaFree >= 2 || betaFree >= 2;
        }

        private bool TryReservePremadePairToSameSide(string userId1, string userId2, out int teamNumber)
        {
            teamNumber = PhotonBattleRoom.NoTeamValue;
            if (string.IsNullOrEmpty(userId1) || string.IsNullOrEmpty(userId2) || userId1 == userId2)
            {
                Debug.LogWarning($"TryReservePremadePairToSameSide: invalid user ids ({userId1},{userId2}).");
                return false;
            }
            if (!PhotonRealtimeClient.InRoom || PhotonRealtimeClient.CurrentRoom == null)
            {
                Debug.LogWarning($"TryReservePremadePairToSameSide: not in room or CurrentRoom null while attempting ({userId1},{userId2}).");
                return false;
            }
            if (!CanMutateRoomPropertiesNow())
            {
                Debug.LogWarning($"TryReservePremadePairToSameSide: cannot mutate room properties now for ({userId1},{userId2}).");
                return false;
            }

            Dictionary<int, string> currentMap = new()
            {
                { PhotonBattleRoom.PlayerPosition1, PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey1, string.Empty) },
                { PhotonBattleRoom.PlayerPosition2, PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey2, string.Empty) },
                { PhotonBattleRoom.PlayerPosition3, PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey3, string.Empty) },
                { PhotonBattleRoom.PlayerPosition4, PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey4, string.Empty) },
            };

            try
            {
                string snapshot = $"pos1={currentMap[PhotonBattleRoom.PlayerPosition1]},pos2={currentMap[PhotonBattleRoom.PlayerPosition2]},pos3={currentMap[PhotonBattleRoom.PlayerPosition3]},pos4={currentMap[PhotonBattleRoom.PlayerPosition4]}";
                Debug.Log($"TryReservePremadePairToSameSide: currentMap for room '{PhotonRealtimeClient.CurrentRoom?.Name}': {snapshot} - attempting to reserve ({userId1},{userId2}).");
            }
            catch { }

            bool IsBotValue(string value) => string.Equals(value, "Bot", StringComparison.Ordinal);

            int FindPosition(Dictionary<int, string> map, string userId)
            {
                foreach (var kvp in map)
                {
                    if (kvp.Value == userId) return kvp.Key;
                }
                return PlayerPositionGuest;
            }

            int GetTeamFromPosition(int position)
            {
                if (position == PhotonBattleRoom.PlayerPosition1 || position == PhotonBattleRoom.PlayerPosition2) return PhotonBattleRoom.TeamAlphaValue;
                if (position == PhotonBattleRoom.PlayerPosition3 || position == PhotonBattleRoom.PlayerPosition4) return PhotonBattleRoom.TeamBetaValue;
                return PhotonBattleRoom.NoTeamValue;
            }

            bool MapsEqual(Dictionary<int, string> first, Dictionary<int, string> second)
            {
                foreach (int key in first.Keys)
                {
                    string firstValue = first[key] ?? string.Empty;
                    string secondValue = second[key] ?? string.Empty;
                    if (!string.Equals(firstValue, secondValue, StringComparison.Ordinal)) return false;
                }
                return true;
            }

            bool TryBuildTeamMap(Dictionary<int, string> sourceMap, int targetTeamNumber, out Dictionary<int, string> resultMap, out int displacedBots)
            {
                resultMap = null;
                displacedBots = int.MaxValue;

                int[] teamSlots = targetTeamNumber == PhotonBattleRoom.TeamAlphaValue
                    ? new[] { PhotonBattleRoom.PlayerPosition1, PhotonBattleRoom.PlayerPosition2 }
                    : new[] { PhotonBattleRoom.PlayerPosition3, PhotonBattleRoom.PlayerPosition4 };

                int[] otherSlots = targetTeamNumber == PhotonBattleRoom.TeamAlphaValue
                    ? new[] { PhotonBattleRoom.PlayerPosition3, PhotonBattleRoom.PlayerPosition4 }
                    : new[] { PhotonBattleRoom.PlayerPosition1, PhotonBattleRoom.PlayerPosition2 };

                Dictionary<int, string> workingMap = new(sourceMap);

                // Capture everyone currently on the target side (including humans) so we can rebalance
                // to the opposite side if a full room has two duos split across teams.
                List<string> displacedValues = new();
                foreach (int slot in teamSlots)
                {
                    string slotValue = workingMap[slot];
                    if (!string.IsNullOrEmpty(slotValue)) displacedValues.Add(slotValue);
                }

                workingMap[teamSlots[0]] = userId1;
                workingMap[teamSlots[1]] = userId2;

                List<string> otherSideCombined = new();
                HashSet<string> seen = new(StringComparer.Ordinal);

                foreach (int slot in otherSlots)
                {
                    string slotValue = workingMap[slot];
                    if (string.IsNullOrEmpty(slotValue)) continue;
                    if (seen.Add(slotValue)) otherSideCombined.Add(slotValue);
                }

                foreach (string displacedValue in displacedValues)
                {
                    if (string.IsNullOrEmpty(displacedValue)) continue;
                    if (seen.Add(displacedValue)) otherSideCombined.Add(displacedValue);
                }

                if (otherSideCombined.Count > otherSlots.Length) return false;

                foreach (int slot in otherSlots)
                {
                    workingMap[slot] = string.Empty;
                }

                for (int i = 0; i < otherSideCombined.Count; i++)
                {
                    workingMap[otherSlots[i]] = otherSideCombined[i];
                }

                displacedBots = 0;
                foreach (string value in displacedValues)
                {
                    if (IsBotValue(value)) displacedBots++;
                }

                if (displacedBots > 0)
                {
                    int botCountOnOtherSide = 0;
                    foreach (int slot in otherSlots)
                    {
                        if (IsBotValue(workingMap[slot])) botCountOnOtherSide++;
                    }
                    if (botCountOnOtherSide < displacedBots)
                    {
                        return false;
                    }
                }

                resultMap = workingMap;
                return true;
            }

            int userId1Position = FindPosition(currentMap, userId1);
            int userId2Position = FindPosition(currentMap, userId2);
            int userId1Team = GetTeamFromPosition(userId1Position);
            int userId2Team = GetTeamFromPosition(userId2Position);

            // Already on same team: don't churn room properties.
            if (userId1Team != PhotonBattleRoom.NoTeamValue && userId1Team == userId2Team)
            {
                teamNumber = userId1Team;
                Debug.Log($"TryReservePremadePairToSameSide: users already on same team {teamNumber} ({userId1},{userId2}).");
                return true;
            }

            Dictionary<int, string> baseMap = new(currentMap);
            foreach (int key in baseMap.Keys.ToArray())
            {
                if (baseMap[key] == userId1 || baseMap[key] == userId2)
                {
                    baseMap[key] = string.Empty;
                }
            }

            bool alphaValid = TryBuildTeamMap(baseMap, PhotonBattleRoom.TeamAlphaValue, out Dictionary<int, string> alphaMap, out int alphaDisplacedBots);
            bool betaValid = TryBuildTeamMap(baseMap, PhotonBattleRoom.TeamBetaValue, out Dictionary<int, string> betaMap, out int betaDisplacedBots);

            if (!alphaValid && !betaValid)
            {
                Debug.LogWarning($"TryReservePremadePairToSameSide: no valid target team for ({userId1},{userId2}) (alphaValid={alphaValid}, betaValid={betaValid}).");
                return false;
            }

            int alphaAffinity = (userId1Team == PhotonBattleRoom.TeamAlphaValue ? 1 : 0) + (userId2Team == PhotonBattleRoom.TeamAlphaValue ? 1 : 0);
            int betaAffinity = (userId1Team == PhotonBattleRoom.TeamBetaValue ? 1 : 0) + (userId2Team == PhotonBattleRoom.TeamBetaValue ? 1 : 0);

            Dictionary<int, string> targetMap;
            if (alphaValid && betaValid)
            {
                if (alphaAffinity > betaAffinity)
                {
                    teamNumber = PhotonBattleRoom.TeamAlphaValue;
                    targetMap = alphaMap;
                }
                else if (betaAffinity > alphaAffinity)
                {
                    teamNumber = PhotonBattleRoom.TeamBetaValue;
                    targetMap = betaMap;
                }
                else if (alphaDisplacedBots < betaDisplacedBots)
                {
                    teamNumber = PhotonBattleRoom.TeamAlphaValue;
                    targetMap = alphaMap;
                }
                else if (betaDisplacedBots < alphaDisplacedBots)
                {
                    teamNumber = PhotonBattleRoom.TeamBetaValue;
                    targetMap = betaMap;
                }
                else
                {
                    teamNumber = PhotonBattleRoom.TeamAlphaValue;
                    targetMap = alphaMap;
                }
            }
            else if (alphaValid)
            {
                teamNumber = PhotonBattleRoom.TeamAlphaValue;
                targetMap = alphaMap;
            }
            else
            {
                teamNumber = PhotonBattleRoom.TeamBetaValue;
                targetMap = betaMap;
            }

            if (MapsEqual(currentMap, targetMap)) return true;

            try
            {
                string targetSnapshot = $"t1={targetMap[PhotonBattleRoom.PlayerPosition1]},t2={targetMap[PhotonBattleRoom.PlayerPosition2]},t3={targetMap[PhotonBattleRoom.PlayerPosition3]},t4={targetMap[PhotonBattleRoom.PlayerPosition4]}";
                Debug.Log($"TryReservePremadePairToSameSide: applying targetMap for team {teamNumber}: {targetSnapshot}");
            }
            catch { }

            foreach (int key in targetMap.Keys)
            {
                string currentValue = currentMap[key] ?? string.Empty;
                string targetValue = targetMap[key] ?? string.Empty;
                if (string.Equals(currentValue, targetValue, StringComparison.Ordinal)) continue;

                try
                {
                    Debug.Log($"TryReservePremadePairToSameSide: setting position {key} -> '{targetValue}' (was '{currentValue}')");
                }
                catch { }

                PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.GetPositionKey(key), targetValue);
                try
                {
                    string rb = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.GetPositionKey(key), string.Empty);
                    Debug.Log($"TryReservePremadePairToSameSide: readback position {key} -> '{rb}'");
                }
                catch { }
            }

            return true;
        }

        #endregion

        #region Game Start & Quantum

        private void OnStartRoomEvent(StartRoomEvent data)
        {
            Debug.Log($"onEvent {data}");
            StartCoroutine(OnStartRoom());
        }

        private IEnumerator OnStartRoom()
        {
            if (_serverMatchmakingActive)
            {
                yield break;
            }

            float startTime =Time.time;
            yield return new WaitUntil(() =>
                _serverMatchmakingActive
                || PhotonRealtimeClient.Client.InRoom
                || Time.time > startTime + 10);

            if (_serverMatchmakingActive)
            {
                yield break;
            }

            if (!PhotonRealtimeClient.Client.InRoom)
            {
                Debug.LogWarning("Failed to join a room in time.");
                if (PhotonRealtimeClient.InRoom) PhotonRealtimeClient.LeaveRoom();
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

            if (_matchmakingHolder != null || _serverMatchmakingActive)
            {
                Debug.Log("OnStartMatchmakingEvent: server matchmaking is already active.");
                return;
            }

            if (data.IsPremadeInRoom && PhotonRealtimeClient.LocalPlayer != null && !PhotonRealtimeClient.LocalPlayer.IsMasterClient)
            {
                Debug.Log("OnStartMatchmakingEvent: ignoring premade server matchmaking start from non-master client.");
                return;
            }

            _lastStartMatchmakingAcceptedTime = Time.time;
            _matchmakingHolder = StartCoroutine(StartServerMatchmaking(
                data.SelectedMatchmakingType,
                data.SelectedGameType,
                data.IsPremadeInRoom));
        }

        private void OnStopMatchmakingEvent(StopMatchmakingEvent data)
        {
            Debug.Log($"onEvent {data}");
            if (_serverMatchmakingActive) StartCoroutine(StopServerMatchmaking());
        }

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
                if (_serverMatchmakingActive && !PhotonLobbyRoom.IsValidPlayerPos(masterPosition))
                {
                    ApplyServerMatchPosition();
                    float positionDeadline = Time.realtimeSinceStartup + ServerPhotonPositionReadyTimeoutSeconds;
                    while (!PhotonLobbyRoom.IsValidPlayerPos(masterPosition)
                        && PhotonRealtimeClient.InRoom
                        && Time.realtimeSinceStartup < positionDeadline)
                    {
                        yield return null;
                        masterPosition = player.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
                    }
                }
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
                    bool hasSlotConflict = PhotonLobbyRoom.IsValidPlayerPos(playerPos)
                        && !string.IsNullOrEmpty(playerUserIds[playerPos - 1])
                        && playerUserIds[playerPos - 1] != roomPlayer.UserId;

                    if (!PhotonLobbyRoom.IsValidPlayerPos(playerPos) || hasSlotConflict)
                    {
                        // If player position is not valid we get new position for them, this method checks for duplicate and missing player positions
                        int newPos = PhotonLobbyRoom.GetFirstFreePlayerPos(new(roomPlayer));
                        if (!PhotonLobbyRoom.IsValidPlayerPos(newPos))
                        {
                            newPos = GetFirstFreePositionWithoutVerification();
                        }

                        bool newPosConflict = PhotonLobbyRoom.IsValidPlayerPos(newPos)
                            && !string.IsNullOrEmpty(playerUserIds[newPos - 1])
                            && playerUserIds[newPos - 1] != roomPlayer.UserId;

                        if (newPosConflict)
                        {
                            newPos = PhotonBattleRoom.PlayerPositionGuest;
                        }

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

                if (PhotonRealtimeClient.InMatchmakingRoom)
                {
                    HashSet<string> mappedHumanUserIds = new(
                        playerUserIds.Where(uid => !string.IsNullOrEmpty(uid) && uid != "Bot"),
                        StringComparer.Ordinal);

                    List<string> missingHumanUserIds = players
                        .Where(p => p != null && !string.IsNullOrEmpty(p.UserId) && p.UserId != "Bot")
                        .Select(p => p.UserId)
                        .Where(uid => !mappedHumanUserIds.Contains(uid))
                        .Distinct(StringComparer.Ordinal)
                        .ToList();

                    if (missingHumanUserIds.Count > 0)
                    {
                        Debug.LogWarning($"StartTheGameplay: refusing partial start because {missingHumanUserIds.Count} human players are missing from start slots. missing=[{string.Join(",", missingHumanUserIds)}], slots=[{string.Join(",", playerUserIds)}].");

                        if (IsCurrentServerMatchRoom())
                        {
                            Debug.LogWarning("The assigned server match room is missing players; gameplay start was cancelled.");
                            OnFailedToStartMatchmakingGame?.Invoke();
                            yield break;
                        }

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

                        StartingGameFailed();
                        yield break;
                    }
                }

                int j = 1;
                foreach(PlayerType type in playerTypes)
                {
                    if(type == PlayerType.None)
                    {
                        string positionKey = PhotonBattleRoom.GetPositionKey(j);
                        string positionValue = PhotonRealtimeClient.LobbyCurrentRoom?.GetCustomProperty<string>(positionKey);
                        if (positionValue == "Bot")
                        {
                            playerTypes[j - 1] = PlayerType.Bot;
                            playerUserNames[j - 1] = "Bot";
                        }
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
                        TestMode = SettingsCarrier.Instance.BattleDebug,
                    };

                }
                bool useCountdown = true;
                try
                {
                    MatchmakingType roomMatchType = (MatchmakingType)room.GetCustomProperty<int>(PhotonBattleRoom.MatchmakingKey);
                    GameType roomGameType = (GameType)room.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey);
                    useCountdown = roomMatchType != MatchmakingType.Custom;
                    data.MatchmakingType = roomMatchType;
                    data.GameType = roomGameType;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"StartTheGameplay: failed to read room game type for countdown selection: {ex.Message}");
                }

                if (useCountdown)
                {
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
                        StartingGameFailed();
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
            _matchHasStartedInCurrentRoom = false;
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
                MatchmakingType gameType = (MatchmakingType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.MatchmakingKey);
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
                        StartingGameFailed();
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
                        ProjectileInitialEmotion = (BattleEmotionState)data.ProjectileInitialEmotion,
                        IsTestMode = data.TestMode,
                        IsTestFlipperGame = data.GameType == GameType.BattleFlipperGame
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

                Room room = PhotonRealtimeClient.CurrentRoom;
                string alphaTeamName = room.GetCustomProperty(TeamAlphaNameKey, "Alpha");
                string alphaTeamId = room.GetCustomProperty(PhotonBattleRoom.ClanIdKey, "");
                string betaTeamName = room.GetCustomProperty(TeamBetaNameKey, "Beta");
                string betaTeamId = room.GetCustomProperty(PhotonBattleRoom.ClanOpponentIdKey, "");

                _roomInfo = new()
                {
                    MatchmakingType = data.MatchmakingType,
                    GameType = data.GameType,
                    Player1Id = data.PlayerSlotUserIds[0],
                    Player1Name = data.PlayerSlotUserNames[0],
                    Player2Id = data.PlayerSlotUserIds[1],
                    Player2Name = data.PlayerSlotUserNames[1],
                    Player3Id = data.PlayerSlotUserIds[2],
                    Player3Name = data.PlayerSlotUserNames[2],
                    Player4Id = data.PlayerSlotUserIds[3],
                    Player4Name = data.PlayerSlotUserNames[3],
                    TeamAlphaName = alphaTeamName,
                    TeamAlphaId = alphaTeamId,
                    TeamBetaName = betaTeamName,
                    TeamBetaId = betaTeamId,

                };

                // Start Battle Countdown (request UI to show countdown)
                _battleStartUiReady = false;
                OnLobbyWindowChangeRequest?.Invoke(LobbyWindowTarget.BattleLoad);
                _isStartFinished = false;

                // Wait for BattleLoad UI to initialize and signal readiness.
                const float onStartUiReadyTimeout = 5f;
                float uiReadyStart = Time.time;
                while (!_battleStartUiReady && Time.time - uiReadyStart < onStartUiReadyTimeout)
                {
                    yield return null;
                }
                if (!_battleStartUiReady)
                {
                    Debug.LogWarning("StartQuantum: Battle start UI was not ready after timeout; proceeding.");
                }

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

                FreezeMatchmakingTimer();

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
                        StartingGameFailed();
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
                    SetPlayerQuantumCharacters(data.GameType, selectedCharacters);
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
                Debug.LogException(ex);
                return false;
            }
            finally
            {
                try { pluginDisconnectListener?.Dispose(); } catch (Exception ex) { Debug.LogWarning($"StartRunner: failed to dispose pluginDisconnectListener: {ex.Message}"); }
            }
            return true;
        }

        public static void ExitQuantum(BattleTeamNumber winningTeam, BattleTeamNumber ownTeam, float gameLengthSec)
        {
            DataCarrier.AddData(DataCarrier.BattleWinner, (int)winningTeam);
            DataCarrier.AddData(DataCarrier.OwnBattleResult, winningTeam == ownTeam);
            Room room = PhotonRealtimeClient.CurrentRoom;
            string alphaTeamName = room.GetCustomProperty(TeamAlphaNameKey, "Alpha");
            string betaTeamName = room.GetCustomProperty(TeamBetaNameKey, "Beta");
            DataCarrier.AddData(DataCarrier.BattleOwnTeamName, ownTeam is BattleTeamNumber.TeamAlpha? alphaTeamName : betaTeamName);
            DataCarrier.AddData(DataCarrier.BattleEnemyTeamName, ownTeam is BattleTeamNumber.TeamAlpha ? betaTeamName : alphaTeamName);
            if (Instance != null
                && ServerManager.Instance != null
                && !string.IsNullOrEmpty(ServerManager.Instance.ActiveMatchmakingMatchId)
                && PhotonRealtimeClient.LocalPlayer != null
                && PhotonRealtimeClient.LocalPlayer.IsMasterClient)
            {
                string winningSide = winningTeam == BattleTeamNumber.TeamAlpha ? "A" : "B";
                Instance.StartCoroutine(ServerManager.Instance.MatchmakingFinishMatch(winningSide));
            }
            CloseRunner();
            OnLobbyWindowChangeRequest?.Invoke(LobbyWindowTarget.BattleStory);
        }

        public static void CloseRunner()
        {
            JukeboxManager.Instance.DisableJukeBox();
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

        #endregion

        #region Player Position & Bots

        private bool _playerPosChangeInProgress = false;

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
                            // Queue-formed rooms can be full with pre-reserved slots for expected users.
                            // If our user is already reserved, sync local player property instead of treating this as a kick.
                            int reservedPosition = GetReservedRoomPositionForUser(PhotonRealtimeClient.LocalPlayer?.UserId ?? string.Empty);
                            if (PhotonLobbyRoom.IsValidPlayerPos(reservedPosition))
                            {
                                success = true;
                                if (setToPlayerProperties)
                                {
                                    try
                                    {
                                        PhotonRealtimeClient.LocalPlayer.SetCustomProperty(PlayerPositionKey, reservedPosition);
                                        Debug.Log($"ReserveFreePosition: room is full but local user already reserved in slot {reservedPosition}; synchronized PlayerPositionKey.");
                                    }
                                    catch (Exception ex) { Debug.LogWarning($"ReserveFreePosition: failed to set reserved local position: {ex.Message}"); }
                                }
                                break;
                            }

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
                        catch (Exception ex) { Debug.LogWarning($"ReserveFreePosition: SetCustomProperties failed: {ex.Message}"); sent = false; }

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
                                        catch (Exception ex) { Debug.LogWarning($"ReserveFreePosition: failed to set local player property: {ex.Message}"); }
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
                                catch (Exception ex) { Debug.LogWarning($"ReserveFreePosition: failed to set local player property: {ex.Message}"); }
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
            if (PhotonRealtimeClient.CurrentRoom.MaxPlayers == 2)
            {
                pos2Set = true;
                pos4Set = true;
            }

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

            if (!pos1Set)
            {
                if (PhotonRealtimeClient.CurrentRoom.GetCustomProperty(PhotonBattleRoom.PlayerPositionKey1, "").Equals("Bot") || AppPlatform.IsEditor || AppPlatform.IsDevelopmentBuild)
                    pos1Set = true;
            }
            if (!pos2Set)
            {
                if (PhotonRealtimeClient.CurrentRoom.GetCustomProperty(PhotonBattleRoom.PlayerPositionKey2, "").Equals("Bot") || AppPlatform.IsEditor || AppPlatform.IsDevelopmentBuild)
                    pos2Set = true;
            }
            if (!pos3Set)
            {
                if (PhotonRealtimeClient.CurrentRoom.GetCustomProperty(PhotonBattleRoom.PlayerPositionKey3, "").Equals("Bot") || AppPlatform.IsEditor || AppPlatform.IsDevelopmentBuild)
                    pos3Set = true;
            }
            if (!pos4Set)
            {
                if (PhotonRealtimeClient.CurrentRoom.GetCustomProperty(PhotonBattleRoom.PlayerPositionKey4, "").Equals("Bot") || AppPlatform.IsEditor || AppPlatform.IsDevelopmentBuild)
                    pos4Set = true;
            }

            Debug.Log($"Is set?: {pos1Set}:{pos2Set}:{pos3Set}:{pos4Set}");
            return pos1Set && pos2Set && pos3Set && pos4Set;
        }

        private bool CheckIfAllPlayersAreReady()
        {
            bool pos1Ready = false;
            bool pos2Ready = false;
            bool pos3Ready = false;
            bool pos4Ready = false;
            if (PhotonRealtimeClient.CurrentRoom.MaxPlayers == 2)
            {
                pos2Ready = true;
                pos4Ready = true;
            }

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
                        if (pos1Ready) return false;
                        if (!player.GetCustomProperty(PhotonBattleRoom.PlayerReadyKey, false)) return false;
                        pos1Ready = true;
                        break;
                    case 2:
                        if (pos2Ready) return false;
                        if (!player.GetCustomProperty(PhotonBattleRoom.PlayerReadyKey, false)) return false;
                        pos2Ready = true;
                        break;
                    case 3:
                        if (pos3Ready) return false;
                        if (!player.GetCustomProperty(PhotonBattleRoom.PlayerReadyKey, false)) return false;
                        pos3Ready = true;
                        break;
                    case 4:
                        if (pos4Ready) return false;
                        if (!player.GetCustomProperty(PhotonBattleRoom.PlayerReadyKey, false)) return false;
                        pos4Ready = true;
                        break;
                    default: return false;

                }
            }

            if (!pos1Ready)
            {
                if (PhotonRealtimeClient.CurrentRoom.GetCustomProperty(PhotonBattleRoom.PlayerPositionKey1, "").Equals("Bot") || AppPlatform.IsEditor || AppPlatform.IsDevelopmentBuild)
                    pos1Ready = true;
            }
            if (!pos2Ready)
            {
                if (PhotonRealtimeClient.CurrentRoom.GetCustomProperty(PhotonBattleRoom.PlayerPositionKey2, "").Equals("Bot") || AppPlatform.IsEditor || AppPlatform.IsDevelopmentBuild)
                    pos2Ready = true;
            }
            if (!pos3Ready)
            {
                if (PhotonRealtimeClient.CurrentRoom.GetCustomProperty(PhotonBattleRoom.PlayerPositionKey3, "").Equals("Bot") || AppPlatform.IsEditor || AppPlatform.IsDevelopmentBuild)
                    pos3Ready = true;
            }
            if (!pos4Ready)
            {
                if (PhotonRealtimeClient.CurrentRoom.GetCustomProperty(PhotonBattleRoom.PlayerPositionKey4, "").Equals("Bot") || AppPlatform.IsEditor || AppPlatform.IsDevelopmentBuild)
                    pos4Ready = true;
            }

            Debug.Log($"Is ready?: {pos1Ready}:{pos2Ready}:{pos3Ready}:{pos4Ready}");
            return pos1Ready && pos2Ready && pos3Ready && pos4Ready;
        }

        // Removed no-op CheckIfBattleCanStart coroutine: it only contained an immediate yield break and was unused.

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

        private void OnReadyToggleEvent(ReadyToggleEvent data)
        {
            StartCoroutine(SetReady(data.PlayerPosition, data.Ready));
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
                        if (playerCount + botCount >= room.MaxPlayers && PhotonRealtimeClient.CurrentRoom.IsOpen) PhotonRealtimeClient.CloseRoom();
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

        private IEnumerator SetReady(int playerPosition, bool active)
        {
            yield return new WaitUntil(() => !_playerPosChangeInProgress);

            _playerPosChangeInProgress = true;

            Assert.IsTrue(PhotonLobbyRoom.IsValidGameplayPosOrGuest(playerPosition));

            // Initializing hash tables for setting the new position as taken
            string readyKey = PhotonBattleRoom.PlayerReadyKey;

            LobbyPlayer player = PhotonRealtimeClient.LobbyCurrentRoom.GetPlayer(PhotonRealtimeClient.LocalPlayer.ActorNumber);

            string newPositionKey = PhotonBattleRoom.GetPositionKey(playerPosition);

            string playerId = PhotonRealtimeClient.LobbyCurrentRoom.GetCustomProperty(newPositionKey, string.Empty);

            if(playerId != player.UserId && active)
            {
                Debug.LogWarning("Player is not in valid position.");
                _playerPosChangeInProgress = false;
                yield break;
            }

            LobbyPhotonHashtable newPosition;
            LobbyPhotonHashtable expectedValue;

            newPosition = new LobbyPhotonHashtable(new Dictionary<object, object> { { readyKey, active } });

            if (player.HasCustomProperty(readyKey))
                expectedValue = new LobbyPhotonHashtable(new Dictionary<object, object> { { readyKey, !active } }); // Expecting the new position to be empty
            else
                expectedValue = null; // Expecting the new position to be empty
            if (player.SetCustomProperties(newPosition, expectedValue))
            {
                float timeout = Time.time + 1f;
                bool success = false;
                while (Time.time < timeout)
                {
                    // Checking if the position is set to have a Bot
                    if (player.GetCustomProperty<bool>(readyKey) == active)
                    {
                        success = true;
                        break;
                    }
                    yield return new WaitForSeconds(0.1f);
                }

                if (success)
                {
                    Debug.Log($"Set Player to Ready.");
                }
            }
            else
            {
                Debug.LogWarning($"Something went wrong.");
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
                    bool waitShortAndContinue = false;
                    try
                    {
                        if (!CanMutateRoomPropertiesNow())
                        {
                            waitShortAndContinue = true;
                        }
                        else
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
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"VerifyRoomPositionsLoop: unexpected error: {ex.Message}");
                    }

                    if (waitShortAndContinue)
                    {
                        yield return new WaitForSeconds(0.5f);
                        continue;
                    }

                    yield return new WaitForSeconds(2f);
                }
            }
            finally
            {
                _verifyPositionsHolder = null;
            }
        }

        public void SetPlayerQuantumCharacters(GameType gameType, List<CustomCharacter> characters)
        {
            int characterCount = gameType == GameType.BattleFlipperGame
                ? RuntimePlayer.FlipperCharacterCount
                : RuntimePlayer.NormalCharacterCount;

            Assert.IsTrue(
                characters.Count == characterCount,
                string.Format("Invalid number of Characters (not {0})", characterCount)
            );

            CustomCharacter character;
            for (int i = 0; i < characterCount; i++) {
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

                _player.Characters[i] = new BattleCharacterBase()
                {
                    Id            = (BattlePlayerCharacterID)character.Id,
                    Class         = (BattlePlayerCharacterClass)character.CharacterClassType,
                    Stats         = stats,
                };
            }
        }
        #endregion

        #region Photon Callbacks

        private void OnGetKickedEvent(GetKickedEvent data)
        {
            if (IsServerAuthoritativeMatchmakingActive())
            {
                OnFailedToStartMatchmakingGame?.Invoke();
            }
            else if (PhotonRealtimeClient.InRoom)
            {
                PhotonRealtimeClient.LeaveRoom();
            }

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
            if (_gamePlayedOut || PhotonRealtimeClient.Client.State == ClientState.Leaving) return;

            if (IsCurrentServerMatchRoom())
            {
                LobbyOnPlayerLeftRoom?.Invoke(new(otherPlayer));
                return;
            }

            if (!_matchHasStartedInCurrentRoom && IsGameStartTransitionActive())
            {
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
                NotifyGameStartCancelled();
            }

            Room room = PhotonRealtimeClient.CurrentRoom;
            if (room != null && PhotonRealtimeClient.LocalPlayer.IsMasterClient)
            {
                int otherPlayerPosition = otherPlayer.GetCustomProperty<int>(PlayerPositionKey, PlayerPositionGuest);
                if (PhotonLobbyRoom.IsValidPlayerPos(otherPlayerPosition))
                {
                    string positionKey = PhotonBattleRoom.GetPositionKey(otherPlayerPosition);
                    var emptyPosition = new LobbyPhotonHashtable(new Dictionary<object, object> { { positionKey, "" } });
                    var expectedValue = new LobbyPhotonHashtable(new Dictionary<object, object> { { positionKey, otherPlayer.UserId } });
                    PhotonRealtimeClient.LobbyCurrentRoom?.SetCustomProperties(emptyPosition, expectedValue);
                }
                _posChangeQueue.Remove(otherPlayer.UserId);
                QueueCustomBattleStartCheck();
            }

            LobbyOnPlayerLeftRoom?.Invoke(new(otherPlayer));
        }

        public void OnJoinedRoom()
        {
            _gamePlayedOut = false;
            _matchHasStartedInCurrentRoom = false;
            _countdownActive = false;
            _lastCountdownStartTime = -100f;
            _pendingInRoomInviteRoomName = string.Empty;
            _pendingAcceptedInRoomInviteRoomName = string.Empty;
            _pendingAcceptedInRoomInviteStartTime = -100f;
            PhotonRealtimeClient.EnableCloseConnection = true;

            if (IsCurrentServerMatchRoom())
            {
                ApplyServerMatchPosition();
                bool isLeader = PhotonRealtimeClient.LocalPlayer != null && PhotonRealtimeClient.LocalPlayer.IsMasterClient;
                NotifyMatchmakingRoomEntered(isLeader);
                OnRoomLeaderChanged?.Invoke(isLeader);

                if (_serverMatchStartConfirmedByBackend
                    || PhotonRealtimeClient.CurrentRoom.GetCustomProperty<bool>(PhotonBattleRoom.ServerMatchStartedKey, false))
                {
                    _serverMatchStartConfirmedByBackend = true;
                    PublishServerMatchStartToPhoton();
                    TryStartServerMatchGameplay();
                }

                StartCoroutine(ConfirmServerPhotonReady());
                return;
            }

            Room room = PhotonRealtimeClient.CurrentRoom;
            MatchmakingType roomType = MatchmakingType.None;
            try
            {
                if (room?.CustomProperties != null && room.CustomProperties.ContainsKey(PhotonBattleRoom.MatchmakingKey))
                {
                    roomType = (MatchmakingType)room.GetCustomProperty<int>(PhotonBattleRoom.MatchmakingKey);
                }
            }
            catch { }

            if (roomType == MatchmakingType.FriendLobby)
            {
                string invitedUserId = room.GetCustomProperty<string>(PhotonBattleRoom.PremadeInvitedUserIdKey, string.Empty);
                if (!string.IsNullOrEmpty(invitedUserId) && invitedUserId == PhotonRealtimeClient.LocalPlayer.UserId)
                {
                    room.SetCustomProperty(PhotonBattleRoom.PremadeInviteStateKey, PhotonBattleRoom.PremadeInviteStateAccepted);
                    room.SetCustomProperty(PhotonBattleRoom.PremadeUserId2Key, invitedUserId);
                    room.SetCustomProperty(PhotonBattleRoom.PremadeUsername2Key, PhotonRealtimeClient.LocalPlayer.NickName);
                }
            }

            if (PhotonRealtimeClient.InMatchmakingRoom)
            {
                bool isLeader = PhotonRealtimeClient.LocalPlayer != null && PhotonRealtimeClient.LocalPlayer.IsMasterClient;
                NotifyMatchmakingRoomEntered(isLeader);
                OnRoomLeaderChanged?.Invoke(isLeader);
                if (isLeader && _verifyPositionsHolder == null)
                {
                    _verifyPositionsHolder = StartCoroutine(VerifyRoomPositionsLoop());
                }
                if (roomType == MatchmakingType.Custom) QueueCustomBattleStartCheck();
                return;
            }

            LobbyOnJoinedRoom?.Invoke();
        }

        public void OnLeftRoom()
        {
            if (ServerManager.Instance != null && ServerManager.Instance.HasActiveMatchmakingRoom)
            {
                StartCoroutine(ServerManager.Instance.MatchmakingLeaveRoom(success =>
                {
                    if (!success) Debug.LogWarning("OnLeftRoom: failed to leave the active server matchmaking room.");
                }));
            }

            _gamePlayedOut = false;
            _matchHasStartedInCurrentRoom = false;
            _countdownActive = false;
            _lastCountdownStartTime = -100f;
            _pendingInRoomInviteRoomName = string.Empty;
            _pendingAcceptedInRoomInviteRoomName = string.Empty;
            _pendingAcceptedInRoomInviteStartTime = -100f;
            _lastAutoInviteRoomName = string.Empty;
            _lastAutoInviteJoinTime = -100f;
            CurrentRooms = null;

            if (PhotonRealtimeClient.LocalPlayer.HasCustomProperty(PlayerPositionKey))
            {
                PhotonRealtimeClient.LocalPlayer.RemoveCustomProperty(PlayerPositionKey);
            }

            StopHolderCoroutines();
            if (_verifyPositionsHolder != null)
            {
                StopCoroutine(_verifyPositionsHolder);
                _verifyPositionsHolder = null;
            }
            if (_serviceHolder == null) _serviceHolder = StartCoroutine(Service());
            LobbyOnLeftRoom?.Invoke();
        }

        public void OnCreatedRoom()
        {
            Debug.Log($"Created room {PhotonRealtimeClient.Client.CurrentRoom.Name}");
            if (_serviceHolder == null) _serviceHolder = StartCoroutine(Service());

            if (_matchmakingHolder == null)
            {
                LobbyOnCreatedRoom?.Invoke();
            }
            else
            {
                NotifyMatchmakingRoomEntered(true);
            }
        }
        public void OnJoinedLobby() { if (_serviceHolder == null) _serviceHolder = StartCoroutine(Service()); LobbyOnJoinedLobby?.Invoke(); }

        public void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            List<LobbyRoomInfo> lobbyRoomList = CurrentRooms != null
                ? CurrentRooms.ToList()
                : new List<LobbyRoomInfo>();

            foreach (RoomInfo roomInfo in roomList)
            {
                LobbyRoomInfo updatedRoom = new(roomInfo);
                int existingIndex = lobbyRoomList.FindIndex(room => room.Name.Equals(updatedRoom.Name, StringComparison.Ordinal));
                if (existingIndex != -1)
                {
                    lobbyRoomList.RemoveAt(existingIndex);
                }

                if (!updatedRoom.RemovedFromList)
                {
                    lobbyRoomList.Add(updatedRoom);
                }
            }

            if (lobbyRoomList.Any(room => room.RemovedFromList))
            {
                lobbyRoomList = lobbyRoomList.Where(room => !room.RemovedFromList).ToList();
            }

            CurrentRooms = lobbyRoomList.AsReadOnly();

            LobbyOnRoomListUpdate?.Invoke(lobbyRoomList);
        }

        public void OnLeftLobby()
        {
            CurrentRooms = null;
            LobbyOnLeftLobby?.Invoke();
        }
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
            LobbyOnJoinRoomFailed?.Invoke(returnCode, message);

            if (!string.IsNullOrEmpty(_pendingAcceptedInRoomInviteRoomName))
            {
                string failedRoomName = _pendingAcceptedInRoomInviteRoomName;
                _pendingAcceptedInRoomInviteRoomName = string.Empty;
                _pendingAcceptedInRoomInviteStartTime = -100f;
                OnFriendLobbyJoinFailed?.Invoke(failedRoomName, returnCode, message);
                return;
            }

            if (IsServerAuthoritativeMatchmakingActive())
            {
                OnFailedToStartMatchmakingGame?.Invoke();
            }
        }
        public void OnJoinRandomFailed(short returnCode, string message) { LobbyOnJoinRandomFailed?.Invoke(returnCode, message); }

        public void OnEvent(EventData photonEvent)
        {
            if(photonEvent.Code != 103) Debug.Log($"Received PhotonEvent {photonEvent.Code}");

            bool isCustomRoom = false;

            switch (photonEvent.Code)
            {
                case PhotonRealtimeClient.PhotonEvent.CancelGameStart:
                {
                    if (IsServerAuthoritativeMatchmakingActive()) break;

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

                    _countdownActive = false;
                    _matchHasStartedInCurrentRoom = false;
                    NotifyGameStartCancelled();
                    OnGameCountdownUpdate?.Invoke(-1);
                    OnLobbyWindowChangeRequest?.Invoke(LobbyWindowTarget.LobbyRoom);

                    if (CanMutateRoomPropertiesNow("CancelGameStart: clear BattleID", true)
                        && PhotonRealtimeClient.CurrentRoom.CustomProperties.ContainsKey(PhotonBattleRoom.BattleID))
                    {
                        PhotonRealtimeClient.CurrentRoom.SetCustomProperties(
                            new PhotonHashtable { { PhotonBattleRoom.BattleID, "" } });
                    }
                    break;
                }                case PhotonRealtimeClient.PhotonEvent.GameCountdown:
                    if (IsServerAuthoritativeMatchmakingActive() && !IsCurrentServerMatchRoom())
                    {
                        Debug.Log("Ignoring stale legacy GameCountdown outside the assigned server match room.");
                        break;
                    }
                    int countdown = (int)photonEvent.CustomData;
                    // Track countdown active state: >0 means active, <=0 or -1 means inactive/cancel
                    _countdownActive = countdown > 0;
                    if (countdown > 0)
                    {
                        _gamePlayedOut = false;
                        _lastCountdownStartTime = Time.time;
                    }
                    if (countdown >= 0) FreezeMatchmakingTimer();
                    OnGameCountdownUpdate?.Invoke(countdown);
                    break;
                case PhotonRealtimeClient.PhotonEvent.StartGame:
                    if (IsServerAuthoritativeMatchmakingActive() && !IsCurrentServerMatchRoom())
                    {
                        Debug.Log("Ignoring stale legacy StartGame outside the assigned server match room.");
                        break;
                    }
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
                    // Starting game clears countdown active flag
                    _countdownActive = false;
                    _gamePlayedOut = false;
                    _matchHasStartedInCurrentRoom = true;

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
                            _matchHasStartedInCurrentRoom = false;
                            NotifyGameStartCancelled();

                            if (PhotonRealtimeClient.LocalLobbyPlayer.IsMasterClient)
                            {
                                if (_matchmakingHolder != null)
                                {
                                    StopCoroutine(_matchmakingHolder);
                                    _matchmakingHolder = null;
                                }
                                StartingGameFailed();
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
                    if (IsServerAuthoritativeMatchmakingActive())
                    {
                        Debug.Log("Ignoring legacy player-position request during server-authoritative matchmaking.");
                        break;
                    }
                    int position = (int)photonEvent.CustomData;
                    Player player = PhotonRealtimeClient.CurrentRoom.GetPlayer(photonEvent.Sender);
                    if (player != null)
                    {
                        if (!_posChangeQueue.Contains(player.UserId)) StartCoroutine(SetPlayer(player, position));
                        else Debug.LogError($"Player {photonEvent.Sender} pos change already queued.");
                    }
                    else Debug.LogError($"Player {photonEvent.Sender} not found in room");

                    isCustomRoom = false;
                    try
                    {
                        if (PhotonRealtimeClient.CurrentRoom != null && PhotonRealtimeClient.CurrentRoom.CustomProperties != null
                                    && PhotonRealtimeClient.CurrentRoom.CustomProperties.ContainsKey(PhotonBattleRoom.MatchmakingKey))
                            isCustomRoom = (MatchmakingType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.MatchmakingKey) == MatchmakingType.Custom;
                    }
                    catch { }

                    if (isCustomRoom) QueueCustomBattleStartCheck();
                    break;

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
            if (room == null) return;

            if (IsCurrentServerMatchRoom())
            {
                LobbyOnPlayerEnteredRoom?.Invoke(new(newPlayer));
                return;
            }

            try
            {
                MatchmakingType roomType = (MatchmakingType)room.GetCustomProperty<int>(PhotonBattleRoom.MatchmakingKey);
                if (roomType == MatchmakingType.FriendLobby)
                {
                    string invitedUserId = room.GetCustomProperty<string>(PhotonBattleRoom.PremadeInvitedUserIdKey, string.Empty);
                    if (!string.IsNullOrEmpty(invitedUserId) && newPlayer?.UserId == invitedUserId)
                    {
                        room.SetCustomProperty(PhotonBattleRoom.PremadeInviteStateKey, PhotonBattleRoom.PremadeInviteStateAccepted);
                        room.SetCustomProperty(PhotonBattleRoom.PremadeUserId2Key, invitedUserId);
                        room.SetCustomProperty(PhotonBattleRoom.PremadeUsername2Key, newPlayer.NickName);
                    }
                }
                else if (roomType == MatchmakingType.Custom)
                {
                    QueueCustomBattleStartCheck();
                }
            }
            catch { }

            if (room.PlayerCount + PhotonBattleRoom.GetBotCount() <= room.MaxPlayers)
            {
                LobbyOnPlayerEnteredRoom?.Invoke(new(newPlayer));
            }
        }
        public void OnRoomPropertiesUpdate(PhotonHashtable propertiesThatChanged)
        {
            if (IsCurrentServerMatchRoom()
                && propertiesThatChanged != null
                && propertiesThatChanged.TryGetValue(PhotonBattleRoom.ServerMatchStartedKey, out object serverStartedValue)
                && serverStartedValue is bool serverStarted
                && serverStarted)
            {
                _serverMatchStartConfirmedByBackend = true;
                TryStartServerMatchGameplay();
            }

            if (PhotonRealtimeClient.LocalPlayer.IsMasterClient)
            {
                bool isCustomRoom = false;
                try
                {
                    if (PhotonRealtimeClient.CurrentRoom != null && PhotonRealtimeClient.CurrentRoom.CustomProperties != null
                            && PhotonRealtimeClient.CurrentRoom.CustomProperties.ContainsKey(PhotonBattleRoom.MatchmakingKey))
                        isCustomRoom = (MatchmakingType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.MatchmakingKey) == MatchmakingType.Custom;
                    if (isCustomRoom) QueueCustomBattleStartCheck();
                }
                catch { }
            }
            LobbyOnRoomPropertiesUpdate?.Invoke(new(propertiesThatChanged));
        }
        public void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonHashtable changedProps)
        {
            if (PhotonRealtimeClient.LocalPlayer.IsMasterClient && changedProps.ContainsKey(PhotonBattleRoom.PlayerReadyKey))
            {
                bool isCustomRoom = false;
                try
                {
                    if (PhotonRealtimeClient.CurrentRoom != null && PhotonRealtimeClient.CurrentRoom.CustomProperties != null
                            && PhotonRealtimeClient.CurrentRoom.CustomProperties.ContainsKey(PhotonBattleRoom.MatchmakingKey))
                        isCustomRoom = (MatchmakingType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.MatchmakingKey) == MatchmakingType.Custom;
                    if (isCustomRoom) QueueCustomBattleStartCheck();
                }
                catch { }
            }
            LobbyOnPlayerPropertiesUpdate?.Invoke(new(targetPlayer),new(changedProps));
        }
        public void OnMasterClientSwitched(Player newMasterClient)
        {
            LobbyOnMasterClientSwitched?.Invoke(new(newMasterClient));
            bool isLocalMaster = newMasterClient != null
                && PhotonRealtimeClient.LocalPlayer != null
                && newMasterClient.UserId == PhotonRealtimeClient.LocalPlayer.UserId;
            OnRoomLeaderChanged?.Invoke(isLocalMaster);

            if (IsCurrentServerMatchRoom())
            {
                if (isLocalMaster) TryStartServerMatchGameplay();
                return;
            }

            if (isLocalMaster) QueueCustomBattleStartCheck();
        }

        #endregion

        #region Event Types

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

        public class ReadyToggleEvent
        {
            public readonly int PlayerPosition;
            public readonly bool Ready;

            public ReadyToggleEvent(int playerPosition, bool value)
            {
                PlayerPosition = playerPosition;
                Ready = value;
            }

            public override string ToString()
            {
                return $"{nameof(PlayerPosition)}: {PlayerPosition}, {nameof(Ready)}: {Ready}";
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
            public readonly MatchmakingType SelectedMatchmakingType;
            public readonly GameType SelectedGameType;
            public readonly bool IsPremadeInRoom;

            public StartMatchmakingEvent(MatchmakingType matchmakingType, GameType gameType, bool isPremadeInRoom = false)
            {
                SelectedMatchmakingType = matchmakingType;
                SelectedGameType = gameType;
                IsPremadeInRoom = isPremadeInRoom;
            }

            public override string ToString()
            {
                return $"{nameof(SelectedGameType)}: {SelectedGameType}, {nameof(IsPremadeInRoom)}: {IsPremadeInRoom}";
            }
        }

        public class StopMatchmakingEvent
        {
            public readonly MatchmakingType SelectedGameType;
            public readonly bool ForceLeave;

            public StopMatchmakingEvent(MatchmakingType gameType, bool forceLeave = false)
            {
                SelectedGameType = gameType;
                ForceLeave = forceLeave;
            }

            public override string ToString()
            {
                return $"{nameof(SelectedGameType)}: {SelectedGameType}, {nameof(ForceLeave)}: {ForceLeave}";
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

        #endregion
    }


    #region StartGameData
    public class StartGameData
    {
        public long StartTime { get; set; }
        public string[] PlayerSlotUserIds { get; set; }
        public string[] PlayerSlotUserNames { get; set; }
        public PlayerType[] PlayerSlotTypes { get; set; }
        public MatchmakingType MatchmakingType { get; set; }
        public GameType GameType { get; set; }
        public Emotion ProjectileInitialEmotion { get; set; }
        public string MapId { get; set; }
        public int PlayerCount { get; set; }
        public int Seed { get; set; }
        public bool TestMode { get; set; }

        public static byte[] Serialize(StartGameData data)
        {
            var b = data;
            byte[] bytes = new byte[0];
            Serializer.Serialize(b.StartTime, ref bytes);
            Serializer.Serialize(b.PlayerSlotUserIds, ref bytes);
            Serializer.Serialize(b.PlayerSlotUserNames, ref bytes);
            Serializer.Serialize(b.PlayerSlotTypes.Cast<int>().ToArray(), ref bytes);
            Serializer.Serialize((int)b.MatchmakingType, ref bytes);
            Serializer.Serialize((int)b.GameType, ref bytes);
            Serializer.Serialize((int)b.ProjectileInitialEmotion, ref bytes);
            Serializer.Serialize(b.MapId, ref bytes);
            Serializer.Serialize(b.PlayerCount, ref bytes);
            Serializer.Serialize(b.Seed, ref bytes);
            Serializer.Serialize(b.TestMode, ref bytes);

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
            result.MatchmakingType = (MatchmakingType)Serializer.DeserializeInt(data, ref offset);
            result.GameType = (GameType)Serializer.DeserializeInt(data, ref offset);
            result.ProjectileInitialEmotion = (Emotion)Serializer.DeserializeInt(data, ref offset);
            result.MapId = Serializer.DeserializeString(data, ref offset);
            result.PlayerCount = Serializer.DeserializeInt(data, ref offset);
            result.Seed = Serializer.DeserializeInt(data, ref offset);
            result.TestMode = Serializer.DeserializeBool(data, ref offset);

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

    #endregion

    public class BattleRoomInfo
    {
        public MatchmakingType MatchmakingType { get; set; }
        public GameType GameType { get; set; }
        public string Player1Id { get; set; }
        public string Player1Name { get; set; }
        public string Player2Id { get; set; }
        public string Player2Name { get; set; }
        public string Player3Id { get; set; }
        public string Player3Name { get; set; }
        public string Player4Id { get; set; }
        public string Player4Name { get; set; }
        public string TeamAlphaName { get; set; }
        public string TeamAlphaId { get; set; }
        public string TeamBetaName { get; set; }
        public string TeamBetaId { get; set; }
    }
}
