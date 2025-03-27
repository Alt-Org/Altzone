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

        private QuantumRunner _runner = null;
        private Coroutine _requestPositionChangeHolder = null;

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
                    { BattleID, PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>("bid")},
                    { TeamAlphaNameKey, blueTeamName },
                    { TeamBetaNameKey, redTeamName },
                    { PlayerCountKey, realPlayerCount }
                });
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
                ProjectileSpec   = _projectileSpec
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
            string playerID = player.GetCustomProperty<string>(PhotonBattleRoom.PlayerIDKey);

            var emptyPosition = new LobbyPhotonHashtable(new Dictionary<object, object> { { previousPositionKey, "" } });
            var expectedValue = new LobbyPhotonHashtable(new Dictionary<object, object> { { previousPositionKey, playerID } }); // Expected to have the player's id in the previous position

            // Setting previous position empty
            PhotonRealtimeClient.LobbyCurrentRoom.SetCustomProperties(emptyPosition, expectedValue);

            // Initializing hash tables for setting the new position as taken
            string newPositionKey = PhotonBattleRoom.GetPositionKey(playerPosition);

            var newPosition = new LobbyPhotonHashtable(new Dictionary<object, object> { { newPositionKey, playerID } });
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

        public void OnDisconnected(DisconnectCause cause)
        {
            // If position change coroutine is running stopping it
            if (_requestPositionChangeHolder != null)
            {
                StopCoroutine( _requestPositionChangeHolder );
                _requestPositionChangeHolder = null;
            }

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

            // Clearing the player position in the room
            int playerPosition = otherPlayer.GetCustomProperty<int>(PlayerPositionKey);
            string positionKey = PhotonBattleRoom.GetPositionKey(playerPosition);
            string playerID = otherPlayer.GetCustomProperty<string>(PhotonBattleRoom.PlayerIDKey);

            var emptyPosition = new LobbyPhotonHashtable(new Dictionary<object, object> { { positionKey, "" } });
            var expectedValue = new LobbyPhotonHashtable(new Dictionary<object, object> { { positionKey, playerID } });

            PhotonRealtimeClient.LobbyCurrentRoom.SetCustomProperties(emptyPosition, expectedValue);

            LobbyOnPlayerLeftRoom?.Invoke(new(otherPlayer));
        }

        public void OnJoinedRoom()
        {
            // Enable: PhotonNetwork.CloseConnection needs to to work across all clients - to kick off invalid players!
            PhotonRealtimeClient.EnableCloseConnection = true;
            LobbyOnJoinedRoom?.Invoke();
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

            LobbyOnCreatedRoom?.Invoke();
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
        public void OnFriendListUpdate(List<FriendInfo> friendList) { LobbyOnFriendListUpdate?.Invoke(); }
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
        public void OnMasterClientSwitched(Player newMasterClient) { LobbyOnMasterClientSwitched?.Invoke(new(newMasterClient)); }

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
    }
}
