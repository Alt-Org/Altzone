using System;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Photon.Realtime;

using Altzone.Scripts.Battle.Photon;
using Altzone.Scripts.Model.Poco.Game;
//using Player = Battle1.PhotonRealtime.Code.Player;

namespace Altzone.Scripts.Lobby.Wrappers
{
    /// <summary>
    ///  wrapper for PhotonBattleRoom
    /// </summary>
    public static class PhotonLobbyRoom
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int GetPlayerPos(LobbyPlayer player) => s_photonBattleRoom.GetPlayerPos(player.GetPlayer(player.ActorNumber));
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsValidPlayerPos(int playerPos) => s_photonBattleRoom.IsValidPlayerPos(playerPos);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsPlayerPosAvailable(LobbyPlayer player) => s_photonBattleRoom.IsPlayerPosAvailable(player.GetPlayer(player.ActorNumber));
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int GetFirstFreePlayerPos(LobbyPlayer player = null, int wantedPlayerPos = PhotonBattleRoom.PlayerPosition1) => s_photonBattleRoom.GetFirstFreePlayerPos(player?.GetPlayer(player.ActorNumber), wantedPlayerPos);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool VerifyPlayerPositions() => s_photonBattleRoom.VerifyPlayerPositions();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int GetTeamNumber(int playerPos) => s_photonBattleRoom.GetTeamNumber(playerPos);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsRealPlayer(LobbyPlayer player) => s_photonBattleRoom.IsRealPlayer(player.GetPlayer(player.ActorNumber));
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int GetPlayerCountForRoom() => s_photonBattleRoom.GetPlayerCountForRoom();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int CountRealPlayers() => s_photonBattleRoom.CountRealPlayers();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int GetPlayerPrefabId(LobbyPlayer player) => (int)s_photonBattleRoom.GetPlayerCharacterID(player.GetPlayer(player.ActorNumber));
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsValidGameplayPosOrGuest(int playerPos) => s_photonBattleRoom.IsValidGameplayPosOrGuest(playerPos);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int GetPrefabIndex(BattleCharacter battleCharacter, int defaultValue) => s_photonBattleRoom.GetPrefabIndex(battleCharacter, defaultValue);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsValidAllSelectedCharacters() => PhotonBattleRoom.IsValidAllSelectedCharacters();

        #region Debug and test utilities

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetDebugPlayer(LobbyPlayer player, int wantedPlayerPos, int playerPrefabId) => s_photonBattleRoom.SetDebugPlayer(player.GetPlayer(player.ActorNumber), wantedPlayerPos, playerPrefabId);

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetDebugPlayerProps(LobbyPlayer player, int playerPos, int playerPrefabId) => s_photonBattleRoom.SetDebugPlayerProps(player.GetPlayer(player.ActorNumber), playerPos, playerPrefabId);

        #endregion Debug and test utilities

        private static readonly PhotonBattleRoom s_photonBattleRoom = new();
    }
}
