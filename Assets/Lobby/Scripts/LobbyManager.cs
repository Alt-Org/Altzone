using System.Collections;
using System.Linq;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Window;
using Altzone.Scripts.Window.ScriptableObjects;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.Assertions;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Lobby.Scripts
{
    /// <summary>
    /// Manages local player position and setup in a room and controls which level is loaded next.
    /// </summary>
    /// <remarks>
    /// Game settings are saved in player custom properties for each participating player.
    /// </remarks>
    public class LobbyManager : MonoBehaviourPunCallbacks
    {
        private const string PlayerPositionKey = PhotonBattle.PlayerPositionKey;

        private const int PlayerPositionGuest = PhotonBattle.PlayerPositionGuest;
        private const int PlayerPosition1 = PhotonBattle.PlayerPosition1;
        private const int PlayerPosition2 = PhotonBattle.PlayerPosition2;
        private const int PlayerPosition3 = PhotonBattle.PlayerPosition3;
        private const int PlayerPosition4 = PhotonBattle.PlayerPosition4;
        private const int PlayerPositionSpectator = PhotonBattle.PlayerPositionSpectator;
        private const int StartPlayingEvent = PhotonBattle.StartPlayingEvent;

        [SerializeField] private WindowDef _lobbyWindow;
        [SerializeField] private WindowDef _gameWindow;

        public override void OnEnable()
        {
            base.OnEnable();
            this.Subscribe<PlayerPosEvent>(OnPlayerPosEvent);
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
                PhotonLobby.leaveRoom();
            }
            else if (PhotonNetwork.InLobby)
            {
                PhotonLobby.leaveLobby();
            }
        }

        private void OnPlayerPosEvent(PlayerPosEvent data)
        {
            Debug.Log($"onEvent {data}");
            if (data.PlayerPosition == StartPlayingEvent)
            {
                StartCoroutine(StartTheGameplay(_gameWindow));
                return;
            }
            SetPlayer(PhotonNetwork.LocalPlayer, data.PlayerPosition);
        }

        private static IEnumerator StartTheGameplay(WindowDef gameWindow)
        {
            Debug.Log($"startTheGameplay {gameWindow}");
            if (!PhotonNetwork.IsMasterClient)
            {
                throw new UnityException("only master client can start the game");
            }
            var masterPosition = PhotonNetwork.LocalPlayer.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
            if (masterPosition < PlayerPosition1 || masterPosition > PlayerPosition4)
            {
                throw new UnityException($"master client does not have valid player position: {masterPosition}");
            }
            // Snapshot player list before iteration because we can change it
            var players = PhotonNetwork.CurrentRoom.Players.Values.ToList();
            foreach (var player in players)
            {
                var curValue = player.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
                if (curValue >= PlayerPosition1 && curValue <= PlayerPosition4 || curValue == PlayerPositionSpectator)
                {
                    continue;
                }
                Debug.Log($"Kick player (close connection) {player.GetDebugLabel()} {PlayerPositionKey}={curValue}");
                PhotonNetwork.CloseConnection(player);
                yield return null;
            }
            WindowManager.Get().ShowWindow(gameWindow);
        }

        private static void SetPlayer(Player player, int playerPosition)
        {
            Assert.IsTrue(PhotonBattle.IsValidGameplayPos(playerPosition));
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

        public override void OnLeftRoom() // IMatchmakingCallbacks
        {
            Debug.Log($"OnLeftRoom {PhotonNetwork.LocalPlayer.GetDebugLabel()}");
            // Goto lobby if we left (in)voluntarily any room
            // - typically master client kicked us off before starting a new game as we did not qualify to participate.
            // - can not use GoBack() because we do not know the reason for player leaving the room.
            WindowManager.Get().ShowWindow(_lobbyWindow);
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
    }
}