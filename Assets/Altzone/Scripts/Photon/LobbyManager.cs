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
using Altzone.Scripts.Model.Poco.Player;
using Prg.Scripts.Common.PubSub;

using System.Threading.Tasks;
using Altzone.Scripts.Battle.Photon;

namespace Altzone.Scripts.Lobby
{
    public enum LobbyWindowTarget
    {
        MainMenu,
        LobbyRoom,
        Battle,
        Raid
    }

    /// <summary>
    /// Manages local player position and setup in a room.
    /// </summary>
    /// <remarks>
    /// Game settings are saved in player custom properties for each participating player.
    /// </remarks>
    public class LobbyManager : MonoBehaviour, ILobbyCallbacks, IMatchmakingCallbacks, IOnEventCallback, IConnectionCallbacks
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

        [Header("Team Names"), SerializeField] private string _blueTeamName;
        [SerializeField] private string _redTeamName;

        [Header("Player"), SerializeField]
        private RuntimePlayer _player;

        [Header("Configs"), SerializeField]
        private SimulationConfig _simulationConfig;
        [SerializeField]
        private SystemsConfig _systemsConfig;
        [SerializeField]
        private Map _map;

        private QuantumRunner _runner = null;

        public static LobbyManager Instance { get; private set; }

        public delegate void LobbyWindowChangeRequest(LobbyWindowTarget target);
        public static event LobbyWindowChangeRequest OnLobbyWindowChangeRequest;


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
                Debug.LogWarning(".");
                yield return new WaitForSeconds(0.1f);
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
                    PhotonRealtimeClient.JoinLobby(null);
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
            Debug.Log($"onEvent {data}");
            SetPlayer(PhotonRealtimeClient.LocalPlayer, data.PlayerPosition);
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
            //Debug.Log($"startTheGameplay {gameWindow}");
            if (!PhotonRealtimeClient.LocalPlayer.IsMasterClient)
            {
                throw new UnityException("only master client can start the game");
            }
            Player player = PhotonRealtimeClient.LocalPlayer;
            int masterPosition = player.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
            if (!PhotonBattleRoom.IsValidPlayerPos(masterPosition))
            {
                throw new UnityException($"master client does not have valid player position: {masterPosition}");
            }
            // Snapshot player list before iteration because we can change it
            List<Player> players = PhotonRealtimeClient.CurrentRoom.Players.Values.ToList();
            int realPlayerCount = 0;
            foreach (Player roomPlayer in players)
            {
                int playerPos = roomPlayer.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
                if (PhotonBattleRoom.IsValidPlayerPos(playerPos))
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
            if (!PhotonRealtimeClient.Client.OpRaiseEvent(PhotonRealtimeClient.PhotonEvent.StartGame,null, new RaiseEventArgs{Receivers = ReceiverGroup.All}, SendOptions.SendReliable))
            {
                Debug.LogError("Unable to start game.");
                yield break;
            }
            Debug.Log("Starting Game");
            //WindowManager.Get().ShowWindow(gameWindow);
        }

