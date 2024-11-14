using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts;
using MenuUi.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;
using Photon.Client;
//using Battle1.PhotonUnityNetworking.Code;
//using MenuUi.Scripts.Window;
//using MenuUi.Scripts.Window.ScriptableObjects;
using Photon.Realtime;
using Prg.Scripts.Common.PubSub;
using Quantum;
using UnityEngine;
using UnityEngine.Assertions;
using Assert = UnityEngine.Assertions.Assert;
//using DisconnectCause = Battle1.PhotonRealtime.Code.DisconnectCause;
using Hashtable = ExitGames.Client.Photon.Hashtable;
//using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;
//using Player = Battle1.PhotonRealtime.Code.Player;

namespace MenuUI.Scripts.Lobby
{
    /// <summary>
    /// Manages local player position and setup in a room.
    /// </summary>
    /// <remarks>
    /// Game settings are saved in player custom properties for each participating player.
    /// </remarks>
    public class LobbyManager : MonoBehaviour, ILobbyCallbacks, IMatchmakingCallbacks, IOnEventCallback
    {
        private const string BattleID = PhotonBattle.BattleID;

        private const string PlayerPositionKey = PhotonBattle.PlayerPositionKey;
        private const string PlayerCountKey = PhotonBattle.PlayerCountKey;
        private const int PlayerPositionGuest = PhotonBattle.PlayerPositionGuest;

        private const int PlayerPositionSpectator = PhotonBattle.PlayerPositionSpectator;

        private const string TeamBlueNameKey = PhotonBattle.TeamAlphaNameKey;
        private const string TeamRedNameKey = PhotonBattle.TeamBetaNameKey;

        [Header("Settings"), SerializeField] private WindowDef _mainMenuWindow;
        [SerializeField] private WindowDef _roomWindow;
        [SerializeField] private WindowDef _gameWindow;
        [SerializeField] private bool _isCloseRoomOnGameStart;
        [SerializeField] private SceneDef _raidScene;

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

        public static LobbyManager Instance { get; private set; }

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
                PhotonRealtimeClient.Client.Service();
                Debug.LogWarning(".");
                yield return new WaitForSeconds(0.1f);
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
            WindowManager.Get().ShowWindow(_roomWindow);
        }

        private void OnStartPlayingEvent(StartPlayingEvent data)
        {
            Debug.Log($"onEvent {data}");
            StartCoroutine(StartTheGameplay(_gameWindow, _isCloseRoomOnGameStart, _blueTeamName, _redTeamName));
        }

        private void OnStartRaidTestEvent(StartRaidTestEvent data)
        {
            Debug.Log($"onEvent {data}");
            StartCoroutine(StartTheRaidTestRoom(_raidScene));
        }

        private IEnumerator StartTheGameplay(WindowDef gameWindow, bool isCloseRoom, string blueTeamName, string redTeamName)
        {
            Debug.Log($"startTheGameplay {gameWindow}");
            if (!PhotonRealtimeClient.LocalPlayer.IsMasterClient)
            {
                throw new UnityException("only master client can start the game");
            }
            var player = PhotonRealtimeClient.LocalPlayer;
            var masterPosition = player.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
            if (!PhotonBattle.IsValidPlayerPos(masterPosition))
            {
                throw new UnityException($"master client does not have valid player position: {masterPosition}");
            }
            // Snapshot player list before iteration because we can change it
            var players = PhotonRealtimeClient.CurrentRoom.Players.Values.ToList();
            var realPlayerCount = 0;
            foreach (var roomPlayer in players)
            {
                var playerPos = roomPlayer.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
                if (PhotonBattle.IsValidPlayerPos(playerPos))
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
                var room = PhotonRealtimeClient.CurrentRoom;
                room.CustomProperties.Add(TeamBlueNameKey, blueTeamName);
                room.CustomProperties.Add(TeamRedNameKey, redTeamName);
                room.CustomProperties.Add(PlayerCountKey, realPlayerCount);
                /*room.SetCustomProperties(new Hashtable
                {
                    { TeamBlueNameKey, blueTeamName },
                    { TeamRedNameKey, redTeamName },
                    { PlayerCountKey, realPlayerCount }
                });*/
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

        private async void StartQuantum()
        {
            if(QuantumRunner.Default != null)
            {
                Debug.Log($"QuantumRunner is already running: {QuantumRunner.Default.Id}");
                return;
            }

            RuntimeConfig config = new RuntimeConfig();

            config.Map = _map;
            config.SimulationConfig = _simulationConfig;
            config.SystemsConfig = _systemsConfig;

            var sessionRunnerArguments = new SessionRunner.Arguments
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

            QuantumRunner runner = (QuantumRunner) await SessionRunner.StartAsync(sessionRunnerArguments);

            //runner.Game.AddPlayer(_player);
        }

        private static IEnumerator StartTheRaidTestRoom(SceneDef raidScene)
        {
            Debug.Log($"RAID TEST {raidScene}");
            if (!PhotonRealtimeClient.LocalPlayer.IsMasterClient)
            {
                throw new UnityException("only master client can start this game");
            }
            yield return null;
            if (!raidScene.IsNetworkScene)
            {
                throw new UnityException($"scene {raidScene} IsNetworkScene = false");
            }
            PhotonRealtimeClient.LoadLevel(raidScene.SceneName);
            //StartCoroutine(StartTheRaidTestRoom(_raidScene));
        }

        private void SetPlayer(Player player, int playerPosition)
        {
            Assert.IsTrue(PhotonBattle.IsValidGameplayPosOrGuest(playerPosition));
            if (!player.HasCustomProperty(PlayerPositionKey))
            {
                Debug.Log($"setPlayer {PlayerPositionKey}={playerPosition}");
                player.SetCustomProperties(new PhotonHashtable { { PlayerPositionKey, playerPosition } });
                return;
            }
            var curValue = player.GetCustomProperty<int>(PlayerPositionKey);
            Debug.Log($"setPlayer {PlayerPositionKey}=({curValue}<-){playerPosition}");
            player.SafeSetCustomProperty(PlayerPositionKey, playerPosition, curValue);
        }

        public void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log($"OnDisconnected {cause}");
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

        public void OnJoinedLobby()
        {
            StartCoroutine(Service());
        }
        public void OnLeftLobby() => throw new NotImplementedException();
        public void OnRoomListUpdate(List<RoomInfo> roomList)
        {

        }
        public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics) => throw new NotImplementedException();
        public void OnFriendListUpdate(List<FriendInfo> friendList) => throw new NotImplementedException();
        public void OnCreatedRoom()
        {
            StartCoroutine(Service());
        }
        public void OnCreateRoomFailed(short returnCode, string message) => throw new NotImplementedException();
        public void OnJoinRoomFailed(short returnCode, string message) => throw new NotImplementedException();
        public void OnJoinRandomFailed(short returnCode, string message) => throw new NotImplementedException();
        public void OnEvent(EventData photonEvent)
        {
            Debug.Log($"Received PhotonEvent {photonEvent.Code}");

            switch (photonEvent.Code)
            {
                case PhotonRealtimeClient.PhotonEvent.StartGame:
                    StartQuantum();
                    break;
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
    }
}
