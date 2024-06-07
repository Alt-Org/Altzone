using System.Collections;
using System.Linq;
using MenuUi.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.Assertions;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace MenuUI.Scripts.Lobby
{
    /// <summary>
    /// Manages local player position and setup in a room.
    /// </summary>
    /// <remarks>
    /// Game settings are saved in player custom properties for each participating player.
    /// </remarks>
    public class LobbyManager : MonoBehaviourPunCallbacks
    {
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

        public override void OnEnable()
        {
            base.OnEnable();
            this.Subscribe<PlayerPosEvent>(OnPlayerPosEvent);
            this.Subscribe<StartRoomEvent>(OnStartRoomEvent);
            this.Subscribe<StartPlayingEvent>(OnStartPlayingEvent);
            this.Subscribe<StartRaidTestEvent>(OnStartRaidTestEvent);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            this.Unsubscribe();
        }

        private void OnApplicationQuit()
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonLobby.LeaveRoom();
            }
            else if (PhotonNetwork.InLobby)
            {
                PhotonLobby.LeaveLobby();
            }
        }

        private void OnPlayerPosEvent(PlayerPosEvent data)
        {
            Debug.Log($"onEvent {data}");
            SetPlayer(PhotonNetwork.LocalPlayer, data.PlayerPosition);
        }

        private void OnStartRoomEvent(StartRoomEvent data)
        {
            Debug.Log($"onEvent {data}");
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
            if (!PhotonNetwork.IsMasterClient)
            {
                throw new UnityException("only master client can start the game");
            }
            var player = PhotonNetwork.LocalPlayer;
            var masterPosition = player.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
            if (!PhotonBattle.IsValidPlayerPos(masterPosition))
            {
                throw new UnityException($"master client does not have valid player position: {masterPosition}");
            }
            // Snapshot player list before iteration because we can change it
            var players = PhotonNetwork.CurrentRoom.Players.Values.ToList();
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
                PhotonNetwork.CloseConnection(roomPlayer);
                yield return null;
            }
            if (player.IsMasterClient)
            {
                Assert.IsTrue(!string.IsNullOrWhiteSpace(blueTeamName), "!string.IsNullOrWhiteSpace(blueTeamName)");
                Assert.IsTrue(!string.IsNullOrWhiteSpace(redTeamName), "!string.IsNullOrWhiteSpace(redTeamName)");
                var room = PhotonNetwork.CurrentRoom;
                room.SetCustomProperties(new Hashtable
                {
                    { TeamBlueNameKey, blueTeamName },
                    { TeamRedNameKey, redTeamName },
                    { PlayerCountKey, realPlayerCount }
                });
                yield return null;
                if (isCloseRoom)
                {
                    PhotonLobby.CloseRoom();
                    yield return null;
                }
            }
            WindowManager.Get().ShowWindow(gameWindow);
        }

        private static IEnumerator StartTheRaidTestRoom(SceneDef raidScene)
        {
            Debug.Log($"RAID TEST {raidScene}");
            if (!PhotonNetwork.IsMasterClient)
            {
                throw new UnityException("only master client can start this game");
            }
            yield return null;
            if (!raidScene.IsNetworkScene)
            {
                throw new UnityException($"scene {raidScene} IsNetworkScene = false");
            }
            PhotonNetwork.LoadLevel(raidScene.SceneName);
        }
        
        private void SetPlayer(Player player, int playerPosition)
        {
            Assert.IsTrue(PhotonBattle.IsValidGameplayPosOrGuest(playerPosition));
            if (!player.HasCustomProperty(PlayerPositionKey))
            {
                Debug.Log($"setPlayer {PlayerPositionKey}={playerPosition}");
                player.SetCustomProperties(new Hashtable { { PlayerPositionKey, playerPosition } });
                return;
            }
            var curValue = player.GetCustomProperty<int>(PlayerPositionKey);
            Debug.Log($"setPlayer {PlayerPositionKey}=({curValue}<-){playerPosition}");
            player.SafeSetCustomProperty(PlayerPositionKey, playerPosition, curValue);
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log($"OnDisconnected {cause}");
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log($"OnPlayerLeftRoom {otherPlayer.GetDebugLabel()}");
        }

        public override void OnJoinedRoom()
        {
            // Enable: PhotonNetwork.CloseConnection needs to to work across all clients - to kick off invalid players!
            PhotonNetwork.EnableCloseConnection = true;
        }

        public override void OnLeftRoom() // IMatchmakingCallbacks
        {
            Debug.Log($"OnLeftRoom {PhotonNetwork.LocalPlayer.GetDebugLabel()}");
            // Goto lobby if we left (in)voluntarily any room
            // - typically master client kicked us off before starting a new game as we did not qualify to participate.
            // - can not use GoBack() because we do not know the reason for player leaving the room.
            // UPDATE 17.11.2023 - Since Lobby-scene has been moved to the main menu we will now load the main menu instead.
            WindowManager.Get().ShowWindow(_mainMenuWindow);
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