        private IEnumerator StartQuantum()
        {
            if (QuantumRunner.Default != null)
            {
                Debug.Log($"QuantumRunner is already running: {QuantumRunner.Default.Id}");
                yield break;
            }

            RuntimeConfig config = new()
            {
                Map = _map,
                SimulationConfig = _simulationConfig,
                SystemsConfig = _systemsConfig
            };

            SessionRunner.Arguments sessionRunnerArguments = new()
            {
                RunnerFactory = QuantumRunnerUnityFactory.DefaultFactory,
                GameParameters = QuantumRunnerUnityFactory.CreateGameParameters,
                ClientId = ServerManager.Instance.Player._id,
                RuntimeConfig = config,
                SessionConfig = QuantumDeterministicSessionConfigAsset.Global.Config,
                GameMode = Photon.Deterministic.DeterministicGameMode.Multiplayer,
                PlayerCount = PhotonRealtimeClient.CurrentRoom.MaxPlayers,
                StartGameTimeoutInSeconds = 10,
                Communicator = new QuantumNetworkCommunicator(PhotonRealtimeClient.Client)
            };

            /*Transform currentRoot = null;
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                if(root.name == "DefaultWindow")
                {
                    currentRoot = root.transform;
                }
            }*/

            //WindowManager.Get().ShowWindow(_gameWindow);
            OnLobbyWindowChangeRequest?.Invoke(LobbyWindowTarget.Battle);

            yield return new WaitUntil(()=>SceneManager.GetActiveScene().name == _map.Scene);

            Task<bool> task = StartRunner(sessionRunnerArguments);

            /*QuantumRunner runner = null;
            try
            {
                runner = (QuantumRunner)await SessionRunner.StartAsync(sessionRunnerArguments);
            }catch (Exception ex)
            {
                pluginDisconnectListener.Dispose();
                Debug.LogException(ex);
            }
            foreach (Transform window in currentRoot)
            {
                Debug.Log(window.name);
                if (window.gameObject.activeSelf == true)
                {
                    window.gameObject.SetActive(false);
                }
            }*/
            yield return new WaitUntil(() => task.IsCompleted);
            if(task.Result)
            _runner?.Game.AddPlayer(_player);
            else
                //WindowManager.Get().GoBack();
                OnLobbyWindowChangeRequest?.Invoke(LobbyWindowTarget.MainMenu);
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
            Assert.IsTrue(PhotonBattleRoom.IsValidGameplayPosOrGuest(playerPosition));
            if (!player.HasCustomProperty(PlayerPositionKey))
            {
                Debug.Log($"setPlayer {PlayerPositionKey}={playerPosition}");
                player.SetCustomProperties(new PhotonHashtable { { PlayerPositionKey, playerPosition } });
                return;
            }
            int curValue = player.GetCustomProperty<int>(PlayerPositionKey);
            Debug.Log($"setPlayer {PlayerPositionKey}=({curValue}<-){playerPosition}");
            player.SafeSetCustomProperty(PlayerPositionKey, playerPosition, curValue);
        }

        public void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log($"OnDisconnected {cause}");
            GameConfig gameConfig = GameConfig.Get();
            PlayerSettings playerSettings = gameConfig.PlayerSettings;
            string photonRegion = string.IsNullOrEmpty(playerSettings.PhotonRegion) ? null : playerSettings.PhotonRegion;
            StartCoroutine(StartLobby(playerSettings.PlayerGuid, playerSettings.PhotonRegion));
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log($"OnPlayerLeftRoom {otherPlayer.GetDebugLabel()}");
        }

        public void OnJoinedRoom()
        {
            // Enable: PhotonNetwork.CloseConnection needs to to work across all clients - to kick off invalid players!
            PhotonRealtimeClient.EnableCloseConnection = true;
        }

        public void OnLeftRoom() // IMatchmakingCallbacks
        {
            Debug.Log($"OnLeftRoom {PhotonRealtimeClient.LocalPlayer.GetDebugLabel()}");
            StartCoroutine(Service());
            // Goto lobby if we left (in)voluntarily any room
            // - typically master client kicked us off before starting a new game as we did not qualify to participate.
            // - can not use GoBack() because we do not know the reason for player leaving the room.
            // UPDATE 17.11.2023 - Since Lobby-scene has been moved to the main menu we will now load the main menu instead.
            //WindowManager.Get().ShowWindow(_mainMenuWindow);
        }

        public void OnCreatedRoom() { StartCoroutine(Service()); }
        public void OnJoinedLobby() { StartCoroutine(Service()); }

        public void OnRoomListUpdate(List<RoomInfo> roomList) {}
        public void OnLeftLobby() => throw new NotImplementedException();
        public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics) => throw new NotImplementedException();
        public void OnFriendListUpdate(List<FriendInfo> friendList) => throw new NotImplementedException();
        public void OnCreateRoomFailed(short returnCode, string message) => throw new NotImplementedException();
        public void OnJoinRoomFailed(short returnCode, string message) => throw new NotImplementedException();
        public void OnJoinRandomFailed(short returnCode, string message) => throw new NotImplementedException();

        public void OnEvent(EventData photonEvent)
        {
            Debug.Log($"Received PhotonEvent {photonEvent.Code}");

            switch (photonEvent.Code)
            {
                case PhotonRealtimeClient.PhotonEvent.StartGame:
                    StartCoroutine(StartQuantum());
                    break;
            }
        }

        public void OnConnected() { }
        public void OnConnectedToMaster() { }
        public void OnRegionListReceived(RegionHandler regionHandler) { }
        public void OnCustomAuthenticationResponse(Dictionary<string, object> data) => throw new NotImplementedException();
        public void OnCustomAuthenticationFailed(string debugMessage) => throw new NotImplementedException();

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