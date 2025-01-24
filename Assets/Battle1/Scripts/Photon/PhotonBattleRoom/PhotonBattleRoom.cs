using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Altzone.Scripts.Model.Poco.Game;
/*using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;
using Player = Battle1.PhotonRealtime.Code.Player;
using Room = Battle1.PhotonRealtime.Code.Room;*/

namespace Battle1.Scripts.Battle.Photon
{
    public class PhotonBattleRoom
    {
        #region Custom property names and values

        public const string BattleID = "bid";
        public const string PlayerPositionKey = "pp";
        public const string PlayerCountKey = "pc";
        public const string PlayerCharacterIdKey = "mk";
        public const string PlayerCharacterIdsKey = "ci";
        public const string PlayerStatsKey = "cs";
        public const string TeamAlphaNameKey = "tb";
        public const string TeamBetaNameKey = "tr";
        public const string TeamWinTypeKey = "wt";
        public const string TeamWinKey = "tw";
        public const string TeamBlueScoreKey = "t1";
        public const string TeamRedScoreKey = "t2";

        //  Red team number 2
        //  - Player numbers 3 and 4
        // | ======= |
        // |  4 |  3 |      Team number 0
        // | ======= |      - Guest number is 0
        // |  1 |  2 |      - Spectator number is 10
        // | ======= |
        //  Blue team number 1
        //  - Player numbers 1 and 2

        // Player should be positioned so that if camera is rotated 180 degrees,
        // player with smaller number is always at the bottom of the left corner of the gameplay area.

        public const int PlayerPositionGuest = 0;
        public const int PlayerPosition1 = 1;
        public const int PlayerPosition2 = 2;
        public const int PlayerPosition3 = 3;
        public const int PlayerPosition4 = 4;
        public const int PlayerPositionSpectator = 10;

        public const int NoTeamValue = 0;
        public const int TeamAlphaValue = 1;
        public const int TeamBetaValue = 2;

        public const int WinTypeNone = 0;
        public const int WinTypeScore = 1;
        public const int WinTypeResign = 2;
        public const int WinTypeDraw = 3;

        private const string NoPlayerName = "noname";

        #endregion

        public bool IsRealPlayer(Player player)
        {
            /*int playerPos = player.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);*/
            int playerPos = 0;
            return IsValidPlayerPos(playerPos);
        }

        public int GetPlayerPos(Player player)
        {
           /* return player.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);*/
            return 0;
        }

        public int GetTeamNumber(Player player)
        {
            int playerPos = GetPlayerPos(player);

            return IsValidPlayerPos(playerPos)
                ? GetTeamNumber(playerPos)
                : NoTeamValue;
        }

        public Player GetTeamPlayer(Player player)
        {
            if (!IsRealPlayer(player)) return null;
            int teamNumber = GetTeamNumber(player);
            /*return PhotonNetwork.PlayerListOthers.FirstOrDefault(x => GetTeamNumber(x) == teamNumber);*/
            return null;
        }

