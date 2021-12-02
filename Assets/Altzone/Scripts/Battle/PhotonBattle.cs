using System.Diagnostics;
using System.Linq;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Battle
{
    public static class PhotonBattle
    {
        private const string PlayerPrefsPlayerNameKey = "PlayerData.PlayerName";

        public const string PlayerPositionKey = "pp";
        public const string PlayerMainSkillKey = "mk";
        public const string TeamBlueKey = "tb";
        public const string TeamRedKey = "tr";

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

        public const int StartPlayingEvent = 123;

        public static bool IsRealPlayer(Player player)
        {
            var playerPos = player.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
            return playerPos >= PlayerPosition1 && playerPos <= PlayerPosition4;
        }

        public static int CountRealPlayers()
        {
            return PhotonNetwork.CurrentRoom.Players.Values.Where(IsRealPlayer).Count();
        }

        public static bool IsValidGameplayPos(int playerPos)
        {
            return IsValidPlayerPos(playerPos) || playerPos == PlayerPositionGuest || playerPos == PlayerPositionSpectator;
        }

        public static bool IsValidPlayerPos(int playerPos)
        {
            return playerPos >= PlayerPosition1 && playerPos <= PlayerPosition4;
        }

        public static int GetPlayerPos(Player player)
        {
            return player.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
        }

        public static string GetLocalPlayerName()
        {
            if (PhotonNetwork.InRoom)
            {
                return PhotonNetwork.NickName;
            }
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            return playerData.PlayerName;
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
            return teamNumber == TeamBlueValue ? TeamRedValue : TeamBlueValue;
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

        public static CharacterModel GetPlayerCharacterModel(Player player)
        {
            var skillId = player.GetCustomProperty(PlayerMainSkillKey, -1);
            var character = Storefront.Get().GetCharacterModel(skillId);
            return character;
        }

        [Conditional("UNITY_EDITOR")]
        public static void SetDebugPlayerProps(Player player, int playerPos, int playerMainSkill = -1)
        {
            Assert.IsTrue(playerPos >= PlayerPosition1 && playerPos <= PlayerPosition4,
                "playerPos >= PlayerPosition1 && playerPos <= PlayerPosition4");
            if (playerMainSkill < (int)Defence.Desensitisation || playerMainSkill > (int)Defence.Confluence)
            {
                // Should be fastest movement skill to have for testing.
                playerMainSkill = (int)Defence.Deflection;
            }
            player.SetCustomProperties(new Hashtable
            {
                { PlayerPositionKey, playerPos },
                { PlayerMainSkillKey, playerMainSkill }
            });
            Debug.LogWarning($"setDebugPlayerProps {player.GetDebugLabel()}");
        }
    }
}