using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;
using Assert = UnityEngine.Assertions.Assert;

using Photon.Client;
using Photon.Realtime;
using Quantum;

using Altzone.Scripts.Config;
using Altzone.Scripts.Settings;
using PlayerData = Altzone.Scripts.Model.Poco.Player.PlayerData;
using Prg.Scripts.Common.PubSub;

using System.Threading.Tasks;
using Altzone.Scripts.Battle.Photon;
using Altzone.Scripts.Lobby.Wrappers;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.AzDebug;
using System.Collections.ObjectModel;
using WebSocketSharp;
using Altzone.Scripts.Common;
using Altzone.Scripts.ModelV2;

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

        [Header("Player")]
        [SerializeField] private RuntimePlayer _player;

        [Header("Configs")]
        [SerializeField] private Map _map;
        [SerializeField] private SimulationConfig _simulationConfig;
        [SerializeField] private SystemsConfig _systemsConfig;
        [SerializeField] private BattleArenaSpec _battleArenaSpec;
        [SerializeField] private ProjectileSpec _projectileSpec;

        [Header("Battle Map reference")]
        [SerializeField] private BattleMapReference _battleMapReference;

        private Emotion _projectileInitialEmotion = Emotion.Sorrow;

        private QuantumRunner _runner = null;

        private Coroutine _requestPositionChangeHolder = null;
        private Coroutine _matchmakingHolder = null;
        private Coroutine _followLeaderHolder = null;

        private string[] _teammates = null;

        private List<FriendInfo> _friendList;

        [HideInInspector] public ReadOnlyCollection<LobbyRoomInfo> CurrentRooms = null; // Set from LobbyRoomListingController.cs through Instance variable maybe this could be refactored?
        public static LobbyManager Instance { get; private set; }

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

        #endregion


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
        }

        public void OnEnable()
        {
            PhotonRealtimeClient.Client.AddCallbackTarget(this);
            PhotonRealtimeClient.Client.StateChanged += OnStateChange;
            this.Subscribe<PlayerPosEvent>(OnPlayerPosEvent);
            this.Subscribe<StartRoomEvent>(OnStartRoomEvent);
            this.Subscribe<StartPlayingEvent>(OnStartPlayingEvent);
            this.Subscribe<StartRaidTestEvent>(OnStartRaidTestEvent);
            this.Subscribe<StartMatchmakingEvent>(OnStartMatchmakingEvent);
            this.Subscribe<StopMatchmakingEvent>(OnStopMatchmakingEvent);
            StartCoroutine(Service());
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

        private void OnPlayerPosEvent(PlayerPosEvent data)
        {
            if (_requestPositionChangeHolder == null)
            {
                _requestPositionChangeHolder = StartCoroutine(RequestPositionChange(data.PlayerPosition));
            }
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

        private IEnumerator StartMatchmaking(GameType gameType)
        {
            // Closing the room so that no others can join
            PhotonRealtimeClient.CurrentRoom.IsOpen = false;

            // Saving custom properties from the room to the variables
            string clanName = PhotonRealtimeClient.CurrentRoom.GetCustomProperty(PhotonBattleRoom.ClanNameKey, "");
            int soulhomeRank = PhotonRealtimeClient.LocalLobbyPlayer.GetCustomProperty(PhotonBattleRoom.SoulhomeRank, 0);

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
                if (!clanName.IsNullOrEmpty())
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
                            // Getting first free position from the room and setting own user id to that position in room
                            int freePosition = PhotonLobbyRoom.GetFirstFreePlayerPos();
                            PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.GetPositionKey(freePosition), PhotonRealtimeClient.LocalLobbyPlayer.UserId);
                        }
                        else // Queuing with a teammate
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


            // Updating player positions from room to player properties
            foreach (var player in PhotonRealtimeClient.CurrentRoom.Players)
            {
                string positionValue1 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey1);
                string positionValue2 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey2);
                string positionValue3 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey3);
                string positionValue4 = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PlayerPositionKey4);

                if (player.Value.UserId == positionValue1)
                {
                    player.Value.SetCustomProperty(PhotonBattleRoom.PlayerPositionKey, PhotonBattleRoom.PlayerPosition1);
                }
                else if (player.Value.UserId == positionValue2)
                {
                    player.Value.SetCustomProperty(PhotonBattleRoom.PlayerPositionKey, PhotonBattleRoom.PlayerPosition2);
                }
                else if (player.Value.UserId == positionValue3)
                {
                    player.Value.SetCustomProperty(PhotonBattleRoom.PlayerPositionKey, PhotonBattleRoom.PlayerPosition3);
                }
                else if (player.Value.UserId == positionValue4)
                {
                    player.Value.SetCustomProperty(PhotonBattleRoom.PlayerPositionKey, PhotonBattleRoom.PlayerPosition4);
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

            // Starting game
            StartCoroutine(StartTheGameplay(_isCloseRoomOnGameStart, _blueTeamName, _redTeamName));
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

            PhotonRealtimeClient.LeaveRoom();

            yield return new WaitUntil(() => PhotonRealtimeClient.InLobby);

            // Creating back the non-matchmaking room which the teammates can join
            switch (matchmakingRoomGameType)
            {
                case GameType.Random2v2:
                    PhotonRealtimeClient.CreateRandom2v2LobbyRoom(_teammates);
                    break;
                case GameType.Clan2v2:
                    string clanName = PhotonRealtimeClient.LocalLobbyPlayer.GetCustomProperty(PhotonBattleRoom.ClanNameKey, "");
                    int soulhomeRank = PhotonRealtimeClient.LocalLobbyPlayer.GetCustomProperty(PhotonBattleRoom.SoulhomeRank, 0);
                    PhotonRealtimeClient.CreateClan2v2LobbyRoom(clanName, soulhomeRank, _teammates);
                    break;
            }
        }

        private IEnumerator StartTheGameplay(bool isCloseRoom, string blueTeamName, string redTeamName)
        {
            if (!PhotonBattleRoom.IsValidAllSelectedCharacters())
            {
                throw new UnityException("can't start game, everyone needs to have 3 defence characters selected");
            }
            //Debug.Log($"startTheGameplay {gameWindow}");
            if (!PhotonRealtimeClient.LocalPlayer.IsMasterClient)
            {
                throw new UnityException("only master client can start the game");
            }
            Player player = PhotonRealtimeClient.LocalPlayer;
            int masterPosition = player.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
            if (!PhotonLobbyRoom.IsValidPlayerPos(masterPosition))
            {
                throw new UnityException($"master client does not have valid player position: {masterPosition}");
            }
            // Snapshot player list before iteration because we can change it
            List<Player> players = PhotonRealtimeClient.CurrentRoom.Players.Values.ToList();
            int realPlayerCount = 0;
            foreach (Player roomPlayer in players)
            {
                int playerPos = roomPlayer.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
                if (PhotonLobbyRoom.IsValidPlayerPos(playerPos))
                {
                    realPlayerCount += 1;
                    continue;
                }
                if (playerPos == PlayerPositionSpectator)
                {
                    continue;
                }
                Debug.Log($"Kick player (close connection) @ {PlayerPositionKey}={playerPos} {roomPlayer.GetDebugLabel()}");
                PhotonRealtimeClient.CloseConnection(roomPlayer);
                yield return null;
            }
            if (player.IsMasterClient)
            {
                Assert.IsTrue(!string.IsNullOrWhiteSpace(blueTeamName), "!string.IsNullOrWhiteSpace(blueTeamName)");
                Assert.IsTrue(!string.IsNullOrWhiteSpace(redTeamName), "!string.IsNullOrWhiteSpace(redTeamName)");
                Room room = PhotonRealtimeClient.CurrentRoom;
                //room.CustomProperties.Add(TeamAlphaNameKey, blueTeamName);
                //room.CustomProperties.Add(TeamBetaNameKey, redTeamName);
                //room.CustomProperties.Add(PlayerCountKey, realPlayerCount);
                room.SetCustomProperties(new PhotonHashtable
                {
                    { BattleID, PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.BattleID)},
                    { TeamAlphaNameKey, blueTeamName },
                    { TeamBetaNameKey, redTeamName },
                    { PlayerCountKey, realPlayerCount }
                });

                // Getting starting emotion from current room custom properties
                Emotion startingEmotion = (Emotion)PhotonRealtimeClient.CurrentRoom.GetCustomProperty(PhotonBattleRoom.StartingEmotionKey, (int)Emotion.Blank);

                // If starting emotion is blank getting a random starting emotion
                if (startingEmotion == Emotion.Blank) 
                {
                    startingEmotion = (Emotion)UnityEngine.Random.Range(0, 4);
                }

                // Saving projectile initial emotion to a variable in case the room closes TODO: remove cast when battle uses Emotion enum also
                _projectileInitialEmotion = startingEmotion;

                // Getting map id from room custom properties
                string mapId = PhotonRealtimeClient.CurrentRoom.GetCustomProperty(PhotonBattleRoom.MapKey, string.Empty);

                // If there is no map id getting a random map
                if (mapId == string.Empty)
                {
                    int mapIndex = UnityEngine.Random.Range(0, _battleMapReference.Maps.Count);
                    mapId = _battleMapReference.Maps[mapIndex].MapId;
                }

                // Setting map to variable
                Map map = _battleMapReference.GetBattleMap(mapId).Map;
                if (map != null) _map = map;

                yield return null;
                if (isCloseRoom)
                {
                    PhotonRealtimeClient.CloseRoom(true);
                    yield return null;
                }
            }
            if (!PhotonRealtimeClient.Client.OpRaiseEvent(PhotonRealtimeClient.PhotonEvent.StartGame, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), new RaiseEventArgs{Receivers = ReceiverGroup.All}, SendOptions.SendReliable))
            {
                Debug.LogError("Unable to start game.");
                yield break;
            }
            Debug.Log("Starting Game");
            //WindowManager.Get().ShowWindow(gameWindow);
        }

        private IEnumerator StartQuantum(long sendTime)
        {
            string battleID = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(BattleID);
            int playerPosition = PhotonRealtimeClient.LocalPlayer.GetCustomProperty<int>(PlayerPositionKey);

            if (QuantumRunner.Default != null)
            {
                Debug.Log($"QuantumRunner is already running: {QuantumRunner.Default.Id}");
                yield break;
            }

            RuntimeConfig config = new()
            {
                Map              = _map,
                SimulationConfig = _simulationConfig,
                SystemsConfig    = _systemsConfig,
                BattleArenaSpec  = _battleArenaSpec,
                ProjectileSpec   = _projectileSpec,
                InitialProjectileEmotion = (EmotionState)_projectileInitialEmotion,
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

            //Start Battle Countdown
            OnLobbyWindowChangeRequest?.Invoke(LobbyWindowTarget.BattleLoad);

            if(sendTime == 0) sendTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long timeToStart = (sendTime+5000) - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long startTime = sendTime + timeToStart;

            yield return new WaitForEndOfFrame();

            do
            {
                if(OnStartTimeSet != null)
                {
                    OnStartTimeSet?.Invoke(timeToStart);
                    break;
                }
                yield return null;
            } while (startTime > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            timeToStart = (sendTime + 5000) - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (timeToStart > 5000) timeToStart = 5000;

            if(timeToStart > 0)
            yield return new WaitForSeconds(timeToStart / 1000f);

            //Move to Battle and start Runner
            OnLobbyWindowChangeRequest?.Invoke(LobbyWindowTarget.Battle);

            yield return new WaitUntil(()=>SceneManager.GetActiveScene().name == _map.Scene);

            DebugLogFileHandler.ContextEnter(DebugLogFileHandler.ContextID.Battle);
            DebugLogFileHandler.FileOpen(battleID, playerPosition);

            Task<bool> task = StartRunner(sessionRunnerArguments);

            yield return new WaitUntil(() => task.IsCompleted);
            if(task.Result)
            {
                _player.PlayerSlot = playerPosition;
                _runner?.Game.AddPlayer(_player);
            }
            else
            {
                OnLobbyWindowChangeRequest?.Invoke(LobbyWindowTarget.MainMenu);
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

        public static void ExitQuantum()
        {
            QuantumRunner.ShutdownAll();
            DebugLogFileHandler.ContextExit();
            OnLobbyWindowChangeRequest?.Invoke(LobbyWindowTarget.BattleStory);
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

        private void SetPlayer(Player player, int playerPosition)
        {
            // Checking if any of the players in the room are already in the position (value is anything else than empty string) and if so return.
            if (PhotonBattleRoom.CheckIfPositionIsFree(playerPosition) == false) return;

            Assert.IsTrue(PhotonLobbyRoom.IsValidGameplayPosOrGuest(playerPosition));

            if (!player.HasCustomProperty(PlayerPositionKey))
            {
                Debug.Log($"setPlayer {PlayerPositionKey}={playerPosition}");
                player.SetCustomProperties(new PhotonHashtable { { PlayerPositionKey, playerPosition } });
                return;
            }

            // Setting new position to player's custom properties
            int curValue = player.GetCustomProperty<int>(PlayerPositionKey);
            player.SafeSetCustomProperty(PlayerPositionKey, playerPosition, curValue);

            // Initializing hash tables for setting the previous position empty
            string previousPositionKey = PhotonBattleRoom.GetPositionKey(curValue);

            var emptyPosition = new LobbyPhotonHashtable(new Dictionary<object, object> { { previousPositionKey, "" } });
            var expectedValue = new LobbyPhotonHashtable(new Dictionary<object, object> { { previousPositionKey, player.UserId } }); // Expected to have the player's id in the previous position

            // Setting previous position empty
            PhotonRealtimeClient.LobbyCurrentRoom.SetCustomProperties(emptyPosition, expectedValue);

            // Initializing hash tables for setting the new position as taken
            string newPositionKey = PhotonBattleRoom.GetPositionKey(playerPosition);

            var newPosition = new LobbyPhotonHashtable(new Dictionary<object, object> { { newPositionKey, player.UserId } });
            expectedValue = new LobbyPhotonHashtable(new Dictionary<object, object> { { newPositionKey, "" } }); // Expecting the new position to be empty

            // Setting new position as taken
            PhotonRealtimeClient.LobbyCurrentRoom.SetCustomProperties(newPosition, expectedValue);
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
                _player.Characters[i] = new BattleCharacterBase()
                {
                    Id            = (int)character.Id,
                    Class         = (int)character.CharacterClassID,

                    Hp            = BaseCharacter.GetStatValueFP(StatType.Hp,            character.Hp),
                    Attack        = BaseCharacter.GetStatValueFP(StatType.Attack,        character.Attack),
                    Defence       = BaseCharacter.GetStatValueFP(StatType.Defence,       character.Defence),
                    CharacterSize = BaseCharacter.GetStatValueFP(StatType.CharacterSize, character.CharacterSize),
                    Speed         = BaseCharacter.GetStatValueFP(StatType.Speed,         character.Speed)
                };
            }
        }

        private void StopHolderCoroutines()
        {
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
                string positionKey = PhotonBattleRoom.GetPositionKey(otherPlayerPosition);

                var emptyPosition = new LobbyPhotonHashtable(new Dictionary<object, object> { { positionKey, "" } });
                var expectedValue = new LobbyPhotonHashtable(new Dictionary<object, object> { { positionKey, otherPlayer.UserId } });

                PhotonRealtimeClient.LobbyCurrentRoom.SetCustomProperties(emptyPosition, expectedValue);
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
            Debug.Log($"Received PhotonEvent {photonEvent.Code}");

            switch (photonEvent.Code)
            {
                case PhotonRealtimeClient.PhotonEvent.StartGame:
                    StartCoroutine(StartQuantum((long)photonEvent.CustomData));
                    break;
                case PhotonRealtimeClient.PhotonEvent.PlayerPositionChangeRequested:
                    int position = (int)photonEvent.CustomData;
                    Player player = PhotonRealtimeClient.CurrentRoom.GetPlayer(photonEvent.Sender);
                    if (player != null) SetPlayer(player, position);
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

        public void OnPlayerEnteredRoom(Player newPlayer) { LobbyOnPlayerEnteredRoom?.Invoke(new(newPlayer)); }
        public void OnRoomPropertiesUpdate(PhotonHashtable propertiesThatChanged) { LobbyOnRoomPropertiesUpdate?.Invoke(new(propertiesThatChanged)); }
        public void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonHashtable changedProps) { LobbyOnPlayerPropertiesUpdate?.Invoke(new(targetPlayer),new(changedProps)); }
        public void OnMasterClientSwitched(Player newMasterClient) {
            LobbyOnMasterClientSwitched?.Invoke(new(newMasterClient));

            if (PhotonRealtimeClient.InMatchmakingRoom && PhotonRealtimeClient.LocalLobbyPlayer.IsMasterClient)
            {
                _matchmakingHolder = StartCoroutine(WaitForMatchmakingPlayers());
            }
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
    }
}
