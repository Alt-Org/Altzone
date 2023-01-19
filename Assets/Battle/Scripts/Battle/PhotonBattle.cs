using System.Collections.Generic;
using System.Diagnostics;
using Altzone.Scripts.Config;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Battle
{
    /// <summary>
    /// Custom property names and values and helper methods to create, update and read them.<br />
    /// Additionally this has methods that supports testing without dependencies to other modules like Lobby.
    /// </summary>
    internal class PhotonBattle : MonoBehaviour
    {
        public const string PlayerPositionKey = "pp";
        public const string PlayerPrefabIdKey = "mk";

        public const int PlayerPositionGuest = 0;
        public const int PlayerPosition1 = 1;
        public const int PlayerPosition2 = 2;
        public const int PlayerPosition3 = 3;
        public const int PlayerPosition4 = 4;
        public const int PlayerPositionSpectator = 10;

        public const int NoTeamValue = 0;
        public const int TeamBlueValue = 1;
        public const int TeamRedValue = 2;

        private const string NoPlayerName = "noname";

        public static int GetPlayerPos(Player player)
        {
            return player.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
        }

        public static int GetPlayerPrefabId(Player player)
        {
            return player.GetCustomProperty(PlayerPrefabIdKey, -1);
        }

        public static int GetTeamNumber(int playerPos)
        {
            switch (playerPos)
            {
                case PlayerPosition1:
                case PlayerPosition2:
                    return TeamBlueValue;

                case PlayerPosition3:
                case PlayerPosition4:
                    return TeamRedValue;

                default:
                    return NoTeamValue;
            }
        }

        public static bool IsPlayerPosAvailable(Player player)
        {
            if (!PhotonNetwork.InRoom)
            {
                return false;
            }
            var playerPos = GetPlayerPos(player);
            if (!IsValidPlayerPos(playerPos))
            {
                return false;
            }
            foreach (var otherPlayer in PhotonNetwork.PlayerListOthers)
            {
                var otherPlayerPos = GetPlayerPos(otherPlayer);
                if (IsValidPlayerPos(otherPlayerPos) && otherPlayerPos == playerPos)
                {
                    return false;
                }
            }
            return true;
        }

        public static string GetLocalPlayerName()
        {
            if (PhotonNetwork.InRoom && !string.IsNullOrWhiteSpace(PhotonNetwork.NickName))
            {
                return PhotonNetwork.NickName;
            }
            var playerData = GameConfig.Get().PlayerSettings;
            return !string.IsNullOrWhiteSpace(playerData.PlayerName) ? playerData.PlayerName : NoPlayerName;
        }

        public static bool IsRealPlayer(Player player)
        {
            var playerPos = player.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
            return IsValidPlayerPos(playerPos);
        }

        public static bool IsValidPlayerPos(int playerPos)
        {
            return playerPos >= PlayerPosition1 && playerPos <= PlayerPosition4;
        }

        public static int GetFirstFreePlayerPos(Player player, int wantedPlayerPos = PlayerPosition1)
        {
            var usedPlayerPositions = new HashSet<int>();
            foreach (var otherPlayer in PhotonNetwork.PlayerList)
            {
                if (otherPlayer.Equals(player))
                {
                    continue;
                }
                var otherPlayerPos = GetPlayerPos(otherPlayer);
                if (IsValidPlayerPos(otherPlayerPos))
                {
                    usedPlayerPositions.Add(otherPlayerPos);
                }
            }
            if (usedPlayerPositions.Contains(wantedPlayerPos))
            {
                var playerPositions = new[] { PlayerPosition1, PlayerPosition2, PlayerPosition3, PlayerPosition4 };

                foreach (var playerPos in playerPositions)
                {
                    if (!usedPlayerPositions.Contains(playerPos))
                    {
                        wantedPlayerPos = playerPos;
                        break;
                    }
                }
            }
            if (!IsValidPlayerPos(wantedPlayerPos))
            {
                wantedPlayerPos = PlayerPositionSpectator;
            }
            return wantedPlayerPos;
        }

        #region Debug and test utilities

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void SetDebugPlayer(Player player, int wantedPlayerPos)
        {
            wantedPlayerPos = GetFirstFreePlayerPos(player, wantedPlayerPos);
            SetDebugPlayerProps(player, wantedPlayerPos);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void SetDebugPlayerProps(Player player, int playerPos)
        {
            Assert.IsTrue(IsValidPlayerPos(playerPos), "IsValidPlayerPos(playerPos)");
            var playerId = 1;
            var curPlayerPos = player.GetCustomProperty(PlayerPositionKey, -1);
            var curPlayerId = player.GetCustomProperty(PlayerPrefabIdKey, -1);
            if (curPlayerPos == playerPos && curPlayerId == playerId)
            {
                // Prevent setting same values because it is hard for client to keep track of asynchronous changes over network.
                return;
            }
            player.SetCustomProperties(new Hashtable
            {
                { PlayerPositionKey, playerPos },
                { PlayerPrefabIdKey, playerId }
            });
            Debug.LogWarning($"{player.GetDebugLabel()} playerPos {playerPos} skill {playerId}");
        }

        #endregion
    }
}
