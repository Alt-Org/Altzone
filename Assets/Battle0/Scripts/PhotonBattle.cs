using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using Altzone.Scripts.Model.Dto;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle0.Scripts
{
    public static class PhotonBattle
    {
        #region Custom property names and values

        public const string PlayerPositionKey = "pp";
        public const string PlayerCountKey = "pc";
        public const string PlayerMainSkillKey = "mk";
        public const string TeamBlueNameKey = "tb";
        public const string TeamRedNameKey = "tr";
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
        public const int TeamBlueValue = 1;
        public const int TeamRedValue = 2;

        public const int WinTypeNone = 0;
        public const int WinTypeScore = 1;
        public const int WinTypeResign = 2;
        public const int WinTypeDraw = 3;

        private const string NoPlayerName = "noname";

        #endregion

        #region Common datatypes

        /// <summary>
        /// Battle room score data.
        /// </summary>
        /// <remarks>
        /// All fields should be zero or positive for valid gameplay result.
        /// </remarks>
        public class RoomScore
        {
            public readonly int WinType;
            public readonly int WinningTeam;
            public readonly int BlueScore;
            public readonly int RedScore;

            /// <summary>
            /// Check that all fields has received a value from network before accessing score data.
            /// </summary>
            /// <remarks>
            /// Setting custom properties for the room must set all fields with proper (>= 0) values.
            /// </remarks>
            public bool IsValid => !(WinType < 0 || WinningTeam < 0 || BlueScore < 0 || RedScore < 0);

            public RoomScore(int winType, int winningTeam, int blueScore, int redScore)
            {
                WinType = winType;
                WinningTeam = winningTeam;
                BlueScore = blueScore;
                RedScore = redScore;
            }

            public override string ToString()
            {
                return
                    $"{nameof(WinType)}: {WinType}, {nameof(WinningTeam)}: {WinningTeam}, {nameof(BlueScore)}: {BlueScore}, {nameof(RedScore)}: {RedScore}";
            }
        }

        #endregion

        #region Player custom properties etc.

        public static bool IsRealPlayer(Player player)
        {
            var playerPos = player.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
            return IsValidPlayerPos(playerPos);
        }

        public static int GetFirstFreePlayerPos(Player player, int wantedPlayerPos = PlayerPosition1, bool isAllocateByTeams = false)
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
                var playerPositions = isAllocateByTeams
                    ? new[] { PlayerPosition2, PlayerPosition3, PlayerPosition4, PlayerPosition1 }
                    : new[] { PlayerPosition3, PlayerPosition2, PlayerPosition4, PlayerPosition1 };
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

        public static int CountRealPlayers()
        {
            return PhotonNetwork.CurrentRoom.Players.Values.Where(IsRealPlayer).Count();
        }

        public static bool IsValidGameplayPos(int playerPos)
        {
            return IsValidPlayerPos(playerPos) || playerPos == PlayerPositionSpectator;
        }

        public static bool IsValidGameplayPosOrGuest(int playerPos)
        {
            return IsValidGameplayPos(playerPos) || playerPos == PlayerPositionGuest;
        }

        public static bool IsValidPlayerPos(int playerPos)
        {
            return playerPos >= PlayerPosition1 && playerPos <= PlayerPosition4;
        }

        public static int GetPlayerPos(Player player)
        {
            return player.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
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

        public static string GetLocalPlayerName()
        {
            if (PhotonNetwork.InRoom && !string.IsNullOrWhiteSpace(PhotonNetwork.NickName))
            {
                return PhotonNetwork.NickName;
            }
            var playerData = GameConfig.Get().PlayerSettings;
            return !string.IsNullOrWhiteSpace(playerData.PlayerName) ? playerData.PlayerName : NoPlayerName;
        }

        public static int GetPlayerIndex(int playerPos)
        {
            Assert.IsTrue(playerPos >= PlayerPosition1 && playerPos <= PlayerPosition4,
                "playerPos >= PlayerPosition1 && playerPos <= PlayerPosition4");
            return playerPos - 1;
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

        public static int GetOppositeTeamNumber(int teamNumber)
        {
            return teamNumber == TeamBlueValue ? TeamRedValue : teamNumber == TeamRedValue ? TeamBlueValue : NoTeamValue;
        }

        public static int GetTeamIndex(int teamNumber)
        {
            Assert.IsTrue(teamNumber >= TeamBlueValue && teamNumber <= TeamRedValue,
                "teamNumber >= TeamBlueValue && teamNumber <= TeamRedValue");
            return teamNumber == TeamBlueValue ? 0 : 1;
        }

        public static int GetTeamMatePos(int playerPos)
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

        #endregion

        #region Room custom properties etc.

        public static int GetPlayerCountForRoom()
        {
            if (!PhotonNetwork.InRoom)
            {
                return 0;
            }
            var room = PhotonNetwork.CurrentRoom;
            var value = room.GetCustomProperty(PlayerCountKey, 0);
            return value;
        }

        public static RoomScore GetRoomScore()
        {
            if (!PhotonNetwork.InRoom)
            {
                return new RoomScore(WinTypeNone, NoTeamValue, 0, 0);
            }
            var room = PhotonNetwork.CurrentRoom;
            var winType = room.GetCustomProperty(TeamWinTypeKey, -1);
            var winnerTeam = room.GetCustomProperty(TeamWinKey, -1);
            var blueScore = room.GetCustomProperty(TeamBlueScoreKey, -1);
            var redScore = room.GetCustomProperty(TeamRedScoreKey, -1);
            return new RoomScore(winType, winnerTeam, blueScore, redScore);
        }

        public static void SetRoomScores(Room room, int winType, int winningTeam, int blueScore, int redScore)
        {
            Assert.IsTrue(winType >= 0 && winType <= 3, "winType >= 0 && winType <= 3");
            Assert.IsTrue(winningTeam >= 0 && winningTeam <= 2, "winningTeam >= 0 && winningTeam <= 2");
            Assert.IsTrue(blueScore >= 0, "blueScore >= 0");
            Assert.IsTrue(redScore >= 0, "redScore >= 0");
            if (winType == WinTypeDraw)
            {
                Assert.IsTrue(winningTeam == NoTeamValue, "winningTeam == NoTeamValue");
                Assert.IsTrue(blueScore == redScore, "blueScore == redScore");
            }
            else if (winType == WinTypeScore)
            {
                Assert.IsTrue(winningTeam == TeamBlueValue || winningTeam == TeamRedValue,
                    "winningTeam == TeamBlueValue || winningTeam == TeamRedValue");
                Assert.IsTrue(blueScore != redScore, "blueScore != redScore");
                if (winningTeam == TeamBlueValue)
                {
                    Assert.IsTrue(blueScore > redScore, "blueScore > redScore");
                }
                else
                {
                    Assert.IsTrue(redScore > blueScore, "redScore > blueScore");
                }
            }
            var props = new Hashtable
            {
                { TeamWinTypeKey, winType },
                { TeamWinKey, winningTeam },
                { TeamBlueScoreKey, blueScore },
                { TeamRedScoreKey, redScore },
            };
            room.SetCustomProperties(props);
        }

        public static void ResetRoomScores(Room room)
        {
            var props = new Hashtable
            {
                { TeamWinTypeKey, WinTypeNone },
                { TeamWinKey, NoTeamValue },
                { TeamBlueScoreKey, 0 },
                { TeamRedScoreKey, 0 },
            };
            room.SetCustomProperties(props);
        }

        #endregion

        #region MyRegion

        /// <summary>
        /// Gets <c>IBattleCharacter</c> for a player in a room.
        /// </summary>
        public static IBattleCharacter GetCharacterModelForPlayer(Player player)
        {
            Assert.IsTrue(PhotonNetwork.InRoom, "PhotonNetwork.InRoom");
            var skillId = player.GetCustomProperty(PlayerMainSkillKey, -1);
            if (!Enum.TryParse(skillId.ToString(), out Defence defence))
            {
                var playerDataCache = GameConfig.Get().PlayerSettings;
                defence = Storefront.Get().GetBattleCharacter(playerDataCache.CustomCharacterModelId).MainDefence;
            }
            return Storefront.Get().GetBattleCharacter((int)defence);
        }

        #endregion

        #region Debug and test utilities

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void SetDebugRoomProperties(Room room, int playerCount, string teamBlueName = "Blue", string teamRedName = "Red")
        {
            room.SetCustomProperties(new Hashtable
            {
                { TeamBlueNameKey, teamBlueName },
                { TeamRedNameKey, teamRedName },
                { PlayerCountKey, playerCount }
            });
            Debug.LogWarning($"{room.GetDebugLabel()} players {playerCount} blue {teamBlueName} red {teamRedName}");
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void SetDebugPlayer(Player player, int wantedPlayerPos, bool isAllocateByTeams, int playerMainSkill)
        {
            wantedPlayerPos = GetFirstFreePlayerPos(player, wantedPlayerPos, isAllocateByTeams);
            SetDebugPlayerProps(player, wantedPlayerPos, playerMainSkill);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void SetDebugPlayerProps(Player player, int playerPos, int playerMainSkill = -1)
        {
            Assert.IsTrue(IsValidPlayerPos(playerPos), "IsValidPlayerPos(playerPos)");
            if (playerMainSkill < (int)Defence.Desensitisation || playerMainSkill > (int)Defence.Confluence)
            {
                // Should be fastest movement skill to have for testing.
                playerMainSkill = (int)Defence.Deflection;
            }
            var curPlayerPos = player.GetCustomProperty(PlayerPositionKey, -1);
            var curPlayerMainSkill = player.GetCustomProperty(PlayerMainSkillKey, -1);
            if (curPlayerPos == playerPos && curPlayerMainSkill == playerMainSkill)
            {
                // Prevent setting same values because it is hard for client to keep track of asynchronous changes over network.
                return;
            }
            player.SetCustomProperties(new Hashtable
            {
                { PlayerPositionKey, playerPos },
                { PlayerMainSkillKey, playerMainSkill }
            });
            Debug.LogWarning($"{player.GetDebugLabel()} playerPos {playerPos} skill {playerMainSkill}");
        }

        #endregion
    }
}