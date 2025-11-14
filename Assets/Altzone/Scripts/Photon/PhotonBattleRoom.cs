using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

using Photon.Realtime;
using Photon.Client;

using Prg.Scripts.Common.PubSub;

using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Lobby.Wrappers;
using static Altzone.Scripts.Lobby.LobbyManager;
//using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;
//using Player = Battle1.PhotonRealtime.Code.Player;
//using Room = Battle1.PhotonRealtime.Code.Room;

namespace Altzone.Scripts.Battle.Photon
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
        public const string MapKey = "m";
        public const string StartingEmotionKey = "e";
        public const string RoomNameKey = "n";
        public const string PasswordKey = "pw";
        public const string GameTypeKey = "gt";
        public const string IsMatchmakingKey = "mm";
        public const string SoulhomeRank = "sr";
        public const string SoulhomeRankVariance = "rv";
        public const string ClanNameKey = "c";
        public const string ClanOpponentNameKey = "c2";
        public const string LeaderIdKey = "lid";

        public static string PlayerPositionKey1 { get => PlayerPosition1.ToString(); }
        public static string PlayerPositionKey2 { get => PlayerPosition2.ToString(); }
        public static string PlayerPositionKey3 { get => PlayerPosition3.ToString(); }
        public static string PlayerPositionKey4 { get => PlayerPosition4.ToString(); }

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
            int playerPos = player.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
            return IsValidPlayerPos(playerPos);
        }

        public int GetPlayerPos(Player player)
        {
            int playerPos = player.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);

            // In matchmaking the player pos isn't saved in player properties, checking room properties
            if (!IsValidPlayerPos(playerPos))
            {
                Room room = PhotonRealtimeClient.CurrentRoom;

                if (room.GetCustomProperty<string>(PlayerPositionKey1) == player.UserId) playerPos = PlayerPosition1;
                else if (room.GetCustomProperty<string>(PlayerPositionKey2) == player.UserId) playerPos = PlayerPosition2;
                else if (room.GetCustomProperty<string>(PlayerPositionKey3) == player.UserId) playerPos = PlayerPosition3;
                else if (room.GetCustomProperty<string>(PlayerPositionKey4) == player.UserId) playerPos = PlayerPosition4;
            }

            return playerPos;
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
            return PhotonRealtimeClient.PlayerListOthers.FirstOrDefault(x => GetTeamNumber(x) == teamNumber);
        }

        public int GetFirstFreePlayerPos(Player player = null, int wantedPlayerPos = PlayerPosition1, bool isAllocateByTeams = false)
        {
            if (player == null) player = PhotonRealtimeClient.LocalPlayer;

            // Checking which of the room's player positions are free
            HashSet<int> usedPlayerPositions = GetUsedPlayerPositions();

            // If for some reason all player positions are reserved
            if (usedPlayerPositions.Count == PhotonRealtimeClient.LobbyCurrentRoom.MaxPlayers)
            {
                // Removing duplicate or missing player slot reservations with this method
                VerifyPlayerPositions();

                // Checking if player is already in a slot
                int playerPos = GetPlayerPos(player);

                if (IsValidPlayerPos(playerPos) && player.UserId == PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(GetPositionKey(playerPos)))
                {
                    return playerPos;
                }

                // If there is still no free position it means player somehow joined already full room then we kick player
                usedPlayerPositions = GetUsedPlayerPositions();
                if (usedPlayerPositions.Count == PhotonRealtimeClient.LobbyCurrentRoom.MaxPlayers)
                {
                    this.Publish<GetKickedEvent>(new(GetKickedEvent.ReasonType.FullRoom));
                    return PlayerPositionSpectator;
                }
            }
            
            
            if (usedPlayerPositions.Contains(wantedPlayerPos))
            {
                int[] playerPositions;
                if (PhotonRealtimeClient.LobbyCurrentRoom.MaxPlayers == 2)
                {
                    playerPositions = new[] { PlayerPosition2, PlayerPosition1 };
                }
                else
                {
                    playerPositions = isAllocateByTeams
                    ? new[] { PlayerPosition2, PlayerPosition3, PlayerPosition4, PlayerPosition1 }
                    : new[] { PlayerPosition3, PlayerPosition2, PlayerPosition4, PlayerPosition1 };
                }
                    
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

        /// <summary>
        /// Get used player positions.
        /// </summary>
        /// <returns>Integer hashset which contains all of the used player positions.</returns>
        public HashSet<int> GetUsedPlayerPositions()
        {
            HashSet<int> usedPlayerPositions = new();

            // Filling hashset with used player positions
            if (CheckIfPositionIsFree(PlayerPosition1) == false) usedPlayerPositions.Add(PlayerPosition1);
            if (CheckIfPositionIsFree(PlayerPosition2) == false) usedPlayerPositions.Add(PlayerPosition2);

            if (PhotonRealtimeClient.LobbyCurrentRoom.MaxPlayers == 4)
            {
                if (CheckIfPositionIsFree(PlayerPosition3) == false) usedPlayerPositions.Add(PlayerPosition3);
                if (CheckIfPositionIsFree(PlayerPosition4) == false) usedPlayerPositions.Add(PlayerPosition4);
            }

            return usedPlayerPositions;
        }

        /// <summary>
        /// Remove any duplicate or missing player positions from room custom properties
        /// </summary>
        /// <returns>True if any duplicate or missing players were found and removed</returns>
        public bool VerifyPlayerPositions()
        {
            bool duplicateOrMissingFound = false;

            string[] positionValues = new[]
                {
                    PhotonRealtimeClient.LobbyCurrentRoom.GetCustomProperty<string>(PlayerPositionKey1),
                    PhotonRealtimeClient.LobbyCurrentRoom.GetCustomProperty<string>(PlayerPositionKey2),
                    PhotonRealtimeClient.LobbyCurrentRoom.GetCustomProperty<string>(PlayerPositionKey3),
                    PhotonRealtimeClient.LobbyCurrentRoom.GetCustomProperty<string>(PlayerPositionKey4)
                };

            HashSet<string> uniquePositionValues = new();
            for (int i = 0; i < positionValues.Count(); i++)
            {
                // Checking if there is duplicate player
                if (!uniquePositionValues.Add(positionValues[i]))
                {
                    // Getting the player whose position is duplicate
                    Player player = null;
                    foreach (KeyValuePair<int, Player> p in PhotonRealtimeClient.CurrentRoom.Players)
                    {
                        if (p.Value.UserId == positionValues[i])
                        {
                            player = p.Value;
                            break;
                        }
                    }

                    // Calculating the duplicate position from the loop index since array starts at 0 instead of 1
                    int duplicatePos = i + 1; 

                    // Getting player's real position with the method, it will check for player's custom properties first
                    if (player != null)
                    {
                        int playerRealPos = GetPlayerPos(player);

                        // If player's real position is this duplicate position, get the first occurence as the duplicate
                        if (playerRealPos == duplicatePos)
                        {
                            duplicatePos = Array.IndexOf(positionValues, positionValues[i]) + 1;
                        }

                        // Clearing duplicate position
                        PhotonRealtimeClient.CurrentRoom.SetCustomProperty(GetPositionKey(duplicatePos), string.Empty);
                        duplicateOrMissingFound = true;
                    }
                }

                // Checking if there is missing player
                bool playerExists = false;
                foreach (KeyValuePair<int, Player> p in PhotonRealtimeClient.CurrentRoom.Players)
                {
                    if (p.Value.UserId == positionValues[i])
                    {
                        playerExists = true;
                    }
                }

                if (!playerExists) // If missing player we clear the custom property
                {
                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(GetPositionKey(i + 1), string.Empty);
                    duplicateOrMissingFound = true;
                }
            }

            return duplicateOrMissingFound;
        }

        /// <summary>
        /// Check if the player position is free in LobbyCurrentRoom. If position value is not one of the existing positions checks if PlayerPosition1 is free.
        /// </summary>
        /// <param name="position">Player position value as integer.</param>
        /// <returns>True if the position is free, false if it's not.</returns>
        public static bool CheckIfPositionIsFree(int position)
        {
            string positionKey = GetPositionKey(position);
            string positionValue = PhotonRealtimeClient.LobbyCurrentRoom?.GetCustomProperty<string>(positionKey);
            if (!string.IsNullOrWhiteSpace(positionValue))
            {
                return false;
            }

            return true;
        }

        public static bool CheckIfPositionHasBot(int position)
        {
            string positionKey = GetPositionKey(position);
            string positionValue = PhotonRealtimeClient.LobbyCurrentRoom?.GetCustomProperty<string>(positionKey);
            if (!string.Equals(positionValue, "Bot"))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get the corresponding position key to a position value.
        /// </summary>
        /// <param name="position">Player position value as integer.</param>
        /// <returns>Player position key as string. If the position integer is not one of the existing positions returns PositionKey1.</returns>
        public static string GetPositionKey(int position)
        {
            string positionKey;

            switch (position)
            {
                case PlayerPosition1:
                    positionKey = PlayerPositionKey1;
                    break;
                case PlayerPosition2:
                    positionKey = PlayerPositionKey2;
                    break;
                case PlayerPosition3:
                    positionKey = PlayerPositionKey3;
                    break;
                case PlayerPosition4:
                    positionKey = PlayerPositionKey4;
                    break;
                default:
                    positionKey = PlayerPositionKey1;
                    break;
            }

            return positionKey;
        }

        /// <summary>
        /// Check if everyone in the room has all 3 selected characters selected.
        /// </summary>
        /// <returns></returns>
        public static bool IsValidAllSelectedCharacters()
        {
            LobbyRoom room = PhotonRealtimeClient.LobbyCurrentRoom;

            foreach (var player in room.Players)
            {
                int[] playerCharacterIds = player.Value.GetCustomProperty<int[]>(PlayerCharacterIdsKey);
                if (playerCharacterIds == null || playerCharacterIds.Length < 3)
                {
                    return false;
                }
                else
                {
                    foreach (int id in playerCharacterIds)
                    {
                        if (id == 0) return false;
                    }
                }
            }
            return true;
        }

        public int CountRealPlayers()
        {
            return PhotonRealtimeClient.CurrentRoom.Players.Values.Sum(x => IsRealPlayer(x) ? 1 : 0);
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
            if (PhotonRealtimeClient.LobbyCurrentRoom.MaxPlayers == 2)
            {
                return playerPos is >= PlayerPosition1 and <= PlayerPosition2;
            }
            else
            {
                return playerPos is >= PlayerPosition1 and <= PlayerPosition4;
            }
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
            if (PhotonRealtimeClient.Client.InRoom && !string.IsNullOrWhiteSpace(PhotonRealtimeClient.NickName))
            {
                return PhotonRealtimeClient.NickName;
            }
            throw new UnityException("Can not get player name outside a room");
        }

        public int GetPlayerIndex(int playerPos)
        {
            if (PhotonRealtimeClient.LobbyCurrentRoom.MaxPlayers == 2)
            {
                Assert.IsTrue(playerPos >= PlayerPosition1 && playerPos <= PlayerPosition2,
                "playerPos >= PlayerPosition1 && playerPos <= PlayerPosition4");
                return playerPos - 1;
            }
            else
            {
                Assert.IsTrue(playerPos >= PlayerPosition1 && playerPos <= PlayerPosition4,
                "playerPos >= PlayerPosition1 && playerPos <= PlayerPosition4");
                return playerPos - 1;
            }
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
            if (!PhotonRealtimeClient.Client.InRoom)
            {
                return false;
            }
            int playerPos = GetPlayerPos(player);
            if (!IsValidPlayerPos(playerPos))
            {
                return false;
            }
            foreach (Player otherPlayer in PhotonRealtimeClient.PlayerListOthers)
            {
                int otherPlayerPos = GetPlayerPos(otherPlayer);
                if (IsValidPlayerPos(otherPlayerPos) && otherPlayerPos == playerPos)
                {
                    return false;
                }
            }
            return true;
        }

        public int GetPlayerCountForRoom()
        {
            if (!PhotonRealtimeClient.Client.InRoom) return 0;
            Room room = PhotonRealtimeClient.CurrentRoom;
            return room.GetCustomProperty(PlayerCountKey, 0);
        }

        public CharacterID GetPlayerCharacterID(Player player)
        {
            return (CharacterID)player.GetCustomProperty(PlayerCharacterIdKey, -1);
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
            room.SetCustomProperties(new PhotonHashtable
            {
                { TeamAlphaNameKey, teamBlueName },
                { TeamBetaNameKey, teamRedName },
                { PlayerCountKey, playerCount }
            });
            Debug.LogWarning($"{room.GetDebugLabel()} players {playerCount} blue {teamBlueName} red {teamRedName}");
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public void SetDebugPlayer(Player player, int wantedPlayerPos, /*bool isAllocateByTeams,*/ int playerMainSkill)
        {
            //wantedPlayerPos = GetFirstFreePlayerPos(player, wantedPlayerPos/*, isAllocateByTeams*/);
            SetDebugPlayerProps(player, wantedPlayerPos, playerMainSkill);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public void SetDebugPlayerProps(Player player, int playerPos, int playerMainSkill = -1)
        {
            Assert.IsTrue(IsValidPlayerPos(playerPos), "IsValidPlayerPos(playerPos)");
            if (playerMainSkill < (int)CharacterClassType.Desensitizer || playerMainSkill > (int)CharacterClassType.Confluent)
            {
                // Should be fastest movement skill to have for testing.
                playerMainSkill = (int)CharacterClassType.Trickster;
            }
            var curPlayerPos = player.GetCustomProperty(PlayerPositionKey, -1);
            var curPlayerMainSkill = player.GetCustomProperty(PlayerCharacterIdKey, -1);
            if (curPlayerPos == playerPos && curPlayerMainSkill == playerMainSkill)
            {
                // Prevent setting same values because it is hard for client to keep track of asynchronous changes over network.
                return;
            }
            player.SetCustomProperties(new PhotonHashtable
            {
                { PlayerPositionKey, playerPos },
                { PlayerCharacterIdKey, playerMainSkill }
            });
            Debug.LogWarning($"{player.GetDebugLabel()} playerPos {playerPos} skill {playerMainSkill}");
        }

        #endregion
    }
}
