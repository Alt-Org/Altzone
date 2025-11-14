using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.SceneManagement;
using Assert = UnityEngine.Assertions.Assert;

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

        private QuantumRunner _runner = null;

        private Coroutine _reserveFreePositionHolder = null;
        private Coroutine _requestPositionChangeHolder = null;
        private Coroutine _matchmakingHolder = null;
        private Coroutine _followLeaderHolder = null;
        private Coroutine _canBattleStartCheckHolder = null;

        private string[] _teammates = null;

        private List<FriendInfo> _friendList;

        [HideInInspector] public ReadOnlyCollection<LobbyRoomInfo> CurrentRooms = null; // Set from LobbyRoomListingController.cs through Instance variable maybe this could be refactored?

        private List<string> _posChangeQueue = new();

        private bool _isStartFinished = false;

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

        public delegate void KickedOutOfTheRoom(GetKickedEvent.ReasonType reason);
        public static event KickedOutOfTheRoom OnKickedOutOfTheRoom;

        #endregion


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
                if (!_isActive && SceneManager.GetActiveScene().buildIndex != 0) Activate();
            }
        }

        public void OnEnable()
        {
            if(!_isActive && SceneManager.GetActiveScene().buildIndex != 0) Activate();
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
            this.Subscribe<StartRoomEvent>(OnStartRoomEvent);
            this.Subscribe<StartPlayingEvent>(OnStartPlayingEvent);
            this.Subscribe<StartRaidTestEvent>(OnStartRaidTestEvent);
            this.Subscribe<StartMatchmakingEvent>(OnStartMatchmakingEvent);
            this.Subscribe<StopMatchmakingEvent>(OnStopMatchmakingEvent);
            this.Subscribe<GetKickedEvent>(OnGetKickedEvent);
            StartCoroutine(Service());
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
            // Loop until player correctly reserves slot
            int freePosition;
            bool success = false;
            do
            {
                // Getting first free position from the room and creating the photon hashtables for setting property
                freePosition = PhotonLobbyRoom.GetFirstFreePlayerPos();
                if (!PhotonLobbyRoom.IsValidPlayerPos(freePosition)) yield break;
                StartCoroutine(RequestPositionChange(freePosition));
                string positionKey = PhotonBattleRoom.GetPositionKey(freePosition);

                /*PhotonHashtable propertyToSet = new() { { positionKey, PhotonRealtimeClient.LocalLobbyPlayer.UserId } };
                PhotonHashtable expectedValue = new() { { positionKey, "" } };

                // Setting custom property, checking if the request could be sent to the server
                if (PhotonRealtimeClient.CurrentRoom.SetCustomProperties(propertyToSet, expectedValue))
                {
                    // Waiting until that position in the room is reserved
                    string positionValue = "";
                    yield return new WaitUntil(() =>
                    {
                        positionValue = PhotonRealtimeClient.CurrentRoom.GetCustomProperty(positionKey, "");
                        return positionValue != "";
                    });

                    // Checking if local player is the one in the slot or if there was a conflict
                    success = positionValue == PhotonRealtimeClient.LocalLobbyPlayer.UserId;
                }*/
                string positionValue = "";
                yield return new WaitUntil(() =>
                {
                    positionValue = PhotonRealtimeClient.CurrentRoom.GetCustomProperty(positionKey, "");
                    return positionValue != "";
                });

                // Checking if local player is the one in the slot or if there was a conflict
                success = positionValue == PhotonRealtimeClient.LocalLobbyPlayer.UserId;

                if (success) break;
                yield return null;
            } while (!success);

            // Setting to player properties
            //if (setToPlayerProperties) PhotonRealtimeClient.LocalPlayer.SetCustomProperty(PhotonBattleRoom.PlayerPositionKey, freePosition);

            _reserveFreePositionHolder = null;
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
            _canBattleStartCheckHolder = null;
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

        private IEnumerator RequestPositionChange(int position)
        {
            // Saving the previous position to a variable
            int oldPosition = PhotonRealtimeClient.LocalPlayer.GetCustomProperty(PlayerPositionKey, -1);
            int currentPosition = oldPosition;

            do
            {
                // Checking if the new position is free before raising event to master client
                if (PhotonBattleRoom.CheckIfPositionIsFree(position) == false)
                {
                    Debug.LogWarning($"Failed to reserve the position {position}. This likely because somebody already is in this position.");
                    _requestPositionChangeHolder = null;
                    yield break;
                }

                // Raising event to master client
                PhotonRealtimeClient.Client.OpRaiseEvent(
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

            _requestPositionChangeHolder = null;
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
            StartCoroutine(StartTheGameplay(_isCloseRoomOnGameStart, _blueTeamName, _redTeamName));
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

            // Sending others event to leave matchmaking
            PhotonRealtimeClient.Client.OpRaiseEvent(
                    PhotonRealtimeClient.PhotonEvent.RoomChangeRequested,
                    PhotonRealtimeClient.LocalPlayer.UserId,
                    new RaiseEventArgs { Receivers = ReceiverGroup.Others },
                    SendOptions.SendReliable
                );

            StartCoroutine(LeaveMatchmaking());
        }
        #region Matchmaking
        private IEnumerator StartMatchmaking(GameType gameType)
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

            PhotonRealtimeClient.Client.OpRaiseEvent(
                    PhotonRealtimeClient.PhotonEvent.RoomChangeRequested,
                    PhotonRealtimeClient.LocalPlayer.UserId,
                    new RaiseEventArgs { Receivers = ReceiverGroup.Others },
                    SendOptions.SendReliable
                );

            // Nulling room list and leaving room so that client can get room list
            CurrentRooms = null;
            PhotonRealtimeClient.LeaveRoom();

            // Waiting until in lobby and that current room list has rooms
            yield return new WaitUntil(() => PhotonRealtimeClient.InLobby && CurrentRooms != null);

            // Searching for suitable room
            bool roomFound = false;
            foreach (LobbyRoomInfo room in CurrentRooms)
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

                // Matchmaking logic
                switch (gameType)
                {
                    case GameType.Clan2v2: // TODO: Add soulhome rank matchmaking and a coroutine which increases the rank variance periodically
                        if ((string)room.CustomProperties[PhotonBattleRoom.ClanNameKey] != clanName && room.MaxPlayers - room.PlayerCount >= _teammates.Length + 1)
                        {
                            PhotonRealtimeClient.JoinRoom(room.Name, _teammates);
                            roomFound = true;
                            break;
                        }
                        break;
                    case GameType.Random2v2:
                        if (room.MaxPlayers - room.PlayerCount >= _teammates.Length + 1)
                        {
                            PhotonRealtimeClient.JoinRoom(room.Name, _teammates);
                            roomFound = true;
                        }
                        break;
                }

            }

            // If suitable room not found creating new room
            if (!roomFound)
            {
                switch (gameType)
                {
                    case GameType.Clan2v2:
                        PhotonRealtimeClient.CreateClan2v2LobbyRoom(clanName, soulhomeRank, _teammates, true);
                        break;
                    case GameType.Random2v2:
                        PhotonRealtimeClient.CreateRandom2v2LobbyRoom(_teammates, true);
                        break;
                }
            }

            // Waiting until client is in room
            yield return new WaitUntil(() => PhotonRealtimeClient.InRoom);

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
        }

        private IEnumerator WaitForMatchmakingPlayers()
        {
            if (!PhotonRealtimeClient.LocalPlayer.IsMasterClient) yield break;

            bool gameStarting = false;
            do
            {
                // Checking every 0,5s if we can start gameplay
                bool canStartGameplay = false;
                do
                {
                    yield return new WaitForSeconds(0.5f);

                    // Checking if room is full
                    if (PhotonRealtimeClient.CurrentRoom.PlayerCount != PhotonRealtimeClient.CurrentRoom.MaxPlayers) continue;

                    // Checking that all of the positions in the room are set
                    bool isSetPosition1 = !PhotonBattleRoom.CheckIfPositionIsFree(PhotonBattleRoom.PlayerPosition1);
                    bool isSetPosition2 = !PhotonBattleRoom.CheckIfPositionIsFree(PhotonBattleRoom.PlayerPosition2);
                    bool isSetPosition3 = !PhotonBattleRoom.CheckIfPositionIsFree(PhotonBattleRoom.PlayerPosition3);
                    bool isSetPosition4 = !PhotonBattleRoom.CheckIfPositionIsFree(PhotonBattleRoom.PlayerPosition4);

                    if (isSetPosition1 && isSetPosition2 && isSetPosition3 && isSetPosition4)
                    {
                        canStartGameplay = true;
                    }

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
                        yield return new WaitUntil(() => PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(positionKey) == position);
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

                // Starting gameplay coroutine if all 4 room members are still present, else we loop again
                if (PhotonRealtimeClient.CurrentRoom.PlayerCount == PhotonRealtimeClient.CurrentRoom.MaxPlayers)
                {
                    StartCoroutine(StartTheGameplay(_isCloseRoomOnGameStart, _blueTeamName, _redTeamName));
                    gameStarting = true;
                }

            } while (!gameStarting);
        }

        private IEnumerator FollowLeaderToNewRoom(string leaderUserId)
        {
            string oldRoomName = PhotonRealtimeClient.CurrentRoom.Name;

            // Leaving room and waiting until in lobby
            PhotonRealtimeClient.LeaveRoom();
            yield return new WaitUntil(() => PhotonRealtimeClient.InLobby);

            // Trying to see which room the leader joined
            bool newRoomJoined = false;
            do
            {
                _friendList = null;
                PhotonRealtimeClient.Client.OpFindFriends(new string[1] { leaderUserId });
                yield return new WaitUntil(() => _friendList != null );

                foreach (FriendInfo friend in _friendList)
                {
                    if (friend.UserId == leaderUserId && friend.IsInRoom && friend.Room != oldRoomName)
                    {
                        PhotonRealtimeClient.JoinRoom(friend.Room);
                        newRoomJoined = true;
                    }
                }
            } while (!newRoomJoined);

            _followLeaderHolder = null;
        }

        private IEnumerator LeaveMatchmaking()
        {
            GameType matchmakingRoomGameType = (GameType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey);

            if (_matchmakingHolder != null)
            {
                StopCoroutine(_matchmakingHolder);
                _matchmakingHolder = null;
            }

            yield return new WaitUntil(() => PhotonRealtimeClient.InRoom);

            PhotonRealtimeClient.LeaveRoom();

            yield return new WaitUntil(() => PhotonRealtimeClient.InLobby);

            // Creating back the non-matchmaking room which the teammates can join
            switch (matchmakingRoomGameType)
            {
                //case GameType.Random2v2:
                //    PhotonRealtimeClient.CreateRandom2v2LobbyRoom(_teammates);
                //    break;
                case GameType.Clan2v2:
                    string clanName = PhotonRealtimeClient.LocalLobbyPlayer.GetCustomProperty(PhotonBattleRoom.ClanNameKey, "");
                    int soulhomeRank = PhotonRealtimeClient.LocalLobbyPlayer.GetCustomProperty(PhotonBattleRoom.SoulhomeRank, 0);
                    PhotonRealtimeClient.CreateClan2v2LobbyRoom(clanName, soulhomeRank, _teammates);
                    break;
            }
        }
        #endregion

        private IEnumerator StartTheGameplay(bool isCloseRoom, string blueTeamName, string redTeamName)
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
                Assert.IsTrue(!string.IsNullOrWhiteSpace(blueTeamName), "!string.IsNullOrWhiteSpace(blueTeamName)");
                Assert.IsTrue(!string.IsNullOrWhiteSpace(redTeamName), "!string.IsNullOrWhiteSpace(redTeamName)");
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
                if (isCloseRoom)
                {
                    PhotonRealtimeClient.CloseRoom(false);
                    yield return null;
                }

                /*for (int i=0; i < playerTypes.Length; i++)
                {
                    if(playerTypes[i] == PlayerType.None) playerTypes[i] = PlayerType.Bot;
                } Disabled for now, reactivate when the bots work a little better again.*/

                data = new()
                {
                    StartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    PlayerSlotUserIds = playerUserIds,
                    PlayerSlotUserNames = playerUserNames,
                    PlayerSlotTypes = playerTypes,
                    ProjectileInitialEmotion = startingEmotion,
                    MapId = mapId,
                    PlayerCount = playerCount
                };

            }
            if (!PhotonRealtimeClient.Client.OpRaiseEvent(PhotonRealtimeClient.PhotonEvent.StartGame, StartGameData.Serialize(data), new RaiseEventArgs{Receivers = ReceiverGroup.All}, SendOptions.SendReliable))
            {
                Debug.LogError("Unable to start game.");
                StartingGameFailed();
                yield break;
            }
            Debug.Log("Starting Game");
            //WindowManager.Get().ShowWindow(gameWindow);
        }

        private void StartingGameFailed()
        {
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

            // Getting the index of own user id from the player slot user id array to determine which player slot is for local player.
            string userId = PhotonRealtimeClient.LocalPlayer.UserId;
            int slotIndex = Array.IndexOf(data.PlayerSlotUserIds, userId);
            BattlePlayerSlot playerSlot = RuntimePlayer.PlayerSlots[slotIndex];

            // Setting map to variable
            Map map = _battleMapReference.GetBattleMap(data.MapId).Map;
            if (map != null) _quantumBattleMap = map;

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

            //Start Battle Countdown
            OnLobbyWindowChangeRequest?.Invoke(LobbyWindowTarget.BattleLoad);
            _isStartFinished = false;

            yield return new WaitUntil(() => OnStartTimeSet != null);
            if (sendTime == 0) sendTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long timeToStart = (sendTime + STARTDELAY) - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long startTime = sendTime + timeToStart;
            do
            {
                if (OnStartTimeSet != null)
                {
                    OnStartTimeSet?.Invoke(0);
                    break;
                }
                yield return null;
            } while (startTime > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

            yield return new WaitUntil(()=>_isStartFinished);

            AudioManager.Instance.StopMusic();

            //Move to Battle and start Runner
            OnLobbyWindowChangeRequest?.Invoke(LobbyWindowTarget.Battle);

            yield return new WaitUntil(()=>SceneManager.GetActiveScene().name == _quantumBattleMap.Scene);

            DebugLogFileHandler.ContextEnter(DebugLogFileHandler.ContextID.Battle);
            DebugLogFileHandler.FileOpen(battleID, (int)playerSlot);

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
                    player.SetCustomProperty(PlayerPositionKey, playerPosition);
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
            if (PhotonBattleRoom.CheckIfPositionIsFree(playerPosition) == false)
            {
                Debug.LogWarning("Requested position is not free.");
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
                        Debug.Log($"Set Bot to position {playerPosition}");
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
                    Hp            = BaseCharacter.GetStatValueFP(StatType.Hp, character.Hp),
                    Attack        = BaseCharacter.GetStatValueFP(StatType.Attack, character.Attack),
                    Defence       = BaseCharacter.GetStatValueFP(StatType.Defence, character.Defence),
                    CharacterSize = BaseCharacter.GetStatValueFP(StatType.CharacterSize, character.CharacterSize),
                    Speed         = BaseCharacter.GetStatValueFP(StatType.Speed, character.Speed)
                };

                _player.Characters[i] = new BattleCharacterBase()
                {
                    Id            = (int)character.Id,
                    Class         = (int)character.CharacterClassType,
                    Stats         = stats,
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
            if (cause != DisconnectCause.DisconnectByClientLogic && cause != DisconnectCause.DisconnectByServerLogic)
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
                    // Changing leader status if the other player was this player's leader
                    PhotonRealtimeClient.LocalPlayer.SetCustomProperty(PhotonBattleRoom.LeaderIdKey, PhotonRealtimeClient.LocalPlayer.UserId);
                    OnRoomLeaderChanged?.Invoke(true);
                }
            }

            LobbyOnPlayerLeftRoom?.Invoke(new(otherPlayer));
        }

        public void OnJoinedRoom()
        {
            // Enable: PhotonNetwork.CloseConnection needs to to work across all clients - to kick off invalid players!
            PhotonRealtimeClient.EnableCloseConnection = true;

            // Getting info if room is matchmaking room or not
            if (PhotonRealtimeClient.InMatchmakingRoom)
            {
                bool isLeader = PhotonRealtimeClient.LocalPlayer.UserId == PhotonRealtimeClient.LocalPlayer.GetCustomProperty<string>(PhotonBattleRoom.LeaderIdKey);
                OnMatchmakingRoomEntered?.Invoke(isLeader);
            }
            else
            {
                LobbyOnJoinedRoom?.Invoke();
            }
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
            LobbyOnJoinRoomFailed?.Invoke(returnCode, message);
        }
        public void OnJoinRandomFailed(short returnCode, string message) { LobbyOnJoinRandomFailed?.Invoke(returnCode, message); }

        public void OnEvent(EventData photonEvent)
        {
            if(photonEvent.Code != 103) Debug.Log($"Received PhotonEvent {photonEvent.Code}");

            switch (photonEvent.Code)
            {
                case PhotonRealtimeClient.PhotonEvent.StartGame:
                    // For some reason sometimes in Android build the CustomData is a ByteArraySlice and causes errors, so doing a null check to the cast
                    byte[] byteArray = photonEvent.CustomData as byte[] ?? ((ByteArraySlice)photonEvent.CustomData).Buffer;
                    StartCoroutine(StartQuantum(StartGameData.Deserialize(byteArray)));
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

                    if (_followLeaderHolder == null && leaderUserId == matchmakingLeaderId)
                    {
                        _followLeaderHolder = StartCoroutine(FollowLeaderToNewRoom(leaderUserId));
                    }
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
            LobbyOnPlayerEnteredRoom?.Invoke(new(newPlayer));

            if(_canBattleStartCheckHolder == null) _canBattleStartCheckHolder = StartCoroutine(CheckIfBattleCanStart());
        }
        public void OnRoomPropertiesUpdate(PhotonHashtable propertiesThatChanged) { LobbyOnRoomPropertiesUpdate?.Invoke(new(propertiesThatChanged)); }
        public void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonHashtable changedProps) { LobbyOnPlayerPropertiesUpdate?.Invoke(new(targetPlayer),new(changedProps)); }
        public void OnMasterClientSwitched(Player newMasterClient) {
            LobbyOnMasterClientSwitched?.Invoke(new(newMasterClient));

            if (PhotonRealtimeClient.InMatchmakingRoom && PhotonRealtimeClient.LocalLobbyPlayer.IsMasterClient)
            {
                _matchmakingHolder = StartCoroutine(WaitForMatchmakingPlayers());
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
                 $"\nPlayerCount: {PlayerCount}";
        }
    }
}
