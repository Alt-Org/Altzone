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
        // Max time the leader waits before filling remaining Random2v2 slots with bots.
        private const float MatchmakingTimeoutSeconds = 30f;
        // Timeout for followers who join a matchmaking room: if not enough human players join within this interval, auto-leave and requeue.
        private const float MatchmakingJoinTimeoutSeconds = 5f;
        // Marker for matchmaking rooms that were created from queue timeout flow.
        private const string QueueFormedMatchKey = "qfm";
        // When true, clients must enter queue first and never directly create/join matchmaking rooms.
        private static bool UseQueueAuthoritativeMatchmaking = true;
        // Flattened queue duo list [duo1UserA, duo1UserB, duo2UserA, duo2UserB, ...].
        private const string QueueDuoPairsKey = "qdp";
        // Flattened queue solo-pair list [pair1UserA, pair1UserB, pair2UserA, pair2UserB, ...].
        private const string QueueSoloPairsKey = "qsp";
        // Delay before requeueing after leaving a matchmaking room due to timeout.
        private const float MatchmakingRequeueDelaySeconds = 2f;
        // Maximum automatic requeue attempts (0 = unlimited)
        private const int MaxAutoRequeueAttempts = 5;

        // Tracks how many times we've auto-requeued to avoid tight infinite loops.
        private int _autoRequeueAttempts = 0;
        // Holder for a join-timeout watcher coroutine so it can be cancelled.
        private Coroutine _joinTimeoutWatcherHolder = null;

        private QuantumRunner _runner = null;

        private Coroutine _reserveFreePositionHolder = null;
        private Coroutine _requestPositionChangeHolder = null;
        private Coroutine _matchmakingHolder = null;
        private GameType _currentMatchmakingGameType = GameType.Random2v2;
        private Coroutine _followLeaderHolder = null;
        private Coroutine _formingMatchHolder = null;
        private Coroutine _startGameHolder = null;
        // Holder for the client-side StartQuantum coroutine so it can be stopped if needed
        private Coroutine _startQuantumHolder = null;
        // Holder for the background Photon Service coroutine (single instance)
        private Coroutine _serviceHolder = null;
        private Coroutine _autoJoinHolder = null;
        private Coroutine _verifyPositionsHolder = null;
        private Coroutine _canBattleStartCheckHolder = null;
        private Coroutine _queueTimerHolder = null;
        // Tracks last computed number of eligible solo players in queue (for diagnostics / selection caps)
        private int _queuedSoloCount = 0;
        private const float QueueWaitSeconds = 30f;
        private const float QueueReadyStartDelaySeconds = 2f;
        // Increased grace window to reduce race conditions that can split queued duos.
        private const float QueuePendingLeaderGraceSeconds = 20f;
        // Last logged diagnostic state keys for SelectQueueFollowersForMatch (track separately per diagnostic section)
        private string _lastSelectQueuePlayersKey = string.Empty;
        private string _lastSelectQueueLeaderPropsKey = string.Empty;
        private string _lastSelectQueueRecentJoinKey = string.Empty;
        // Backwards-compat fallback key (unused)
        private string _lastSelectQueueFollowersState = string.Empty;
        // Grace interval to consider a player as "just joined" so selection defers briefly
        // allowing a duo partner to arrive and avoid splitting pairs.
        private const float QueueNewJoinGraceSeconds = 1f;
        // Monotonic counter for join attempts so logs can correlate async callbacks to attempts
        private int _joinAttemptCounter = 0;
        // Join attempt tracking to correlate Photon callbacks to specific join attempts
        private readonly object _joinAttemptsLock = new object();
        private int _currentJoinAttemptId = 0;
        private readonly Dictionary<int, JoinAttemptInfo> _joinAttempts = new();

        private class JoinAttemptInfo
        {
            public string RoomName = string.Empty;
            public string[] ExpectedUsers = null;
            public float StartTime = 0f;
            public bool Completed = false;
            public bool Success = false;
            public float CompletionTime = 0f;
            public short? FailureCode = null;
            public string FailureMessage = null;
        }

        private int BeginJoinAttempt(string roomName, string[] expectedUsers = null)
        {
            int id = ++_joinAttemptCounter;
            var info = new JoinAttemptInfo()
            {
                RoomName = roomName ?? string.Empty,
                ExpectedUsers = expectedUsers,
                StartTime = Time.time,
                Completed = false,
                Success = false
            };
            lock (_joinAttemptsLock)
            {
                _joinAttempts[id] = info;
                _currentJoinAttemptId = id;
            }
            Debug.Log($"JoinAttempt[{id}] BEGIN room='{info.RoomName}' teammates={expectedUsers?.Length ?? 0}");
            return id;
        }

        private void MarkJoinAttemptSuccess(int id)
        {
            lock (_joinAttemptsLock)
            {
                if (id == 0) id = _currentJoinAttemptId;
                if (id == 0) return;
                if (_joinAttempts.TryGetValue(id, out var info))
                {
                    info.Completed = true;
                    info.Success = true;
                    info.CompletionTime = Time.time;
                    Debug.Log($"JoinAttempt[{id}] SUCCESS room='{info.RoomName}'");
                }
                if (_currentJoinAttemptId == id) _currentJoinAttemptId = 0;
                _joinAttempts.Remove(id);
            }
        }

        private void MarkJoinAttemptFailure(int id, short returnCode, string message)
        {
            lock (_joinAttemptsLock)
            {
                if (id == 0) id = _currentJoinAttemptId;
                if (id == 0)
                {
                    Debug.LogWarning($"JoinAttempt: failure callback with no current attempt (code={returnCode} msg={message})");
                    return;
                }
                if (_joinAttempts.TryGetValue(id, out var info))
                {
                    info.Completed = true;
                    info.Success = false;
                    info.FailureCode = returnCode;
                    info.FailureMessage = message;
                    info.CompletionTime = Time.time;
                    Debug.Log($"JoinAttempt[{id}] FAILED room='{info.RoomName}' code={returnCode} msg={message}");
                }
                if (_currentJoinAttemptId == id) _currentJoinAttemptId = 0;
                _joinAttempts.Remove(id);
            }
        }

        // Timestamp of the last CancelGameStart handling (used to detect quick rejoins)
        private float _lastStartCancelTime = -100f;
        private string[] _teammates = null;
        private bool _isPremadeMatchmakingFlow = false;
        private string _premadeTeammateUserId = string.Empty;
        private string _lastAutoInviteRoomName = string.Empty;
        private float _lastAutoInviteJoinTime = -100f;
        private string _pendingInRoomInviteRoomName = string.Empty;
        private string _pendingAcceptedInRoomInviteRoomName = string.Empty;
        private float _pendingAcceptedInRoomInviteStartTime = -100f;
        private readonly Dictionary<string, float> _declinedInRoomInviteUntil = new();
        private readonly Dictionary<string, float> _queuePendingLeaderUntil = new(StringComparer.Ordinal);
        private readonly Dictionary<string, float> _queuePendingExpectedUserUntil = new(StringComparer.Ordinal);
        // Record first-seen timestamps for users observed in a matchmaking room to
        // detect very recent joins and avoid splitting transient duo joins.
        private readonly Dictionary<string, float> _queuePlayerFirstSeenAt = new(StringComparer.Ordinal);
        private const float InRoomInvitePromptThrottleSeconds = 5f;
        private const float InRoomInviteDeclineCooldownSeconds = 30f;
        private const float InRoomInviteValiditySeconds = 60f;
        private const float InRoomInviteJoinTimeoutSeconds = 12f;

        private List<FriendInfo> _friendList;

        [HideInInspector] public ReadOnlyCollection<LobbyRoomInfo> CurrentRooms = null; // Set from LobbyRoomListingController.cs through Instance variable maybe this could be refactored?

        private List<string> _posChangeQueue = new();

        private bool _isStartFinished = false;
        private bool _returnToMainMenuOnMatchmakingRejoin = false;
        // Tracks whether a game start countdown is active (GameCountdown > 0)
        private bool _countdownActive = false;
        // Timestamp when a GameCountdown > 0 was last observed locally (Time.time)
        private float _lastCountdownStartTime = -100f;
        // If a master leaves during start, defer returning to the LobbyRoom UI until master switch completes
        private bool _deferReturnToLobbyRoomOnMasterSwitch = false;
        // True after StartGame has been received for the current room; used to avoid pre-match leave/requeue logic after match start.
        private bool _matchHasStartedInCurrentRoom = false;
        // Debounce UI/event spam so StartMatchmaking cannot re-enter in the same transition window.
        private float _lastStartMatchmakingAcceptedTime = -100f;
        // Tracks whether a full-room join failure is already being recovered by LobbyManager.
        private bool _joinFailureAutoRequeueInFlight = false;

        public static LobbyManager Instance { get; private set; }
        public bool IsStartFinished {set => _isStartFinished = value; }
        public bool IsJoinFailureAutoRequeueInFlight => _joinFailureAutoRequeueInFlight;
        public static bool IsActive { get => _isActive;}

        private static bool _isActive = false;
        private static bool _gamePlayedOut = false;
        private static bool _battleStartUiReady = false;

        public bool RunnerActive => _runner != null;

        private void LogSelectQueueStateIfChanged(string tag, string key, string msg)
        {
            try
            {
                bool changed = false;
                if (string.Equals(tag, "players", StringComparison.Ordinal))
                {
                    if (!string.Equals(_lastSelectQueuePlayersKey, key, StringComparison.Ordinal))
                    {
                        _lastSelectQueuePlayersKey = key;
                        changed = true;
                    }
                }
                else if (string.Equals(tag, "leaderProps", StringComparison.Ordinal))
                {
                    if (!string.Equals(_lastSelectQueueLeaderPropsKey, key, StringComparison.Ordinal))
                    {
                        _lastSelectQueueLeaderPropsKey = key;
                        changed = true;
                    }
                }
                else if (string.Equals(tag, "recentJoin", StringComparison.Ordinal))
                {
                    if (!string.Equals(_lastSelectQueueRecentJoinKey, key, StringComparison.Ordinal))
                    {
                        _lastSelectQueueRecentJoinKey = key;
                        changed = true;
                    }
                }
                else
                {
                    if (!string.Equals(_lastSelectQueueFollowersState, key, StringComparison.Ordinal))
                    {
                        _lastSelectQueueFollowersState = key;
                        changed = true;
                    }
                }

                if (changed)
                {
                    Debug.Log(msg);
                }
            }
            catch { }
        }

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

        public delegate void InRoomInviteReceived(InRoomInviteInfo inviteInfo);
        public static event InRoomInviteReceived OnInRoomInviteReceived;

        public delegate void InRoomInviteJoinFailed(string roomName, short returnCode, string message);
        public static event InRoomInviteJoinFailed OnInRoomInviteJoinFailed;

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

            GameType gameType;
            try
            {
                gameType = (GameType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey);
            }
            catch
            {
                return;
            }

            if (gameType != GameType.Custom) return;
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
                if (room.PlayerCount != room.MaxPlayers) yield break;

                if (CheckIfAllPlayersInPosition())
                {
                    GameType gameType = (GameType)room.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey);
                    if (gameType == GameType.Custom)
                    {
                        OnStartPlayingEvent(new StartPlayingEvent());
                    }
                }
            }
            finally
            {
                _canBattleStartCheckHolder = null;
            }
        }

        public void AcceptInRoomInvite(string roomName)
        {
            if (string.IsNullOrEmpty(roomName)) return;
            if (!PhotonRealtimeClient.InLobby || PhotonRealtimeClient.InRoom) return;

            string localUserId = PhotonRealtimeClient.LocalPlayer?.UserId;
            if (string.IsNullOrEmpty(localUserId)) return;

            _pendingInRoomInviteRoomName = string.Empty;
            _pendingAcceptedInRoomInviteRoomName = roomName;
            _pendingAcceptedInRoomInviteStartTime = Time.time;
            _lastAutoInviteRoomName = roomName;
            _lastAutoInviteJoinTime = Time.time;
            Debug.Log($"AcceptInRoomInvite: joining room '{roomName}'.");
            PhotonRealtimeClient.JoinRoom(roomName, new[] { localUserId });
        }

        public void DeclineInRoomInvite(string roomName)
        {
            if (string.IsNullOrEmpty(roomName)) return;

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

        private IEnumerator FormMatchFromQueue(string[] selected, int roomGameTypeInt, string clanName, int soulhomeRank)
        {
            bool queuePremadeMode = false;
            string queuePremadeUserId1 = string.Empty;
            string queuePremadeUserId2 = string.Empty;
            int queuePremadeTargetGameType = roomGameTypeInt;
            string queueLocalTeammateUserId = string.Empty;
            List<(string userId1, string userId2)> queueCompleteDuoPairs = new();
            List<(string userId1, string userId2)> queueSoloPairBlocks = new();
            bool reservationFailed = false;

            try
            {
                try
                {
                    if (PhotonRealtimeClient.CurrentRoom != null)
                    {
                        queuePremadeMode = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<bool>(PhotonBattleRoom.PremadeModeKey, false);
                        queuePremadeUserId1 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PremadeUserId1Key, string.Empty);
                        queuePremadeUserId2 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PremadeUserId2Key, string.Empty);
                        queuePremadeTargetGameType = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.PremadeTargetGameTypeKey, roomGameTypeInt);
                    }

                    string localUserIdForQueuePair = PhotonRealtimeClient.LocalPlayer?.UserId ?? string.Empty;
                    if (!string.IsNullOrEmpty(localUserIdForQueuePair))
                    {
                        TryGetQueueLocalTeammateUserId(localUserIdForQueuePair, out queueLocalTeammateUserId);
                    }

                    HashSet<string> participantUserIds = new(StringComparer.Ordinal);
                    if (!string.IsNullOrEmpty(localUserIdForQueuePair)) participantUserIds.Add(localUserIdForQueuePair);
                    if (selected != null)
                    {
                        foreach (string uid in selected)
                        {
                            if (!string.IsNullOrEmpty(uid)) participantUserIds.Add(uid);
                        }
                    }

                    queueCompleteDuoPairs = GetQueueCompleteDuoPairsForParticipants(participantUserIds);
                    queueSoloPairBlocks = GetQueueSoloPairBlocksForParticipants(participantUserIds, queueCompleteDuoPairs);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"FormMatchFromQueue: failed to read premade metadata from queue room: {ex.Message}");
                }

                // Notify queue members that leader is forming a match so they can start follow flow
                bool preNotifySent = false;
                try
                {
                    if (PhotonRealtimeClient.InRoom)
                    {
                        // diagnostic snapshot before pre-notify
                        try
                        {
                            string sel = selected == null ? "null" : string.Join(",", selected);
                            string pairs = queueCompleteDuoPairs == null ? "null" : string.Join(";", queueCompleteDuoPairs.Select(p => p.userId1 + "/" + p.userId2));
                            Debug.Log($"FormMatchFromQueue: pre-notify: selected=[{sel}], queuePremadeMode={queuePremadeMode}, queueLocalTeammate={queueLocalTeammateUserId}, queuePairs=[{pairs}], queueSoloBlocks={queueSoloPairBlocks.Count}");
                        }
                        catch { }
                        // payload: { leaderUserId, expectedUsers[] }
                        // DO NOT include room name in pre-notify; followers will wait for explicit room name in post-notify.
                        // With sequence-numbered room names, including a deterministic pre-notify room name causes followers
                        // to attempt joining a non-existent room before the actual sequenced room is created.

                        SafeRaiseEvent(
                            PhotonRealtimeClient.PhotonEvent.RoomChangeRequested,
                            new object[] { PhotonRealtimeClient.LocalPlayer.UserId, selected },
                            new RaiseEventArgs { Receivers = ReceiverGroup.Others },
                            SendOptions.SendReliable
                        );
                        Debug.Log("FormMatchFromQueue: pre-notify RoomChangeRequested (no-room) sent to queue members before leaving.");
                        preNotifySent = true;
                    }
                }
                catch (Exception ex) { Debug.LogWarning($"FormMatchFromQueue: pre-notify failed: {ex.Message}"); }

                // Perform the small yield-based wait outside of the try/catch to avoid CS1626
                // (cannot yield inside a try with a catch). Only wait if we actually sent pre-notify.
                if (preNotifySent)
                {
                    float preNotifyWaitStart = Time.time;
                    while (Time.time - preNotifyWaitStart < 0.5f)
                    {
                        yield return null;
                    }
                }

                float waitStart = Time.time;
                // If we are not on MasterServer and in lobby, leave the current room and wait until we're back in lobby on MasterServer
                if (PhotonRealtimeClient.Client == null || PhotonRealtimeClient.Client.Server != ServerConnection.MasterServer || !PhotonRealtimeClient.InLobby || !PhotonRealtimeClient.Client.IsConnectedAndReady)
                {
                    Debug.Log("FormMatchFromQueue: not on MasterServer/in lobby; leaving room and waiting for lobby...");
                    if (PhotonRealtimeClient.InRoom) PhotonRealtimeClient.LeaveRoom();
                    while ((PhotonRealtimeClient.Client == null || PhotonRealtimeClient.Client.Server != ServerConnection.MasterServer || !PhotonRealtimeClient.InLobby || !PhotonRealtimeClient.Client.IsConnectedAndReady) && Time.time - waitStart < 8f)
                    {
                        yield return null;
                    }
                }

                if (!(PhotonRealtimeClient.Client != null && PhotonRealtimeClient.Client.Server == ServerConnection.MasterServer && PhotonRealtimeClient.InLobby && PhotonRealtimeClient.Client.IsConnectedAndReady))
                {
                    Debug.LogWarning("FormMatchFromQueue: still not on MasterServer/in lobby; aborting match creation.");
                    yield break;
                }

                // Final safety: if there are pending duo signals from other leaders/followers,
                // abort forming a new room to avoid splitting an in-flight duo handoff.
                int pendingLeaders = 0;
                int pendingExpectedUsers = 0;
                try
                {
                    pendingLeaders = GetQueuePendingLeaderCount();
                    pendingExpectedUsers = GetQueuePendingExpectedUserCount();
                }
                catch { }

                if (pendingLeaders > 0 || pendingExpectedUsers > 0)
                {
                    Debug.LogWarning($"FormMatchFromQueue: deferring match creation due to pending duo signals (leaders={pendingLeaders}, expectedUsers={pendingExpectedUsers}).");
                    reservationFailed = true;
                    yield break;
                }

                bool created = false;
                // Use deterministic server-side join-or-create to avoid leader create races.
                if ((GameType)roomGameTypeInt == GameType.Clan2v2)
                {
                    created = PhotonRealtimeClient.JoinOrCreateMatchmakingRoom(GameType.Clan2v2, selected, clanName, soulhomeRank);
                    Debug.Log($"FormMatchFromQueue: JoinOrCreateMatchmakingRoom(Clan2v2) returned: {created}");
                }
                else
                {
                    created = PhotonRealtimeClient.JoinOrCreateMatchmakingRoom(GameType.Random2v2, selected);
                    Debug.Log($"FormMatchFromQueue: JoinOrCreateMatchmakingRoom(Random2v2) returned: {created}");
                }

                string createdRoomName = null;
                if (created)
                {
                    float joinWaitStart = Time.time;
                    while (!PhotonRealtimeClient.InRoom && Time.time - joinWaitStart < 6f)
                    {
                        yield return null;
                    }

                    if (PhotonRealtimeClient.InRoom && PhotonRealtimeClient.CurrentRoom != null)
                    {
                        createdRoomName = PhotonRealtimeClient.CurrentRoom.Name;
                        Debug.Log($"FormMatchFromQueue: master joined new room '{createdRoomName}'");
                            // Post-join atomic safety: re-check pending duo signals that may have
                            // arrived after the pre-notify / pre-create check. If any pending
                            // leader or expected-user signals are present, abort and requeue
                            // to avoid splitting an in-flight duo handoff.
                            try
                            {
                                int pendingLeadersPostJoin = 0;
                                int pendingExpectedUsersPostJoin = 0;
                                try { pendingLeadersPostJoin = GetQueuePendingLeaderCount(); } catch { }
                                try { pendingExpectedUsersPostJoin = GetQueuePendingExpectedUserCount(); } catch { }
                                if (pendingLeadersPostJoin > 0 || pendingExpectedUsersPostJoin > 0)
                                {
                                    Debug.LogWarning($"FormMatchFromQueue: aborting post-join due to pending duo signals (leaders={pendingLeadersPostJoin}, expectedUsers={pendingExpectedUsersPostJoin}). Leaving created room and requeueing leader.");
                                    reservationFailed = true;
                                    try { if (PhotonRealtimeClient.InRoom) PhotonRealtimeClient.LeaveRoom(); } catch (Exception ex) { Debug.LogWarning($"FormMatchFromQueue: failed to leave room after post-join abort: {ex.Message}"); }
                                }
                            }
                            catch { }
                            // Mark self as leader for followers and start leader matchmaking wait loop so bot backfill and game start proceed
                            try
                            {
                                try { PhotonRealtimeClient.LocalPlayer.SetCustomProperty(PhotonBattleRoom.LeaderIdKey, PhotonRealtimeClient.LocalPlayer.UserId); } catch (Exception ex) { Debug.LogWarning($"FormMatchFromQueue: {ex.Message}"); }
                                try { OnRoomLeaderChanged?.Invoke(true); } catch (Exception ex) { Debug.LogWarning($"FormMatchFromQueue: {ex.Message}"); }
                                // Record how many expected users leader requested so WaitForMatchmakingPlayers can decide join timeouts
                                try
                                {
                                    PhotonHashtable queueExpectedProps = new()
                                    {
                                        { "qe", selected?.Length ?? 0 },
                                        { QueueFormedMatchKey, true }
                                    };
                                    if (selected != null && selected.Length > 0)
                                    {
                                        queueExpectedProps["eu"] = selected;
                                    }
                                    PhotonRealtimeClient.CurrentRoom.SetCustomProperties(queueExpectedProps);
                                    try
                                    {
                                        int _qe = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>("qe", 0);
                                        string[] _eu = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string[]>("eu", null);
                                        Debug.Log($"FormMatchFromQueue: applied queue expected props qe={_qe}, eu=[{(_eu==null?"":string.Join(",", _eu))}]");
                                    }
                                    catch { }
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogWarning($"FormMatchFromQueue: failed to apply queue expected-user metadata: {ex.Message}");
                                }
                                try
                                {
                                    if (queueCompleteDuoPairs.Count > 0)
                                    {
                                            string[] flatPairs = queueCompleteDuoPairs.SelectMany(pair => new[] { pair.userId1, pair.userId2 }).ToArray();
                                            PhotonRealtimeClient.CurrentRoom.SetCustomProperty(QueueDuoPairsKey, flatPairs);

                                            reservationFailed = false;
                                            foreach (var pair in queueCompleteDuoPairs)
                                            {
                                                if (string.IsNullOrEmpty(pair.userId1) || string.IsNullOrEmpty(pair.userId2) || pair.userId1 == pair.userId2) continue;
                                                if (!TryReservePremadePairToSameSide(pair.userId1, pair.userId2, out _))
                                                {
                                                    Debug.LogWarning($"FormMatchFromQueue: failed queue duo side reservation for ({pair.userId1},{pair.userId2}). Aborting match creation and requeueing leader.");
                                                    reservationFailed = true;
                                                    break;
                                                }
                                            }

                                            if (reservationFailed)
                                            {
                                                try
                                                {
                                                    // Leave the created room; actual requeue will happen after the outer try/finally.
                                                    if (PhotonRealtimeClient.InRoom) PhotonRealtimeClient.LeaveRoom();
                                                }
                                                catch (Exception ex)
                                                {
                                                    Debug.LogWarning($"FormMatchFromQueue: failed to leave room after reservation failure: {ex.Message}");
                                                }
                                            }
                                    }

                                    // Safety: never persist a mixed composition where a solo pair
                                    // contains a member that is part of a recorded duo pair.
                                    if (ContainsMixedDuoSoloPair(queueCompleteDuoPairs, queueSoloPairBlocks))
                                    {
                                        Debug.LogWarning("FormMatchFromQueue: mixed duo/solo block detected in computed queue blocks; aborting match creation and requeueing leader.");
                                        reservationFailed = true;
                                        try { if (PhotonRealtimeClient.InRoom) PhotonRealtimeClient.LeaveRoom(); } catch { }
                                    }

                                    if (!reservationFailed && queueSoloPairBlocks.Count > 0)
                                    {
                                        string[] flatSoloPairs = queueSoloPairBlocks.SelectMany(pair => new[] { pair.userId1, pair.userId2 }).ToArray();
                                        PhotonRealtimeClient.CurrentRoom.SetCustomProperty(QueueSoloPairsKey, flatSoloPairs);
                                    }
                                }
                                catch (Exception ex) { Debug.LogWarning($"FormMatchFromQueue: failed to apply queue pair reservations: {ex.Message}"); }

                                // Preserve premade metadata when a premade duo came from queue to keep same-side reservation logic active.
                                try
                                {
                                    HashSet<string> matchUserIds = new();
                                    if (!string.IsNullOrEmpty(PhotonRealtimeClient.LocalPlayer?.UserId))
                                    {
                                        matchUserIds.Add(PhotonRealtimeClient.LocalPlayer.UserId);
                                    }
                                    if (selected != null)
                                    {
                                        foreach (string uid in selected)
                                        {
                                            if (!string.IsNullOrEmpty(uid)) matchUserIds.Add(uid);
                                        }
                                    }

                                    bool queuePremadePairInThisMatch = queuePremadeMode
                                        && !string.IsNullOrEmpty(queuePremadeUserId1)
                                        && !string.IsNullOrEmpty(queuePremadeUserId2)
                                        && matchUserIds.Contains(queuePremadeUserId1)
                                        && matchUserIds.Contains(queuePremadeUserId2);

                                    bool queueLocalPairInThisMatch = !string.IsNullOrEmpty(queueLocalTeammateUserId)
                                        && !string.IsNullOrEmpty(PhotonRealtimeClient.LocalPlayer?.UserId)
                                        && matchUserIds.Contains(PhotonRealtimeClient.LocalPlayer.UserId)
                                        && matchUserIds.Contains(queueLocalTeammateUserId);

                                    bool localPremadePairInThisMatch = _isPremadeMatchmakingFlow
                                        && !string.IsNullOrEmpty(_premadeTeammateUserId)
                                        && !string.IsNullOrEmpty(PhotonRealtimeClient.LocalPlayer?.UserId)
                                        && matchUserIds.Contains(PhotonRealtimeClient.LocalPlayer.UserId)
                                        && matchUserIds.Contains(_premadeTeammateUserId);

                                    if (queueLocalPairInThisMatch || localPremadePairInThisMatch || queuePremadePairInThisMatch)
                                    {
                                        string premadeUserId1;
                                        string premadeUserId2;
                                        int premadeTargetGameType;

                                        if (queueLocalPairInThisMatch)
                                        {
                                            premadeUserId1 = PhotonRealtimeClient.LocalPlayer.UserId;
                                            premadeUserId2 = queueLocalTeammateUserId;
                                            premadeTargetGameType = roomGameTypeInt;
                                        }
                                        else if (localPremadePairInThisMatch)
                                        {
                                            premadeUserId1 = PhotonRealtimeClient.LocalPlayer.UserId;
                                            premadeUserId2 = _premadeTeammateUserId;
                                            premadeTargetGameType = roomGameTypeInt;
                                        }
                                        else
                                        {
                                            premadeUserId1 = queuePremadeUserId1;
                                            premadeUserId2 = queuePremadeUserId2;
                                            premadeTargetGameType = queuePremadeTargetGameType;
                                        }

                                        PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeModeKey, true);
                                        PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeTargetGameTypeKey, premadeTargetGameType);
                                        PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeLeaderUserIdKey, PhotonRealtimeClient.LocalPlayer.UserId);
                                        PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeUserId1Key, premadeUserId1);
                                        PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeUserId2Key, premadeUserId2);
                                        PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeInviteStateKey, PhotonBattleRoom.PremadeInviteStateAccepted);

                                        // Keep runtime fields in sync for any downstream logic that still checks them.
                                        _isPremadeMatchmakingFlow = true;
                                        if (premadeUserId1 == PhotonRealtimeClient.LocalPlayer.UserId) _premadeTeammateUserId = premadeUserId2;
                                        else if (premadeUserId2 == PhotonRealtimeClient.LocalPlayer.UserId) _premadeTeammateUserId = premadeUserId1;
                                    }
                                }
                                catch (Exception ex) { Debug.LogWarning($"FormMatchFromQueue: failed to set premade metadata: {ex.Message}"); }

                                if (PhotonRealtimeClient.LocalPlayer != null && PhotonRealtimeClient.LocalPlayer.IsMasterClient && _matchmakingHolder == null)
                                {
                                    _matchmakingHolder = StartCoroutine(WaitForMatchmakingPlayers());
                                }

                                // Diagnostic snapshot after applying reservations and before notifying followers
                                try
                                {
                                    string p1 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey1, string.Empty);
                                    string p2 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey2, string.Empty);
                                    string p3 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey3, string.Empty);
                                    string p4 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey4, string.Empty);
                                    string[] duoPairs = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string[]>(QueueDuoPairsKey, null);
                                    bool prem = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<bool>(PhotonBattleRoom.PremadeModeKey, false);
                                    string prem1 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PremadeUserId1Key, string.Empty);
                                    string prem2 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PremadeUserId2Key, string.Empty);
                                    Debug.Log($"FormMatchFromQueue: post-reservation snapshot: pos1={p1},pos2={p2},pos3={p3},pos4={p4}, queueDuoPairs=[{(duoPairs==null?"null":string.Join(",",duoPairs))}], premadeMode={prem}, prem1={prem1}, prem2={prem2}");
                                }
                                catch (Exception ex) { Debug.LogWarning($"FormMatchFromQueue: failed to log post-reservation snapshot: {ex.Message}"); }
                            }
                            catch (Exception ex) { Debug.LogWarning($"FormMatchFromQueue: failed to prepare leader matchmaking: {ex.Message}"); }
                    }
                    else
                    {
                        Debug.LogWarning("FormMatchFromQueue: master did not join the created room in time.");
                    }
                }

                // Notify others to follow the leader (this triggers FollowLeaderToNewRoom on clients)
                try
                {
                    // payload: { leaderUserId, expectedUsers[], optionalRoomName }
                    object[] payload = createdRoomName == null ? new object[] { PhotonRealtimeClient.LocalPlayer.UserId, selected } : new object[] { PhotonRealtimeClient.LocalPlayer.UserId, selected, createdRoomName };
                    SafeRaiseEvent(
                        PhotonRealtimeClient.PhotonEvent.RoomChangeRequested,
                        payload,
                        new RaiseEventArgs { Receivers = ReceiverGroup.Others },
                        SendOptions.SendReliable
                    );
                    Debug.Log($"FormMatchFromQueue: RoomChangeRequested sent to others. roomName={createdRoomName}");
                }
                catch (Exception ex) { Debug.LogWarning($"FormMatchFromQueue: failed to send RoomChangeRequested: {ex.Message}"); }
            }
            finally
            {
                _formingMatchHolder = null;
            }

            if (reservationFailed)
            {
                // Perform requeue outside of try/catch/finally to allow yielding safely.
                yield return StartCoroutine(RequeueToPersistentQueue((GameType)roomGameTypeInt, queuePremadeMode, queuePremadeUserId1, queuePremadeUserId2, queuePremadeTargetGameType));
                yield break;
            }
        }

        // Removed unused helper GetRoomCreationTimestamp: no references found and logic is deprecated.

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
                : new[] { PhotonBattleRoom.PlayerPosition1, PhotonBattleRoom.PlayerPosition2 };

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

        // AutoJoinLargestMatchmakingRoom removed: client-side opportunistic joining
        // is now fully deprecated in favor of centralized queue-based matchmaking.

        // Requeue the local player into the persistent queue room for the given game type.
        private IEnumerator RequeueToPersistentQueue(GameType gameType, bool premadeMode = false, string premadeUserId1 = "", string premadeUserId2 = "", int premadeTargetGameType = -1)
        {
            try
            {
                string localUserId = PhotonRealtimeClient.LocalPlayer?.UserId ?? string.Empty;
                if (premadeTargetGameType < 0) premadeTargetGameType = (int)gameType;

                // Snapshot premade info before leaving room so non-master requeues can preserve same-side constraints.
                try
                {
                    if ((!premadeMode || string.IsNullOrEmpty(premadeUserId1) || string.IsNullOrEmpty(premadeUserId2)) && PhotonRealtimeClient.CurrentRoom != null)
                    {
                        bool roomPremadeMode = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<bool>(PhotonBattleRoom.PremadeModeKey, false);
                        string roomPremadeUserId1 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PremadeUserId1Key, string.Empty);
                        string roomPremadeUserId2 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PremadeUserId2Key, string.Empty);
                        int roomPremadeTargetGameType = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.PremadeTargetGameTypeKey, (int)gameType);

                        if (roomPremadeMode && !string.IsNullOrEmpty(roomPremadeUserId1) && !string.IsNullOrEmpty(roomPremadeUserId2))
                        {
                            premadeMode = true;
                            premadeUserId1 = roomPremadeUserId1;
                            premadeUserId2 = roomPremadeUserId2;
                            premadeTargetGameType = roomPremadeTargetGameType;
                        }
                    }
                }
                catch (Exception ex) { Debug.LogWarning($"RequeueToPersistentQueue: failed to snapshot room premade metadata: {ex.Message}"); }

                if ((!premadeMode || string.IsNullOrEmpty(premadeUserId1) || string.IsNullOrEmpty(premadeUserId2))
                    && _isPremadeMatchmakingFlow
                    && !string.IsNullOrEmpty(localUserId)
                    && !string.IsNullOrEmpty(_premadeTeammateUserId))
                {
                    premadeMode = true;
                    premadeUserId1 = localUserId;
                    premadeUserId2 = _premadeTeammateUserId;
                    premadeTargetGameType = (int)gameType;
                }

                if (premadeMode)
                {
                    bool localInPair = !string.IsNullOrEmpty(localUserId) && (localUserId == premadeUserId1 || localUserId == premadeUserId2);
                    if (!localInPair || string.IsNullOrEmpty(premadeUserId1) || string.IsNullOrEmpty(premadeUserId2) || premadeUserId1 == premadeUserId2)
                    {
                        premadeMode = false;
                    }
                }

                Debug.Log($"RequeueToPersistentQueue: rejoining persistent queue for {gameType}");
                try { StopMatchmakingCoroutines(); } catch (Exception ex) { Debug.LogWarning($"RequeueToPersistentQueue: failed to stop matchmaking coroutines: {ex.Message}"); }
                try { StopHolderCoroutines(); } catch (Exception ex) { Debug.LogWarning($"RequeueToPersistentQueue: failed to stop holder coroutines: {ex.Message}"); }

                if (PhotonRealtimeClient.InRoom) PhotonRealtimeClient.LeaveRoom();
                float waitStart = Time.time;
                while (!PhotonRealtimeClient.InLobby && Time.time - waitStart < 6f)
                {
                    yield return null;
                }

                if (PhotonRealtimeClient.Client != null && PhotonRealtimeClient.Client.Server == ServerConnection.MasterServer && PhotonRealtimeClient.InLobby)
                {
                    bool joined = false;
                    try
                    {
                        joined = PhotonRealtimeClient.JoinOrCreateQueueRoom(gameType);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"RequeueToPersistentQueue: JoinOrCreateQueueRoom threw: {ex.Message}");
                    }

                    if (!joined)
                    {
                        Debug.LogWarning("RequeueToPersistentQueue: JoinOrCreateQueueRoom failed; aborting requeue (no direct matchmaking-room fallback in queue-authoritative flow).");
                    }
                    else
                    {
                        float joinStart = Time.time;
                        while (!PhotonRealtimeClient.InRoom && Time.time - joinStart < 6f)
                        {
                            yield return null;
                        }

                        if (premadeMode && PhotonRealtimeClient.InRoom && PhotonRealtimeClient.CurrentRoom != null)
                        {
                            bool isQueueRoom = false;
                            try { isQueueRoom = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<bool>(PhotonBattleRoom.IsQueueKey, false); } catch { }
                            if (isQueueRoom)
                            {
                                try
                                {
                                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeModeKey, true);
                                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeTargetGameTypeKey, premadeTargetGameType);
                                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeUserId1Key, premadeUserId1);
                                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeUserId2Key, premadeUserId2);
                                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeLeaderUserIdKey, localUserId);
                                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeInviteStateKey, PhotonBattleRoom.PremadeInviteStateAccepted);

                                    _isPremadeMatchmakingFlow = true;
                                    if (localUserId == premadeUserId1) _premadeTeammateUserId = premadeUserId2;
                                    else if (localUserId == premadeUserId2) _premadeTeammateUserId = premadeUserId1;

                                    Debug.Log($"RequeueToPersistentQueue: preserved premade metadata in queue room ({premadeUserId1},{premadeUserId2}).");
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogWarning($"RequeueToPersistentQueue: failed to preserve premade metadata: {ex.Message}");
                                }
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("RequeueToPersistentQueue: not connected to MasterServer lobby; cannot join queue.");
                }
            }
            finally
            {
                _autoJoinHolder = null;
            }
        }

        private void StartQueueTimer()
        {
            try
            {
                if (_queueTimerHolder != null) return;
                _queueTimerHolder = StartCoroutine(QueueTimerCoroutine());
            }
            catch (Exception ex) { Debug.LogWarning($"StartQueueTimer: failed to start: {ex.Message}"); }
        }

        private void StopQueueTimer()
        {
            try
            {
                if (_queueTimerHolder != null)
                {
                    StopCoroutine(_queueTimerHolder);
                    _queueTimerHolder = null;
                }
            }
            catch (Exception ex) { Debug.LogWarning($"StopQueueTimer: failed to stop: {ex.Message}"); }
        }

        private int GetQueueRequiredFollowerCount(int roomGameTypeInt)
        {
            switch ((GameType)roomGameTypeInt)
            {
                case GameType.Random2v2:
                case GameType.Clan2v2:
                default:
                    return 3;
            }
        }

        private string GetQueuePlayerLeaderId(Player player)
        {
            if (player == null) return string.Empty;
            try
            {
                return player.GetCustomProperty<string>(PhotonBattleRoom.LeaderIdKey, string.Empty) ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private bool IsQueueRoom(Room room)
        {
            if (room == null || room.CustomProperties == null) return false;
            try
            {
                return room.CustomProperties.ContainsKey(PhotonBattleRoom.IsQueueKey)
                    && room.GetCustomProperty<bool>(PhotonBattleRoom.IsQueueKey);
            }
            catch
            {
                return false;
            }
        }

        private bool IsQueueFormedExpectedUserFlowRoom(Room room)
        {
            if (room == null) return false;

            try
            {
                bool queueFormedMatch = room.GetCustomProperty<bool>(QueueFormedMatchKey, false);
                int expectedFollowers = room.GetCustomProperty<int>("qe", 0);
                string[] expectedUsers = room.GetCustomProperty<string[]>("eu", null);
                string[] photonExpectedUsers = room.ExpectedUsers;

                bool hasExpectedUsers = expectedUsers != null && expectedUsers.Any(uid => !string.IsNullOrEmpty(uid));
                bool hasPhotonExpectedUsers = photonExpectedUsers != null && photonExpectedUsers.Any(uid => !string.IsNullOrEmpty(uid));

                return queueFormedMatch || expectedFollowers > 0 || hasExpectedUsers || hasPhotonExpectedUsers;
            }
            catch
            {
                return false;
            }
        }

        private bool IsInQueueFormedExpectedUserMatchmakingFlow()
        {
            if (!PhotonRealtimeClient.InMatchmakingRoom) return false;
            return IsQueueFormedExpectedUserFlowRoom(PhotonRealtimeClient.CurrentRoom);
        }

        private bool HasPendingQueueDuoSignals()
        {
            try
            {
                int pendingExpectedUserCount = GetQueuePendingExpectedUserCount();
                int pendingLeaderCount = GetQueuePendingLeaderCount();
                return pendingExpectedUserCount > 0 || pendingLeaderCount > 0;
            }
            catch
            {
                return false;
            }
        }

        private int GetQueueOrphanFollowerCount()
        {
            Room room = PhotonRealtimeClient.CurrentRoom;
            if (room == null || room.Players == null) return 0;

            HashSet<string> humanUserIds = new(
                room.Players.Values
                    .Where(p => p != null && !string.IsNullOrEmpty(p.UserId) && p.UserId != "Bot")
                    .Select(p => p.UserId),
                StringComparer.Ordinal);

            int orphanCount = 0;
            foreach (Player player in room.Players.Values)
            {
                if (player == null || string.IsNullOrEmpty(player.UserId) || player.UserId == "Bot") continue;

                string leaderId = GetQueuePlayerLeaderId(player);
                if (string.IsNullOrEmpty(leaderId) || leaderId == player.UserId) continue;
                if (humanUserIds.Contains(leaderId)) continue;

                orphanCount++;
            }

            return orphanCount;
        }

        private int GetQueuePendingLeaderCount()
        {
            Room room = PhotonRealtimeClient.CurrentRoom;
            if (room == null || room.Players == null || _queuePendingLeaderUntil.Count == 0) return 0;

            Dictionary<string, Player> playersById = room.Players.Values
                .Where(p => p != null && !string.IsNullOrEmpty(p.UserId) && p.UserId != "Bot")
                .GroupBy(p => p.UserId)
                .Select(g => g.First())
                .ToDictionary(p => p.UserId, p => p, StringComparer.Ordinal);

            float now = Time.time;
            int pendingCount = 0;
            List<string> staleLeaderIds = null;

            foreach (var kv in _queuePendingLeaderUntil)
            {
                string leaderUserId = kv.Key;
                float validUntil = kv.Value;

                // Expired or invalid entries should be removed
                if (string.IsNullOrEmpty(leaderUserId) || validUntil <= now)
                {
                    staleLeaderIds ??= new List<string>();
                    staleLeaderIds.Add(leaderUserId);
                    continue;
                }

                // If both leader and a follower that references the leader are present,
                // the duo is already in-room and the pending entry can be cleared.
                bool bothMembersPresent = false;
                try
                {
                    bool leaderPresent = playersById.ContainsKey(leaderUserId);
                    bool followerPresent = playersById.Values
                        .Any(p => p != null && p.UserId != leaderUserId && GetQueuePlayerLeaderId(p) == leaderUserId);
                    bothMembersPresent = leaderPresent && followerPresent;
                }
                catch { }

                if (bothMembersPresent)
                {
                    staleLeaderIds ??= new List<string>();
                    staleLeaderIds.Add(leaderUserId);
                    continue;
                }

                // Otherwise, this leader is considered pending within the grace window.
                pendingCount++;
            }

            if (staleLeaderIds != null)
            {
                foreach (string leaderId in staleLeaderIds)
                {
                    if (string.IsNullOrEmpty(leaderId)) continue;
                    _queuePendingLeaderUntil.Remove(leaderId);
                }
            }

            return pendingCount;
        }

        private int GetQueuePendingExpectedUserCount()
        {
            Room room = PhotonRealtimeClient.CurrentRoom;
            if (room == null || room.Players == null || _queuePendingExpectedUserUntil.Count == 0) return 0;

            HashSet<string> presentUserIds = new(
                room.Players.Values
                    .Where(p => p != null && !string.IsNullOrEmpty(p.UserId) && p.UserId != "Bot")
                    .Select(p => p.UserId),
                StringComparer.Ordinal);

            float now = Time.time;
            int pendingCount = 0;
            List<string> staleExpectedUserIds = null;

            foreach (var kv in _queuePendingExpectedUserUntil)
            {
                string expectedUserId = kv.Key;
                float validUntil = kv.Value;

                bool shouldRemove = string.IsNullOrEmpty(expectedUserId)
                    || validUntil <= now
                    || presentUserIds.Contains(expectedUserId);

                if (shouldRemove)
                {
                    staleExpectedUserIds ??= new List<string>();
                    staleExpectedUserIds.Add(expectedUserId);
                }
                else
                {
                    pendingCount++;
                }
            }

            if (staleExpectedUserIds != null)
            {
                foreach (string expectedUserId in staleExpectedUserIds)
                {
                    if (string.IsNullOrEmpty(expectedUserId)) continue;
                    _queuePendingExpectedUserUntil.Remove(expectedUserId);
                }
            }

            return pendingCount;
        }

        private bool TryTransferQueueMaster(string newMasterUserId, string reason)
        {
            try
            {
                if (string.IsNullOrEmpty(newMasterUserId)) return false;
                if (PhotonRealtimeClient.LocalPlayer == null || !PhotonRealtimeClient.LocalPlayer.IsMasterClient) return false;

                Room room = PhotonRealtimeClient.CurrentRoom;
                if (room == null || room.Players == null) return false;

                Player candidate = room.Players.Values.FirstOrDefault(p => p != null && p.UserId == newMasterUserId);
                if (candidate == null) return false;

                var lobbyPlayer = PhotonRealtimeClient.LobbyCurrentRoom?.GetPlayer(candidate.ActorNumber);
                if (lobbyPlayer == null) return false;

                if (PhotonRealtimeClient.LobbyCurrentRoom.SetMasterClient(lobbyPlayer))
                {
                    Debug.Log($"QueueTimerCoroutine: transferred master to {newMasterUserId} (actor {candidate.ActorNumber}) because {reason}.");
                    return true;
                }

                Debug.LogWarning($"QueueTimerCoroutine: SetMasterClient returned false when transferring to {newMasterUserId}.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"QueueTimerCoroutine: failed to transfer master to {newMasterUserId}: {ex.Message}");
            }

            return false;
        }

        private bool TryGetQueueLocalTeammateUserId(string localUserId, out string teammateUserId)
        {
            teammateUserId = string.Empty;
            if (string.IsNullOrEmpty(localUserId)) return false;

            Room room = PhotonRealtimeClient.CurrentRoom;
            if (room == null || room.Players == null) return false;

            Dictionary<string, Player> playersById = room.Players.Values
                .Where(p => p != null && !string.IsNullOrEmpty(p.UserId) && p.UserId != "Bot")
                .GroupBy(p => p.UserId)
                .Select(g => g.First())
                .ToDictionary(p => p.UserId, p => p);

            if (!playersById.TryGetValue(localUserId, out Player localPlayer) || localPlayer == null)
            {
                return false;
            }

            string localLeaderId = GetQueuePlayerLeaderId(localPlayer);
            if (!string.IsNullOrEmpty(localLeaderId) && localLeaderId != localUserId)
            {
                if (playersById.ContainsKey(localLeaderId))
                {
                    teammateUserId = localLeaderId;
                    return true;
                }

                return false;
            }

            List<string> followers = playersById.Values
                .Where(p => p.UserId != localUserId)
                .Where(p => GetQueuePlayerLeaderId(p) == localUserId)
                .OrderBy(p => p.ActorNumber)
                .Select(p => p.UserId)
                .ToList();

            if (followers.Count == 1)
            {
                teammateUserId = followers[0];
                return true;
            }

            return false;
        }

        private List<(string userId1, string userId2)> GetQueueCompleteDuoPairsForParticipants(ICollection<string> participantUserIds)
        {
            List<(string userId1, string userId2)> result = new();
            if (participantUserIds == null || participantUserIds.Count < 2) return result;

            Room room = PhotonRealtimeClient.CurrentRoom;
            if (room == null || room.Players == null) return result;

            HashSet<string> participantSet = new(participantUserIds.Where(id => !string.IsNullOrEmpty(id)), StringComparer.Ordinal);
            if (participantSet.Count < 2) return result;

            Dictionary<string, Player> playersById = room.Players.Values
                .Where(p => p != null && !string.IsNullOrEmpty(p.UserId) && p.UserId != "Bot")
                .GroupBy(p => p.UserId)
                .Select(g => g.First())
                .ToDictionary(p => p.UserId, p => p);

            HashSet<string> addedUsers = new(StringComparer.Ordinal);
            foreach (Player leader in playersById.Values.OrderBy(p => p.ActorNumber))
            {
                if (leader == null || !participantSet.Contains(leader.UserId)) continue;

                List<Player> followers = playersById.Values
                    .Where(p => p != null && p.UserId != leader.UserId)
                    .Where(p => participantSet.Contains(p.UserId))
                    .Where(p => GetQueuePlayerLeaderId(p) == leader.UserId)
                    .OrderBy(p => p.ActorNumber)
                    .ToList();

                if (followers.Count != 1) continue;

                string followerId = followers[0].UserId;
                if (string.IsNullOrEmpty(followerId)) continue;

                if (!addedUsers.Add(leader.UserId)) continue;
                if (!addedUsers.Add(followerId))
                {
                    addedUsers.Remove(leader.UserId);
                    continue;
                }

                result.Add((leader.UserId, followerId));
            }

            return result;
        }

        private List<(string userId1, string userId2)> GetQueueSoloPairBlocksForParticipants(
            ICollection<string> participantUserIds,
            ICollection<(string userId1, string userId2)> completeDuoPairs)
        {
            List<(string userId1, string userId2)> result = new();
            if (participantUserIds == null || participantUserIds.Count < 2) return result;

            Room room = PhotonRealtimeClient.CurrentRoom;
            if (room == null || room.Players == null) return result;

            HashSet<string> participantSet = new(participantUserIds.Where(id => !string.IsNullOrEmpty(id)), StringComparer.Ordinal);
            if (participantSet.Count < 2) return result;

            HashSet<string> duoMemberIds = new(StringComparer.Ordinal);
            if (completeDuoPairs != null)
            {
                foreach (var pair in completeDuoPairs)
                {
                    if (!string.IsNullOrEmpty(pair.userId1)) duoMemberIds.Add(pair.userId1);
                    if (!string.IsNullOrEmpty(pair.userId2)) duoMemberIds.Add(pair.userId2);
                }
            }

            List<Player> soloParticipants = room.Players.Values
                .Where(p => p != null && !string.IsNullOrEmpty(p.UserId) && p.UserId != "Bot")
                .Where(p => participantSet.Contains(p.UserId))
                .Where(p => !duoMemberIds.Contains(p.UserId))
                .GroupBy(p => p.UserId)
                .Select(g => g.First())
                .OrderBy(p => p.ActorNumber)
                .ToList();

            for (int i = 0; i + 1 < soloParticipants.Count; i += 2)
            {
                string userId1 = soloParticipants[i].UserId;
                string userId2 = soloParticipants[i + 1].UserId;
                if (string.IsNullOrEmpty(userId1) || string.IsNullOrEmpty(userId2) || userId1 == userId2) continue;
                result.Add((userId1, userId2));
            }

            // Defensive cleanup: ensure no duo members slipped into solo pair results.
            if (duoMemberIds.Count > 0)
            {
                result.RemoveAll(p => duoMemberIds.Contains(p.userId1) || duoMemberIds.Contains(p.userId2));
            }

            return result;
        }

        private bool ContainsMixedDuoSoloPair(IEnumerable<(string userId1, string userId2)> duoPairs, IEnumerable<(string userId1, string userId2)> soloPairs)
        {
            if (duoPairs == null || soloPairs == null) return false;
            var duoMembers = new HashSet<string>(StringComparer.Ordinal);
            foreach (var d in duoPairs)
            {
                if (!string.IsNullOrEmpty(d.userId1)) duoMembers.Add(d.userId1);
                if (!string.IsNullOrEmpty(d.userId2)) duoMembers.Add(d.userId2);
            }
            foreach (var s in soloPairs)
            {
                if (!string.IsNullOrEmpty(s.userId1) && duoMembers.Contains(s.userId1)) return true;
                if (!string.IsNullOrEmpty(s.userId2) && duoMembers.Contains(s.userId2)) return true;
            }
            return false;
        }

        private bool IsTwoPlayerBlockQueueMode(int roomGameTypeInt)
        {
            GameType gameType = (GameType)roomGameTypeInt;
            return gameType == GameType.Random2v2 || gameType == GameType.Clan2v2;
        }

        private List<string> SelectQueueFollowersFromTwoPlayerBlocks(
            int requiredFollowers,
            string localUserId,
            List<Player> realPlayers,
            Dictionary<string, Player> playersById,
            List<(Player leader, Player follower)> completeDuos,
            HashSet<string> incompleteMemberIds,
            HashSet<string> orphanFollowerIds,
            out string preferredMasterUserId,
            out int eligibleSoloCount)
        {
            preferredMasterUserId = string.Empty;
            eligibleSoloCount = 0;
            List<string> selected = new();
            bool hasPendingQueueDuoSignals = HasPendingQueueDuoSignals();
            bool allowOrphanAsStaleSolo = realPlayers != null
                && realPlayers.Count <= requiredFollowers + 1
                && !hasPendingQueueDuoSignals;

            // If any orphan follower currently reports a LeaderId, treat them as a duo member
            // and do not allow treating them as a stale solo pair. This prevents splitting
            // a follower who explicitly references a leader (even if that leader momentarily
            // isn't visible to the master selection snapshot).
            try
            {
                if (allowOrphanAsStaleSolo && orphanFollowerIds != null && orphanFollowerIds.Count > 0)
                {
                    foreach (var orphanId in orphanFollowerIds)
                    {
                        if (string.IsNullOrEmpty(orphanId)) continue;
                        if (!playersById.TryGetValue(orphanId, out Player orphanPlayer) || orphanPlayer == null) continue;
                        string orphanLeaderId = GetQueuePlayerLeaderId(orphanPlayer);
                        if (!string.IsNullOrEmpty(orphanLeaderId))
                        {
                            Debug.Log($"SelectQueueFollowersFromTwoPlayerBlocks: orphan follower {orphanId} has leader mapping {orphanLeaderId}; not treating orphan as stale solo.");
                            allowOrphanAsStaleSolo = false;
                            break;
                        }
                    }
                }
            }
            catch { }

            HashSet<string> completeDuoMemberIds = new();
            foreach (var duo in completeDuos)
            {
                completeDuoMemberIds.Add(duo.leader.UserId);
                completeDuoMemberIds.Add(duo.follower.UserId);
            }

            bool localInCompleteDuo = completeDuoMemberIds.Contains(localUserId);
            string localLeaderId = GetQueuePlayerLeaderId(playersById[localUserId]);
            bool localIsFollower = !string.IsNullOrEmpty(localLeaderId) && localLeaderId != localUserId;

            if (localIsFollower)
            {
                bool localLeaderMissing = !playersById.ContainsKey(localLeaderId);
                bool localShouldNotFollowAsMaster = PhotonRealtimeClient.LocalPlayer != null && PhotonRealtimeClient.LocalPlayer.IsMasterClient;
                if (localLeaderMissing || localShouldNotFollowAsMaster)
                {
                    Debug.Log($"SelectQueueFollowersFromTwoPlayerBlocks: clearing stale local leader reference '{localLeaderId}' (leaderMissing={localLeaderMissing}, localIsMaster={localShouldNotFollowAsMaster}).");
                    localLeaderId = string.Empty;
                    localIsFollower = false;
                    try
                    {
                        if (PhotonRealtimeClient.LocalPlayer != null)
                        {
                            if (PhotonRealtimeClient.LocalPlayer.IsMasterClient)
                            {
                                PhotonRealtimeClient.LocalPlayer.SetCustomProperty(PhotonBattleRoom.LeaderIdKey, localUserId);
                            }
                            else
                            {
                                PhotonRealtimeClient.LocalPlayer.RemoveCustomProperty(PhotonBattleRoom.LeaderIdKey);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"SelectQueueFollowersFromTwoPlayerBlocks: failed to clear stale local leader reference: {ex.Message}");
                    }
                }
            }

            if (localIsFollower)
            {
                if (!string.IsNullOrEmpty(localLeaderId) && playersById.ContainsKey(localLeaderId))
                {
                    preferredMasterUserId = localLeaderId;
                }
                else
                {
                    preferredMasterUserId = completeDuos
                        .Select(d => d.leader.UserId)
                        .FirstOrDefault(id => !string.IsNullOrEmpty(id) && id != localUserId) ?? string.Empty;
                }

                return selected;
            }

            if (orphanFollowerIds != null && orphanFollowerIds.Count > 0 && hasPendingQueueDuoSignals)
            {
                Debug.Log("SelectQueueFollowersFromTwoPlayerBlocks: deferring selection because orphan followers exist while pending queue duo handoff signals are active.");
                return new List<string>();
            }

            // Build local follower map so we can check if a user has followers
            // without relying on outer-scope variables.
            Dictionary<string, List<Player>> localFollowersByLeader = new();
            foreach (Player p in realPlayers)
            {
                string leaderId = GetQueuePlayerLeaderId(p);
                if (string.IsNullOrEmpty(leaderId) || leaderId == p.UserId) continue;
                if (!localFollowersByLeader.TryGetValue(leaderId, out List<Player> lst))
                {
                    lst = new List<Player>();
                    localFollowersByLeader[leaderId] = lst;
                }
                lst.Add(p);
            }

            List<Player> eligibleSoloPlayers = realPlayers
                .Where(p => !completeDuoMemberIds.Contains(p.UserId))
                .Where(p => !incompleteMemberIds.Contains(p.UserId))
                .Where(p =>
                {
                    if (orphanFollowerIds != null && orphanFollowerIds.Contains(p.UserId)) return allowOrphanAsStaleSolo;
                    string leaderId = GetQueuePlayerLeaderId(p);
                    return string.IsNullOrEmpty(leaderId) || leaderId == p.UserId;
                })
                .ToList();

            // Exclude very recent solo joins from eligibility unless they show duo relation
            try
            {
                float now = Time.time;
                var filtered = new List<Player>();
                foreach (var p in eligibleSoloPlayers)
                {
                    if (!string.IsNullOrEmpty(p?.UserId) && _queuePlayerFirstSeenAt.TryGetValue(p.UserId, out float firstAt)
                        && now - firstAt < QueueNewJoinGraceSeconds)
                    {
                        // Keep recently-joined player only if they appear to be part of a duo
                        string leaderId = GetQueuePlayerLeaderId(p);
                        bool hasFollower = localFollowersByLeader.TryGetValue(p.UserId, out var fl) && fl != null && fl.Count > 0;
                        if (string.IsNullOrEmpty(leaderId) && !hasFollower)
                        {
                            // skip this just-joined solo player for now
                            continue;
                        }
                    }
                    filtered.Add(p);
                }

                eligibleSoloPlayers = filtered;
            }
            catch { }

            bool localEligibleSolo = eligibleSoloPlayers.Any(p => p.UserId == localUserId);
            eligibleSoloCount = eligibleSoloPlayers.Count(p => p.UserId != localUserId);

            // Solo selection caps (legacy): if exactly 3 solos are eligible, limit selection to 2
            // but only when there are duos present (to avoid splitting them). If no duos are
            // present (four solos total), allow selecting enough solos to form two solo pairs.
            // If there's exactly one eligible solo (excluding the local user) but the local
            // master is itself an eligible solo, allow pairing the local+other solo — do not
            // apply the restrictive solo cap in that case.
            int soloCap;
            if (eligibleSoloCount == 3)
            {
                soloCap = (completeDuos != null && completeDuos.Count > 0) ? 2 : int.MaxValue;
            }
            else if (eligibleSoloCount == 1)
            {
                soloCap = localEligibleSolo ? int.MaxValue : 0;
            }
            else soloCap = int.MaxValue;
            int solosSelected = 0;

            // If there's exactly one eligible solo (excluding the local user) and the local
            // player is not an eligible solo, avoid pairing that lone solo into a match — but
            // only defer when the available complete duos cannot satisfy the required followers
            // by themselves. If duos alone suffice, allow selection to proceed.
            if (eligibleSoloCount == 1 && !localEligibleSolo)
            {
                int duoPlayers = completeDuos.Count * 2;
                if (duoPlayers < requiredFollowers)
                {
                    Debug.Log("SelectQueueFollowersFromTwoPlayerBlocks: exactly one eligible solo present and duos insufficient; deferring selection to avoid pairing lone solo.");
                    return new List<string>();
                }
            }

            if (!localInCompleteDuo && completeDuos.Count >= 2)
            {
                preferredMasterUserId = completeDuos
                    .Where(d => d.leader.UserId != localUserId && d.follower.UserId != localUserId)
                    .Select(d => d.leader.UserId)
                    .FirstOrDefault() ?? string.Empty;

                if (!string.IsNullOrEmpty(preferredMasterUserId))
                {
                    return selected;
                }
            }

            HashSet<string> localBlockMembers = new(StringComparer.Ordinal) { localUserId };
            HashSet<string> selectedSet = new(StringComparer.Ordinal);

            if (localInCompleteDuo)
            {
                var localDuo = completeDuos.FirstOrDefault(d => d.leader.UserId == localUserId || d.follower.UserId == localUserId);
                if (localDuo.leader == null || localDuo.follower == null)
                {
                    return new List<string>();
                }

                string teammateId = localDuo.leader.UserId == localUserId ? localDuo.follower.UserId : localDuo.leader.UserId;
                if (string.IsNullOrEmpty(teammateId) || teammateId == localUserId)
                {
                    return new List<string>();
                }

                localBlockMembers.Add(teammateId);
                selectedSet.Add(teammateId);
                selected.Add(teammateId);
                }
            else
            {
                if (!localEligibleSolo)
                {
                    return new List<string>();
                }

                Player localSoloPartner = eligibleSoloPlayers.FirstOrDefault(p => p.UserId != localUserId);
                if (localSoloPartner == null || string.IsNullOrEmpty(localSoloPartner.UserId))
                {
                    return new List<string>();
                }

                localBlockMembers.Add(localSoloPartner.UserId);
                    if (selectedSet.Add(localSoloPartner.UserId))
                    {
                        if (soloCap != int.MaxValue && solosSelected >= soloCap)
                        {
                            // cannot select solos due to cap; abort selection
                            return new List<string>();
                        }
                        selected.Add(localSoloPartner.UserId);
                        solosSelected++;
                    }
            }

            var secondDuo = completeDuos.FirstOrDefault(d =>
                !localBlockMembers.Contains(d.leader.UserId)
                && !localBlockMembers.Contains(d.follower.UserId));

            if (secondDuo.leader != null && secondDuo.follower != null)
            {
                if (selectedSet.Add(secondDuo.leader.UserId)) selected.Add(secondDuo.leader.UserId);
                if (selectedSet.Add(secondDuo.follower.UserId)) selected.Add(secondDuo.follower.UserId);
            }
                else
                {
                    List<Player> remainingSolos = eligibleSoloPlayers
                        .Where(p => p.UserId != localUserId)
                        .Where(p => !localBlockMembers.Contains(p.UserId))
                        .ToList();

                    // need two solos to complete the block; respect solo cap when selecting
                    int need = 2;
                    int canTake = soloCap == int.MaxValue ? need : Math.Max(0, soloCap - solosSelected);
                    if (remainingSolos.Count < need || canTake < need)
                    {
                        return new List<string>();
                    }

                    if (selectedSet.Add(remainingSolos[0].UserId)) { selected.Add(remainingSolos[0].UserId); solosSelected++; }
                    if (selectedSet.Add(remainingSolos[1].UserId)) { selected.Add(remainingSolos[1].UserId); solosSelected++; }
                }

            if (selected.Count != requiredFollowers)
            {
                return new List<string>();
            }

            HashSet<string> participantIds = new(StringComparer.Ordinal) { localUserId };
            foreach (string userId in selected)
            {
                if (!string.IsNullOrEmpty(userId)) participantIds.Add(userId);
            }

            bool selectedContainsOrphanFollower = orphanFollowerIds != null && participantIds.Any(orphanFollowerIds.Contains);
            if (selectedContainsOrphanFollower && !allowOrphanAsStaleSolo)
            {
                Debug.LogWarning("SelectQueueFollowersFromTwoPlayerBlocks: selected composition contains orphan follower, retrying.");
                return new List<string>();
            }
            if (selectedContainsOrphanFollower && allowOrphanAsStaleSolo)
            {
                Debug.Log("SelectQueueFollowersFromTwoPlayerBlocks: allowing orphan follower as stale solo in exact-size queue composition.");
            }

            // If the room contains an available complete duo pair that is not included
            // in the current participant set, and our current selection doesn't include
            // any duo member, defer selection so the duo can be preserved for the
            // next formation attempt. This prevents splitting duos when solos would
            // otherwise fill the match.
            try
            {
                bool hasAvailableUnselectedDuo = completeDuos != null && completeDuos.Any(d =>
                    d.leader != null && d.follower != null
                    && playersById.ContainsKey(d.leader.UserId)
                    && playersById.ContainsKey(d.follower.UserId)
                    && !participantIds.Contains(d.leader.UserId)
                    && !participantIds.Contains(d.follower.UserId)
                );

                if (hasAvailableUnselectedDuo)
                {
                    bool anySelectedIsDuoMember = false;
                    if (completeDuos != null)
                    {
                        foreach (var d in completeDuos)
                        {
                            if (d.leader == null || d.follower == null) continue;
                            if (participantIds.Contains(d.leader.UserId) || participantIds.Contains(d.follower.UserId))
                            {
                                anySelectedIsDuoMember = true;
                                break;
                            }
                        }
                    }

                    if (!anySelectedIsDuoMember)
                    {
                        Debug.LogWarning("SelectQueueFollowersFromTwoPlayerBlocks: deferring selection because selecting only solos would leave available duo behind.");
                        return new List<string>();
                    }
                }
            }
            catch { }

            List<(string userId1, string userId2)> selectedDuoPairs = GetQueueCompleteDuoPairsForParticipants(participantIds);
            List<(string userId1, string userId2)> selectedSoloPairs = GetQueueSoloPairBlocksForParticipants(participantIds, selectedDuoPairs);
            int coveredParticipants = selectedDuoPairs.Count * 2 + selectedSoloPairs.Count * 2;
            if (coveredParticipants != participantIds.Count)
            {
                Debug.LogWarning($"SelectQueueFollowersFromTwoPlayerBlocks: selected composition failed block validation (covered={coveredParticipants}, participants={participantIds.Count}).");
                return new List<string>();
            }

            // Ensure we never mix a solo pair with a single member of a duo pair
            if (ContainsMixedDuoSoloPair(selectedDuoPairs, selectedSoloPairs))
            {
                Debug.LogWarning("SelectQueueFollowersFromTwoPlayerBlocks: selected composition mixes solo with duo member; rejecting selection.");
                return new List<string>();
            }

            return selected;
        }

        private List<string> SelectQueueFollowersForMatch(int roomGameTypeInt, int requiredFollowers, out string preferredMasterUserId, out int completeDuoCount, out int eligibleSoloCount, out int orphanFollowerCount, out string singleEligibleSoloUserId)
        {
            preferredMasterUserId = string.Empty;
            completeDuoCount = 0;
            eligibleSoloCount = 0;
            orphanFollowerCount = 0;
            singleEligibleSoloUserId = string.Empty;
            List<string> selected = new();

            Room room = PhotonRealtimeClient.CurrentRoom;
            string localUserId = PhotonRealtimeClient.LocalPlayer?.UserId ?? string.Empty;
            if (room == null || string.IsNullOrEmpty(localUserId) || requiredFollowers <= 0 || room.Players == null)
            {
                return selected;
            }

            List<Player> realPlayers = room.Players.Values
                .Where(p => p != null && !string.IsNullOrEmpty(p.UserId) && p.UserId != "Bot")
                .OrderBy(p => p.ActorNumber)
                .ToList();

            Dictionary<string, Player> playersById = new();
            foreach (Player p in realPlayers)
            {
                if (!playersById.ContainsKey(p.UserId)) playersById[p.UserId] = p;
            }

            if (!playersById.ContainsKey(localUserId))
            {
                return selected;
            }

            // Track first-seen timestamps for players in the matchmaking room so we can
            // detect very recent joins and avoid splitting pairs by deferring selection
            // for transient newcomers.
            float _qnow = Time.time;
            try
            {
                foreach (var uid in playersById.Keys)
                {
                    if (string.IsNullOrEmpty(uid)) continue;
                    if (!_queuePlayerFirstSeenAt.ContainsKey(uid)) _queuePlayerFirstSeenAt[uid] = _qnow;
                }

                // Remove stale entries for users no longer present
                var stale = _queuePlayerFirstSeenAt.Keys.Where(k => !playersById.ContainsKey(k)).ToList();
                foreach (var k in stale) _queuePlayerFirstSeenAt.Remove(k);
            }
            catch { }

            void ClearStaleQueuePremadeMetadata(string reason)
            {
                try
                {
                    if (PhotonRealtimeClient.LocalPlayer == null || !PhotonRealtimeClient.LocalPlayer.IsMasterClient) return;
                    if (room == null) return;
                    if (!room.GetCustomProperty<bool>(PhotonBattleRoom.IsQueueKey, false)) return;

                    // Log premade ids before clearing so we can diagnose one-sided/stale metadata
                    try
                    {
                        string premade1 = room.GetCustomProperty<string>(PhotonBattleRoom.PremadeUserId1Key, string.Empty);
                        string premade2 = room.GetCustomProperty<string>(PhotonBattleRoom.PremadeUserId2Key, string.Empty);
                        Debug.Log($"SelectQueueFollowersForMatch: clearing stale queue premade metadata for room '{room?.Name}' (premade1={premade1}, premade2={premade2}) reason={reason}.");
                    }
                    catch { }

                    room.SetCustomProperty(PhotonBattleRoom.PremadeModeKey, false);
                    room.SetCustomProperty(PhotonBattleRoom.PremadeUserId1Key, string.Empty);
                    room.SetCustomProperty(PhotonBattleRoom.PremadeUserId2Key, string.Empty);
                    room.SetCustomProperty(PhotonBattleRoom.PremadeLeaderUserIdKey, string.Empty);
                    room.SetCustomProperty(PhotonBattleRoom.PremadeInviteStateKey, PhotonBattleRoom.PremadeInviteStateNone);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"SelectQueueFollowersForMatch: failed to clear stale queue premade metadata: {ex.Message}");
                }
            }

            Dictionary<string, List<Player>> followersByLeader = new();
            HashSet<string> incompleteMemberIds = new();
            HashSet<string> orphanFollowerIds = new(StringComparer.Ordinal);

            foreach (Player p in realPlayers)
            {
                string leaderId = GetQueuePlayerLeaderId(p);
                if (string.IsNullOrEmpty(leaderId) || leaderId == p.UserId) continue;

                if (!followersByLeader.TryGetValue(leaderId, out List<Player> followers))
                {
                    followers = new List<Player>();
                    followersByLeader[leaderId] = followers;
                }

                followers.Add(p);
            }

            List<(Player leader, Player follower)> completeDuos = new();
            foreach (var kv in followersByLeader.OrderBy(kv => playersById.ContainsKey(kv.Key) ? playersById[kv.Key].ActorNumber : int.MaxValue))
            {
                if (!playersById.TryGetValue(kv.Key, out Player leader) || leader == null)
                {
                    foreach (Player orphanFollower in kv.Value)
                    {
                        if (orphanFollower != null && !string.IsNullOrEmpty(orphanFollower.UserId)) orphanFollowerIds.Add(orphanFollower.UserId);
                    }
                    continue;
                }

                if (kv.Value.Count == 1 && kv.Value[0] != null && kv.Value[0].UserId != leader.UserId)
                {
                    completeDuos.Add((leader, kv.Value[0]));
                }
                else
                {
                    incompleteMemberIds.Add(leader.UserId);
                    foreach (Player follower in kv.Value)
                    {
                        if (follower != null && !string.IsNullOrEmpty(follower.UserId)) incompleteMemberIds.Add(follower.UserId);
                    }
                }
            }

            // Merge any persisted queue duo pairs from room metadata. This helps preserve
            // duos even when LeaderId links are temporarily missing on one or both members.
            try
            {
                string[] queueDuoFlat = room.GetCustomProperty<string[]>(QueueDuoPairsKey, null);
                if (queueDuoFlat != null && queueDuoFlat.Length >= 2)
                {
                    HashSet<string> usedMembers = new(StringComparer.Ordinal);
                    HashSet<string> existingKeys = new(StringComparer.Ordinal);

                    foreach (var duo in completeDuos)
                    {
                        if (duo.leader == null || duo.follower == null) continue;
                        if (!string.IsNullOrEmpty(duo.leader.UserId)) usedMembers.Add(duo.leader.UserId);
                        if (!string.IsNullOrEmpty(duo.follower.UserId)) usedMembers.Add(duo.follower.UserId);

                        string existingKey = string.CompareOrdinal(duo.leader.UserId, duo.follower.UserId) <= 0
                            ? $"{duo.leader.UserId}|{duo.follower.UserId}"
                            : $"{duo.follower.UserId}|{duo.leader.UserId}";
                        existingKeys.Add(existingKey);
                    }

                    for (int i = 0; i + 1 < queueDuoFlat.Length; i += 2)
                    {
                        string userId1 = queueDuoFlat[i] ?? string.Empty;
                        string userId2 = queueDuoFlat[i + 1] ?? string.Empty;
                        if (string.IsNullOrEmpty(userId1) || string.IsNullOrEmpty(userId2) || userId1 == userId2) continue;

                        if (!playersById.TryGetValue(userId1, out Player player1) || player1 == null) continue;
                        if (!playersById.TryGetValue(userId2, out Player player2) || player2 == null) continue;

                        string key = string.CompareOrdinal(userId1, userId2) <= 0
                            ? $"{userId1}|{userId2}"
                            : $"{userId2}|{userId1}";

                        if (existingKeys.Contains(key)) continue;
                        if (usedMembers.Contains(userId1) || usedMembers.Contains(userId2)) continue;

                        completeDuos.Add((player1, player2));
                        usedMembers.Add(userId1);
                        usedMembers.Add(userId2);
                        existingKeys.Add(key);
                        incompleteMemberIds.Remove(userId1);
                        incompleteMemberIds.Remove(userId2);
                    }
                }
            }
            catch { }

            // Diagnostic: log selection state to help debug missing/partial duo selection
            try
            {
                var duoPairs = completeDuos
                    .Select(d => (d.leader?.UserId ?? string.Empty) + "/" + (d.follower?.UserId ?? string.Empty))
                    .OrderBy(s => s)
                    .ToArray();

                var followerMap = followersByLeader
                    .OrderBy(kv => kv.Key)
                    .Select(kv => kv.Key + ":[" + string.Join(",", kv.Value.Where(p => p != null).Select(p => p.UserId).OrderBy(id => id)) + "]")
                    .ToArray();

                bool pending = HasPendingQueueDuoSignals();

                // Avoid spamming the logs when only a single human player is in the queue
                if (playersById.Count > 1 || pending)
                {
                    var playerKeysOrdered = playersById.Keys.OrderBy(k => k).ToArray();
                    string key = $"p:{string.Join(",", playerKeysOrdered)}|duos:{string.Join(";", duoPairs)}|inc:{string.Join(",", incompleteMemberIds.OrderBy(id => id))}|orph:{string.Join(",", orphanFollowerIds.OrderBy(id => id))}|followers:{string.Join(",", followerMap)}|pending:{pending}";
                    string msg = $"SelectQueueFollowersForMatch: players=[{string.Join(",", playerKeysOrdered)}], completeDuos=[{string.Join(";", duoPairs)}], incomplete=[{string.Join(",", incompleteMemberIds)}], orphans=[{string.Join(",", orphanFollowerIds)}], followersByLeader=[{string.Join(",", followerMap)}], pendingSignals={pending}";
                    LogSelectQueueStateIfChanged("players", key, msg);
                }

                // Additional diagnostics: show each player's recorded LeaderId and any pending duo signals
                try
                {
                    var leaderProps = playersById.Values
                        .OrderBy(p => p.UserId)
                        .Select(p => (p.UserId ?? string.Empty) + "->" + (GetQueuePlayerLeaderId(p) ?? string.Empty))
                        .ToArray();

                    string pendingLeaders = string.Empty;
                    string pendingExpected = string.Empty;
                    try { pendingLeaders = string.Join(",", _queuePendingLeaderUntil.Keys.OrderBy(k => k)); } catch { }
                    try { pendingExpected = string.Join(",", _queuePendingExpectedUserUntil.Keys.OrderBy(k => k)); } catch { }

                    {
                        string key2 = $"lp:{string.Join(",", leaderProps)}|pl:{pendingLeaders}|pe:{pendingExpected}";
                        string msg2 = $"SelectQueueFollowersForMatch: leaderProps=[{string.Join(",", leaderProps)}], pendingLeaders=[{pendingLeaders}], pendingExpected=[{pendingExpected}]";
                        LogSelectQueueStateIfChanged("leaderProps", key2, msg2);
                    }
                }
                catch { }
            }
            catch { }

            bool roomPremadeMode = false;
            string roomPremadeUserId1 = string.Empty;
            string roomPremadeUserId2 = string.Empty;

            try
            {
                roomPremadeMode = room.GetCustomProperty<bool>(PhotonBattleRoom.PremadeModeKey, false);
                roomPremadeUserId1 = room.GetCustomProperty<string>(PhotonBattleRoom.PremadeUserId1Key, string.Empty);
                roomPremadeUserId2 = room.GetCustomProperty<string>(PhotonBattleRoom.PremadeUserId2Key, string.Empty);
            }
            catch { }

            if (orphanFollowerIds.Count > 0)
            {
                // A follower with missing leader is stale queue metadata; treat as solo candidate.
                Debug.Log($"SelectQueueFollowersForMatch: ignoring {orphanFollowerIds.Count} orphan leader references, treating as solos.");
            }

            bool hasPendingQueueDuoSignals = HasPendingQueueDuoSignals();

            // Count very recent joins observed in the room (within grace window).
            int recentJoinCount = 0;
            try
            {
                float now = Time.time;
                recentJoinCount = playersById.Keys.Count(uid => _queuePlayerFirstSeenAt.TryGetValue(uid, out float t) && now - t < QueueNewJoinGraceSeconds);
            }
            catch { recentJoinCount = 0; }

            // If there are extra humans beyond the exact-size case, pending duo signals,
            // or very recent joins, treat the situation as transient/orphan and propagate
            // a non-zero orphanFollowerCount so higher-level logic defers forming matches
            // to avoid splitting duo joins.
            bool considerTransientOrphans = realPlayers.Count > requiredFollowers + 1 || hasPendingQueueDuoSignals || recentJoinCount > 0;
            if (considerTransientOrphans)
            {
                orphanFollowerCount = orphanFollowerIds.Count > 0 ? orphanFollowerIds.Count : recentJoinCount;
            }
            else
            {
                orphanFollowerCount = 0;
            }

            // Diagnostic: show recent-join detection results to help debug transient joins
            try
            {
                float nowDbg = Time.time;
                var ages = _queuePlayerFirstSeenAt
                    .Select(kv => (kv.Key ?? string.Empty) + ":" + (nowDbg - kv.Value).ToString("F2"))
                    .ToArray();
                string key3 = $"recentJoin:{recentJoinCount}|transient:{considerTransientOrphans}|orphanCount:{orphanFollowerCount}";
                string msg3 = $"SelectQueueFollowersForMatch: recentJoinCount={recentJoinCount}, considerTransientOrphans={considerTransientOrphans}, orphanFollowerCount={orphanFollowerCount}, firstSeenAges=[{string.Join(",", ages)}]";
                LogSelectQueueStateIfChanged("recentJoin", key3, msg3);
            }
            catch { }

            try
            {
                if (roomPremadeMode)
                {
                    bool user1Present = !string.IsNullOrEmpty(roomPremadeUserId1) && playersById.ContainsKey(roomPremadeUserId1);
                    bool user2Present = !string.IsNullOrEmpty(roomPremadeUserId2) && playersById.ContainsKey(roomPremadeUserId2);

                    bool invalidPair = string.IsNullOrEmpty(roomPremadeUserId1)
                        || string.IsNullOrEmpty(roomPremadeUserId2)
                        || roomPremadeUserId1 == roomPremadeUserId2;

                    if (invalidPair || (!user1Present && !user2Present))
                    {
                        ClearStaleQueuePremadeMetadata(invalidPair ? "invalid ids" : "no premade users present");
                    }
                    else if (user1Present ^ user2Present)
                    {
                        if (IsTwoPlayerBlockQueueMode(roomGameTypeInt))
                        {
                            bool extraHumansPresent = realPlayers.Count > requiredFollowers + 1;
                            if (extraHumansPresent)
                            {
                                // In larger queues, one-sided premade may indicate teammate still arriving.
                                string presentPremadeUserId = user1Present ? roomPremadeUserId1 : roomPremadeUserId2;
                                if (!string.IsNullOrEmpty(presentPremadeUserId))
                                {
                                    incompleteMemberIds.Add(presentPremadeUserId);
                                }
                                Debug.Log("SelectQueueFollowersForMatch: one-sided premade metadata detected in two-player-block mode with extra humans; deferring selection for present premade user.");
                            }
                            else
                            {
                                // In exact-size queues, avoid immediately clearing premade metadata for a single missing teammate.
                                // Defer clearing and treat the present premade user as "incomplete" for a short grace period
                                // so the duo is not split while the missing user arrives.
                                string presentPremadeUserId = user1Present ? roomPremadeUserId1 : roomPremadeUserId2;
                                string missingPremadeUserId = user1Present ? roomPremadeUserId2 : roomPremadeUserId1;

                                // If a pending expected-user signal already exists for the missing user or global pending duo signals
                                // are active, mark the present user as incomplete and defer clearing.
                                bool pendingForMissing = false;
                                try { pendingForMissing = !string.IsNullOrEmpty(missingPremadeUserId) && _queuePendingExpectedUserUntil.ContainsKey(missingPremadeUserId); } catch { pendingForMissing = false; }

                                if (pendingForMissing || HasPendingQueueDuoSignals())
                                {
                                    if (!string.IsNullOrEmpty(presentPremadeUserId)) incompleteMemberIds.Add(presentPremadeUserId);
                                    Debug.Log($"SelectQueueFollowersForMatch: one-sided premade metadata detected in exact-size queue; deferring clear due to pending expected signal (missing={missingPremadeUserId}).");
                                }
                                else
                                {
                                    // Install a short pending expected-user window to avoid immediate clearing by other clients
                                    try
                                    {
                                        if (!string.IsNullOrEmpty(missingPremadeUserId))
                                        {
                                            _queuePendingExpectedUserUntil[missingPremadeUserId] = Time.time + QueuePendingLeaderGraceSeconds;
                                        }
                                    }
                                    catch { }

                                    if (!string.IsNullOrEmpty(presentPremadeUserId)) incompleteMemberIds.Add(presentPremadeUserId);
                                    Debug.Log($"SelectQueueFollowersForMatch: one-sided premade metadata detected in exact-size queue; deferring clear and marking present user incomplete (missing={missingPremadeUserId}).");
                                }
                            }
                        }
                        else
                        {
                            // In non-block modes, stale one-sided metadata should not block queue progress.
                            Debug.Log("SelectQueueFollowersForMatch: stale one-sided premade metadata detected, clearing and continuing.");
                            ClearStaleQueuePremadeMetadata("one premade user missing");
                        }
                    }
                    else
                    {
                        // Both premade users are present in the queue room: treat them as a complete duo
                        try
                        {
                            if (!string.IsNullOrEmpty(roomPremadeUserId1) && !string.IsNullOrEmpty(roomPremadeUserId2)
                                && playersById.ContainsKey(roomPremadeUserId1) && playersById.ContainsKey(roomPremadeUserId2))
                            {
                                bool exists = completeDuos.Any(d =>
                                    (d.leader != null && d.follower != null && ((d.leader.UserId == roomPremadeUserId1 && d.follower.UserId == roomPremadeUserId2)
                                        || (d.leader.UserId == roomPremadeUserId2 && d.follower.UserId == roomPremadeUserId1))));

                                if (!exists)
                                {
                                    Player p1 = playersById[roomPremadeUserId1];
                                    Player p2 = playersById[roomPremadeUserId2];
                                    if (p1 != null && p2 != null)
                                    {
                                        completeDuos.Add((p1, p2));
                                        incompleteMemberIds.Remove(roomPremadeUserId1);
                                        incompleteMemberIds.Remove(roomPremadeUserId2);
                                        Debug.Log($"SelectQueueFollowersForMatch: preserving premade pair ({roomPremadeUserId1},{roomPremadeUserId2}) as complete duo for selection.");
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }

            // Compute final count after all duo enrichment sources (leader/follower,
            // queue metadata and premade metadata) have been applied.
            completeDuoCount = completeDuos.Count;

            if (IsTwoPlayerBlockQueueMode(roomGameTypeInt))
            {
                return SelectQueueFollowersFromTwoPlayerBlocks(
                    requiredFollowers,
                    localUserId,
                    realPlayers,
                    playersById,
                    completeDuos,
                    incompleteMemberIds,
                    orphanFollowerIds,
                    out preferredMasterUserId,
                    out eligibleSoloCount);
            }

            HashSet<string> completeDuoMemberIds = new();
            foreach (var duo in completeDuos)
            {
                completeDuoMemberIds.Add(duo.leader.UserId);
                completeDuoMemberIds.Add(duo.follower.UserId);
            }

            bool localInCompleteDuo = completeDuoMemberIds.Contains(localUserId);
            string localLeaderId = GetQueuePlayerLeaderId(playersById[localUserId]);
            bool localIsFollower = !string.IsNullOrEmpty(localLeaderId) && localLeaderId != localUserId;

            if (localIsFollower)
            {
                bool localLeaderMissing = !playersById.ContainsKey(localLeaderId);
                bool localShouldNotFollowAsMaster = PhotonRealtimeClient.LocalPlayer != null && PhotonRealtimeClient.LocalPlayer.IsMasterClient;
                if (localLeaderMissing || localShouldNotFollowAsMaster)
                {
                    Debug.Log($"SelectQueueFollowersForMatch: clearing stale local leader reference '{localLeaderId}' (leaderMissing={localLeaderMissing}, localIsMaster={localShouldNotFollowAsMaster}).");
                    localLeaderId = string.Empty;
                    localIsFollower = false;
                    try
                    {
                        if (PhotonRealtimeClient.LocalPlayer != null)
                        {
                            if (PhotonRealtimeClient.LocalPlayer.IsMasterClient)
                            {
                                PhotonRealtimeClient.LocalPlayer.SetCustomProperty(PhotonBattleRoom.LeaderIdKey, localUserId);
                            }
                            else
                            {
                                PhotonRealtimeClient.LocalPlayer.RemoveCustomProperty(PhotonBattleRoom.LeaderIdKey);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"SelectQueueFollowersForMatch: failed to clear stale local leader reference: {ex.Message}");
                    }
                }
            }

            if (localIsFollower)
            {
                if (!string.IsNullOrEmpty(localLeaderId) && playersById.ContainsKey(localLeaderId))
                {
                    preferredMasterUserId = localLeaderId;
                }
                else
                {
                    preferredMasterUserId = completeDuos
                        .Select(d => d.leader.UserId)
                        .FirstOrDefault(id => !string.IsNullOrEmpty(id) && id != localUserId) ?? string.Empty;
                }

                return selected;
            }

            if (!localInCompleteDuo && completeDuos.Count >= 2)
            {
                preferredMasterUserId = completeDuos
                    .Where(d => d.leader.UserId != localUserId && d.follower.UserId != localUserId)
                    .Select(d => d.leader.UserId)
                    .FirstOrDefault() ?? string.Empty;

                return selected;
            }

            HashSet<string> selectedSet = new();

            if (localInCompleteDuo)
            {
                foreach (var duo in completeDuos)
                {
                    if (duo.leader.UserId != localUserId && duo.follower.UserId != localUserId) continue;

                    string teammateId = duo.leader.UserId == localUserId ? duo.follower.UserId : duo.leader.UserId;
                    if (!string.IsNullOrEmpty(teammateId) && teammateId != localUserId && selectedSet.Add(teammateId))
                    {
                        selected.Add(teammateId);
                    }
                    break;
                }
            }

            foreach (var duo in completeDuos.OrderBy(d => d.leader.ActorNumber))
            {
                string leaderId = duo.leader.UserId;
                string followerId = duo.follower.UserId;

                if (leaderId == localUserId || followerId == localUserId) continue;

                int blockCount = 0;
                if (!selectedSet.Contains(leaderId)) blockCount++;
                if (!selectedSet.Contains(followerId)) blockCount++;
                if (blockCount == 0) continue;
                if (selected.Count + blockCount > requiredFollowers) continue;

                if (selectedSet.Add(leaderId)) selected.Add(leaderId);
                if (selectedSet.Add(followerId)) selected.Add(followerId);

                if (selected.Count >= requiredFollowers)
                {
                    return selected;
                }
            }

            List<string> eligibleSolos = realPlayers
                .Where(p => p.UserId != localUserId)
                .Where(p => !completeDuoMemberIds.Contains(p.UserId))
                .Where(p => !incompleteMemberIds.Contains(p.UserId))
                .Where(p =>
                {
                    if (orphanFollowerIds.Contains(p.UserId)) return true;
                    string leaderId = GetQueuePlayerLeaderId(p);
                    return string.IsNullOrEmpty(leaderId) || leaderId == p.UserId;
                })
                .Select(p => p.UserId)
                .ToList();

            eligibleSoloCount = eligibleSolos.Count;
            // expose single eligible solo id so callers can invite them when forming a queue-formed match
            try { singleEligibleSoloUserId = eligibleSoloCount == 1 ? (eligibleSolos.Count > 0 ? eligibleSolos[0] : string.Empty) : string.Empty; } catch { singleEligibleSoloUserId = string.Empty; }
            try
            {
                Debug.Log($"SelectQueueFollowersForMatch: eligibleSoloCount={eligibleSoloCount}, singleEligibleSoloUserId='{singleEligibleSoloUserId}', eligibleSolos=[{string.Join(",", eligibleSolos)}]");
            }
            catch { }
            // keep a record of how many solos are currently eligible in the queue
            _queuedSoloCount = eligibleSoloCount;

            // Solo selection caps:
            // - If exactly 3 solos are eligible, limit selection to 2 (legacy behavior)
            // - If exactly 1 solo is eligible, select 0 solos to avoid pairing a lone solo into a match
            int soloCap;
            if (eligibleSoloCount == 3) soloCap = 2;
            else if (eligibleSoloCount == 1) soloCap = 0;
            else soloCap = int.MaxValue;
            int solosSelected = 0;

            foreach (string soloUserId in eligibleSolos)
            {
                // enforce cap for solos
                if (soloCap != int.MaxValue && solosSelected >= soloCap)
                {
                    break;
                }

                if (!selectedSet.Add(soloUserId)) continue;

                selected.Add(soloUserId);
                solosSelected++;

                if (selected.Count >= requiredFollowers)
                {
                    break;
                }
            }

            return selected;
        }

        private bool ValidateQueueTwoPlayerBlockComposition(Room room, out string reason)
        {
            reason = string.Empty;
            if (room == null || room.Players == null)
            {
                reason = "room missing";
                return false;
            }

            HashSet<string> humanUserIds = new(
                room.Players.Values
                    .Where(p => p != null && !string.IsNullOrEmpty(p.UserId) && p.UserId != "Bot")
                    .Select(p => p.UserId),
                StringComparer.Ordinal);

            if (humanUserIds.Count == 0)
            {
                reason = "no human players";
                return false;
            }

            // Only enforce strict two-block coverage for full-human 2v2 rooms.
            if (humanUserIds.Count < 4) return true;
            if (humanUserIds.Count > 4)
            {
                reason = $"unexpected human count {humanUserIds.Count}";
                return false;
            }

            List<(string userId1, string userId2)> blocks = new();
            HashSet<string> seenPairKeys = new(StringComparer.Ordinal);

            void AddFlatPairs(string[] flatPairs)
            {
                if (flatPairs == null || flatPairs.Length < 2) return;

                for (int i = 0; i + 1 < flatPairs.Length; i += 2)
                {
                    string userId1 = flatPairs[i];
                    string userId2 = flatPairs[i + 1];
                    if (string.IsNullOrEmpty(userId1) || string.IsNullOrEmpty(userId2) || userId1 == userId2) continue;

                    string key = string.CompareOrdinal(userId1, userId2) <= 0
                        ? $"{userId1}|{userId2}"
                        : $"{userId2}|{userId1}";
                    if (!seenPairKeys.Add(key)) continue;

                    blocks.Add((userId1, userId2));
                }
            }

            try
            {
                AddFlatPairs(room.GetCustomProperty<string[]>(QueueDuoPairsKey, null));
                AddFlatPairs(room.GetCustomProperty<string[]>(QueueSoloPairsKey, null));
            }
            catch (Exception ex)
            {
                reason = $"failed to read block metadata: {ex.Message}";
                return false;
            }

            // Build duo pair key set and member set so we can detect mixed blocks
            HashSet<string> duoPairKeysNormalized = new(StringComparer.Ordinal);
            HashSet<string> duoMemberIds = new(StringComparer.Ordinal);
            try
            {
                var duoFlat = room.GetCustomProperty<string[]>(QueueDuoPairsKey, null);
                if (duoFlat != null)
                {
                    for (int i = 0; i + 1 < duoFlat.Length; i += 2)
                    {
                        string u1 = duoFlat[i] ?? string.Empty;
                        string u2 = duoFlat[i + 1] ?? string.Empty;
                        if (string.IsNullOrEmpty(u1) || string.IsNullOrEmpty(u2) || u1 == u2) continue;
                        string key = string.CompareOrdinal(u1, u2) <= 0 ? $"{u1}|{u2}" : $"{u2}|{u1}";
                        duoPairKeysNormalized.Add(key);
                        duoMemberIds.Add(u1);
                        duoMemberIds.Add(u2);
                    }
                }
            }
            catch { }

            if (blocks.Count < 2)
            {
                reason = $"insufficient blocks ({blocks.Count})";
                return false;
            }

            HashSet<string> coveredHumans = new(StringComparer.Ordinal);
            int presentBlockCount = 0;
            foreach (var block in blocks)
            {
                bool blockPresent = humanUserIds.Contains(block.userId1) && humanUserIds.Contains(block.userId2);
                if (!blockPresent) continue;

                // If either member of the present block is known to be part of a duo,
                // the pair must match a recorded duo pair exactly. This rejects mixed
                // blocks that would pair a solo with half of a duo.
                try
                {
                    bool hasDuoMember = duoMemberIds.Contains(block.userId1) || duoMemberIds.Contains(block.userId2);
                    if (hasDuoMember)
                    {
                        string normKey = string.CompareOrdinal(block.userId1, block.userId2) <= 0
                            ? $"{block.userId1}|{block.userId2}"
                            : $"{block.userId2}|{block.userId1}";
                        if (!duoPairKeysNormalized.Contains(normKey))
                        {
                            reason = $"mixed duo/solo block detected ({block.userId1}/{block.userId2})";
                            return false;
                        }
                    }
                }
                catch { }

                presentBlockCount++;
                coveredHumans.Add(block.userId1);
                coveredHumans.Add(block.userId2);
            }

            if (presentBlockCount < 2)
            {
                reason = $"present blocks {presentBlockCount}";
                return false;
            }

            if (coveredHumans.Count != humanUserIds.Count)
            {
                reason = $"covered humans {coveredHumans.Count}/{humanUserIds.Count}";
                return false;
            }

            return true;
        }

        private bool ShouldDeferTwoPlayerBlockStartForMultiDuo(int requiredFollowers, ICollection<string> selectedFollowers, out string reason)
        {
            reason = string.Empty;

            Room room = PhotonRealtimeClient.CurrentRoom;
            string localUserId = PhotonRealtimeClient.LocalPlayer?.UserId ?? string.Empty;
            if (room == null || room.Players == null || string.IsNullOrEmpty(localUserId))
            {
                return false;
            }

            HashSet<string> humanUserIds = new(
                room.Players.Values
                    .Where(p => p != null && !string.IsNullOrEmpty(p.UserId) && p.UserId != "Bot")
                    .Select(p => p.UserId),
                StringComparer.Ordinal);

            int requiredTotal = requiredFollowers + 1;
            if (humanUserIds.Count <= requiredTotal)
            {
                return false;
            }

            List<(string userId1, string userId2)> roomCompleteDuoPairs = GetQueueCompleteDuoPairsForParticipants(humanUserIds);
            if (roomCompleteDuoPairs.Count < 2)
            {
                return false;
            }

            HashSet<string> selectedParticipantIds = new(StringComparer.Ordinal) { localUserId };
            if (selectedFollowers != null)
            {
                foreach (string userId in selectedFollowers)
                {
                    if (!string.IsNullOrEmpty(userId)) selectedParticipantIds.Add(userId);
                }
            }

            List<(string userId1, string userId2)> selectedCompleteDuoPairs = GetQueueCompleteDuoPairsForParticipants(selectedParticipantIds);
            if (selectedCompleteDuoPairs.Count >= 2)
            {
                return false;
            }

            reason = $"roomHumans={humanUserIds.Count}, roomCompleteDuos={roomCompleteDuoPairs.Count}, selectedCompleteDuos={selectedCompleteDuoPairs.Count}, requiredTotal={requiredTotal}";
            return true;
        }

        private bool ShouldDeferTwoPlayerBlockStartForPendingQueueDuo(int requiredFollowers, ICollection<string> selectedFollowers, out string reason)
        {
            reason = string.Empty;

            Room room = PhotonRealtimeClient.CurrentRoom;
            string localUserId = PhotonRealtimeClient.LocalPlayer?.UserId ?? string.Empty;
            if (room == null || room.Players == null || string.IsNullOrEmpty(localUserId))
            {
                return false;
            }

            int humanCount = room.Players.Values
                .Count(p => p != null && !string.IsNullOrEmpty(p.UserId) && p.UserId != "Bot");

            int requiredTotal = requiredFollowers + 1;
            if (humanCount < requiredTotal)
            {
                return false;
            }

            int pendingLeaderCount = GetQueuePendingLeaderCount();
            int pendingExpectedUserCount = GetQueuePendingExpectedUserCount();

            bool shouldDeferForPendingSignals = pendingExpectedUserCount > 0
                || (humanCount > requiredTotal && pendingLeaderCount > 0);

            if (!shouldDeferForPendingSignals)
            {
                return false;
            }

            HashSet<string> selectedParticipantIds = new(StringComparer.Ordinal) { localUserId };
            if (selectedFollowers != null)
            {
                foreach (string userId in selectedFollowers)
                {
                    if (!string.IsNullOrEmpty(userId)) selectedParticipantIds.Add(userId);
                }
            }

            int selectedCompleteDuoCount = GetQueueCompleteDuoPairsForParticipants(selectedParticipantIds).Count;
            if (selectedCompleteDuoCount >= 2)
            {
                return false;
            }

            reason = $"humanCount={humanCount}, requiredTotal={requiredTotal}, pendingLeaders={pendingLeaderCount}, pendingExpectedUsers={pendingExpectedUserCount}, selectedCompleteDuos={selectedCompleteDuoCount}";
            return true;
        }

        private bool ShouldDeferTwoPlayerBlockEarlyStartForOneSidedPremadeExactSize(int requiredFollowers, int completeDuoCount, out string reason)
        {
            reason = string.Empty;

            if (completeDuoCount != 1)
            {
                return false;
            }

            Room room = PhotonRealtimeClient.CurrentRoom;
            string localUserId = PhotonRealtimeClient.LocalPlayer?.UserId ?? string.Empty;
            if (room == null || room.Players == null || string.IsNullOrEmpty(localUserId))
            {
                return false;
            }

            int requiredTotal = requiredFollowers + 1;
            int humanCount = room.Players.Values.Count(p => p != null && !string.IsNullOrEmpty(p.UserId) && p.UserId != "Bot");
            if (humanCount != requiredTotal)
            {
                return false;
            }

            // Only guard the case where the current queue master is not part of a duo pair.
            if (TryGetQueueLocalTeammateUserId(localUserId, out _))
            {
                return false;
            }

            bool premadeMode = false;
            string premadeUserId1 = string.Empty;
            string premadeUserId2 = string.Empty;
            try
            {
                premadeMode = room.GetCustomProperty<bool>(PhotonBattleRoom.PremadeModeKey, false);
                premadeUserId1 = room.GetCustomProperty<string>(PhotonBattleRoom.PremadeUserId1Key, string.Empty);
                premadeUserId2 = room.GetCustomProperty<string>(PhotonBattleRoom.PremadeUserId2Key, string.Empty);
            }
            catch
            {
                return false;
            }

            if (!premadeMode)
            {
                return false;
            }

            bool user1Present = !string.IsNullOrEmpty(premadeUserId1) && room.Players.Values.Any(p => p != null && p.UserId == premadeUserId1);
            bool user2Present = !string.IsNullOrEmpty(premadeUserId2) && room.Players.Values.Any(p => p != null && p.UserId == premadeUserId2);
            if (!(user1Present ^ user2Present))
            {
                return false;
            }

            string presentPremadeUser = user1Present ? premadeUserId1 : premadeUserId2;
            string missingPremadeUser = user1Present ? premadeUserId2 : premadeUserId1;

            reason = $"humanCount={humanCount}, requiredTotal={requiredTotal}, completeDuos={completeDuoCount}, localSoloMaster={localUserId}, presentPremadeUser={presentPremadeUser}, missingPremadeUser={missingPremadeUser}";
            return true;
        }

        private IEnumerator QueueTimerCoroutine()
        {
            try
            {
                while (true)
                {
                    float start = Time.time;
                    while (Time.time - start < QueueWaitSeconds)
                    {
                        if (!PhotonRealtimeClient.InRoom || PhotonRealtimeClient.CurrentRoom == null)
                        {
                            _queueTimerHolder = null;
                            yield break;
                        }

                        try
                        {
                            if (PhotonRealtimeClient.LocalPlayer == null || !PhotonRealtimeClient.LocalPlayer.IsMasterClient)
                            {
                                _queueTimerHolder = null;
                                yield break;
                            }

                            var room = PhotonRealtimeClient.CurrentRoom;
                            if (room == null || room.CustomProperties == null || !room.CustomProperties.ContainsKey(PhotonBattleRoom.IsQueueKey) || !room.GetCustomProperty<bool>(PhotonBattleRoom.IsQueueKey))
                            {
                                _queueTimerHolder = null;
                                yield break;
                            }
                        }
                        catch (Exception ex) { Debug.LogWarning($"StartQueueTimer: loop check failed: {ex.Message}"); _queueTimerHolder = null; yield break; }

                        try
                        {
                            if (_formingMatchHolder == null && Time.time - start >= QueueReadyStartDelaySeconds)
                            {
                                int loopGameTypeInt = (int)GameType.Random2v2;
                                string loopClanName = string.Empty;
                                int loopSoulhomeRank = 0;
                                try { loopGameTypeInt = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey); } catch { }
                                try { loopClanName = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.ClanNameKey, ""); } catch { }
                                try { loopSoulhomeRank = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.SoulhomeRank, 0); } catch { }

                                int loopRequiredFollowers = GetQueueRequiredFollowerCount(loopGameTypeInt);
                                string loopPreferredMasterUserId;
                                int loopCompleteDuoCount;
                                int loopEligibleSoloCount;
                                int loopOrphanFollowerCount;
                                string loopSingleEligibleSoloUserId;
                                List<string> loopSelected = SelectQueueFollowersForMatch(loopGameTypeInt, loopRequiredFollowers, out loopPreferredMasterUserId, out loopCompleteDuoCount, out loopEligibleSoloCount, out loopOrphanFollowerCount, out loopSingleEligibleSoloUserId);

                                if (!string.IsNullOrEmpty(loopPreferredMasterUserId))
                                {
                                    if (TryTransferQueueMaster(loopPreferredMasterUserId, $"early duo-priority selection (completeDuos={loopCompleteDuoCount})"))
                                    {
                                        yield break;
                                    }
                                }

                                if (loopSelected.Count >= loopRequiredFollowers)
                                {
                                    bool twoPlayerBlockMode = IsTwoPlayerBlockQueueMode(loopGameTypeInt);
                                    if (twoPlayerBlockMode && ShouldDeferTwoPlayerBlockStartForMultiDuo(loopRequiredFollowers, loopSelected, out string loopMultiDuoReason))
                                    {
                                        Debug.Log($"QueueTimerCoroutine: early readiness deferred to preserve complete duo pairs ({loopMultiDuoReason}).");
                                    }
                                    else if (twoPlayerBlockMode && ShouldDeferTwoPlayerBlockStartForPendingQueueDuo(loopRequiredFollowers, loopSelected, out string loopPendingDuoReason))
                                    {
                                        Debug.Log($"QueueTimerCoroutine: early readiness deferred for pending queue duo handoff ({loopPendingDuoReason}).");
                                    }
                                    else if (twoPlayerBlockMode && ShouldDeferTwoPlayerBlockEarlyStartForOneSidedPremadeExactSize(loopRequiredFollowers, loopCompleteDuoCount, out string oneSidedPremadeReason))
                                    {
                                        Debug.Log($"QueueTimerCoroutine: early readiness deferred due to one-sided premade metadata in exact-size queue ({oneSidedPremadeReason}).");
                                    }
                                    else
                                    {
                                    int loopHumanCount = 0;
                                        if (twoPlayerBlockMode && loopCompleteDuoCount == 1)
                                        {
                                            try
                                            {
                                                if (PhotonRealtimeClient.CurrentRoom?.Players != null)
                                                {
                                                    loopHumanCount = PhotonRealtimeClient.CurrentRoom.Players.Values
                                                        .Count(p => p != null && !string.IsNullOrEmpty(p.UserId) && p.UserId != "Bot");
                                                }
                                            }
                                            catch { }

                                            int loopOrphanRawCount = 0;
                                            try { loopOrphanRawCount = GetQueueOrphanFollowerCount(); } catch { }

                                            bool localHasQueueTeammate = false;
                                            try { localHasQueueTeammate = TryGetQueueLocalTeammateUserId(PhotonRealtimeClient.LocalPlayer?.UserId ?? string.Empty, out _); } catch { }

                                            bool deferTransientOrphanEarly = loopHumanCount == loopRequiredFollowers + 1
                                                && loopOrphanRawCount > 0
                                                && !localHasQueueTeammate;

                                            if (deferTransientOrphanEarly)
                                            {
                                                Debug.Log($"QueueTimerCoroutine: early readiness deferred for transient orphan state (orphanRawCount={loopOrphanRawCount}, humanCount={loopHumanCount}, requiredTotal={loopRequiredFollowers + 1}, localHasQueueTeammate={localHasQueueTeammate}).");
                                            }
                                            else if (loopOrphanFollowerCount > 0)
                                            {
                                                Debug.Log($"QueueTimerCoroutine: early readiness deferred for one-duo block composition (orphans={loopOrphanFollowerCount}, humanCount={loopHumanCount}, requiredTotal={loopRequiredFollowers + 1}); waiting for queue timeout to avoid splitting pending duo joins.");
                                            }
                                            else
                                            {
                                                // If we have enough selected followers and there are no orphan followers
                                                // or pending duo signals, it's safe to form the match immediately even
                                                // if the room currently contains extra humans beyond the exact-size case.
                                                if (loopSelected.Count >= loopRequiredFollowers && !HasPendingQueueDuoSignals())
                                                {
                                                    bool allowForm = true;
                                                    try
                                                    {
                                                        Room curr = PhotonRealtimeClient.CurrentRoom;
                                                        if (curr != null)
                                                        {
                                                            // Build quick playersById map
                                                            Dictionary<string, Player> playersById = curr.Players.Values
                                                                .Where(p => p != null && !string.IsNullOrEmpty(p.UserId) && p.UserId != "Bot")
                                                                .GroupBy(p => p.UserId)
                                                                .Select(g => g.First())
                                                                .ToDictionary(p => p.UserId, p => p);

                                                            // Check room premade metadata: if a premade pair is present in the room
                                                            // but not both included in the selected set, defer formation to avoid splitting.
                                                            try
                                                            {
                                                                bool roomPremadeMode = curr.GetCustomProperty<bool>(PhotonBattleRoom.PremadeModeKey, false);
                                                                if (roomPremadeMode)
                                                                {
                                                                    string prem1 = curr.GetCustomProperty<string>(PhotonBattleRoom.PremadeUserId1Key, string.Empty);
                                                                    string prem2 = curr.GetCustomProperty<string>(PhotonBattleRoom.PremadeUserId2Key, string.Empty);
                                                                    if (!string.IsNullOrEmpty(prem1) && !string.IsNullOrEmpty(prem2)
                                                                        && playersById.ContainsKey(prem1) && playersById.ContainsKey(prem2))
                                                                    {
                                                                            string localUserId = PhotonRealtimeClient.LocalPlayer?.UserId ?? string.Empty;
                                                                            bool p1Sel = prem1 == localUserId || loopSelected.Contains(prem1);
                                                                            bool p2Sel = prem2 == localUserId || loopSelected.Contains(prem2);
                                                                            if (!(p1Sel && p2Sel))
                                                                        {
                                                                            allowForm = false;
                                                                            Debug.Log($"QueueTimerCoroutine: deferring formation because premade pair ({prem1},{prem2}) present but not both selected; selected=[{string.Join(",", loopSelected)}].");
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            catch { }

                                                            // Also check leader/follower mappings: if any selected user has a queued teammate
                                                            // (via LeaderIdKey mapping) present in the room but that teammate is not selected,
                                                            // defer formation to avoid splitting.
                                                            if (allowForm)
                                                            {
                                                                    string localUserId = PhotonRealtimeClient.LocalPlayer?.UserId ?? string.Empty;
                                                                    foreach (string uid in loopSelected)
                                                                {
                                                                    try
                                                                    {
                                                                            if (TryGetQueueLocalTeammateUserId(uid, out string buddy) && !string.IsNullOrEmpty(buddy) && playersById.ContainsKey(buddy))
                                                                            {
                                                                                bool buddySelected = buddy == localUserId || loopSelected.Contains(buddy);
                                                                                if (!buddySelected)
                                                                                {
                                                                                    allowForm = false;
                                                                                    Debug.Log($"QueueTimerCoroutine: deferring formation because teammate {buddy} of selected user {uid} is present but not selected.");
                                                                                    break;
                                                                                }
                                                                            }
                                                                    }
                                                                    catch { }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    catch { }

                                                    if (allowForm)
                                                    {
                                                        Debug.Log($"QueueTimerCoroutine: one-duo composition safe to form; forming match with followers [{string.Join(",", loopSelected)}].");
                                                        try
                                                        {
                                                            _formingMatchHolder = StartCoroutine(FormMatchFromQueue(loopSelected.ToArray(), loopGameTypeInt, loopClanName, loopSoulhomeRank));
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            Debug.LogWarning($"QueueTimerCoroutine: failed to start FormMatchFromQueue: {ex.Message}");
                                                        }
                                                        yield break;
                                                    }
                                                    else
                                                    {
                                                        Debug.Log($"QueueTimerCoroutine: early formation deferred due to premade/teammate integrity; selected=[{string.Join(",", loopSelected)}].");
                                                    }
                                                }

                                                Debug.Log($"QueueTimerCoroutine: early readiness deferred for one-duo composition (orphans={loopOrphanFollowerCount}, humanCount={loopHumanCount}, requiredTotal={loopRequiredFollowers + 1}); waiting for queue timeout to preserve duo integrity.");
                                            }
                                        }
                                        else
                                        {
                                            Debug.Log($"QueueTimerCoroutine: queue became ready before timeout, forming match with followers [{string.Join(",", loopSelected)}].");
                                            _formingMatchHolder = StartCoroutine(FormMatchFromQueue(loopSelected.ToArray(), loopGameTypeInt, loopClanName, loopSoulhomeRank));
                                            yield break;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"QueueTimerCoroutine: early readiness check failed: {ex.Message}");
                        }

                        yield return null;
                    }

                    int gameTypeInt = (int)GameType.Random2v2;
                    string clanName = string.Empty;
                    int soulhomeRank = 0;
                    try { gameTypeInt = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey); } catch (Exception ex) { Debug.LogWarning($"QueueTimerCoroutine: failed to read game type: {ex.Message}"); }
                    try { clanName = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.ClanNameKey, ""); } catch (Exception ex) { Debug.LogWarning($"QueueTimerCoroutine: failed to read clan name: {ex.Message}"); }
                    try { soulhomeRank = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.SoulhomeRank, 0); } catch (Exception ex) { Debug.LogWarning($"QueueTimerCoroutine: failed to read soulhome rank: {ex.Message}"); }

                    int requiredFollowers = GetQueueRequiredFollowerCount(gameTypeInt);
                    string preferredMasterUserId;
                    int completeDuoCount;
                    int eligibleSoloCount;
                    int orphanFollowerCount;
                    string singleEligibleSoloUserId;
                    List<string> selected = SelectQueueFollowersForMatch(gameTypeInt, requiredFollowers, out preferredMasterUserId, out completeDuoCount, out eligibleSoloCount, out orphanFollowerCount, out singleEligibleSoloUserId);
                    try
                    {
                        Debug.Log($"QueueTimerCoroutine: selection debug -> preferredMaster='{preferredMasterUserId}', completeDuos={completeDuoCount}, eligibleSolos={eligibleSoloCount}, singleEligibleSoloUserId='{singleEligibleSoloUserId}', selectedCount={selected.Count}, selected=[{string.Join(",", selected)}]");
                    }
                    catch { }

                    if (!string.IsNullOrEmpty(preferredMasterUserId))
                    {
                        if (TryTransferQueueMaster(preferredMasterUserId, $"duo-priority selection (completeDuos={completeDuoCount})"))
                        {
                            yield break;
                        }
                    }

                    if (selected.Count < requiredFollowers)
                    {
                        // If there are no selected followers, try to preserve any present duo(s)
                        // by adding their member IDs so the leader can persist expected-users
                        // and allow bots to fill remaining slots.
                        if (selected.Count == 0 && completeDuoCount > 0)
                        {
                            try
                            {
                                var roomScan = PhotonRealtimeClient.CurrentRoom;
                                if (roomScan?.Players != null)
                                {
                                    var humanUserIds = roomScan.Players.Values
                                        .Where(p => p != null && !string.IsNullOrEmpty(p.UserId) && p.UserId != "Bot")
                                        .Select(p => p.UserId)
                                        .ToList();

                                    var roomPairs = GetQueueCompleteDuoPairsForParticipants(humanUserIds);
                                    var localId = PhotonRealtimeClient.LocalPlayer?.UserId ?? string.Empty;

                                    if (roomPairs != null && roomPairs.Count > 0)
                                    {
                                        // Prefer a pair that doesn't include the local user (leader). If none,
                                        // fall back to the first available pair and add the non-local member.
                                        (string userId1, string userId2) chosen = (null, null);
                                        foreach (var p in roomPairs)
                                        {
                                            if (p.userId1 != localId && p.userId2 != localId)
                                            {
                                                chosen = p;
                                                break;
                                            }
                                        }
                                        if (chosen.userId1 == null) chosen = roomPairs[0];

                                        if (!string.IsNullOrEmpty(chosen.userId1) && !string.IsNullOrEmpty(chosen.userId2))
                                        {
                                            if (chosen.userId1 != localId) selected.Add(chosen.userId1);
                                            if (chosen.userId2 != localId) selected.Add(chosen.userId2);
                                            Debug.Log($"QueueTimerCoroutine: Queue wait expired; added duo [{string.Join(",", selected)}] to selected to preserve duo and allow botfill (requiredFollowers={requiredFollowers}).");
                                        }
                                    }

                                    // If the duo helper could not re-identify a pair, still salvage the timeout by
                                    // seeding the match from the visible non-local humans. This lets botfill proceed
                                    // for duo+solo compositions instead of retrying forever on a transient duo lookup miss.
                                    if (selected.Count == 0)
                                    {
                                        var broadFallbackCandidates = humanUserIds
                                            .Where(uid => !string.IsNullOrEmpty(uid) && uid != localId)
                                            .Where(uid => uid != "Bot")
                                            .Take(requiredFollowers)
                                            .ToList();

                                        if (broadFallbackCandidates.Count > 0)
                                        {
                                            foreach (var uid in broadFallbackCandidates) selected.Add(uid);
                                            Debug.Log($"QueueTimerCoroutine: Queue wait expired; broad fallback selected [{string.Join(",", broadFallbackCandidates)}] for botfill (requiredFollowers={requiredFollowers}).");
                                        }
                                    }

                                    // If adding the duo's member(s) still leaves us short of required followers,
                                    // try to include any available solo humans so the master can persist expected-users
                                    // and let bots fill the remainder. This covers the case where the local master
                                    // is part of a duo and a lone solo is present in the room.
                                    try
                                    {
                                        if (selected.Count > 0 && selected.Count < requiredFollowers && PhotonRealtimeClient.CurrentRoom?.Players != null)
                                        {
                                            // Prefer the singleEligibleSoloUserId when provided by the selector and not already selected.
                                            var localIdCheck = PhotonRealtimeClient.LocalPlayer?.UserId ?? string.Empty;
                                            if (!string.IsNullOrEmpty(singleEligibleSoloUserId) && !selected.Contains(singleEligibleSoloUserId) && singleEligibleSoloUserId != localIdCheck)
                                            {
                                                selected.Add(singleEligibleSoloUserId);
                                                Debug.Log($"QueueTimerCoroutine: Queue wait expired; added solo '{singleEligibleSoloUserId}' alongside duo to selected (requiredFollowers={requiredFollowers}).");
                                            }
                                            else
                                            {
                                                var extraCandidates = PhotonRealtimeClient.CurrentRoom.Players.Values
                                                    .Where(p => p != null && !string.IsNullOrEmpty(p.UserId) && p.UserId != "Bot" && p.UserId != PhotonRealtimeClient.LocalPlayer?.UserId)
                                                    .Select(p => p.UserId)
                                                    .Where(uid => !selected.Contains(uid))
                                                    .Distinct()
                                                    .Take(requiredFollowers - selected.Count)
                                                    .ToList();

                                                if (extraCandidates.Count > 0)
                                                {
                                                    foreach (var uid in extraCandidates) selected.Add(uid);
                                                    Debug.Log($"QueueTimerCoroutine: Queue wait expired; added solos [{string.Join(",", extraCandidates)}] alongside duo to selected (requiredFollowers={requiredFollowers}).");
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.LogWarning($"QueueTimerCoroutine: failed to add solo alongside duo fallback: {ex.Message}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning($"QueueTimerCoroutine: failed to locate duo fallback: {ex.Message}");
                            }
                        }

                        // If we have no selected followers and no complete duos, attempt to salvage
                        // the timeout by including any available solo humans so the leader can
                        // persist `qe`/`eu` and WaitForMatchmakingPlayers will preserve them while bots fill.
                        // NOTE: allow the fallback even when the selector reported 0 eligible solos
                        // (eligibleSoloCount==0) so transient/filtered solos are still considered
                        // for botfill when the wait expires.
                        if (selected.Count == 0 && completeDuoCount == 0)
                        {
                            // Prefer the singleEligibleSoloUserId when provided by the selector.
                            if (eligibleSoloCount == 1 && !string.IsNullOrEmpty(singleEligibleSoloUserId))
                            {
                                selected.Add(singleEligibleSoloUserId);
                                Debug.Log($"QueueTimerCoroutine: Queue wait expired with lone solo present; adding solo '{singleEligibleSoloUserId}' to selected and forming queue match (requiredFollowers={requiredFollowers}).");
                            }
                            else
                            {
                                // Best-effort: scan the room for non-bot, non-local human players and add
                                // up to `requiredFollowers` of them to `selected` so they become expected.
                                try
                                {
                                    var roomScan = PhotonRealtimeClient.CurrentRoom;
                                    if (roomScan?.Players != null)
                                    {
                                        var candidates = roomScan.Players.Values
                                            .Where(p => p != null && !string.IsNullOrEmpty(p.UserId) && p.UserId != "Bot" && p.UserId != PhotonRealtimeClient.LocalPlayer?.UserId)
                                            .Select(p => p.UserId)
                                            .Where(uid => !selected.Contains(uid))
                                            .Distinct()
                                            .Take(requiredFollowers)
                                            .ToList();

                                        if (candidates.Count > 0)
                                        {
                                            foreach (var uid in candidates) selected.Add(uid);
                                            Debug.Log($"QueueTimerCoroutine: Queue wait expired; added eligible solos [{string.Join(",", candidates)}] to selected (requiredFollowers={requiredFollowers}).");
                                        }
                                        else
                                        {
                                            Debug.Log($"QueueTimerCoroutine: Queue wait expired but no eligible solo candidates were found in room; forming queue match to trigger botfill (requiredFollowers={requiredFollowers}, eligibleSolos={eligibleSoloCount}).");
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogWarning($"QueueTimerCoroutine: failed to locate eligible solos fallback: {ex.Message}");
                                }
                            }
                        }
                        else if (selected.Count == 0)
                        {
                            Debug.Log($"QueueTimerCoroutine: Queue wait expired but not enough eligible players (requiredFollowers={requiredFollowers}, selectedFollowers={selected.Count}, completeDuos={completeDuoCount}, eligibleSolos={eligibleSoloCount}). Retrying.");
                            continue;
                        }
                    }

                    if (IsTwoPlayerBlockQueueMode(gameTypeInt) && ShouldDeferTwoPlayerBlockStartForMultiDuo(requiredFollowers, selected, out string timeoutMultiDuoReason))
                    {
                        Debug.Log($"QueueTimerCoroutine: timeout selection deferred to preserve complete duo pairs ({timeoutMultiDuoReason}).");
                        continue;
                    }

                    if (IsTwoPlayerBlockQueueMode(gameTypeInt) && ShouldDeferTwoPlayerBlockStartForPendingQueueDuo(requiredFollowers, selected, out string timeoutPendingDuoReason))
                    {
                        Debug.Log($"QueueTimerCoroutine: timeout selection deferred for pending queue duo handoff ({timeoutPendingDuoReason}).");
                        continue;
                    }

                    if (IsTwoPlayerBlockQueueMode(gameTypeInt) && ShouldDeferTwoPlayerBlockEarlyStartForOneSidedPremadeExactSize(requiredFollowers, completeDuoCount, out string timeoutOneSidedPremadeReason))
                    {
                        Debug.Log($"QueueTimerCoroutine: timeout selection deferred due to one-sided premade metadata in exact-size queue ({timeoutOneSidedPremadeReason}).");
                        continue;
                    }

                    if (IsTwoPlayerBlockQueueMode(gameTypeInt) && completeDuoCount == 1)
                    {
                        int humanCount = 0;
                        try
                        {
                            if (PhotonRealtimeClient.CurrentRoom?.Players != null)
                            {
                                humanCount = PhotonRealtimeClient.CurrentRoom.Players.Values
                                    .Count(p => p != null && !string.IsNullOrEmpty(p.UserId) && p.UserId != "Bot");
                            }
                        }
                        catch { }

                        bool deferOneDuoTimeout = orphanFollowerCount > 0 || humanCount > requiredFollowers + 1;
                        if (deferOneDuoTimeout)
                        {
                            Debug.Log($"QueueTimerCoroutine: timeout selection deferred because one-duo composition may split pending duo joins (orphans={orphanFollowerCount}, humanCount={humanCount}, requiredTotal={requiredFollowers + 1}). Retrying for complete duo pairs.");
                            continue;
                        }
                    }

                    if (_formingMatchHolder != null)
                    {
                        Debug.Log("QueueTimerCoroutine: match formation already in progress; retrying in next queue cycle.");
                        continue;
                    }

                        // Before forming match, verify duo integrity: do not form if a premade pair
                        // is present in the room but not both included in the selected followers,
                        // or if any selected user has a queued teammate present but not selected.
                        bool allowFinalForm = true;
                        try
                        {
                            Room curr = PhotonRealtimeClient.CurrentRoom;
                            if (curr != null)
                            {
                                Dictionary<string, Player> playersById = curr.Players.Values
                                    .Where(p => p != null && !string.IsNullOrEmpty(p.UserId) && p.UserId != "Bot")
                                    .GroupBy(p => p.UserId)
                                    .Select(g => g.First())
                                    .ToDictionary(p => p.UserId, p => p);

                                try
                                {
                                    bool roomPremadeMode = curr.GetCustomProperty<bool>(PhotonBattleRoom.PremadeModeKey, false);
                                    if (roomPremadeMode)
                                    {
                                        string prem1 = curr.GetCustomProperty<string>(PhotonBattleRoom.PremadeUserId1Key, string.Empty);
                                        string prem2 = curr.GetCustomProperty<string>(PhotonBattleRoom.PremadeUserId2Key, string.Empty);
                                        if (!string.IsNullOrEmpty(prem1) && !string.IsNullOrEmpty(prem2)
                                            && playersById.ContainsKey(prem1) && playersById.ContainsKey(prem2))
                                        {
                                            // Consider a premade user "selected" if they are either in the
                                            // selected followers list or are the local/master user forming
                                            // the match. This avoids deferring formation when the local
                                            // master is part of the premade pair but not present in
                                            // the `selected` array (expected-users should only list
                                            // followers, not the leader).
                                            string localId = PhotonRealtimeClient.LocalPlayer?.UserId ?? string.Empty;
                                            bool p1Sel = selected.Contains(prem1) || prem1 == localId;
                                            bool p2Sel = selected.Contains(prem2) || prem2 == localId;
                                            if (!(p1Sel && p2Sel))
                                            {
                                                allowFinalForm = false;
                                                Debug.Log($"QueueTimerCoroutine: deferring final formation because premade pair ({prem1},{prem2}) present but not both included among participants; selected=[{string.Join(",", selected)}], local={localId}.");
                                            }
                                        }
                                    }
                                }
                                catch { }

                                if (allowFinalForm)
                                {
                                    string localId = PhotonRealtimeClient.LocalPlayer?.UserId ?? string.Empty;
                                    foreach (string uid in selected)
                                    {
                                        try
                                        {
                                            if (TryGetQueueLocalTeammateUserId(uid, out string buddy) && !string.IsNullOrEmpty(buddy) && playersById.ContainsKey(buddy))
                                            {
                                                // If the buddy is the local/master user, treat them as implicitly
                                                // present and selected (the leader isn't listed among followers).
                                                if (!selected.Contains(buddy) && buddy != localId)
                                                {
                                                    allowFinalForm = false;
                                                    Debug.Log($"QueueTimerCoroutine: deferring final formation because teammate {buddy} of selected user {uid} is present but not selected.");
                                                    break;
                                                }
                                            }
                                        }
                                        catch { }
                                    }
                                }
                            }
                        }
                        catch { }

                        if (!allowFinalForm)
                        {
                            Debug.Log($"QueueTimerCoroutine: final formation deferred due to premade/teammate integrity; selected=[{string.Join(",", selected)}].");
                            continue;
                        }

                        Debug.Log($"QueueTimerCoroutine: Queue wait expired after {QueueWaitSeconds}s, forming match with followers [{string.Join(",", selected)}].");
                        try
                        {
                            _formingMatchHolder = StartCoroutine(FormMatchFromQueue(selected.ToArray(), gameTypeInt, clanName, soulhomeRank));
                            yield break;
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"QueueTimerCoroutine: FormMatchFromQueue failed to start: {ex.Message}");
                        }
                }
            }
            finally
            {
                _queueTimerHolder = null;
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
                _premadeTeammateUserId = string.Empty;
                _isPremadeMatchmakingFlow = false;
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

            if (_canBattleStartCheckHolder != null)
            {
                StopCoroutine(_canBattleStartCheckHolder);
                _canBattleStartCheckHolder = null;
            }
        }

        private IEnumerator LeaveAndAutoRequeue(GameType gameType)
        {
            try
            {
                string localUserId = PhotonRealtimeClient.LocalPlayer?.UserId ?? string.Empty;
                bool requeuePremadeMode = false;
                string requeuePremadeUserId1 = string.Empty;
                string requeuePremadeUserId2 = string.Empty;
                int requeuePremadeTargetGameType = (int)gameType;

                // Capture premade metadata before stopping coroutines/leave so requeue preserves same-side pairing.
                try
                {
                    if (PhotonRealtimeClient.CurrentRoom != null)
                    {
                        requeuePremadeMode = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<bool>(PhotonBattleRoom.PremadeModeKey, false);
                        requeuePremadeUserId1 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PremadeUserId1Key, string.Empty);
                        requeuePremadeUserId2 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PremadeUserId2Key, string.Empty);
                        requeuePremadeTargetGameType = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.PremadeTargetGameTypeKey, (int)gameType);
                    }
                }
                catch (Exception ex) { Debug.LogWarning($"LeaveAndAutoRequeue: failed to read premade metadata from current room: {ex.Message}"); }

                if ((!requeuePremadeMode || string.IsNullOrEmpty(requeuePremadeUserId1) || string.IsNullOrEmpty(requeuePremadeUserId2))
                    && _isPremadeMatchmakingFlow
                    && !string.IsNullOrEmpty(localUserId)
                    && !string.IsNullOrEmpty(_premadeTeammateUserId))
                {
                    requeuePremadeMode = true;
                    requeuePremadeUserId1 = localUserId;
                    requeuePremadeUserId2 = _premadeTeammateUserId;
                    requeuePremadeTargetGameType = (int)gameType;
                }

                if (requeuePremadeMode)
                {
                    bool localInPair = !string.IsNullOrEmpty(localUserId) && (localUserId == requeuePremadeUserId1 || localUserId == requeuePremadeUserId2);
                    if (!localInPair || string.IsNullOrEmpty(requeuePremadeUserId1) || string.IsNullOrEmpty(requeuePremadeUserId2) || requeuePremadeUserId1 == requeuePremadeUserId2)
                    {
                        requeuePremadeMode = false;
                    }
                }

                Debug.Log($"LeaveAndAutoRequeue: preparing to leave and requeue for {gameType}");

                // Stop any existing matchmaking/holder coroutines to avoid conflicts
                try { StopMatchmakingCoroutines(); } catch (Exception ex) { Debug.LogWarning($"LeaveAndAutoRequeue: failed to stop matchmaking coroutines: {ex.Message}"); }
                try { StopHolderCoroutines(); } catch (Exception ex) { Debug.LogWarning($"LeaveAndAutoRequeue: failed to stop holder coroutines: {ex.Message}"); }

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
                    if (requeuePremadeMode)
                    {
                        _isPremadeMatchmakingFlow = true;
                        if (localUserId == requeuePremadeUserId1) _premadeTeammateUserId = requeuePremadeUserId2;
                        else if (localUserId == requeuePremadeUserId2) _premadeTeammateUserId = requeuePremadeUserId1;
                    }

                    // Keep requeue behavior symmetric for all clients so everyone returns to
                    // the persistent queue room and queue timer can form the next match.
                    if (_autoJoinHolder != null)
                    {
                        StopCoroutine(_autoJoinHolder);
                        _autoJoinHolder = null;
                    }
                    _autoJoinHolder = StartCoroutine(RequeueToPersistentQueue(gameType, requeuePremadeMode, requeuePremadeUserId1, requeuePremadeUserId2, requeuePremadeTargetGameType));
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
                        _autoJoinHolder = StartCoroutine(RequeueToPersistentQueue(gameType, requeuePremadeMode, requeuePremadeUserId1, requeuePremadeUserId2, requeuePremadeTargetGameType));
                    }
                    else
                    {
                        Debug.Log("LeaveAndAutoRequeue: skipping auto-join for Custom game type.");
                    }
                }
            }
            finally { }
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
            this.Subscribe<StartRoomEvent>(OnStartRoomEvent);
            this.Subscribe<StartPlayingEvent>(OnStartPlayingEvent);
            this.Subscribe<StartRaidTestEvent>(OnStartRaidTestEvent);
            this.Subscribe<StartMatchmakingEvent>(OnStartMatchmakingEvent);
            this.Subscribe<StopMatchmakingEvent>(OnStopMatchmakingEvent);
            this.Subscribe<GetKickedEvent>(OnGetKickedEvent);
            if (_serviceHolder == null) _serviceHolder = StartCoroutine(Service());

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

        #endregion




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

            if (_canBattleStartCheckHolder != null)
            {
                StopCoroutine(_canBattleStartCheckHolder);
                _canBattleStartCheckHolder = null;
            }
        }

        /// <summary>
        /// Leader-side matchmaking entry point.
        /// Flow: lock current room -> gather teammate/position context -> notify followers -> leave to lobby -> join or create a matchmaking room.
        /// </summary>
        private IEnumerator StartMatchmaking(GameType gameType, bool broadcastRoomChange = true)
        {
            // remember which game type we're matchmaking for so failure handlers can requeue
            _currentMatchmakingGameType = gameType;
            bool keepHolder = false;
            try
            {
            string localUserId = PhotonRealtimeClient.LocalPlayer.UserId;

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
            if (_isPremadeMatchmakingFlow)
            {
                string resolvedPremadeTeammateUserId = string.Empty;
                bool teammateFound = TryGetQueueLocalTeammateUserId(localUserId, out resolvedPremadeTeammateUserId);
                if (!teammateFound || string.IsNullOrEmpty(resolvedPremadeTeammateUserId))
                {
                    resolvedPremadeTeammateUserId = !string.IsNullOrEmpty(_premadeTeammateUserId)
                        ? _premadeTeammateUserId
                        : expectedUsers.FirstOrDefault(id => !string.IsNullOrEmpty(id));
                }

                _premadeTeammateUserId = resolvedPremadeTeammateUserId ?? string.Empty;
                _teammates = string.IsNullOrEmpty(_premadeTeammateUserId)
                    ? Array.Empty<string>()
                    : new[] { _premadeTeammateUserId };
            }
            else
            {
                _premadeTeammateUserId = string.Empty;
            }

            // Sending other players in the room the room change request, setting own leader id key as own userid to indicate being the leader
            PhotonRealtimeClient.LocalPlayer.SetCustomProperty(PhotonBattleRoom.LeaderIdKey, PhotonRealtimeClient.LocalPlayer.UserId);
            try { OnRoomLeaderChanged?.Invoke(true); } catch (Exception ex) { Debug.LogWarning($"StartMatchmaking: OnRoomLeaderChanged invocation failed: {ex.Message}"); }

            if (broadcastRoomChange && PhotonRealtimeClient.Client != null && PhotonRealtimeClient.Client.Server == ServerConnection.GameServer && PhotonRealtimeClient.Client.IsConnectedAndReady && PhotonRealtimeClient.InRoom)
            {
                object roomChangePayload = PhotonRealtimeClient.LocalPlayer.UserId;
                if (_isPremadeMatchmakingFlow)
                {
                    roomChangePayload = new object[] { localUserId, _teammates, $"Queue_{gameType}" };
                }

                SafeRaiseEvent(
                    PhotonRealtimeClient.PhotonEvent.RoomChangeRequested,
                    roomChangePayload,
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

            // Wait for lobby and initial room listing; room search below depends on CurrentRooms.
            yield return new WaitUntil(() => PhotonRealtimeClient.InLobby && CurrentRooms != null);

            // Queue-authoritative flow: all clients enter queue room first and queue owner forms matches.
            if (UseQueueAuthoritativeMatchmaking)
            {
                bool queueJoinRequested = false;
                try
                {
                    queueJoinRequested = PhotonRealtimeClient.JoinOrCreateQueueRoom(gameType);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"StartMatchmaking: JoinOrCreateQueueRoom failed: {ex.Message}");
                }

                if (queueJoinRequested)
                {
                    float queueJoinStart = Time.time;
                    yield return new WaitUntil(() => PhotonRealtimeClient.InRoom || Time.time > queueJoinStart + 8f);

                    bool joinedQueueRoom = false;
                    string queueRoomName = string.Empty;
                    try
                    {
                        joinedQueueRoom = PhotonRealtimeClient.InRoom
                            && PhotonRealtimeClient.CurrentRoom != null
                            && PhotonRealtimeClient.CurrentRoom.GetCustomProperty<bool>(PhotonBattleRoom.IsQueueKey);
                        if (joinedQueueRoom)
                        {
                            queueRoomName = PhotonRealtimeClient.CurrentRoom.Name;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"StartMatchmaking: failed to verify queue room join: {ex.Message}");
                    }

                    if (joinedQueueRoom)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(_premadeTeammateUserId))
                            {
                                PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeModeKey, true);
                                PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeTargetGameTypeKey, (int)gameType);
                                PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeLeaderUserIdKey, localUserId);
                                PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeUserId1Key, localUserId);
                                PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeUserId2Key, _premadeTeammateUserId);
                            }
                            PhotonRealtimeClient.CurrentRoom.SetCustomProperty("qe", _teammates?.Length ?? 0);
                            if (_teammates != null && _teammates.Length > 0)
                            {
                                PhotonRealtimeClient.CurrentRoom.SetCustomProperty("eu", _teammates);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"StartMatchmaking: failed to stamp queue metadata: {ex.Message}");
                        }

                        if (broadcastRoomChange && !string.IsNullOrEmpty(queueRoomName))
                        {
                            SafeRaiseEvent(
                                PhotonRealtimeClient.PhotonEvent.RoomChangeRequested,
                                new object[] { localUserId, _teammates, queueRoomName },
                                new RaiseEventArgs { Receivers = ReceiverGroup.Others },
                                SendOptions.SendReliable
                            );
                        }

                        // Queue room handling takes over from OnJoinedRoom (queue timer / queue master logic).
                        yield break;
                    }

                    Debug.LogWarning("StartMatchmaking: queue join did not land in a queue room.");
                }
                else
                {
                    Debug.LogWarning("StartMatchmaking: queue join request failed.");
                }

                // Ensure failure path starts from lobby if queue join attempt left us in a room.
                if (PhotonRealtimeClient.InRoom)
                {
                    PhotonRealtimeClient.LeaveRoom();
                    yield return new WaitUntil(() => PhotonRealtimeClient.InLobby);
                }

                Debug.LogWarning("StartMatchmaking: aborting matchmaking start because queue-authoritative entry failed.");
                OnFailedToStartMatchmakingGame?.Invoke();
                yield break;
            }

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
                                shouldTryJoin = !_isPremadeMatchmakingFlow || RoomHasSameSideCapacityForPremade(room);
                            }
                            break;
                    }

                    if (!shouldTryJoin) continue;

                    // Attempt join and wait until success or timeout. Use scoped attempt id for diagnostics.
                    int joinAttemptId = BeginJoinAttempt(room.Name, _teammates);
                    try { PhotonRealtimeClient.JoinRoom(room.Name, _teammates); } catch (Exception ex) { Debug.LogWarning($"JoinAttempt[{joinAttemptId}]: JoinRoom threw: {ex.Message}"); MarkJoinAttemptFailure(joinAttemptId, -1, ex.Message); }
                    float joinStart = Time.time;
                    while (!PhotonRealtimeClient.InRoom && Time.time - joinStart < joinAttemptTimeout)
                    {
                        bool attemptCompleted = false;
                        lock (_joinAttemptsLock) { attemptCompleted = _joinAttempts.TryGetValue(joinAttemptId, out var a) && a.Completed; }
                        if (attemptCompleted) break;
                        yield return null;
                    }

                    if (PhotonRealtimeClient.InRoom)
                    {
                        Debug.Log($"JoinAttempt[{joinAttemptId}]: joined room '{PhotonRealtimeClient.CurrentRoom?.Name}'");
                        roomFound = true;
                        joinedExistingRoom = true;
                        break;
                    }
                    else
                    {
                        bool failed = false;
                        string failMsg = null;
                        lock (_joinAttemptsLock) { if (_joinAttempts.TryGetValue(joinAttemptId, out var a) && a.Completed && !a.Success) { failed = true; failMsg = a.FailureMessage; } }
                        if (failed)
                        {
                            Debug.LogWarning($"JoinAttempt[{joinAttemptId}]: failed (error) joining '{room.Name}': {failMsg}");
                        }
                        else
                        {
                            Debug.LogWarning($"JoinAttempt[{joinAttemptId}]: timed out after {joinAttemptTimeout}s for '{room.Name}', trying next candidate.");
                        }
                        // try next room
                    }
                }
            }

            // If no candidate worked, let backend pick or create a suitable room atomically.
            if (!joinedExistingRoom)
            {
                switch (gameType)
                {
                    case GameType.Clan2v2:
                        if (_isPremadeMatchmakingFlow)
                        {
                            PhotonRealtimeClient.JoinRandomOrCreateClan2v2Room(clanName, soulhomeRank, _teammates, true);
                        }
                        else
                        {
                            PhotonRealtimeClient.JoinOrCreateMatchmakingRoom(GameType.Clan2v2, _teammates, clanName, soulhomeRank);
                        }
                        break;
                    case GameType.Random2v2:
                        if (_isPremadeMatchmakingFlow)
                        {
                            PhotonRealtimeClient.JoinRandomOrCreateRandom2v2Room(_teammates, true);
                        }
                        else
                        {
                            PhotonRealtimeClient.JoinOrCreateMatchmakingRoom(GameType.Random2v2, _teammates);
                        }
                        break;
                }
            }

            // Block until our matchmaking-room join completes.
            yield return new WaitUntil(() => PhotonRealtimeClient.InRoom);

            if (_isPremadeMatchmakingFlow && !string.IsNullOrEmpty(_premadeTeammateUserId))
            {
                try
                {
                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeModeKey, true);
                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeTargetGameTypeKey, (int)gameType);
                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeLeaderUserIdKey, localUserId);
                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeUserId1Key, localUserId);
                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeUserId2Key, _premadeTeammateUserId);
                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeInviteStateKey, PhotonBattleRoom.PremadeInviteStateAccepted);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"StartMatchmaking: failed to set premade matchmaking room properties: {ex.Message}");
                }
            }

            // Reconcile split groups caused by near-simultaneous room creation.
            // Disabled: deterministically converge toward one shared room (commented out).
            /*
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
                            catch (Exception ex) { Debug.LogWarning($"ReconcileMatchmakingRooms: reading room properties failed: {ex.Message}"); continue; }

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
                            catch (Exception ex) { Debug.LogWarning($"ReconcileMatchmakingRooms: reading room properties failed: {ex.Message}"); continue; }

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
            */

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
                        else if (_isPremadeMatchmakingFlow && !string.IsNullOrEmpty(_premadeTeammateUserId))
                        {
                            if (!TryReservePremadePairToSameSide(localUserId, _premadeTeammateUserId, out _))
                            {
                                Debug.LogWarning("StartMatchmaking: no same-side capacity for premade duo, requeueing.");
                                StartCoroutine(LeaveAndAutoRequeue(gameType));
                                yield break;
                            }
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
            bool IsRealPlayerValue(string value) => !string.IsNullOrEmpty(value) && !IsBotValue(value);

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

        /// <summary>
        /// Master-side wait loop that decides when a matchmaking room can start.
        /// Handles: missing expected users -> short requeue, long Random2v2 wait -> bot backfill, then start countdown/gameplay.
        /// </summary>
        private IEnumerator WaitForMatchmakingPlayers()
        {
            try
            {
                if (!PhotonRealtimeClient.LocalPlayer.IsMasterClient) yield break;

                bool gameStarting = false;
                float waitStartTime = Time.time;
                bool botBackfillApplied = false;
                try
                {
                    if (PhotonRealtimeClient.InRoom && PhotonRealtimeClient.CurrentRoom != null)
                    {
                        botBackfillApplied = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<bool>(PhotonBattleRoom.BotFillKey, false);
                    }
                }
                catch (Exception ex) { Debug.LogWarning($"WaitForMatchmakingPlayers: failed to read initial BotFillKey state: {ex.Message}"); }

                do
                {
                // Checking every 0,5s if we can start gameplay
                bool canStartGameplay = false;
                do
                {
                    yield return new WaitForSeconds(0.5f);

                    // If we lost the room (race during master switch/leave), stop waiting
                    if (!PhotonRealtimeClient.InRoom || PhotonRealtimeClient.CurrentRoom == null)
                    {
                        Debug.LogWarning("WaitForMatchmakingPlayers: Not in a room anymore, aborting matchmaking wait.");
                        yield break;
                    }

                    // Check if matchmaking timeout expired and fill remaining slots with bots (Random2v2 only)
                    GameType currentGameType = GameType.Random2v2;
                    try
                    {
                        currentGameType = (GameType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey);
                    }
                    catch (Exception ex) { Debug.LogWarning($"WaitForMatchmakingPlayers: failed to read game type: {ex.Message}"); }

                    // Short join timeout: if after MatchmakingJoinTimeoutSeconds the countdown hasn't started,
                    // master should instruct all clients to leave and requeue so the group can reform.
                    int expectedFollowers = 0;
                    try { expectedFollowers = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>("qe", 0); } catch (Exception ex) { Debug.LogWarning($"WaitForMatchmakingPlayers: failed to read expected followers: {ex.Message}"); expectedFollowers = 0; }

                    // Track whether expected users are missing. Use explicit custom property first
                    // and fall back to Photon slot-reservation metadata.
                    bool expectedUsersMissing = true;
                    string[] expectedUsers = null;
                    try
                    {
                        expectedUsers = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string[]>("eu", null);
                        if ((expectedUsers == null || expectedUsers.Length == 0) && PhotonRealtimeClient.CurrentRoom.ExpectedUsers != null && PhotonRealtimeClient.CurrentRoom.ExpectedUsers.Length > 0)
                        {
                            expectedUsers = PhotonRealtimeClient.CurrentRoom.ExpectedUsers;
                        }

                        if (expectedUsers != null && expectedUsers.Length > 0)
                        {
                            bool allPresent = true;
                            foreach (var uid in expectedUsers)
                            {
                                if (string.IsNullOrEmpty(uid)) continue;
                                bool present = PhotonRealtimeClient.CurrentRoom.Players.Values.Any(p => p.UserId == uid);
                                if (!present) { allPresent = false; break; }
                            }
                            expectedUsersMissing = !allPresent;
                        }
                        else
                        {
                            expectedUsersMissing = expectedFollowers > 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        expectedUsersMissing = true;
                        Debug.LogWarning($"WaitForMatchmakingPlayers: failed to evaluate expected users presence: {ex.Message}");
                    }

                    bool expectedUsersConfigured = expectedUsers != null && expectedUsers.Any(uid => !string.IsNullOrEmpty(uid));
                    bool expectedPlayersRequired = expectedFollowers > 0 || expectedUsersConfigured;

                    // Detailed diagnostics to investigate premature requeue issues
                    try
                    {
                        var currentUserIds = PhotonRealtimeClient.CurrentRoom.Players.Values.Select(p => p.UserId).ToArray();
                        Debug.Log($"WaitForMatchmakingPlayers: expectedFollowers={expectedFollowers}, expectedPlayersRequired={expectedPlayersRequired}, expectedUsers=[{(expectedUsers == null ? "null" : string.Join(",", expectedUsers))}], currentPlayers=[{string.Join(",", currentUserIds)}], expectedUsersMissing={expectedUsersMissing}");
                    }
                    catch (Exception ex) { Debug.LogWarning($"WaitForMatchmakingPlayers: diagnostics failed: {ex.Message}"); }

                    // Short-timeout branch for queue-formed groups: if expected users do not arrive quickly,
                    // cancel this start attempt and requeue everyone together.
                    if (!botBackfillApplied
                        && Time.time - waitStartTime >= MatchmakingJoinTimeoutSeconds
                        && expectedPlayersRequired
                        && expectedUsersMissing)
                    {
                        bool recheckFound = false;
                        if (expectedUsersConfigured)
                        {
                            // If expected users appear missing, allow a brief grace window to re-check
                            // (helps with join/property propagation races).
                            float recheckStart = Time.time;
                            while (Time.time - recheckStart < 1.0f) // up to 1s grace
                            {
                                yield return new WaitForSeconds(0.15f);
                                try
                                {
                                    var nowExpected = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string[]>("eu", null);
                                    if ((nowExpected == null || nowExpected.Length == 0) && PhotonRealtimeClient.CurrentRoom.ExpectedUsers != null && PhotonRealtimeClient.CurrentRoom.ExpectedUsers.Length > 0)
                                    {
                                        nowExpected = PhotonRealtimeClient.CurrentRoom.ExpectedUsers;
                                    }

                                    if (nowExpected != null && nowExpected.Length > 0)
                                    {
                                        bool allNowPresent = true;
                                        foreach (var uid in nowExpected)
                                        {
                                            if (string.IsNullOrEmpty(uid)) continue;
                                            bool present = PhotonRealtimeClient.CurrentRoom.Players.Values.Any(p => p.UserId == uid);
                                            if (!present) { allNowPresent = false; break; }
                                        }
                                        if (allNowPresent)
                                        {
                                            recheckFound = true;
                                            expectedUsersMissing = false;
                                            Debug.Log("WaitForMatchmakingPlayers: grace re-check found expected users present; skipping short requeue.");
                                            break;
                                        }
                                    }
                                }
                                catch (Exception ex) { Debug.LogWarning($"WaitForMatchmakingPlayers: recheck failed: {ex.Message}"); }
                            }
                        }

                        if (!recheckFound)
                        {
                        if (!_countdownActive)
                        {
                            Debug.Log($"Matchmaking short timeout ({MatchmakingJoinTimeoutSeconds}s) reached and countdown not started; master will request requeue.");

                            // Notify all clients to requeue
                            try
                            {
                                SafeRaiseEvent(
                                    PhotonRealtimeClient.PhotonEvent.CancelGameStart,
                                    new object[] { true, (int)currentGameType },
                                    new RaiseEventArgs { Receivers = ReceiverGroup.All },
                                    SendOptions.SendReliable
                                );
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning($"Failed to broadcast CancelGameStart requeue: {ex.Message}");
                            }

                            // Master leaves and requeues (LeaveAndAutoRequeue will handle master vs non-master paths).
                            try
                            {
                                StartCoroutine(LeaveAndAutoRequeue(currentGameType));
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning($"Failed to start LeaveAndAutoRequeue after short timeout: {ex.Message}");
                            }

                            yield break;
                        }
                        else
                        {
                            // Continue waiting as expected users are now present
                        }
                        }
                    }

                    // Fast-path: expected followers arrived, so fill any remaining slots with bots immediately
                    // instead of waiting the full matchmaking timeout.
                    if (!botBackfillApplied && currentGameType == GameType.Random2v2)
                    {
                        ClearStaleHumanPositionReservations("WaitForMatchmakingPlayers");

                        bool expectedFollowersPresent = expectedUsers != null && expectedUsers.Length > 0 && !expectedUsersMissing;
                        if (expectedFollowersPresent)
                        {
                            bool shouldYieldAfterPremadeReservation = false;
                            try
                            {
                                bool premadeMode = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<bool>(PhotonBattleRoom.PremadeModeKey, false);
                                string earlyPremadeUserId1 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PremadeUserId1Key, string.Empty);
                                string earlyPremadeUserId2 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PremadeUserId2Key, string.Empty);

                                if (premadeMode && !string.IsNullOrEmpty(earlyPremadeUserId1) && !string.IsNullOrEmpty(earlyPremadeUserId2))
                                {
                                    if (!TryReservePremadePairToSameSide(earlyPremadeUserId1, earlyPremadeUserId2, out _))
                                    {
                                        Debug.LogWarning($"WaitForMatchmakingPlayers: failed premade same-side reservation before botfill for ({earlyPremadeUserId1},{earlyPremadeUserId2}), requeueing.");
                                        StartCoroutine(LeaveAndAutoRequeue(currentGameType));
                                        yield break;
                                    }

                                    shouldYieldAfterPremadeReservation = true;
                                }
                            }
                            catch (Exception ex) { Debug.LogWarning($"WaitForMatchmakingPlayers: premade reservation before botfill failed: {ex.Message}"); }

                            if (shouldYieldAfterPremadeReservation)
                            {
                                // Let room properties settle before filling remaining slots with bots.
                                yield return null;
                            }
                        }

                        bool appliedEarly = false;
                        try
                        {
                            if (expectedUsers != null && expectedUsers.Length > 0 && !expectedUsersMissing)
                            {
                                Debug.Log("WaitForMatchmakingPlayers: all expected users present; evaluating early botfill.");
                                appliedEarly = true;
                            }
                        }
                        catch (Exception ex) { Debug.LogWarning($"WaitForMatchmakingPlayers: failed to determine early bot backfill: {ex.Message}"); }

                        if (appliedEarly)
                        {
                            int currentHumanCount = 0;
                            int currentMaxPlayers = 0;
                            try
                            {
                                currentHumanCount = PhotonRealtimeClient.CurrentRoom.Players.Values
                                    .Count(p => p != null && !string.IsNullOrEmpty(p.UserId) && p.UserId != "Bot");
                                currentMaxPlayers = PhotonRealtimeClient.CurrentRoom.MaxPlayers;
                            }
                            catch (Exception ex) { Debug.LogWarning($"WaitForMatchmakingPlayers: failed to evaluate early botfill human count: {ex.Message}"); }

                            if (currentMaxPlayers > 0 && currentHumanCount >= currentMaxPlayers)
                            {
                                Debug.Log($"WaitForMatchmakingPlayers: all expected users present and room already full with humans ({currentHumanCount}/{currentMaxPlayers}); skipping early botfill.");
                            }
                            else
                            {
                                Debug.Log($"Matchmaking: applying early botfill to complete room.");
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
                                try { PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.BotFillKey, true); } catch (Exception ex) { Debug.LogWarning($"WaitForMatchmakingPlayers: failed to set BotFillKey: {ex.Message}"); }
                                botBackfillApplied = true;
                            }
                        }
                    }

                    // Queue-formed solo room already waited in queue; skip duplicated long botfill wait.
                    float effectiveBotfillTimeoutSeconds = MatchmakingTimeoutSeconds;
                    try
                    {
                        bool queueFormedMatch = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<bool>(QueueFormedMatchKey, false);
                        if (queueFormedMatch && !expectedPlayersRequired)
                        {
                            // Queue already consumed the long wait; apply botfill immediately in queue-formed solo matchmaking rooms.
                            effectiveBotfillTimeoutSeconds = 0f;
                        }
                    }
                    catch (Exception ex) { Debug.LogWarning($"WaitForMatchmakingPlayers: failed to evaluate queue-formed match timeout: {ex.Message}"); }

                    if (!botBackfillApplied && currentGameType == GameType.Random2v2 && Time.time - waitStartTime >= effectiveBotfillTimeoutSeconds)
                    {
                        int timeoutHumanCount = 0;
                        int timeoutMaxPlayers = 0;
                        try
                        {
                            timeoutHumanCount = PhotonRealtimeClient.CurrentRoom.Players.Values
                                .Count(p => p != null && !string.IsNullOrEmpty(p.UserId) && p.UserId != "Bot");
                            timeoutMaxPlayers = PhotonRealtimeClient.CurrentRoom.MaxPlayers;
                        }
                        catch (Exception ex) { Debug.LogWarning($"WaitForMatchmakingPlayers: failed to evaluate timeout botfill human count: {ex.Message}"); }

                        if (timeoutMaxPlayers > 0 && timeoutHumanCount >= timeoutMaxPlayers)
                        {
                            Debug.Log($"WaitForMatchmakingPlayers: botfill timeout reached but room already full with humans ({timeoutHumanCount}/{timeoutMaxPlayers}); skipping botfill.");
                        }
                        else
                        {
                            Debug.Log($"Matchmaking timeout ({effectiveBotfillTimeoutSeconds}s) reached for Random2v2. Filling remaining slots with bots.");

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
                    }

                    if (!botBackfillApplied)
                    {
                        // Normal path: wait for real players to fill the room
                        try
                        {
                            if (PhotonRealtimeClient.CurrentRoom.PlayerCount != PhotonRealtimeClient.CurrentRoom.MaxPlayers) continue;
                        }
                        catch (Exception ex) { Debug.LogWarning($"WaitForMatchmakingPlayers: failed to check player counts: {ex.Message}"); continue; }
                    }

                    // At this point either the room is full or we've applied bot backfill.
                    // Proceed to mapping player -> room position keys even if some position slots are not yet set.
                    canStartGameplay = true;

                } while (!canStartGameplay);


                // Updating player positions from room to player properties, and waiting that they have been synced
                if (!PhotonRealtimeClient.InRoom || PhotonRealtimeClient.CurrentRoom == null)
                {
                    Debug.LogWarning("WaitForMatchmakingPlayers: CurrentRoom lost before setting positions; aborting.");
                    yield break;
                }

                string positionValue1 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey1);
                string positionValue2 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey2);
                string positionValue3 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey3);
                string positionValue4 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey4);
                bool refreshAfterQueueDuoReservation = false;

                try
                {
                    string[] queueDuoPairs = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string[]>(QueueDuoPairsKey, null);
                    if (queueDuoPairs != null && queueDuoPairs.Length >= 2)
                    {
                        bool changedByQueueDuoReservation = false;
                        for (int i = 0; i + 1 < queueDuoPairs.Length; i += 2)
                        {
                            string pairUserId1 = queueDuoPairs[i];
                            string pairUserId2 = queueDuoPairs[i + 1];
                            if (string.IsNullOrEmpty(pairUserId1) || string.IsNullOrEmpty(pairUserId2) || pairUserId1 == pairUserId2) continue;

                            bool pairPresent = PhotonRealtimeClient.CurrentRoom.Players.Values.Any(p => p.UserId == pairUserId1)
                                && PhotonRealtimeClient.CurrentRoom.Players.Values.Any(p => p.UserId == pairUserId2);
                            if (!pairPresent) continue;

                            if (!TryReservePremadePairToSameSide(pairUserId1, pairUserId2, out _))
                            {
                                GameType requeueGameType = GameType.Random2v2;
                                try { requeueGameType = (GameType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey); } catch { }
                                Debug.LogWarning($"WaitForMatchmakingPlayers: could not keep queue duo ({pairUserId1},{pairUserId2}) on same side, requeueing.");
                                StartCoroutine(LeaveAndAutoRequeue(requeueGameType));
                                yield break;
                            }

                            changedByQueueDuoReservation = true;
                        }

                        if (changedByQueueDuoReservation)
                        {
                            refreshAfterQueueDuoReservation = true;
                        }
                    }
                }
                catch (Exception ex) { Debug.LogWarning($"WaitForMatchmakingPlayers: queue duo same-side enforcement failed: {ex.Message}"); }

                if (refreshAfterQueueDuoReservation)
                {
                    yield return null;
                    positionValue1 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey1);
                    positionValue2 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey2);
                    positionValue3 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey3);
                    positionValue4 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey4);
                }

                bool isPremadeMatch = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<bool>(PhotonBattleRoom.PremadeModeKey, false);
                string premadeUserId1 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PremadeUserId1Key, string.Empty);
                string premadeUserId2 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PremadeUserId2Key, string.Empty);
                if (isPremadeMatch && !string.IsNullOrEmpty(premadeUserId1) && !string.IsNullOrEmpty(premadeUserId2))
                {
                    if (!TryReservePremadePairToSameSide(premadeUserId1, premadeUserId2, out _))
                    {
                        GameType requeueGameType = GameType.Random2v2;
                        try { requeueGameType = (GameType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey); } catch { }
                        Debug.LogWarning($"WaitForMatchmakingPlayers: could not keep premade pair ({premadeUserId1},{premadeUserId2}) on same side, requeueing.");
                        StartCoroutine(LeaveAndAutoRequeue(requeueGameType));
                        yield break;
                    }

                    // Refresh position snapshot after forced pair reservation.
                    yield return null;
                    positionValue1 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey1);
                    positionValue2 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey2);
                    positionValue3 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey3);
                    positionValue4 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey4);
                }

                try
                {
                    bool queueFormedMatch = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<bool>(QueueFormedMatchKey, false);
                    int queueGameType = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey, (int)GameType.Random2v2);
                    if (queueFormedMatch && IsTwoPlayerBlockQueueMode(queueGameType))
                    {
                        if (!ValidateQueueTwoPlayerBlockComposition(PhotonRealtimeClient.CurrentRoom, out string blockValidationReason))
                        {
                            GameType requeueGameType = (GameType)queueGameType;
                            Debug.LogWarning($"WaitForMatchmakingPlayers: invalid two-player block composition ({blockValidationReason}), requeueing.");
                            StartCoroutine(LeaveAndAutoRequeue(requeueGameType));
                            yield break;
                        }
                    }
                }
                catch (Exception ex) { Debug.LogWarning($"WaitForMatchmakingPlayers: block composition validation failed: {ex.Message}"); }

                foreach (var player in PhotonRealtimeClient.CurrentRoom.Players)
                {
                    int position = PhotonBattleRoom.PlayerPositionGuest;

                    if (player.Value.UserId == positionValue1) position = PhotonBattleRoom.PlayerPosition1;
                    else if (player.Value.UserId == positionValue2) position = PhotonBattleRoom.PlayerPosition2;
                    else if (player.Value.UserId == positionValue3) position = PhotonBattleRoom.PlayerPosition3;
                    else if (player.Value.UserId == positionValue4) position = PhotonBattleRoom.PlayerPosition4;
                    else
                    {
                        // Prefer player's existing position if it can be reclaimed from empty/Bot value.
                        int preferredPosition = player.Value.GetCustomProperty<int>(PhotonBattleRoom.PlayerPositionKey, PhotonBattleRoom.PlayerPositionGuest);
                        if (PhotonLobbyRoom.IsValidPlayerPos(preferredPosition))
                        {
                            string preferredKey = PhotonBattleRoom.GetPositionKey(preferredPosition);
                            string preferredValue = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(preferredKey, string.Empty);
                            if (string.IsNullOrEmpty(preferredValue) || preferredValue == "Bot" || preferredValue == player.Value.UserId)
                            {
                                position = preferredPosition;
                            }
                        }

                        // Fall back to side-effect-free room scanning to avoid VerifyPlayerPositions clearing bot slots.
                        if (!PhotonLobbyRoom.IsValidPlayerPos(position))
                        {
                            position = GetFirstFreePositionWithoutVerification(); // TODO: if Clan2v2 ensure that player ends on the correct side
                        }

                        if (!PhotonLobbyRoom.IsValidPlayerPos(position)) continue;
                        string positionKey = PhotonBattleRoom.GetPositionKey(position);

                        // Setting position to room and waiting until it's synced
                        if (PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(positionKey, string.Empty) != player.Value.UserId)
                        {
                            PhotonRealtimeClient.CurrentRoom.SetCustomProperty(positionKey, player.Value.UserId);
                            yield return new WaitUntil(() => PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(positionKey) == player.Value.UserId);
                        }
                    }

                    // Setting position to player properties and waiting until it's synced
                    if (player.Value.GetCustomProperty<int>(PhotonBattleRoom.PlayerPositionKey, PhotonBattleRoom.PlayerPositionGuest) != position)
                    {
                        player.Value.SetCustomProperty(PhotonBattleRoom.PlayerPositionKey, position);
                        yield return new WaitUntil(() => player.Value.GetCustomProperty<int>(PhotonBattleRoom.PlayerPositionKey) == position);
                    }
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

                // If botfill is active, reconcile any still-empty slots to Bot before start check.
                bool botFillActive = false;
                try { botFillActive = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<bool>(PhotonBattleRoom.BotFillKey, false); }
                catch (Exception ex) { Debug.LogWarning($"WaitForMatchmakingPlayers: failed to read BotFillKey before start check: {ex.Message}"); }

                // Bot-fill reconcile disabled.
                /*
                if (roomGameType == GameType.Random2v2 && botFillActive)
                {
                    int[] positions = {
                        PhotonBattleRoom.PlayerPosition1, PhotonBattleRoom.PlayerPosition2,
                        PhotonBattleRoom.PlayerPosition3, PhotonBattleRoom.PlayerPosition4
                    };
                    foreach (int pos in positions)
                    {
                        if (PhotonBattleRoom.CheckIfPositionIsFree(pos))
                        {
                            PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.GetPositionKey(pos), "Bot");
                        }
                    }
                }
                */

                // Starting gameplay coroutine if all positions are filled (real players + bots), else we loop again.
                // When botfill is active in Random2v2, allow start even if bot position replication is still catching up.
                int botCount = PhotonBattleRoom.GetBotCount();
                bool roomIsFullWithBots = PhotonRealtimeClient.CurrentRoom.PlayerCount + botCount >= PhotonRealtimeClient.CurrentRoom.MaxPlayers;
                bool canStartWithBotFill = roomGameType == GameType.Random2v2 && botFillActive;
                if (roomIsFullWithBots || canStartWithBotFill)
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

        // Follower safety-net: if countdown does not begin soon after joining matchmaking,
        // leave and requeue to avoid getting stuck in a stale room.
        private IEnumerator MatchmakingJoinWatcher(GameType gameType, float timeoutSeconds)
        {
            try
            {
                Debug.Log($"MatchmakingJoinWatcher: started for gameType={gameType}, timeout={timeoutSeconds}s");
                float start = Time.time;
                while (Time.time - start < timeoutSeconds)
                {
                    if (!PhotonRealtimeClient.InRoom || PhotonRealtimeClient.CurrentRoom == null) yield break;

                    // Queue-formed rooms use master-side expected-user timeout handling; follower watcher must not force requeue.
                    try
                    {
                        bool queueFormedMatch = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<bool>(QueueFormedMatchKey, false);
                        int expectedFollowers = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>("qe", 0);
                        string[] expectedUsers = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string[]>("eu", null);
                        string[] photonExpectedUsers = PhotonRealtimeClient.CurrentRoom.ExpectedUsers;
                        bool hasExpectedUsers = expectedUsers != null && expectedUsers.Any(uid => !string.IsNullOrEmpty(uid));
                        bool hasPhotonExpectedUsers = photonExpectedUsers != null && photonExpectedUsers.Any(uid => !string.IsNullOrEmpty(uid));
                        if (queueFormedMatch || expectedFollowers > 0 || hasExpectedUsers || hasPhotonExpectedUsers)
                        {
                            Debug.Log("MatchmakingJoinWatcher: queue-formed expected-user flow detected, stopping follower watcher.");
                            _autoRequeueAttempts = 0;
                            yield break;
                        }
                    }
                    catch { }

                    // If countdown started after we began watching, cancel watcher
                    if (_lastCountdownStartTime >= start)
                    {
                        _autoRequeueAttempts = 0;
                        yield break;
                    }

                    yield return new WaitForSeconds(0.5f);
                }

                // Timeout reached: countdown did not start; leave and requeue
                GameType requeueGameType = GameType.Random2v2;
                try { requeueGameType = (GameType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey); } catch (Exception ex) { Debug.LogWarning($"MatchmakingJoinWatcher: failed to read game type: {ex.Message}"); }
                Debug.Log($"MatchmakingJoinWatcher: countdown did not start within {timeoutSeconds}s in room '{PhotonRealtimeClient.CurrentRoom?.Name}'; leaving and requeueing for {requeueGameType}.");

                _autoRequeueAttempts++;
                if (MaxAutoRequeueAttempts > 0 && _autoRequeueAttempts > MaxAutoRequeueAttempts)
                {
                    Debug.LogWarning($"MatchmakingJoinWatcher: exceeded max auto-requeue attempts ({MaxAutoRequeueAttempts}); not requeueing.");
                    yield break;
                }

                yield return new WaitForSeconds(MatchmakingRequeueDelaySeconds);
                try
                {
                    StartCoroutine(LeaveAndAutoRequeue(requeueGameType));
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"MatchmakingJoinWatcher: failed to start LeaveAndAutoRequeue: {ex.Message}");
                }
            }
            finally
            {
                _joinTimeoutWatcherHolder = null;
            }
        }

        /// <summary>
        /// Follower-side room handoff after RoomChangeRequested.
        /// Join priority: explicit leader room name -> leader via friend lookup -> best matchmaking fallback -> join/create fallback.
        /// </summary>
        private IEnumerator FollowLeaderToNewRoom(string leaderUserId, string leaderRoomName = null, string[] expectedUsersOverride = null)
        {
            try
            {
                string localUserId = PhotonRealtimeClient.LocalPlayer?.UserId;
                string[] followTeammates = expectedUsersOverride != null
                    ? expectedUsersOverride
                        .Where(uid => !string.IsNullOrEmpty(uid) && uid != localUserId)
                        .Distinct(StringComparer.Ordinal)
                        .ToArray()
                    : _teammates;
                bool queueRoomRequested = !string.IsNullOrEmpty(leaderRoomName) && leaderRoomName.StartsWith("Queue_", StringComparison.Ordinal);

                // Duplicate handoff events are common during queue->match transitions.
                // If we are already in the explicitly requested room, ignore the handoff.
                if (!string.IsNullOrEmpty(leaderRoomName)
                    && PhotonRealtimeClient.InRoom
                    && PhotonRealtimeClient.CurrentRoom != null
                    && PhotonRealtimeClient.CurrentRoom.Name == leaderRoomName)
                {
                    Debug.Log($"FollowLeaderToNewRoom: already in target room '{leaderRoomName}', ignoring duplicate handoff.");
                    yield break;
                }

                if (string.IsNullOrEmpty(leaderRoomName) && IsInQueueFormedExpectedUserMatchmakingFlow())
                {
                    Debug.Log("FollowLeaderToNewRoom: ignoring stale targeted no-room handoff while already in queue-formed expected-user matchmaking flow.");
                    yield break;
                }

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
                catch (Exception ex) { Debug.LogWarning($"FollowLeaderToNewRoom: failed to evaluate room type: {ex.Message}"); }

                string oldRoomName = PhotonRealtimeClient.CurrentRoom?.Name ?? string.Empty;

                // Leave current room and wait until in lobby
                if (PhotonRealtimeClient.InRoom) PhotonRealtimeClient.LeaveRoom();
                yield return new WaitUntil(() => PhotonRealtimeClient.InLobby);

                // If leaderRoomName provided, try to join it directly
                bool newRoomJoined = false;
                if (!string.IsNullOrEmpty(leaderRoomName))
                {
                    if (PhotonRealtimeClient.InLobby)
                    {
                        int joinAttemptId = BeginJoinAttempt(leaderRoomName, followTeammates);
                        Debug.Log($"FollowLeaderToNewRoom: JoinAttempt[{joinAttemptId}] direct join requested: {leaderRoomName}");
                        try { PhotonRealtimeClient.JoinRoom(leaderRoomName); } catch (Exception ex) { Debug.LogWarning($"FollowLeaderToNewRoom: JoinAttempt[{joinAttemptId}] JoinRoom threw: {ex.Message}"); MarkJoinAttemptFailure(joinAttemptId, -1, ex.Message); }
                        float joinStartDirect = Time.time;
                        while (!PhotonRealtimeClient.InRoom && Time.time - joinStartDirect < 6f)
                        {
                            bool attemptCompleted = false;
                            lock (_joinAttemptsLock) { attemptCompleted = _joinAttempts.TryGetValue(joinAttemptId, out var a) && a.Completed; }
                            if (attemptCompleted) break;
                            yield return null;
                        }
                        if (PhotonRealtimeClient.InRoom)
                        {
                            Debug.Log($"FollowLeaderToNewRoom: JoinAttempt[{joinAttemptId}] succeeded, joined '{PhotonRealtimeClient.CurrentRoom?.Name}'");
                            newRoomJoined = true;
                        }
                        else
                        {
                            bool failed = false; string failMsg = null;
                            lock (_joinAttemptsLock) { if (_joinAttempts.TryGetValue(joinAttemptId, out var a) && a.Completed && !a.Success) { failed = true; failMsg = a.FailureMessage; } }
                            if (failed) Debug.LogWarning($"FollowLeaderToNewRoom: JoinAttempt[{joinAttemptId}] failed joining '{leaderRoomName}': {failMsg}");
                            else Debug.LogWarning($"FollowLeaderToNewRoom: JoinAttempt[{joinAttemptId}] timed out joining '{leaderRoomName}'");
                        }
                    }

                    // Queue room may not be visible immediately after leader leaves old room.
                    // Retry joins and then fall back to JoinOrCreateQueueRoom so follower reliably converges.
                    if (!newRoomJoined && queueRoomRequested && PhotonRealtimeClient.InLobby)
                    {
                        float queueRetryStart = Time.time;
                        while (!newRoomJoined && Time.time - queueRetryStart < 8f)
                        {
                                try
                                {
                                    int retryJoinAttemptId = BeginJoinAttempt(leaderRoomName, followTeammates);
                                    Debug.Log($"FollowLeaderToNewRoom: JoinAttempt[{retryJoinAttemptId}] retry joining queue room: {leaderRoomName}");
                                    PhotonRealtimeClient.JoinRoom(leaderRoomName);
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogWarning($"FollowLeaderToNewRoom: queue JoinRoom retry failed: {ex.Message}");
                                }

                            float retryJoinWaitStart = Time.time;
                            while (!PhotonRealtimeClient.InRoom && Time.time - retryJoinWaitStart < 2.5f)
                            {
                                yield return null;
                            }

                            if (PhotonRealtimeClient.InRoom)
                            {
                                newRoomJoined = true;
                                break;
                            }

                            yield return new WaitForSeconds(0.4f);
                        }

                            if (!newRoomJoined)
                            {
                            GameType queueGameType = GameType.Random2v2;
                            try
                            {
                                if (leaderRoomName.StartsWith("Queue_", StringComparison.Ordinal)
                                    && Enum.TryParse(leaderRoomName.Substring("Queue_".Length), out GameType parsedQueueType))
                                {
                                    queueGameType = parsedQueueType;
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning($"FollowLeaderToNewRoom: queue game type parse failed: {ex.Message}");
                            }

                            bool joinedOrCreatedQueue = false;
                            try
                            {
                                    int joinOrCreateId = BeginJoinAttempt($"Queue_{queueGameType}", followTeammates);
                                    Debug.Log($"FollowLeaderToNewRoom: JoinAttempt[{joinOrCreateId}] JoinOrCreateQueueRoom({queueGameType})");
                                    joinedOrCreatedQueue = PhotonRealtimeClient.JoinOrCreateQueueRoom(queueGameType);
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning($"FollowLeaderToNewRoom: JoinOrCreateQueueRoom fallback failed: {ex.Message}");
                            }

                            if (joinedOrCreatedQueue)
                            {
                                float queueJoinStart = Time.time;
                                while (!PhotonRealtimeClient.InRoom && Time.time - queueJoinStart < 6f)
                                {
                                    yield return null;
                                }
                                if (PhotonRealtimeClient.InRoom)
                                {
                                    newRoomJoined = true;
                                }
                            }
                        }
                    }
                }

                // Try to find leader via friends list; fallback to joining the matchmaking room with most players
                int attempts = 0;
                while (!newRoomJoined && attempts < 10 && !queueRoomRequested)
                {
                    attempts++;
                    _friendList = null;
                    // Only call OpFindFriends when the client is connected and the leader is not this client.
                    if (PhotonRealtimeClient.Client != null && PhotonRealtimeClient.Client.IsConnectedAndReady &&
                        !string.IsNullOrEmpty(leaderUserId) && PhotonRealtimeClient.LocalPlayer != null && leaderUserId != PhotonRealtimeClient.LocalPlayer.UserId)
                    {
                        PhotonRealtimeClient.Client.OpFindFriends(new string[1] { leaderUserId });
                        float friendLookupStart = Time.time;
                        while (_friendList == null && Time.time - friendLookupStart < 4f)
                        {
                            yield return null;
                        }
                        if (_friendList == null)
                        {
                            _friendList = new List<FriendInfo>();
                        }
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
                                catch (Exception ex) { Debug.LogWarning($"FollowLeaderToNewRoom: reading room properties failed: {ex.Message}"); continue; }

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
                }

                // If we couldn't join the leader's room or any candidate, create/join a new matchmaking room
                bool attemptedFollowJoinCreate = false;
                try
                {
                    if (!newRoomJoined && PhotonRealtimeClient.InLobby)
                    {
                        if (queueRoomRequested)
                        {
                            GameType queueGameType = GameType.Random2v2;
                            if (!string.IsNullOrEmpty(leaderRoomName)
                                && leaderRoomName.StartsWith("Queue_", StringComparison.Ordinal)
                                && Enum.TryParse(leaderRoomName.Substring("Queue_".Length), out GameType parsedQueueType))
                            {
                                queueGameType = parsedQueueType;
                            }
                            Debug.Log($"FollowLeaderToNewRoom: failed initial queue join, using JoinOrCreateQueueRoom for {queueGameType}.");
                            PhotonRealtimeClient.JoinOrCreateQueueRoom(queueGameType);
                        }
                        else
                        {
                            Debug.Log("FollowLeaderToNewRoom: failed to join leader room; rejoining queue instead of creating a matchmaking room directly.");
                            try
                            {
                                int joinOrCreateId = BeginJoinAttempt($"Queue_{_currentMatchmakingGameType}", followTeammates);
                                Debug.Log($"FollowLeaderToNewRoom: JoinAttempt[{joinOrCreateId}] JoinOrCreateQueueRoom({_currentMatchmakingGameType})");
                                PhotonRealtimeClient.JoinOrCreateQueueRoom(_currentMatchmakingGameType);
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning($"FollowLeaderToNewRoom: JoinOrCreateQueueRoom failed: {ex.Message}");
                            }
                        }

                        attemptedFollowJoinCreate = true;
                    }
                }
                catch (Exception ex) { Debug.LogWarning($"FollowLeaderToNewRoom: unexpected error: {ex.Message}"); }

                if (attemptedFollowJoinCreate)
                {
                    // Wait for the most recent join attempt to complete or timeout
                    int observedAttemptId = 0;
                    lock (_joinAttemptsLock) { observedAttemptId = _currentJoinAttemptId; }
                    Debug.Log($"FollowLeaderToNewRoom: awaiting JoinAttempt[{observedAttemptId}] result...");
                    float joinStart = Time.time;
                    while (!PhotonRealtimeClient.InRoom && Time.time - joinStart < 5f)
                    {
                        bool attemptCompleted = false;
                        lock (_joinAttemptsLock) { attemptCompleted = observedAttemptId != 0 && _joinAttempts.TryGetValue(observedAttemptId, out var a) && a.Completed; }
                        if (attemptCompleted) break;
                        yield return null;
                    }
                    lock (_joinAttemptsLock) { Debug.Log($"FollowLeaderToNewRoom: JoinAttempt[{observedAttemptId}] finished. InRoom={PhotonRealtimeClient.InRoom}"); }
                    // If still not in a room, wait for MasterServer lobby and retry once
                    if (!PhotonRealtimeClient.InRoom)
                    {
                        float waitStart = Time.time;
                        while ((PhotonRealtimeClient.Client == null || PhotonRealtimeClient.Client.Server != ServerConnection.MasterServer || !PhotonRealtimeClient.InLobby) && Time.time - waitStart < 5f)
                        {
                            yield return null;
                        }

                        if (PhotonRealtimeClient.Client != null && PhotonRealtimeClient.Client.Server == ServerConnection.MasterServer && PhotonRealtimeClient.InLobby)
                        {
                            if (queueRoomRequested && !string.IsNullOrEmpty(leaderRoomName))
                            {
                                try
                                {
                                    GameType queueGameType = GameType.Random2v2;
                                    if (leaderRoomName.StartsWith("Queue_", StringComparison.Ordinal)
                                        && Enum.TryParse(leaderRoomName.Substring("Queue_".Length), out GameType parsedQueueType))
                                    {
                                        queueGameType = parsedQueueType;
                                    }
                                    PhotonRealtimeClient.JoinOrCreateQueueRoom(queueGameType);
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogWarning($"FollowLeaderToNewRoom: second queue JoinOrCreate failed: {ex.Message}");
                                }
                            }
                            else
                            {
                                try
                                {
                                    PhotonRealtimeClient.JoinOrCreateQueueRoom(_currentMatchmakingGameType);
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogWarning($"FollowLeaderToNewRoom: second queue JoinOrCreate failed: {ex.Message}");
                                }
                            }

                            float joinStart2 = Time.time;
                            while (!PhotonRealtimeClient.InRoom && Time.time - joinStart2 < 5f)
                            {
                                yield return null;
                            }
                        }
                    }
                }
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

        #region Game Start & Quantum

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

            try
            {
                ClientState clientState = PhotonRealtimeClient.Client != null ? PhotonRealtimeClient.Client.State : ClientState.Disconnected;
                if (clientState != ClientState.Joined)
                {
                    Debug.Log($"OnStartMatchmakingEvent: ignoring start while client state is {clientState}.");
                    return;
                }
            }
            catch { }

            if (Time.time - _lastStartMatchmakingAcceptedTime < 1.0f)
            {
                Debug.Log("OnStartMatchmakingEvent: ignored duplicate start (debounce).");
                return;
            }

            try
            {
                if (PhotonRealtimeClient.CurrentRoom != null
                    && PhotonRealtimeClient.CurrentRoom.CustomProperties != null
                    && PhotonRealtimeClient.CurrentRoom.GetCustomProperty<bool>(PhotonBattleRoom.IsQueueKey, false))
                {
                    Debug.Log("OnStartMatchmakingEvent: already in queue room, ignoring duplicate start request.");
                    return;
                }
            }
            catch { }

            if (_matchmakingHolder != null)
            {
                Debug.Log("OnStartMatchmakingEvent: matchmaking already in progress, ignoring duplicate start request.");
                return;
            }

            // In premade in-room flow only the room master should start matchmaking.
            if (data.IsPremadeInRoom && PhotonRealtimeClient.LocalPlayer != null && !PhotonRealtimeClient.LocalPlayer.IsMasterClient)
            {
                Debug.Log("OnStartMatchmakingEvent: ignoring premade start from non-master client.");
                return;
            }

            _isPremadeMatchmakingFlow = data.IsPremadeInRoom;
            if (!_isPremadeMatchmakingFlow)
            {
                _premadeTeammateUserId = string.Empty;
            }

            if (_isPremadeMatchmakingFlow && PhotonRealtimeClient.CurrentRoom != null)
            {
                try
                {
                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeModeKey, true);
                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeTargetGameTypeKey, (int)data.SelectedGameType);
                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeLeaderUserIdKey, PhotonRealtimeClient.LocalPlayer.UserId);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"OnStartMatchmakingEvent: failed to set premade properties: {ex.Message}");
                }
            }

            // Starting matchmaking coroutine
            if (_matchmakingHolder == null)
            {
                _lastStartMatchmakingAcceptedTime = Time.time;
                _matchmakingHolder = StartCoroutine(StartMatchmaking(data.SelectedGameType));
            }
        }

        private void OnStopMatchmakingEvent(StopMatchmakingEvent data)
        {
            Debug.Log($"onEvent {data}");
            _isPremadeMatchmakingFlow = false;
            _premadeTeammateUserId = string.Empty;
            try
            {
                // If we're in a persistent queue room and the local player is the master,
                // transfer master to another real player instead of leaving so the queue stays alive.
                var currentRoom = PhotonRealtimeClient.CurrentRoom;
                bool isQueueRoom = currentRoom != null && currentRoom.CustomProperties != null && currentRoom.CustomProperties.ContainsKey(PhotonBattleRoom.IsQueueKey) && (currentRoom.CustomProperties[PhotonBattleRoom.IsQueueKey] is bool qb && qb);
                if (!data.ForceLeave && isQueueRoom && PhotonRealtimeClient.LocalPlayer != null && PhotonRealtimeClient.LocalPlayer.IsMasterClient)
                {
                    var others = PhotonRealtimeClient.PlayerListOthers;
                    // pick first other real player (has UserId and not a bot)
                    Player candidate = null;
                    foreach (var p in others)
                    {
                        if (p == null) continue;
                        if (string.IsNullOrEmpty(p.UserId)) continue;
                        // skip reserved "Bot" user ids
                        if (p.UserId == "Bot") continue;
                        candidate = p;
                        break;
                    }

                    if (candidate != null)
                    {
                        try
                        {
                            var lobbyPlayer = PhotonRealtimeClient.LobbyCurrentRoom.GetPlayer(candidate.ActorNumber);
                            if (lobbyPlayer != null && PhotonRealtimeClient.LobbyCurrentRoom.SetMasterClient(lobbyPlayer))
                            {
                                Debug.Log($"Queue: transferred master to {candidate.UserId} (actor {candidate.ActorNumber})");
                                // OnMasterClientSwitched will handle updating LeaderIdKey and UI on all clients.
                                return;
                            }
                            else
                            {
                                Debug.LogWarning("Queue: SetMasterClient returned false; falling back to normal leave.");
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogWarning($"Queue: failed to set new master: {ex.Message}");
                        }
                    }
                    else
                    {
                        Debug.Log("Queue: no suitable candidate found for master transfer; leaving matchmaking.");
                    }
                }

                // Only send RoomChangeRequested if we're connected to the Game server, ready and in a room.
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

                // Stop holder coroutines to clear runtime teammate/premade state immediately
                try { StopHolderCoroutines(); } catch (Exception ex) { Debug.LogWarning($"OnStopMatchmakingEvent: failed to stop holder coroutines: {ex.Message}"); }

                // If we are the master in a persistent queue room, clear stale premade metadata
                try
                {
                    var currentRoom2 = PhotonRealtimeClient.CurrentRoom;
                    if (currentRoom2 != null && PhotonRealtimeClient.LocalPlayer != null && PhotonRealtimeClient.LocalPlayer.IsMasterClient)
                    {
                        bool isQueueRoom2 = currentRoom2.CustomProperties != null && currentRoom2.CustomProperties.ContainsKey(PhotonBattleRoom.IsQueueKey) && (currentRoom2.CustomProperties[PhotonBattleRoom.IsQueueKey] is bool qb2 && qb2);
                        if (isQueueRoom2)
                        {
                            Debug.Log($"OnStopMatchmakingEvent: clearing premade metadata on queue room '{currentRoom2.Name}'");
                            currentRoom2.SetCustomProperty(PhotonBattleRoom.PremadeModeKey, false);
                            currentRoom2.SetCustomProperty(PhotonBattleRoom.PremadeUserId1Key, string.Empty);
                            currentRoom2.SetCustomProperty(PhotonBattleRoom.PremadeUserId2Key, string.Empty);
                            currentRoom2.SetCustomProperty(PhotonBattleRoom.PremadeLeaderUserIdKey, string.Empty);
                            currentRoom2.SetCustomProperty(PhotonBattleRoom.PremadeInviteStateKey, PhotonBattleRoom.PremadeInviteStateNone);
                        }
                    }
                }
                catch (Exception ex) { Debug.LogWarning($"OnStopMatchmakingEvent: failed to clear premade metadata: {ex.Message}"); }

                StartCoroutine(LeaveMatchmaking());
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"OnStopMatchmakingEvent: unexpected error: {ex.Message}");
                StartCoroutine(LeaveMatchmaking());
            }
        }

        private IEnumerator StartTheGameplay(bool isCloseRoom, string blueTeamName, string redTeamName)
        {
            try
            {
                // Do not start gameplay from a queue room; queue rooms are for waiting only.
                try
                {
                    var currentRoom = PhotonRealtimeClient.CurrentRoom;
                    if (currentRoom != null && currentRoom.CustomProperties != null && currentRoom.CustomProperties.ContainsKey(PhotonBattleRoom.IsQueueKey) && (currentRoom.CustomProperties[PhotonBattleRoom.IsQueueKey] is bool qb && qb))
                    {
                        Debug.Log("StartTheGameplay: aborting start because current room is a queue room.");
                        yield break;
                    }
                }
                catch (Exception ex) { Debug.LogWarning($"StartTheGameplay: failed to evaluate current room: {ex.Message}"); }
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

                        _lastStartCancelTime = Time.time;
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
                bool useCountdown = true;
                try
                {
                    GameType roomGameType = (GameType)room.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey);
                    useCountdown = roomGameType != GameType.Custom;
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
                        // Record cancel time so a quick rejoin can trigger a leader-led requeue
                        _lastStartCancelTime = Time.time;
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
                    _lastStartCancelTime = Time.time;
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
                Debug.LogException(ex);
                return false;
            }
            finally
            {
                try { pluginDisconnectListener?.Dispose(); } catch (Exception ex) { Debug.LogWarning($"StartRunner: failed to dispose pluginDisconnectListener: {ex.Message}"); }
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
        #endregion

        #region Photon Callbacks

        private void OnGetKickedEvent(GetKickedEvent data)
        {
            try
            {
                if (data.Reason == GetKickedEvent.ReasonType.FullRoom)
                {
                    GameType requeueGameType = _currentMatchmakingGameType;
                    try
                    {
                        if (PhotonRealtimeClient.CurrentRoom != null && PhotonRealtimeClient.CurrentRoom.CustomProperties != null
                            && PhotonRealtimeClient.CurrentRoom.CustomProperties.ContainsKey(PhotonBattleRoom.GameTypeKey))
                        {
                            requeueGameType = (GameType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey);
                        }
                    }
                    catch (Exception ex) { Debug.LogWarning($"OnGetKickedEvent: failed to read room game type: {ex.Message}"); }

                    if (requeueGameType == GameType.Random2v2 || requeueGameType == GameType.Clan2v2)
                    {
                        Debug.Log($"OnGetKickedEvent: room full, auto-requeueing for {requeueGameType}.");
                        StartCoroutine(LeaveAndAutoRequeue(requeueGameType));
                        return;
                    }
                }
            }
            catch (Exception ex) { Debug.LogWarning($"OnGetKickedEvent: auto-requeue failed: {ex.Message}"); }

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

            if (_gamePlayedOut)
            {
                Debug.Log("OnPlayerLeftRoom ignored because game has already played out.");
                return;
            }

            if (PhotonRealtimeClient.Client.State == ClientState.Leaving) return;

            // If a game start countdown or start flow is in progress, cancel it.
            bool startCountdownInProgress = !_matchHasStartedInCurrentRoom && IsGameStartTransitionActive();

            if (startCountdownInProgress)
            {
                // If a player leaves during countdown, force all players to leave and requeue
                GameType currentRoomGameType = GameType.Random2v2;
                try
                {
                    if (PhotonRealtimeClient.CurrentRoom != null)
                    {
                        currentRoomGameType = (GameType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey);
                    }
                }
                catch (Exception ex) { Debug.LogWarning($"OnPlayerLeftRoom: failed to read current room game type: {ex.Message}"); }

                // If this is a Custom game, keep existing behavior (do not force requeue)
                bool isCustomRoom = false;
                try
                {
                    if (PhotonRealtimeClient.CurrentRoom != null && PhotonRealtimeClient.CurrentRoom.CustomProperties != null
                        && PhotonRealtimeClient.CurrentRoom.CustomProperties.ContainsKey(PhotonBattleRoom.GameTypeKey))
                    {
                        isCustomRoom = (GameType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey) == GameType.Custom;
                    }
                }
                catch (Exception ex) { Debug.LogWarning($"OnPlayerLeftRoom: failed to determine if room is Custom: {ex.Message}"); }

                if (!isCustomRoom)
                {
                    // Broadcast CancelGameStart with requeue instruction so clients know to requeue
                    _lastStartCancelTime = Time.time;
                    SafeRaiseEvent(
                        PhotonRealtimeClient.PhotonEvent.CancelGameStart,
                        new object[] { true, (int)currentRoomGameType },
                        new RaiseEventArgs { Receivers = ReceiverGroup.All },
                        SendOptions.SendReliable
                    );

                    // Stop any local start coroutines and matchmaking holders
                    try { if (_startGameHolder != null) { StopCoroutine(_startGameHolder); _startGameHolder = null; } } catch (Exception ex) { Debug.LogWarning($"OnPlayerLeftRoom: failed to stop _startGameHolder: {ex.Message}"); }
                    try { if (_startQuantumHolder != null) { StopCoroutine(_startQuantumHolder); _startQuantumHolder = null; } } catch (Exception ex) { Debug.LogWarning($"OnPlayerLeftRoom: failed to stop _startQuantumHolder: {ex.Message}"); }
                    try { StopMatchmakingCoroutines(); } catch (Exception ex) { Debug.LogWarning($"OnPlayerLeftRoom: failed to stop matchmaking coroutines: {ex.Message}"); }

                    OnGameStartCancelled?.Invoke();

                    // All clients should leave and requeue (master will handle creating new room)
                    try
                    {
                        if (PhotonRealtimeClient.InMatchmakingRoom)
                        {
                            StartCoroutine(LeaveAndAutoRequeue(currentRoomGameType));
                        }
                        else
                        {
                            StartingGameFailed();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Failed to initiate LeaveAndAutoRequeue: {ex.Message}");
                    }
                }
                else
                {
                    // Preserve original behavior for Custom rooms
                    if (PhotonRealtimeClient.LocalPlayer.IsMasterClient)
                    {
                        if (_startGameHolder != null)
                        {
                            StopCoroutine(_startGameHolder);
                            _startGameHolder = null;
                        }

                        if (PhotonRealtimeClient.InMatchmakingRoom)
                        {
                            if (_matchmakingHolder != null)
                            {
                                StopCoroutine(_matchmakingHolder);
                                _matchmakingHolder = null;
                            }
                            _matchmakingHolder = StartCoroutine(WaitForMatchmakingPlayers());
                        }
                        else
                        {
                            StartingGameFailed();
                        }
                    }
                    else
                    {
                        if (_startQuantumHolder != null)
                        {
                            StopCoroutine(_startQuantumHolder);
                            _startQuantumHolder = null;
                        }
                        OnGameStartCancelled?.Invoke();
                        try { StopMatchmakingCoroutines(); } catch (Exception ex) { Debug.LogWarning($"CancelGameStart: StopMatchmakingCoroutines failed: {ex.Message}"); }
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
                        try
                        {
                            if (otherPlayer != null && otherPlayer.IsMasterClient)
                            {
                                _deferReturnToLobbyRoomOnMasterSwitch = true;
                            }
                            else
                            {
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

                if (CanMutateRoomPropertiesNow("OnPlayerLeftRoom: clear leaving player slot", true) && PhotonRealtimeClient.LobbyCurrentRoom != null)
                {
                    PhotonRealtimeClient.LobbyCurrentRoom.SetCustomProperties(emptyPosition, expectedValue);
                }
                if(_posChangeQueue.Contains(otherPlayer.UserId)) _posChangeQueue.Remove(otherPlayer.UserId);
            }

            if (!_matchHasStartedInCurrentRoom && PhotonRealtimeClient.InMatchmakingRoom && _followLeaderHolder == null)
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

            // Reset auto-requeue attempts if enough human players are present
            try
            {
                if (PhotonRealtimeClient.InMatchmakingRoom && PhotonRealtimeClient.CurrentRoom != null)
                {
                    int humanPlayers = PhotonRealtimeClient.CurrentRoom.Players.Values.Count(p => !string.IsNullOrEmpty(p.UserId) && p.UserId != "Bot");
                    if (humanPlayers >= 4) _autoRequeueAttempts = 0;
                }
            }
            catch (Exception ex) { Debug.LogWarning($"LobbyManager: caught exception: {ex.Message}"); }

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
                    if (room != null && CanMutateRoomPropertiesNow("OnPlayerLeftRoom: clear stale positions", true))
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

                // Queue match formation is centralized in QueueTimerCoroutine.
                try
                {
                    Room qroom = PhotonRealtimeClient.CurrentRoom;
                    if (qroom != null && qroom.CustomProperties != null && qroom.CustomProperties.ContainsKey(PhotonBattleRoom.IsQueueKey) && (qroom.CustomProperties[PhotonBattleRoom.IsQueueKey] is bool qb && qb))
                    {
                        if (PhotonRealtimeClient.LocalPlayer.IsMasterClient)
                        {
                            StartQueueTimer();
                        }
                    }
                }
                catch (Exception ex) { Debug.LogWarning($"LobbyManager: caught exception: {ex.Message}"); }
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
            _gamePlayedOut = false;
            _matchHasStartedInCurrentRoom = false;
            _countdownActive = false;
            _lastCountdownStartTime = -100f;
            _pendingInRoomInviteRoomName = string.Empty;
            _pendingAcceptedInRoomInviteRoomName = string.Empty;
            _pendingAcceptedInRoomInviteStartTime = -100f;

            // Correlate OnJoinedRoom to any outstanding join attempt and mark it succeeded.
            try
            {
                string joinedRoomName = PhotonRealtimeClient.CurrentRoom?.Name;
                int matchedId = 0;
                lock (_joinAttemptsLock)
                {
                    if (_currentJoinAttemptId != 0 && _joinAttempts.TryGetValue(_currentJoinAttemptId, out var cur) && cur.RoomName == joinedRoomName)
                    {
                        matchedId = _currentJoinAttemptId;
                    }
                    else
                    {
                        foreach (var kvp in _joinAttempts)
                        {
                            if (kvp.Value.RoomName == joinedRoomName)
                            {
                                matchedId = kvp.Key;
                                break;
                            }
                        }
                    }
                }
                // If we found a matching attempt, mark success; otherwise mark current attempt if present.
                if (matchedId != 0) MarkJoinAttemptSuccess(matchedId);
                else MarkJoinAttemptSuccess(0);
            }
            catch { }

            bool preserveQueuePendingSignals = false;
            try
            {
                Room joinedRoom = PhotonRealtimeClient.CurrentRoom;
                preserveQueuePendingSignals = IsQueueRoom(joinedRoom) || IsQueueFormedExpectedUserFlowRoom(joinedRoom);
            }
            catch { }

            if (!preserveQueuePendingSignals)
            {
                _queuePendingLeaderUntil.Clear();
                _queuePendingExpectedUserUntil.Clear();
            }

            // Enable: PhotonNetwork.CloseConnection needs to to work across all clients - to kick off invalid players!
            PhotonRealtimeClient.EnableCloseConnection = true;

            // Getting info if room is matchmaking room, queue room or a normal lobby room
            // If this is a persistent queue room, show matchmaking UI so player can leave the queue.
            try
            {
                var currentRoom = PhotonRealtimeClient.CurrentRoom;
                if (currentRoom != null && currentRoom.CustomProperties != null && currentRoom.CustomProperties.ContainsKey(PhotonBattleRoom.IsQueueKey) && (currentRoom.CustomProperties[PhotonBattleRoom.IsQueueKey] is bool qb && qb))
                {
                    bool isLeader = PhotonRealtimeClient.LocalPlayer != null && PhotonRealtimeClient.LocalPlayer.IsMasterClient;
                    OnMatchmakingRoomEntered?.Invoke(isLeader);
                    // Start verify loop for master so stale reservations are cleared in queue as well
                    if (PhotonRealtimeClient.LocalPlayer != null && PhotonRealtimeClient.LocalPlayer.IsMasterClient && _verifyPositionsHolder == null)
                    {
                        _verifyPositionsHolder = StartCoroutine(VerifyRoomPositionsLoop());
                    }
                    // Start queue timer for leader: after QueueWaitSeconds form a match from queue
                    if (isLeader)
                    {
                        StartQueueTimer();
                    }
                    return;
                }
            }
            catch { }

            try
            {
                if (PhotonRealtimeClient.CurrentRoom != null && PhotonRealtimeClient.CurrentRoom.CustomProperties != null
                    && PhotonRealtimeClient.CurrentRoom.CustomProperties.ContainsKey(PhotonBattleRoom.GameTypeKey)
                    && (GameType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey) == GameType.FriendLobby)
                {
                    string invitedUserId = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PremadeInvitedUserIdKey, string.Empty);
                    if (!string.IsNullOrEmpty(invitedUserId) && invitedUserId == PhotonRealtimeClient.LocalPlayer.UserId)
                    {
                        PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeInviteStateKey, PhotonBattleRoom.PremadeInviteStateAccepted);
                        PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeUserId2Key, invitedUserId);
                    }
                }
            }
            catch (Exception ex) { Debug.LogWarning($"OnJoinedRoom: failed to update FriendLobby invite state: {ex.Message}"); }

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
                catch (Exception ex) { Debug.LogWarning($"OnJoinedRoom: failed to clear LeaderIdKey: {ex.Message}"); }

                bool isLeader = PhotonRealtimeClient.LocalPlayer.UserId == PhotonRealtimeClient.LocalPlayer.GetCustomProperty<string>(PhotonBattleRoom.LeaderIdKey);
                OnMatchmakingRoomEntered?.Invoke(isLeader);

                // Start join watcher for non-master clients: if not enough human players join within timeout, auto-leave and requeue.
                try
                {
                    bool isQueueRoom = false;
                    bool queueExpectedJoinFlowActive = false;
                    try
                    {
                        var curr = PhotonRealtimeClient.CurrentRoom;
                        if (curr != null && curr.CustomProperties != null && curr.CustomProperties.ContainsKey(PhotonBattleRoom.IsQueueKey) && (curr.CustomProperties[PhotonBattleRoom.IsQueueKey] is bool qb && qb))
                        {
                            isQueueRoom = true;
                        }
                    }
                    catch (Exception ex) { Debug.LogWarning($"OnJoinedRoom: failed to check isQueueRoom: {ex.Message}"); }

                    if (!PhotonRealtimeClient.LocalPlayer.IsMasterClient && !isQueueRoom)
                    {
                        GameType roomGameType = GameType.Random2v2;
                        try { roomGameType = (GameType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey); } catch (Exception ex) { Debug.LogWarning($"OnJoinedRoom: failed to read room game type: {ex.Message}"); }
                        bool queueFormedMatch = false;
                        int expectedFollowers = 0;
                        string[] expectedUsers = null;
                        string[] photonExpectedUsers = null;
                        try { queueFormedMatch = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<bool>(QueueFormedMatchKey, false); } catch { }
                        try { expectedFollowers = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>("qe", 0); } catch { }
                        try { expectedUsers = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string[]>("eu", null); } catch { }
                        try { photonExpectedUsers = PhotonRealtimeClient.CurrentRoom.ExpectedUsers; } catch { }

                        bool hasExpectedUsers = expectedUsers != null && expectedUsers.Any(uid => !string.IsNullOrEmpty(uid));
                        bool hasPhotonExpectedUsers = photonExpectedUsers != null && photonExpectedUsers.Any(uid => !string.IsNullOrEmpty(uid));
                        if (!hasExpectedUsers && hasPhotonExpectedUsers)
                        {
                            expectedUsers = photonExpectedUsers;
                            hasExpectedUsers = true;
                        }
                        bool queueExpectedJoinFlowForWatcher = queueFormedMatch || expectedFollowers > 0 || hasExpectedUsers || hasPhotonExpectedUsers;
                        queueExpectedJoinFlowActive = queueExpectedJoinFlowForWatcher;

                        if (_joinTimeoutWatcherHolder != null) { StopCoroutine(_joinTimeoutWatcherHolder); _joinTimeoutWatcherHolder = null; }
                        if (!queueExpectedJoinFlowForWatcher)
                        {
                            float effectiveTimeout = MatchmakingJoinTimeoutSeconds;
                            Debug.Log($"OnJoinedRoom: non-master joined matchmaking room '{PhotonRealtimeClient.CurrentRoom?.Name}' with PlayerCount={PhotonRealtimeClient.CurrentRoom?.PlayerCount}, starting MatchmakingJoinWatcher(timeout={effectiveTimeout}s) (qe={PhotonRealtimeClient.CurrentRoom?.GetCustomProperty<int>("qe", -999)})");
                            _joinTimeoutWatcherHolder = StartCoroutine(MatchmakingJoinWatcher(roomGameType, effectiveTimeout));
                        }
                        else
                        {
                            Debug.Log($"OnJoinedRoom: non-master joined queue-formed matchmaking room '{PhotonRealtimeClient.CurrentRoom?.Name}' (qe={expectedFollowers}, euCount={(expectedUsers?.Length ?? 0)}, photonExpectedCount={(photonExpectedUsers?.Length ?? 0)}); skipping MatchmakingJoinWatcher.");
                        }
                        // Attempt to reserve a free position for non-master clients when no slot is already reserved.
                        try
                        {
                            string localUserId = PhotonRealtimeClient.LocalPlayer?.UserId ?? string.Empty;
                            int reservedPosition = GetReservedRoomPositionForUser(localUserId);

                            if (PhotonLobbyRoom.IsValidPlayerPos(reservedPosition))
                            {
                                int localPosition = PhotonRealtimeClient.LocalPlayer.HasCustomProperty(PlayerPositionKey)
                                    ? PhotonRealtimeClient.LocalPlayer.GetCustomProperty<int>(PlayerPositionKey)
                                    : PlayerPositionGuest;

                                if (localPosition != reservedPosition)
                                {
                                    PhotonRealtimeClient.LocalPlayer.SetCustomProperty(PlayerPositionKey, reservedPosition);
                                    Debug.Log($"OnJoinedRoom: synchronized local PlayerPositionKey to reserved room slot {reservedPosition}.");
                                }
                            }
                            else if (!PhotonRealtimeClient.LocalPlayer.HasCustomProperty(PlayerPositionKey) && _reserveFreePositionHolder == null)
                            {
                                _reserveFreePositionHolder = StartCoroutine(ReserveFreePosition(true));
                            }
                        }
                        catch (Exception ex) { Debug.LogWarning($"OnJoinedRoom: failed to ensure local position reservation: {ex.Message}"); }

                        // Diagnostic: snapshot room position keys and local player position after join
                        try
                        {
                            string sp1 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey1, string.Empty);
                            string sp2 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey2, string.Empty);
                            string sp3 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey3, string.Empty);
                            string sp4 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey4, string.Empty);
                            int localPos = PhotonRealtimeClient.LocalPlayer.GetCustomProperty<int>(PlayerPositionKey, PlayerPositionGuest);
                            Debug.Log($"OnJoinedRoom: join snapshot pos1={sp1},pos2={sp2},pos3={sp3},pos4={sp4}, localPlayerPos={localPos}");
                        }
                        catch (Exception ex) { Debug.LogWarning($"OnJoinedRoom: failed to log join snapshot: {ex.Message}"); }

                        if (queueExpectedJoinFlowActive)
                        {
                            // Queue-formed expected-user rooms can temporarily look sparse for followers; avoid false auto-requeue.
                            try { _autoRequeueAttempts = 0; } catch { }
                        }
                    }
                }
                catch (Exception ex) { Debug.LogWarning($"OnJoinedRoom: start join watcher failed: {ex.Message}"); }

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

                bool queueExpectedJoinFlow = false;
                try
                {
                    if (PhotonRealtimeClient.CurrentRoom != null)
                    {
                        bool queueFormedMatch = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<bool>(QueueFormedMatchKey, false);
                        int expectedFollowers = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>("qe", 0);
                        string[] expectedUsers = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string[]>("eu", null);
                        string[] photonExpectedUsers = PhotonRealtimeClient.CurrentRoom.ExpectedUsers;

                        bool hasExpectedUsers = expectedUsers != null && expectedUsers.Any(uid => !string.IsNullOrEmpty(uid));
                        bool hasPhotonExpectedUsers = photonExpectedUsers != null && photonExpectedUsers.Any(uid => !string.IsNullOrEmpty(uid));
                        queueExpectedJoinFlow = queueFormedMatch || expectedFollowers > 0 || hasExpectedUsers || hasPhotonExpectedUsers;
                    }
                }
                catch { }

                if (!PhotonRealtimeClient.LocalPlayer.IsMasterClient && PhotonRealtimeClient.CurrentRoom != null && PhotonRealtimeClient.CurrentRoom.PlayerCount <= 1 && !inCustomRoom && !queueExpectedJoinFlow)
                {
                    Debug.Log($"OnJoinedRoom: non-master appears alone in matchmaking room (PlayerCount={PhotonRealtimeClient.CurrentRoom.PlayerCount}); starting auto-requeue.");
                    if (_autoJoinHolder == null)
                    {
                        _autoJoinHolder = StartCoroutine(RequeueToPersistentQueue(_currentMatchmakingGameType));
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
        
        public void OnLeftRoom() // IMatchmakingCallbacks
        {
            _gamePlayedOut = false;
            _matchHasStartedInCurrentRoom = false;
            _countdownActive = false;
            _lastCountdownStartTime = -100f;
            _pendingInRoomInviteRoomName = string.Empty;
            _pendingAcceptedInRoomInviteRoomName = string.Empty;
            _pendingAcceptedInRoomInviteStartTime = -100f;
            _queuePendingExpectedUserUntil.Clear();
            _queuePendingLeaderUntil.Clear();

            // Clearing player position key from own custom properties
            if (PhotonRealtimeClient.LocalPlayer.HasCustomProperty(PlayerPositionKey)) PhotonRealtimeClient.LocalPlayer.RemoveCustomProperty(PlayerPositionKey);

            // If position change coroutine is running stopping it
            if (_requestPositionChangeHolder != null)
            {
                StopCoroutine(_requestPositionChangeHolder);
                _requestPositionChangeHolder = null;
            }

            Debug.Log($"OnLeftRoom {PhotonRealtimeClient.LocalPlayer.GetDebugLabel()}");
            if (_serviceHolder == null) _serviceHolder = StartCoroutine(Service());
            // Stop verify loop when leaving room
            if (_verifyPositionsHolder != null)
            {
                StopCoroutine(_verifyPositionsHolder);
                _verifyPositionsHolder = null;
            }
            // Stop queue timer when leaving any room
            try
            {
                StopQueueTimer();
            }
            catch (Exception ex) { Debug.LogWarning($"OnLeftRoom: StopQueueTimer failed: {ex.Message}"); }
            // Stop any join-timeout watcher
            if (_joinTimeoutWatcherHolder != null)
            {
                StopCoroutine(_joinTimeoutWatcherHolder);
                _joinTimeoutWatcherHolder = null;
            }

            if (_matchmakingHolder == null && _followLeaderHolder == null)
            {
                _isPremadeMatchmakingFlow = false;
                _premadeTeammateUserId = string.Empty;
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
            if (_serviceHolder == null) _serviceHolder = StartCoroutine(Service());

            if (_matchmakingHolder == null)
            {
                LobbyOnCreatedRoom?.Invoke();
            }
            else
            {
                OnMatchmakingRoomEntered?.Invoke(true);
            }
        }
        public void OnJoinedLobby() { if (_serviceHolder == null) _serviceHolder = StartCoroutine(Service()); LobbyOnJoinedLobby?.Invoke(); }

        public void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            List<LobbyRoomInfo> lobbyRoomList = new();
            foreach (RoomInfo roomInfo in roomList)
            {
                lobbyRoomList.Add(new(roomInfo));
            }

            if (!string.IsNullOrEmpty(_pendingAcceptedInRoomInviteRoomName)
                && !PhotonRealtimeClient.InRoom
                && _pendingAcceptedInRoomInviteStartTime > 0f
                && Time.time - _pendingAcceptedInRoomInviteStartTime > InRoomInviteJoinTimeoutSeconds)
            {
                string timedOutRoomName = _pendingAcceptedInRoomInviteRoomName;
                _pendingAcceptedInRoomInviteRoomName = string.Empty;
                _pendingAcceptedInRoomInviteStartTime = -100f;
                Debug.LogWarning($"Accepted InRoom invite join timed out for room '{timedOutRoomName}'.");
                OnInRoomInviteJoinFailed?.Invoke(timedOutRoomName, -1, "timeout");
            }

            LobbyOnRoomListUpdate?.Invoke(lobbyRoomList);
            TryAutoJoinInRoomInvite(lobbyRoomList);
        }

        private void TryAutoJoinInRoomInvite(List<LobbyRoomInfo> roomList)
        {
            if (roomList == null || roomList.Count == 0) return;
            if (!PhotonRealtimeClient.InLobby || PhotonRealtimeClient.InRoom) return;

            string localUserId = PhotonRealtimeClient.LocalPlayer?.UserId;
            if (string.IsNullOrEmpty(localUserId)) return;

            if (!string.IsNullOrEmpty(_pendingInRoomInviteRoomName)
                && roomList.All(room => room == null || room.RemovedFromList || room.Name != _pendingInRoomInviteRoomName))
            {
                _pendingInRoomInviteRoomName = string.Empty;
            }

            foreach (LobbyRoomInfo room in roomList)
            {
                if (room == null || room.RemovedFromList || !room.IsOpen || room.CustomProperties == null) continue;
                if (!room.CustomProperties.ContainsKey(PhotonBattleRoom.GameTypeKey)) continue;

                GameType gameType;
                try { gameType = (GameType)room.CustomProperties[PhotonBattleRoom.GameTypeKey]; }
                catch { continue; }

                if (gameType != GameType.FriendLobby) continue;

                string invitedUserId = room.CustomProperties.ContainsKey(PhotonBattleRoom.PremadeInvitedUserIdKey)
                    ? room.CustomProperties[PhotonBattleRoom.PremadeInvitedUserIdKey]?.ToString()
                    : string.Empty;
                if (invitedUserId != localUserId) continue;

                int inviteState = PhotonBattleRoom.PremadeInviteStateNone;
                if (room.CustomProperties.ContainsKey(PhotonBattleRoom.PremadeInviteStateKey))
                {
                    try { inviteState = Convert.ToInt32(room.CustomProperties[PhotonBattleRoom.PremadeInviteStateKey]); }
                    catch { inviteState = PhotonBattleRoom.PremadeInviteStateNone; }
                }
                if (inviteState != PhotonBattleRoom.PremadeInviteStatePending) continue;

                long inviteTimestampSeconds = 0;
                if (room.CustomProperties.ContainsKey(PhotonBattleRoom.PremadeInviteTimestampKey))
                {
                    try { inviteTimestampSeconds = Convert.ToInt64(room.CustomProperties[PhotonBattleRoom.PremadeInviteTimestampKey]); }
                    catch { inviteTimestampSeconds = 0; }
                }

                if (inviteTimestampSeconds > 0)
                {
                    long nowSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    if (nowSeconds - inviteTimestampSeconds > InRoomInviteValiditySeconds)
                    {
                        continue;
                    }
                }

                if (IsInRoomInviteDeclinedRecently(room.Name)) continue;
                if (_pendingInRoomInviteRoomName == room.Name) continue;

                if (_lastAutoInviteRoomName == room.Name && Time.time - _lastAutoInviteJoinTime < InRoomInvitePromptThrottleSeconds)
                {
                    continue;
                }

                string leaderUserId = room.CustomProperties.ContainsKey(PhotonBattleRoom.PremadeLeaderUserIdKey)
                    ? room.CustomProperties[PhotonBattleRoom.PremadeLeaderUserIdKey]?.ToString()
                    : string.Empty;

                GameType targetGameType = GameType.Random2v2;
                if (room.CustomProperties.ContainsKey(PhotonBattleRoom.PremadeTargetGameTypeKey))
                {
                    try { targetGameType = (GameType)Convert.ToInt32(room.CustomProperties[PhotonBattleRoom.PremadeTargetGameTypeKey]); }
                    catch { targetGameType = GameType.Random2v2; }
                }

                InRoomInviteReceived inviteReceivedHandler = OnInRoomInviteReceived;
                _lastAutoInviteRoomName = room.Name;
                _lastAutoInviteJoinTime = Time.time;

                if (inviteReceivedHandler == null)
                {
                    Debug.LogWarning($"Detected pending FriendLobby invite to room '{room.Name}', but no UI listener is active yet. Will retry shortly.");
                    break;
                }

                _pendingInRoomInviteRoomName = room.Name;
                Debug.Log($"Detected pending FriendLobby invite to room '{room.Name}', requesting decision from UI.");
                try
                {
                    inviteReceivedHandler.Invoke(new InRoomInviteInfo(room.Name, leaderUserId, invitedUserId, targetGameType));
                }
                catch (Exception ex)
                {
                    _pendingInRoomInviteRoomName = string.Empty;
                    Debug.LogError($"TryAutoJoinInRoomInvite: invite UI callback failed for room '{room.Name}': {ex.Message}");
                }
                break;
            }
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
            Debug.LogError($"JoinRoomFailed {returnCode} {message} (joinAttemptId={_joinAttemptCounter})");
            // Correlate this failure to the most recent join attempt
            try { MarkJoinAttemptFailure(0, returnCode, message); } catch { }

            bool failedInviteAcceptJoin = !string.IsNullOrEmpty(_pendingAcceptedInRoomInviteRoomName);
            string failedInviteRoomName = _pendingAcceptedInRoomInviteRoomName;

            bool isGameFull = false;
            try
            {
                if (!string.IsNullOrEmpty(message) && message.ToLower().Contains("game full")) isGameFull = true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"OnJoinRoomFailed: failed to inspect message: {ex.Message}");
            }
            if (!isGameFull && returnCode == 32765) isGameFull = true;

            if (isGameFull && !failedInviteAcceptJoin)
            {
                _joinFailureAutoRequeueInFlight = true;
            }

            try
            {
                LobbyOnJoinRoomFailed?.Invoke(returnCode, message);

                if (failedInviteAcceptJoin)
                {
                    _pendingAcceptedInRoomInviteRoomName = string.Empty;
                    _pendingAcceptedInRoomInviteStartTime = -100f;
                    Debug.LogWarning($"JoinRoomFailed during accepted InRoom invite join. Room='{failedInviteRoomName}', code={returnCode}, msg={message}");
                    OnInRoomInviteJoinFailed?.Invoke(failedInviteRoomName, returnCode, message);
                    return;
                }

                // If the failure is a full-game error, loop back to queue/requeue flow
                if (isGameFull)
                {
                    GameType requeueGameType = _currentMatchmakingGameType;
                    try
                    {
                        if (PhotonRealtimeClient.CurrentRoom != null && PhotonRealtimeClient.CurrentRoom.CustomProperties != null && PhotonRealtimeClient.CurrentRoom.CustomProperties.ContainsKey(PhotonBattleRoom.GameTypeKey))
                        {
                            requeueGameType = (GameType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey);
                        }
                    }
                    catch (Exception ex) { Debug.LogWarning($"OnJoinRoomFailed: failed to read current room game type: {ex.Message}"); }

                    try
                    {
                        Debug.Log($"JoinRoomFailed: game full, requeueing for {requeueGameType}");
                        StartCoroutine(LeaveAndAutoRequeue(requeueGameType));
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Failed to start LeaveAndAutoRequeue after JoinRoomFailed: {ex.Message}");
                    }
                }
            }
            catch (Exception ex) { Debug.LogWarning($"OnJoinRoomFailed: unexpected error: {ex.Message}"); }
            finally
            {
                _joinFailureAutoRequeueInFlight = false;
            }
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

                    // Record cancel time so rejoins shortly after can trigger leader-led requeue
                    try { _lastStartCancelTime = Time.time; } catch { }

                    // Clear any return-to-main flag; keep players in-room and restore pre-countdown UI.
                    _returnToMainMenuOnMatchmakingRejoin = false;
                    OnGameStartCancelled?.Invoke();
                    // Also signal countdown listeners with a sentinel value so older UI code can cancel
                    // clear countdown active flag and notify listeners
                    _countdownActive = false;
                    _matchHasStartedInCurrentRoom = false;
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
                            if (CanMutateRoomPropertiesNow("CancelGameStart: clear BattleID", true)
                                && PhotonRealtimeClient.CurrentRoom.CustomProperties.ContainsKey(PhotonBattleRoom.BattleID))
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
                    // Track countdown active state: >0 means active, <=0 or -1 means inactive/cancel
                    _countdownActive = countdown > 0;
                    if (countdown > 0)
                    {
                        _gamePlayedOut = false;
                        _lastCountdownStartTime = Time.time;
                        // Countdown started: reset auto-requeue attempts and stop any join watcher
                        _autoRequeueAttempts = 0;
                        try { if (_joinTimeoutWatcherHolder != null) { StopCoroutine(_joinTimeoutWatcherHolder); _joinTimeoutWatcherHolder = null; } } catch { }
                    }
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

                    QueueCustomBattleStartCheck();
                    break;

                case PhotonRealtimeClient.PhotonEvent.RoomChangeRequested:
                {
                    // Payload can be either a leaderUserId string, or an object[] { leaderUserId, expectedUsers[] }
                    string leaderUserId = string.Empty;
                    string[] expectedUsers = null;
                    string leaderRoomName = null;
                    try
                    {
                        // Local helper flattened here so all branches in this try can use it.
                        string[] FlattenExpected(object obj)
                        {
                            var list = new List<string>();
                            if (obj == null) return null;
                            try
                            {
                                if (obj is string ss)
                                {
                                    if (!string.IsNullOrEmpty(ss)) list.Add(ss);
                                    return list.ToArray();
                                }
                                if (obj is string[] ssarr)
                                {
                                    return ssarr.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                                }
                                if (obj is object[] oarr)
                                {
                                    foreach (var o in oarr)
                                    {
                                        var sub = FlattenExpected(o);
                                        if (sub != null && sub.Length > 0) list.AddRange(sub);
                                    }
                                    return list.Distinct().Where(x => !string.IsNullOrEmpty(x)).ToArray();
                                }
                                if (obj is System.Collections.IEnumerable ie)
                                {
                                    foreach (var o in ie)
                                    {
                                        var sub = FlattenExpected(o);
                                        if (sub != null && sub.Length > 0) list.AddRange(sub);
                                    }
                                    return list.Distinct().Where(x => !string.IsNullOrEmpty(x)).ToArray();
                                }
                                // Fallback: use ToString
                                string s = obj.ToString();
                                if (!string.IsNullOrEmpty(s)) list.Add(s);
                                return list.Distinct().Where(x => !string.IsNullOrEmpty(x)).ToArray();
                            }
                            catch { return null; }
                        }

                        if (photonEvent.CustomData is object[] arr && arr.Length > 0)
                        {
                            leaderUserId = arr[0]?.ToString() ?? string.Empty;

                            // Robustly handle multiple payload shapes for expected users.
                            // arr[1] may be a string[], an object[] of strings, a nested array, or even a single string when only one expected user.
                            object maybeExpected = arr.Length > 1 ? arr[1] : null;

                            expectedUsers = FlattenExpected(maybeExpected);

                            if (arr.Length > 2 && arr[2] is string rn)
                            {
                                // optional room name provided by leader
                                leaderRoomName = rn;
                            }
                        }
                        else if (photonEvent.CustomData is string s)
                        {
                            leaderUserId = s;
                        }
                        else if (photonEvent.CustomData is PhotonHashtable pht)
                        {
                            if (pht.ContainsKey("leader")) leaderUserId = pht["leader"].ToString();
                            if (pht.ContainsKey("expectedUsers"))
                            {
                                expectedUsers = FlattenExpected(pht["expectedUsers"]);
                            }
                        }
                    }
                    catch { }

                    string matchmakingLeaderId = string.Empty;
                    bool targetedByExpectedUsers = false;
                    try
                    {
                        if (expectedUsers != null && expectedUsers.Length > 0)
                        {
                            string localId = PhotonRealtimeClient.LocalPlayer?.UserId;
                            targetedByExpectedUsers = !string.IsNullOrEmpty(localId) && expectedUsers.Contains(localId);
                        }
                    }
                    catch { }

                    // If room is not a matchmaking room the person sending the event is the leader.
                    if (!PhotonRealtimeClient.InMatchmakingRoom)
                    {
                        if (!string.IsNullOrEmpty(leaderUserId))
                        {
                            _matchHasStartedInCurrentRoom = false;
                            string localUserId = PhotonRealtimeClient.LocalPlayer?.UserId;
                            bool localIsLeader = !string.IsNullOrEmpty(localUserId) && localUserId == leaderUserId;
                            bool shouldUpdateLeaderId = targetedByExpectedUsers || localIsLeader;
                            if (shouldUpdateLeaderId)
                            {
                                PhotonRealtimeClient.LocalPlayer.SetCustomProperty(PhotonBattleRoom.LeaderIdKey, leaderUserId);
                                try { OnRoomLeaderChanged?.Invoke(leaderUserId == PhotonRealtimeClient.LocalPlayer.UserId); } catch { }
                                matchmakingLeaderId = leaderUserId;
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(matchmakingLeaderId))
                    {
                        matchmakingLeaderId = PhotonRealtimeClient.LocalPlayer.GetCustomProperty(PhotonBattleRoom.LeaderIdKey, string.Empty);
                    }

                    Debug.Log($"RoomChangeRequested parsed: leaderUserId={leaderUserId}, matchmakingLeaderId={matchmakingLeaderId}, expectedUsersCount={(expectedUsers?.Length ?? 0)}, leaderRoomName={leaderRoomName}");

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

                    // If expectedUsers is provided, only follow if local user is in the list
                    bool shouldFollow = true;
                    try
                    {
                        if (expectedUsers != null && expectedUsers.Length > 0)
                        {
                            shouldFollow = targetedByExpectedUsers;
                        }
                    }
                    catch { }

                    // If current room is a queue room and leaving is disabled for testing, do not follow leader
                    bool isQueueRoom = false;
                    try
                    {
                        var curr = PhotonRealtimeClient.CurrentRoom;
                        if (curr != null && curr.CustomProperties != null && curr.CustomProperties.ContainsKey(PhotonBattleRoom.IsQueueKey) && (curr.CustomProperties[PhotonBattleRoom.IsQueueKey] is bool qb && qb))
                        {
                            isQueueRoom = true;
                        }
                    }
                    catch { }

                    bool leaderMatch = leaderUserId == matchmakingLeaderId;
                    bool hasExplicitLeaderRoom = !string.IsNullOrEmpty(leaderRoomName);
                    bool hasExpectedUsers = expectedUsers != null && expectedUsers.Length > 0;
                    bool alreadyInQueueFormedExpectedUserFlow = IsInQueueFormedExpectedUserMatchmakingFlow();
                    bool isQueueLeaderHandoff = isQueueRoom
                        && hasExplicitLeaderRoom
                        && hasExpectedUsers
                        && !string.IsNullOrEmpty(leaderUserId)
                        && leaderRoomName.StartsWith("Queue_", StringComparison.Ordinal);

                    if (isQueueLeaderHandoff)
                    {
                        _queuePendingLeaderUntil[leaderUserId] = Time.time + QueuePendingLeaderGraceSeconds;
                        int pendingExpectedAdded = 0;
                        try
                        {
                            HashSet<string> presentQueueUsers = new(
                                PhotonRealtimeClient.CurrentRoom.Players.Values
                                    .Where(p => p != null && !string.IsNullOrEmpty(p.UserId) && p.UserId != "Bot")
                                    .Select(p => p.UserId),
                                StringComparer.Ordinal);

                            foreach (string expectedUserId in expectedUsers)
                            {
                                if (string.IsNullOrEmpty(expectedUserId) || expectedUserId == leaderUserId) continue;
                                if (presentQueueUsers.Contains(expectedUserId)) continue;

                                _queuePendingExpectedUserUntil[expectedUserId] = Time.time + QueuePendingLeaderGraceSeconds;
                                pendingExpectedAdded++;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"RoomChangeRequested: failed to record pending expected queue users: {ex.Message}");
                        }

                        Debug.Log($"RoomChangeRequested: recorded pending queue duo handoff for leader {leaderUserId} (expectedUsers={expectedUsers.Length}, grace={QueuePendingLeaderGraceSeconds}s).");
                        if (pendingExpectedAdded > 0)
                        {
                            Debug.Log($"RoomChangeRequested: recorded {pendingExpectedAdded} unresolved expected queue users for pending duo handoff.");
                        }
                    }

                    string[] followExpectedUsers = null;
                    if (hasExpectedUsers && shouldFollow)
                    {
                        string localUserId = PhotonRealtimeClient.LocalPlayer?.UserId;
                        followExpectedUsers = expectedUsers
                            .Where(uid => !string.IsNullOrEmpty(uid) && uid != localUserId)
                            .Distinct(StringComparer.Ordinal)
                            .ToArray();
                    }
                    bool leaderOnlyNoRoomFallback = !hasExplicitLeaderRoom && !hasExpectedUsers;

                    bool alreadyInExplicitLeaderRoom = false;
                    if (hasExplicitLeaderRoom)
                    {
                        try
                        {
                            alreadyInExplicitLeaderRoom = PhotonRealtimeClient.InRoom
                                && PhotonRealtimeClient.CurrentRoom != null
                                && PhotonRealtimeClient.CurrentRoom.Name == leaderRoomName;
                        }
                        catch { }
                    }

                    Debug.Log($"RoomChangeRequested decision: isCustomRoom={isCustomRoom}, followHolderNull={_followLeaderHolder==null}, leaderMatch={leaderMatch}, shouldFollow={shouldFollow}, hasExplicitLeaderRoom={hasExplicitLeaderRoom}, hasExpectedUsers={hasExpectedUsers}");

                    if (!isCustomRoom && leaderOnlyNoRoomFallback && PhotonRealtimeClient.InMatchmakingRoom)
                    {
                        Debug.Log("RoomChangeRequested: ignoring leader-only no-room handoff while already in matchmaking room.");
                        break;
                    }

                    if (!isCustomRoom && shouldFollow && hasExpectedUsers && !hasExplicitLeaderRoom && alreadyInQueueFormedExpectedUserFlow)
                    {
                        Debug.Log("RoomChangeRequested: ignoring targeted no-room handoff while already in queue-formed expected-user matchmaking flow.");
                        break;
                    }

                    // Targeted handoff without explicit room is queue pre-notify.
                    // Start follow flow only from queue rooms; if already in matchmaking room,
                    // treat it as stale duplicate and ignore to avoid leave cascades.
                    if (!isCustomRoom && shouldFollow && hasExpectedUsers && !hasExplicitLeaderRoom)
                    {
                        if (isQueueRoom)
                        {
                            if (_followLeaderHolder != null)
                            {
                                try { StopCoroutine(_followLeaderHolder); } catch { }
                                _followLeaderHolder = null;
                                Debug.Log("RoomChangeRequested: replacing stale follow coroutine for targeted queue pre-notify handoff.");
                            }

                            // Record pending leader handoff for targeted queue pre-notify (no explicit room name)
                            try
                            {
                                _queuePendingLeaderUntil[leaderUserId] = Time.time + QueuePendingLeaderGraceSeconds;
                                int pendingExpectedAdded = 0;
                                HashSet<string> presentQueueUsers = new(
                                    PhotonRealtimeClient.CurrentRoom.Players.Values
                                        .Where(p => p != null && !string.IsNullOrEmpty(p.UserId) && p.UserId != "Bot")
                                        .Select(p => p.UserId),
                                    StringComparer.Ordinal);

                                if (expectedUsers != null)
                                {
                                    foreach (string expectedUserId in expectedUsers)
                                    {
                                        if (string.IsNullOrEmpty(expectedUserId) || expectedUserId == leaderUserId) continue;
                                        if (presentQueueUsers.Contains(expectedUserId)) continue;

                                        _queuePendingExpectedUserUntil[expectedUserId] = Time.time + QueuePendingLeaderGraceSeconds;
                                        pendingExpectedAdded++;
                                    }
                                }

                                Debug.Log($"RoomChangeRequested: recorded pending queue duo handoff for leader {leaderUserId} (expectedUsers={(expectedUsers?.Length ?? 0)}, grace={QueuePendingLeaderGraceSeconds}s).");
                                if (pendingExpectedAdded > 0)
                                {
                                    Debug.Log($"RoomChangeRequested: recorded {pendingExpectedAdded} unresolved expected queue users for pending duo handoff.");
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning($"RoomChangeRequested: failed to record pending expected queue users for targeted pre-notify: {ex.Message}");
                            }

                            Debug.Log("RoomChangeRequested: targeted queue pre-notify without explicit room name, starting immediate follow flow.");
                            _followLeaderHolder = StartCoroutine(FollowLeaderToNewRoom(leaderUserId, null, followExpectedUsers));
                        }
                        else
                        {
                            Debug.Log("RoomChangeRequested: ignored targeted no-room handoff outside queue room (stale duplicate).");
                        }
                    }
                    else if (!isCustomRoom && shouldFollow && (hasExplicitLeaderRoom || leaderMatch))
                    {
                        if (alreadyInExplicitLeaderRoom)
                        {
                            Debug.Log($"RoomChangeRequested: already in explicit leader room '{leaderRoomName}', ignoring duplicate handoff.");
                            break;
                        }

                        // Explicit room handoff has highest priority; replace any stale follow coroutine.
                        if (hasExplicitLeaderRoom && _followLeaderHolder != null)
                        {
                            try { StopCoroutine(_followLeaderHolder); } catch { }
                            _followLeaderHolder = null;
                        }

                        if (_followLeaderHolder == null)
                        {
                            if (hasExplicitLeaderRoom) _followLeaderHolder = StartCoroutine(FollowLeaderToNewRoom(leaderUserId, leaderRoomName, followExpectedUsers));
                            else _followLeaderHolder = StartCoroutine(FollowLeaderToNewRoom(leaderUserId, null, followExpectedUsers));
                        }
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

            try
            {
                if (room != null && room.CustomProperties != null && room.CustomProperties.ContainsKey(PhotonBattleRoom.GameTypeKey)
                    && (GameType)room.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey) == GameType.FriendLobby)
                {
                    string invitedUserId = room.GetCustomProperty<string>(PhotonBattleRoom.PremadeInvitedUserIdKey, string.Empty);
                    if (!string.IsNullOrEmpty(invitedUserId) && newPlayer != null && newPlayer.UserId == invitedUserId)
                    {
                        room.SetCustomProperty(PhotonBattleRoom.PremadeInviteStateKey, PhotonBattleRoom.PremadeInviteStateAccepted);
                        room.SetCustomProperty(PhotonBattleRoom.PremadeUserId2Key, invitedUserId);
                    }

                    if (PhotonRealtimeClient.LocalPlayer.IsMasterClient && room.PlayerCount >= room.MaxPlayers)
                    {
                        room.IsOpen = false;
                    }
                }
            }
            catch (Exception ex) { Debug.LogWarning($"OnPlayerEnteredRoom: failed to update FriendLobby premade state: {ex.Message}"); }

            // Queue match formation is centralized in QueueTimerCoroutine.
            try
            {
                if (room != null && room.CustomProperties != null && room.CustomProperties.ContainsKey(PhotonBattleRoom.IsQueueKey) && (bool)room.GetCustomProperty<bool>(PhotonBattleRoom.IsQueueKey))
                {
                    if (PhotonRealtimeClient.LocalPlayer.IsMasterClient)
                    {
                        StartQueueTimer();
                    }
                }
            }
            catch { }
                if (PhotonRealtimeClient.LocalPlayer.IsMasterClient)
                {
                    if (playerCount + botCount == room.MaxPlayers && room.IsOpen) PhotonRealtimeClient.CloseRoom();

                    QueueCustomBattleStartCheck();

                    // Ensure master continues matchmaking loop so countdowns can be restarted when new players join
                    if (PhotonRealtimeClient.InMatchmakingRoom && _matchmakingHolder == null)
                    {
                        _matchmakingHolder = StartCoroutine(WaitForMatchmakingPlayers());
                    }
                // If a start was cancelled recently  trigger leader-led room change
                try
                {
                    if (PhotonRealtimeClient.LocalPlayer.IsMasterClient && PhotonRealtimeClient.InMatchmakingRoom && Time.time - _lastStartCancelTime < 15f)
                    {
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

                        if (!isCustomRoom)
                        {
                            try { PhotonRealtimeClient.LocalPlayer.SetCustomProperty(PhotonBattleRoom.LeaderIdKey, PhotonRealtimeClient.LocalPlayer.UserId); OnRoomLeaderChanged?.Invoke(true); } catch { }
                            SafeRaiseEvent(
                                PhotonRealtimeClient.PhotonEvent.RoomChangeRequested,
                                PhotonRealtimeClient.LocalPlayer.UserId,
                                new RaiseEventArgs { Receivers = ReceiverGroup.Others },
                                SendOptions.SendReliable
                            );
                            // prevent immediate repeated broadcasts
                            _lastStartCancelTime = -100f;
                        }
                    }
                }
                catch { }
            }
            if (playerCount + botCount <= room.MaxPlayers) LobbyOnPlayerEnteredRoom?.Invoke(new(newPlayer));

            // If this is a queue room, stop further matchmaking/start processing here.
            try
            {
                if (room != null && room.CustomProperties != null && room.CustomProperties.ContainsKey(PhotonBattleRoom.IsQueueKey) && (room.CustomProperties[PhotonBattleRoom.IsQueueKey] is bool b && b))
                {
                    return;
                }
            }
            catch { }
        }
        public void OnRoomPropertiesUpdate(PhotonHashtable propertiesThatChanged) { LobbyOnRoomPropertiesUpdate?.Invoke(new(propertiesThatChanged)); }
        public void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonHashtable changedProps) { LobbyOnPlayerPropertiesUpdate?.Invoke(new(targetPlayer),new(changedProps)); }
        public void OnMasterClientSwitched(Player newMasterClient) {
            LobbyOnMasterClientSwitched?.Invoke(new(newMasterClient));

            bool currentRoomIsQueue = false;
            try
            {
                Room room = PhotonRealtimeClient.CurrentRoom;
                currentRoomIsQueue = room != null
                    && room.CustomProperties != null
                    && room.CustomProperties.ContainsKey(PhotonBattleRoom.IsQueueKey)
                    && room.GetCustomProperty<bool>(PhotonBattleRoom.IsQueueKey);
            }
            catch { }

            // Cancel any in-progress countdown locally when master changes (previous master might have started it)
            if (_startGameHolder != null)
            {
                StopCoroutine(_startGameHolder);
                _startGameHolder = null;
            }
            OnGameStartCancelled?.Invoke();

            // If the master left while a local countdown/start transition is in progress,
            // non-master clients may not have executed the OnPlayerLeftRoom requeue path because
            // the switch can race with room/property updates. Ensure clients still perform cancel+requeue here.
            try
            {
                var room = PhotonRealtimeClient.CurrentRoom;
                bool wasStarting = !_matchHasStartedInCurrentRoom && IsGameStartTransitionActive();
                if (wasStarting && PhotonRealtimeClient.InMatchmakingRoom && !currentRoomIsQueue && !PhotonRealtimeClient.LocalPlayer.IsMasterClient)
                {
                    // Mirror CancelGameStart handling with requeue=true for non-master clients
                    _lastStartCancelTime = Time.time;
                    try { if (_startGameHolder != null) { StopCoroutine(_startGameHolder); _startGameHolder = null; } } catch { }
                    try { if (_startQuantumHolder != null) { StopCoroutine(_startQuantumHolder); _startQuantumHolder = null; } } catch { }
                    try { StopMatchmakingCoroutines(); } catch { }
                    OnGameStartCancelled?.Invoke();

                    // Decide game type for requeue
                    GameType roomGameType = GameType.Random2v2;
                    try
                    {
                        if (room != null && room.CustomProperties != null && room.CustomProperties.ContainsKey(PhotonBattleRoom.GameTypeKey))
                        {
                            roomGameType = (GameType)room.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey);
                        }
                    }
                    catch { }

                    // Only perform requeue for non-Custom matchmaking rooms
                    if (roomGameType != GameType.Custom)
                    {
                        try { StartCoroutine(LeaveAndAutoRequeue(roomGameType)); } catch { }
                    }
                    else
                    {
                        try { OnLobbyWindowChangeRequest?.Invoke(LobbyWindowTarget.LobbyRoom); } catch { }
                    }
                }
                // If this client became the new master while a countdown was active,
                // take over leader-led requeue (create new matchmaking room) so players follow.
                if (wasStarting && PhotonRealtimeClient.InMatchmakingRoom && !currentRoomIsQueue && PhotonRealtimeClient.LocalPlayer.IsMasterClient)
                {
                    _lastStartCancelTime = Time.time;
                    try { if (_matchmakingHolder != null) { StopCoroutine(_matchmakingHolder); _matchmakingHolder = null; } } catch { }
                    try { if (_startGameHolder != null) { StopCoroutine(_startGameHolder); _startGameHolder = null; } } catch { }
                    try { if (_startQuantumHolder != null) { StopCoroutine(_startQuantumHolder); _startQuantumHolder = null; } } catch { }
                    try { StopMatchmakingCoroutines(); } catch { }
                    OnGameStartCancelled?.Invoke();

                    GameType roomGameType = GameType.Random2v2;
                    try
                    {
                        if (room != null && room.CustomProperties != null && room.CustomProperties.ContainsKey(PhotonBattleRoom.GameTypeKey))
                        {
                            roomGameType = (GameType)room.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey);
                        }
                    }
                    catch { }

                    if (roomGameType != GameType.Custom)
                    {
                        try { StartCoroutine(LeaveAndAutoRequeue(roomGameType)); } catch { }
                        try
                        {
                            SafeRaiseEvent(
                                PhotonRealtimeClient.PhotonEvent.RoomChangeRequested,
                                PhotonRealtimeClient.LocalPlayer.UserId,
                                new RaiseEventArgs { Receivers = ReceiverGroup.Others },
                                SendOptions.SendReliable
                            );
                        }
                        catch { }
                        _lastStartCancelTime = -100f;
                    }
                    else
                    {
                        try { OnLobbyWindowChangeRequest?.Invoke(LobbyWindowTarget.LobbyRoom); } catch { }
                    }
                }
            }
            catch { }

            // Queue timer handling: if we are now the master and inside a queue room, start the queue timer; otherwise stop it.
            try
            {
                if (PhotonRealtimeClient.LocalPlayer != null && PhotonRealtimeClient.LocalPlayer.IsMasterClient)
                {
                    var room = PhotonRealtimeClient.CurrentRoom;
                    if (room != null && room.CustomProperties != null && room.CustomProperties.ContainsKey(PhotonBattleRoom.IsQueueKey) && (room.GetCustomProperty<bool>(PhotonBattleRoom.IsQueueKey)))
                    {
                        if (_verifyPositionsHolder == null) _verifyPositionsHolder = StartCoroutine(VerifyRoomPositionsLoop());
                        StartQueueTimer();
                    }
                }
                else
                {
                    StopQueueTimer();
                }
            }
            catch { }

            // Ensure any stale BattleID is cleared when master changes so new master does not see a hanging start
            try
            {
                if (CanMutateRoomPropertiesNow("OnMasterClientSwitched: clear BattleID", true)
                    && PhotonRealtimeClient.CurrentRoom.CustomProperties.ContainsKey(PhotonBattleRoom.BattleID))
                {
                    PhotonRealtimeClient.CurrentRoom.SetCustomProperties(new PhotonHashtable { { PhotonBattleRoom.BattleID, "" } });
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to clear BattleID on master switch: {ex.Message}");
            }

            // If we are in a matchmaking room, new master should continue matchmaking; others stay and wait
            if (PhotonRealtimeClient.InMatchmakingRoom && !currentRoomIsQueue)
            {
                bool isQueueRoom = false;
                try
                {
                    Room currentRoom = PhotonRealtimeClient.CurrentRoom;
                    isQueueRoom = currentRoom != null
                        && currentRoom.CustomProperties != null
                        && currentRoom.CustomProperties.ContainsKey(PhotonBattleRoom.IsQueueKey)
                        && currentRoom.GetCustomProperty<bool>(PhotonBattleRoom.IsQueueKey);
                }
                catch { }

                // Update local player's known leader id to the current master so returning/disconnected players don't reclaim leadership.
                try
                {
                    if (!isQueueRoom)
                    {
                        PhotonRealtimeClient.LocalPlayer.SetCustomProperty(PhotonBattleRoom.LeaderIdKey, newMasterClient.UserId);
                    }
                    try { OnRoomLeaderChanged?.Invoke(newMasterClient.UserId == PhotonRealtimeClient.LocalPlayer.UserId); } catch { }
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

                // If this client became the new master, broadcast a RoomChangeRequested so clients follow to new room
                try
                {
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

                    if (!isCustomRoom && PhotonRealtimeClient.LocalPlayer.IsMasterClient && PhotonRealtimeClient.Client != null && PhotonRealtimeClient.Client.IsConnectedAndReady && PhotonRealtimeClient.InRoom)
                    {
                        // Only trigger leader-led requeue if a start was cancelled recently
                        try
                        {
                            if (Time.time - _lastStartCancelTime < 15f)
                            {
                                GameType roomGameType = GameType.Random2v2;
                                try
                                {
                                    if (PhotonRealtimeClient.CurrentRoom != null && PhotonRealtimeClient.CurrentRoom.CustomProperties != null && PhotonRealtimeClient.CurrentRoom.CustomProperties.ContainsKey(PhotonBattleRoom.GameTypeKey))
                                    {
                                        roomGameType = (GameType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey);
                                    }
                                }
                                catch { }

                                try { StartCoroutine(LeaveAndAutoRequeue(roomGameType)); } catch { }

                                SafeRaiseEvent(
                                    PhotonRealtimeClient.PhotonEvent.RoomChangeRequested,
                                    PhotonRealtimeClient.LocalPlayer.UserId,
                                    new RaiseEventArgs { Receivers = ReceiverGroup.Others },
                                    SendOptions.SendReliable
                                );
                                // prevent immediate repeated broadcasts
                                _lastStartCancelTime = -100f;
                            }
                        }
                        catch { }
                    }
                }
                catch { }

                // If we deferred returning to the MainMenu because the master left,
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

                    if (CanMutateRoomPropertiesNow("OnMasterClientSwitched: clear stale positions", true))
                    {
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
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"OnMasterClientSwitched: failed to clean stale positions: {ex.Message}");
            }
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
            public readonly bool IsPremadeInRoom;

            public StartMatchmakingEvent(GameType gameType, bool isPremadeInRoom = false)
            {
                SelectedGameType = gameType;
                IsPremadeInRoom = isPremadeInRoom;
            }

            public override string ToString()
            {
                return $"{nameof(SelectedGameType)}: {SelectedGameType}, {nameof(IsPremadeInRoom)}: {IsPremadeInRoom}";
            }
        }

        public class InRoomInviteInfo
        {
            public readonly string RoomName;
            public readonly string LeaderUserId;
            public readonly string InvitedUserId;
            public readonly GameType TargetGameType;

            public InRoomInviteInfo(string roomName, string leaderUserId, string invitedUserId, GameType targetGameType)
            {
                RoomName = roomName;
                LeaderUserId = leaderUserId;
                InvitedUserId = invitedUserId;
                TargetGameType = targetGameType;
            }

            public override string ToString()
            {
                return $"{nameof(RoomName)}: {RoomName}, {nameof(LeaderUserId)}: {LeaderUserId}, {nameof(InvitedUserId)}: {InvitedUserId}, {nameof(TargetGameType)}: {TargetGameType}";
            }
        }

        public class StopMatchmakingEvent
        {
            public readonly GameType SelectedGameType;
            public readonly bool ForceLeave;

            public StopMatchmakingEvent(GameType gameType, bool forceLeave = false)
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

    #endregion
}
