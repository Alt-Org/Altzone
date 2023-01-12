using Altzone.Scripts.Model;
using Photon.Realtime;
using UnityEngine;

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

        public const int NoTeamValue = 0;
        public const int TeamBlueValue = 1;
        public const int TeamRedValue = 2;

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

        public static IBattleCharacter GetCharacterModelForPlayer(Player player)
        {
            throw new System.NotImplementedException();
        }
    }
}