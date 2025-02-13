using System.Diagnostics;
using System.Runtime.CompilerServices;
using Altzone.Scripts.Battle.Photon;
using Altzone.Scripts.Model.Poco.Game;
using Photon.Realtime;
/*using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;
using Player = Battle1.PhotonRealtime.Code.Player;*/
using PhotonBattleRoom = Battle1.Scripts.Battle.Photon.PhotonBattleRoom;

namespace Battle1.Scripts.Battle
{
    internal enum BattleTeamNumber
    {
        NoTeam = PhotonBattleRoom.NoTeamValue,
        TeamAlpha = PhotonBattleRoom.TeamAlphaValue,
        TeamBeta = PhotonBattleRoom.TeamBetaValue,
    }

    /// <summary>
    /// Battle wrapper for <see cref="PhotonBattleRoom"/>
    /// </summary>
    internal static class PhotonBattle
    {
        // Team positions in world coordinates (game arena when camera isn't rotated)
        //  Beta team number 2
        //  - Player positions 3 and 4
        // | ======= |
        // |  4 | 3  |      Team number 0
        // | ======= |      - Guest number is 0
        // |  1 | 2  |      - Spectator number is 10
        // | ======= |
        //  Alpha team number 1
        //  - Player positions 1 and 2

        // Player should be positioned so that if camera is rotated 180 degrees,
        // player with smaller number is always at the bottom of the left corner of the gameplay area.

        public const string BattleID = PhotonBattleRoom.BattleID;
        public const int PlayerPositionGuest = PhotonBattleRoom.PlayerPositionGuest;
        public const int PlayerPosition1 = PhotonBattleRoom.PlayerPosition1;
        public const int PlayerPosition2 = PhotonBattleRoom.PlayerPosition2;
        public const int PlayerPosition3 = PhotonBattleRoom.PlayerPosition3;
        public const int PlayerPosition4 = PhotonBattleRoom.PlayerPosition4;
        public const int PlayerPositionSpectator = PhotonBattleRoom.PlayerPositionSpectator;

        internal static class EventCodes
        {
            public const byte PlayerClassTricksterSetPhotonViewIdEventCode = 11; // picked randomly
            public const byte PlayerClassTricksterPos1SetPhotonViewIdEventCode = PlayerClassTricksterSetPhotonViewIdEventCode + 0;
            public const byte PlayerClassTricksterPos2SetPhotonViewIdEventCode = PlayerClassTricksterSetPhotonViewIdEventCode + 1;
            public const byte PlayerClassTricksterPos3SetPhotonViewIdEventCode = PlayerClassTricksterSetPhotonViewIdEventCode + 2;
            public const byte PlayerClassTricksterPos4SetPhotonViewIdEventCode = PlayerClassTricksterSetPhotonViewIdEventCode + 3;
        }

        /*[MethodImpl(MethodImplOptions.AggressiveInlining)] public static int GetPlayerPos(Player player) => s_photonBattleRoom.GetPlayerPos(player);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsValidPlayerPos(int playerPos) => s_photonBattleRoom.IsValidPlayerPos(playerPos);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsPlayerPosAvailable(Player player) => s_photonBattleRoom.IsPlayerPosAvailable(player);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int GetFirstFreePlayerPos(Player player, int wantedPlayerPos = PhotonBattleRoom.PlayerPosition1) => s_photonBattleRoom.GetFirstFreePlayerPos(player, wantedPlayerPos);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static BattleTeamNumber GetTeamNumber(int playerPos) => (BattleTeamNumber)s_photonBattleRoom.GetTeamNumber(playerPos);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static BattleTeamNumber GetTeamNumber(Player player) => GetTeamNumber(GetPlayerPos(player));
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsRealPlayer(Player player) => s_photonBattleRoom.IsRealPlayer(player);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int GetPlayerCountForRoom() => s_photonBattleRoom.GetPlayerCountForRoom();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int CountRealPlayers() => s_photonBattleRoom.CountRealPlayers();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static CharacterID GetPlayerCharacterId(Player player) => s_photonBattleRoom.GetPlayerCharacterID(player);
*/
        public static string GetBattleID()
        {
            /*return PhotonNetwork.CurrentRoom.GetCustomProperty<string>(BattleID, null);*/
            return "PhotonNetwork.CurrentRoom.GetCustomProperty<string>(BattleID, null)";
        }

        public static BattleCharacter GetBattleCharacter(Player player)
        {
            /*  int[] characterIds = player.GetCustomProperty<int[]>(PhotonBattleRoom.PlayerCharacterIdsKey, null);
              int[] characterStats = player.GetCustomProperty<int[]>(PhotonBattleRoom.PlayerStatsKey, null);*/
            int[] characterIds = { 0 };
            int[] characterStats = { 0 };

            BattleCharacter battleCharacter = new(
                (CharacterID)characterIds[0],
                "placeholder",
                BaseCharacter.GetStatValue(StatType.Hp, characterStats[0]),
                BaseCharacter.GetStatValue(StatType.Speed, characterStats[1]),
                BaseCharacter.GetStatValue(StatType.CharacterSize, characterStats[2]),
                BaseCharacter.GetStatValue(StatType.Attack, characterStats[3]),
                BaseCharacter.GetStatValue(StatType.Defence, characterStats[4])
            );

            return battleCharacter;
        }

        #region Debug and test utilities

 /*       [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetDebugPlayer(Player player, int wantedPlayerPos, int playerPrefabId) => s_photonBattleRoom.SetDebugPlayer(player, wantedPlayerPos, playerPrefabId);

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetDebugPlayerProps(Player player, int playerPos, int playerPrefabId) => s_photonBattleRoom.SetDebugPlayerProps(player, playerPos, playerPrefabId);
*/
        #endregion Debug and test utilities

        private static readonly PhotonBattleRoom s_photonBattleRoom = new();
    }
}
