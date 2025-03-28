using System;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Altzone.Scripts.Battle.Photon;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Lobby.Wrappers;

namespace MenuUi.Scripts.Lobby
{
    /// <summary>
    ///  wrapper for PhotonBattleRoom
    /// </summary>
    internal static class PhotonBattleLobbyRoom
    {
        public const string BattleID = PhotonBattleRoom.BattleID;
        public const string PlayerPositionKey = PhotonBattleRoom.PlayerPositionKey;
        public const string PlayerCountKey = PhotonBattleRoom.PlayerCountKey;
        public const string PlayerPrefabIdKey = PhotonBattleRoom.PlayerCharacterIdKey;
        public const string PlayerPrefabIdsKey = PhotonBattleRoom.PlayerCharacterIdsKey;
        public const string PlayerStatsKey = PhotonBattleRoom.PlayerStatsKey;
        public const string TeamAlphaNameKey = PhotonBattleRoom.TeamAlphaNameKey;
        public const string TeamBetaNameKey = PhotonBattleRoom.TeamBetaNameKey;
        public const string TeamWinTypeKey = PhotonBattleRoom.TeamWinTypeKey;
        public const string TeamWinKey = PhotonBattleRoom.TeamWinKey;
        public const string TeamBlueScoreKey = PhotonBattleRoom.TeamBlueScoreKey;
        public const string TeamRedScoreKey = PhotonBattleRoom.TeamRedScoreKey;
        public const string PasswordKey = PhotonBattleRoom.PasswordKey;
        public const string GameTypeKey = PhotonBattleRoom.GameTypeKey;
        public const string MatchmakingKey = PhotonBattleRoom.IsMatchmakingKey;
        public const string SoulhomeRank = PhotonBattleRoom.SoulhomeRank;
        public const string SoulhomeRankVariance = PhotonBattleRoom.SoulhomeRankVariance;
        public const string ClanNameKey = PhotonBattleRoom.ClanNameKey;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int GetPlayerPos(LobbyPlayer player) => PhotonLobbyRoom.GetPlayerPos(player);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsValidPlayerPos(int playerPos) => PhotonLobbyRoom.IsValidPlayerPos(playerPos);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsPlayerPosAvailable(LobbyPlayer player) => PhotonLobbyRoom.IsPlayerPosAvailable(player);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int GetFirstFreePlayerPos(int wantedPlayerPos = PhotonLobbyRoom.PlayerPosition1) => PhotonLobbyRoom.GetFirstFreePlayerPos(wantedPlayerPos);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int GetTeamNumber(int playerPos) => PhotonLobbyRoom.GetTeamNumber(playerPos);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsRealPlayer(LobbyPlayer player) => PhotonLobbyRoom.IsRealPlayer(player);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int GetPlayerCountForRoom() => PhotonLobbyRoom.GetPlayerCountForRoom();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int CountRealPlayers() => PhotonLobbyRoom.CountRealPlayers();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int GetPlayerPrefabId(LobbyPlayer player) => PhotonLobbyRoom.GetPlayerPrefabId(player);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsValidGameplayPosOrGuest(int playerPos) => PhotonLobbyRoom.IsValidGameplayPosOrGuest(playerPos);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int GetPrefabIndex(BattleCharacter battleCharacter, int defaultValue) => PhotonLobbyRoom.GetPrefabIndex(battleCharacter, defaultValue);

        #region Debug and test utilities

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetDebugPlayer(LobbyPlayer player, int wantedPlayerPos, int playerPrefabId) => PhotonLobbyRoom.SetDebugPlayer(player, wantedPlayerPos, playerPrefabId);

        /*[Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetDebugPlayerProps(LobbyPlayer player, int playerPos, int playerPrefabId) => PhotonLobbyRoom.SetDebugPlayerProps(player, playerPos, playerPrefabId);*/

        #endregion Debug and test utilities

    }
}