        public int GetFirstFreePlayerPos(Player player, int wantedPlayerPos = PlayerPosition1, bool isAllocateByTeams = false)
        {
            HashSet<int> usedPlayerPositions = new HashSet<int>();
          /*  foreach (Player otherPlayer in PhotonNetwork.PlayerList)
            {
                if (otherPlayer.Equals(player))
                {
                    continue;
                }
                int otherPlayerPos = GetPlayerPos(otherPlayer);
                if (IsValidPlayerPos(otherPlayerPos))
                {
                    usedPlayerPositions.Add(otherPlayerPos);
                }
            }*/
            if (usedPlayerPositions.Contains(wantedPlayerPos))
            {
                int[] playerPositions = isAllocateByTeams
                    ? new[] { PlayerPosition2, PlayerPosition3, PlayerPosition4, PlayerPosition1 }
                    : new[] { PlayerPosition3, PlayerPosition2, PlayerPosition4, PlayerPosition1 };
                foreach (int playerPos in playerPositions)
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

        public int CountRealPlayers()
        {
            /*return PhotonNetwork.CurrentRoom.Players.Values.Sum(x => IsRealPlayer(x) ? 1 : 0);*/
            return 0;
        }

        public bool IsValidGameplayPos(int playerPos)
        {
            return IsValidPlayerPos(playerPos) || playerPos == PlayerPositionSpectator;
        }

        public bool IsValidGameplayPosOrGuest(int playerPos)
        {
            return IsValidGameplayPos(playerPos) || playerPos == PlayerPositionGuest;
        }

        public bool IsValidPlayerPos(int playerPos)
        {
            return playerPos is >= PlayerPosition1 and <= PlayerPosition4;
        }

        public static int GetTeamMemberPlayerPos(int playerPos)
        {
            switch (playerPos)
            {
                case PlayerPosition1:
                    return PlayerPosition2;
                case PlayerPosition2:
                    return PlayerPosition1;
                case PlayerPosition3:
                    return PlayerPosition4;
                case PlayerPosition4:
                    return PlayerPosition3;
                default:
                    throw new UnityException($"invalid playerPos: {playerPos}");
            }
        }

        public string GetLocalPlayerName()
        {
            /*if (PhotonNetwork.InRoom && !string.IsNullOrWhiteSpace(PhotonNetwork.NickName))
            {
                return PhotonNetwork.NickName;
            }*/
            throw new UnityException("Can not get player name outside a room");
        }

        public int GetPlayerIndex(int playerPos)
        {
            Assert.IsTrue(playerPos >= PlayerPosition1 && playerPos <= PlayerPosition4,
                "playerPos >= PlayerPosition1 && playerPos <= PlayerPosition4");
            return playerPos - 1;
        }

        public int GetTeamNumber(int playerPos)
        {
            switch (playerPos)
            {
                case PlayerPosition1:
                case PlayerPosition2:
                    return TeamAlphaValue;
                case PlayerPosition3:
                case PlayerPosition4:
                    return TeamBetaValue;
                default:
                    return NoTeamValue;
            }
        }

        public int GetOppositeTeamNumber(int teamNumber)
        {
            return teamNumber == TeamAlphaValue ? TeamBetaValue : teamNumber == TeamBetaValue ? TeamAlphaValue : NoTeamValue;
        }

        public int GetTeamIndex(int teamNumber)
        {
            Assert.IsTrue(teamNumber >= TeamAlphaValue && teamNumber <= TeamBetaValue,
                "teamNumber >= TeamBlueValue && teamNumber <= TeamRedValue");
            return teamNumber == TeamAlphaValue ? 0 : 1;
        }

        public int GetTeamMatePos(int playerPos)
        {
            switch (playerPos)
            {
                case PlayerPosition1:
                    return PlayerPosition2;
                case PlayerPosition2:
                    return PlayerPosition1;
                case PlayerPosition3:
                    return PlayerPosition4;
                case PlayerPosition4:
                    return PlayerPosition3;
                default:
                    throw new UnityException($"invalid player pos: {playerPos}");
            }
        }

        public bool IsPlayerPosAvailable(Player player)
        {
        /*    if (!PhotonNetwork.InRoom)
            {
                return false;
            }
            int playerPos = GetPlayerPos(player);
            if (!IsValidPlayerPos(playerPos))
            {
                return false;
            }
            foreach (Player otherPlayer in PhotonNetwork.PlayerListOthers)
            {
                int otherPlayerPos = GetPlayerPos(otherPlayer);
                if (IsValidPlayerPos(otherPlayerPos) && otherPlayerPos == playerPos)
                {
                    return false;
                }
            }*/
            return true;
        }

        public int GetPlayerCountForRoom()
        {
            /*if (!PhotonNetwork.InRoom) return 0;
            Room room = PhotonNetwork.CurrentRoom;
            return room.GetCustomProperty(PlayerCountKey, 0);*/
            return 0;
        }

        public CharacterID GetPlayerCharacterID(Player player)
        {
            /*return (CharacterID)player.GetCustomProperty(PlayerCharacterIdKey, -1);*/
            return 0;
        }

        public int GetPrefabIndex(BattleCharacter battleCharacter, int defaultValue)
        {
            if ((int)battleCharacter.CharacterID != -1) return (int)battleCharacter.CharacterID;
            return defaultValue;
        }
        #region Debug and test utilities

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public void SetDebugRoomProperties(Room room, int playerCount, string teamBlueName = "Blue", string teamRedName = "Red")
        {
            room.SetCustomProperties(new Hashtable
            {
                { TeamAlphaNameKey, teamBlueName },
                { TeamBetaNameKey, teamRedName },
                { PlayerCountKey, playerCount }
            });
            Debug.LogWarning($"{room.GetDebugLabel()} players {playerCount} blue {teamBlueName} red {teamRedName}");
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public void SetDebugPlayer(Player player, int wantedPlayerPos, bool isAllocateByTeams, int playerMainSkill)
        {
            wantedPlayerPos = GetFirstFreePlayerPos(player, wantedPlayerPos, isAllocateByTeams);
            SetDebugPlayerProps(player, wantedPlayerPos, playerMainSkill);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public void SetDebugPlayerProps(Player player, int playerPos, int playerMainSkill = -1)
        {
            Assert.IsTrue(IsValidPlayerPos(playerPos), "IsValidPlayerPos(playerPos)");
            if (playerMainSkill < (int)CharacterClassID.Desensitizer || playerMainSkill > (int)CharacterClassID.Confluent)
            {
                // Should be fastest movement skill to have for testing.
                playerMainSkill = (int)CharacterClassID.Trickster;
            }
            /*var curPlayerPos = player.GetCustomProperty(PlayerPositionKey, -1);
            var curPlayerMainSkill = player.GetCustomProperty(PlayerCharacterIdKey, -1);*/
            var curPlayerPos = 0;
            var curPlayerMainSkill = 0;
            if (curPlayerPos == playerPos && curPlayerMainSkill == playerMainSkill)
            {
                // Prevent setting same values because it is hard for client to keep track of asynchronous changes over network.
                return;
            }
            player.SetCustomProperties(new Hashtable
            {
                { PlayerPositionKey, playerPos },
                { PlayerCharacterIdKey, playerMainSkill }
            });
            Debug.LogWarning($"{player.GetDebugLabel()} playerPos {playerPos} skill {playerMainSkill}");
        }

        #endregion
    }
}
