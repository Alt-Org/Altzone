using System.Runtime.CompilerServices;
using System.Diagnostics;

using Photon.Realtime;

using Altzone.Scripts.Battle.Photon;
using Altzone.Scripts.Model.Poco.Game;

namespace Battle.Scripts.Battle
{
    /// <summary>
    /// Battle wrapper for <see cref="PhotonBattleRoom"/>
    /// </summary>
    internal static class PhotonBattle
    {
        // Team positions in world coordinates (game arena when camera isn't rotated)
        //  Beta team number 2
        //  - Player positions 3 and 4
        // | ======= |
        // |  4 |  3 |      Team number 0
        // | ======= |      - Guest number is 0
        // |  1 |  2 |      - Spectator number is 10
        // | ======= |
        //  Alpha team number 1
        //  - Player positions 1 and 2

        // Player should be positioned so that if camera is rotated 180 degrees,
        // player with smaller number is always at the bottom of the left corner of the gameplay area.

        public const int PlayerPositionGuest = PhotonBattleRoom.PlayerPositionGuest;
        public const int PlayerPosition1 = PhotonBattleRoom.PlayerPosition1;
        public const int PlayerPosition2 = PhotonBattleRoom.PlayerPosition2;
        public const int PlayerPosition3 = PhotonBattleRoom.PlayerPosition3;
        public const int PlayerPosition4 = PhotonBattleRoom.PlayerPosition4;
        public const int PlayerPositionSpectator = PhotonBattleRoom.PlayerPositionSpectator;

        public const int NoTeamValue = PhotonBattleRoom.NoTeamValue;
        public const int TeamAlphaValue = PhotonBattleRoom.TeamAlphaValue;
        public const int TeamBetaValue = PhotonBattleRoom.TeamBetaValue;

        internal static class EventCodes
        {
            public const byte PLAYER_CLASS_TRICKSTER_SET_PHOTON_VIEW_ID_EVENTCODE = 11; // picked randomly
            public const byte PLAYER_CLASS_TRICKSTER_POS1_SET_PHOTON_VIEW_ID_EVENTCODE = PLAYER_CLASS_TRICKSTER_SET_PHOTON_VIEW_ID_EVENTCODE + 0;
            public const byte PLAYER_CLASS_TRICKSTER_POS2_SET_PHOTON_VIEW_ID_EVENTCODE = PLAYER_CLASS_TRICKSTER_SET_PHOTON_VIEW_ID_EVENTCODE + 1;
            public const byte PLAYER_CLASS_TRICKSTER_POS3_SET_PHOTON_VIEW_ID_EVENTCODE = PLAYER_CLASS_TRICKSTER_SET_PHOTON_VIEW_ID_EVENTCODE + 2;
            public const byte PLAYER_CLASS_TRICKSTER_POS4_SET_PHOTON_VIEW_ID_EVENTCODE = PLAYER_CLASS_TRICKSTER_SET_PHOTON_VIEW_ID_EVENTCODE + 3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int GetPlayerPos(Player player) => s_photonBattleRoom.GetPlayerPos(player);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsValidPlayerPos(int playerPos) => s_photonBattleRoom.IsValidPlayerPos(playerPos);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsPlayerPosAvailable(Player player) => s_photonBattleRoom.IsPlayerPosAvailable(player);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int GetFirstFreePlayerPos(Player player, int wantedPlayerPos = PhotonBattleRoom.PlayerPosition1) => s_photonBattleRoom.GetFirstFreePlayerPos(player, wantedPlayerPos);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int GetTeamNumber(int playerPos) => s_photonBattleRoom.GetTeamNumber(playerPos);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int GetTeamNumber(Player player) => GetTeamNumber(GetPlayerPos(player));
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsRealPlayer(Player player) => s_photonBattleRoom.IsRealPlayer(player);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int GetPlayerCountForRoom() => s_photonBattleRoom.GetPlayerCountForRoom();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int CountRealPlayers() => s_photonBattleRoom.CountRealPlayers();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static CharacterID GetPlayerCharacterId(Player player) => s_photonBattleRoom.GetPlayerCharacterID(player);

        #region Debug and test utilities

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetDebugPlayer(Player player, int wantedPlayerPos, int playerPrefabId) => s_photonBattleRoom.SetDebugPlayer(player, wantedPlayerPos, playerPrefabId);

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetDebugPlayerProps(Player player, int playerPos, int playerPrefabId) => s_photonBattleRoom.SetDebugPlayerProps(player, playerPos, playerPrefabId);

        #endregion Debug and test utilities

        private static readonly PhotonBattleRoom s_photonBattleRoom = new();
    }
}
