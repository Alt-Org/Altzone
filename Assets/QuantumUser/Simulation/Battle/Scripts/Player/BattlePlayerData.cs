using System.Diagnostics;
using System.Runtime.CompilerServices;
using Battle.QSimulation;
using Battle.QSimulation.Game;
using Battle.QSimulation.Player;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial struct BattlePlayerData
    {
        public struct Ref
        {
            public PlayerRef PRef
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Low_Level.PRef; }

            public BattlePlayerSlot Slot
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Low_Level.Slot; }

            public BattleTeamNumber Team
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Low_Level.Team; }

            public BattlePlayerPlayState PlayState
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Low_Level.PlayState; }

            public bool IsBot
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Low_Level.IsBot; }

            public bool IsAbandoned
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Low_Level.IsAbandoned; }

            public bool StateAllowCharacterSwapping
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Low_Level.StateAllowCharacterSwapping; }

            public bool StatePlayerGiveUp
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Low_Level.StatePlayerGiveUp; }

            public FrameTimer RespawnTimer
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Low_Level.RespawnTimer; }

            public int SelectedCharacterNumber
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Low_Level.SelectedCharacterNumber; }

            public ref BattlePlayerData Low_Level
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _playerManagerData->PlayerArray[_index]; }

            public Ref(BattlePlayerManagerDataQSingleton* playerManagerData, int index)
            {
                _playerManagerData = playerManagerData;
                _index = index;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetOutOfPlayRespawning()
            {
                if (!PlayState.IsOutOfPlay())
                {
                    s_debugLogger.Error("Can not set player that is not OutOfPlay as OutOfPlayRespawning");
                    return;
                }
                Low_Level.PlayState = BattlePlayerPlayState.OutOfPlayRespawning;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetOutOfPlayFinal()
            {
                if (!PlayState.IsOutOfPlay())
                {
                    s_debugLogger.Error("Can not set player that is not OutOfPlay as OutOfPlayFinal");
                    return;
                }
                Low_Level.PlayState = BattlePlayerPlayState.OutOfPlayFinal;
            }

            public int GetIndex() => _index;

            private BattlePlayerManagerDataQSingleton* _playerManagerData;

            private int _index;
        }

        public static int NoSelectedCharacter => -1;

        #region Public Static Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidCharacterNumber(int characterNumber)
        {
            return characterNumber >= 0 && characterNumber < Constants.BATTLE_PLAYER_CHARACTER_COUNT;
        }

        [Conditional("UNITY_EDITOR")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DevAssertIsValidCharacterNumber(string source, int characterNumber)
        {
            BattleDebugLogger.DevAssertFormat(source, IsValidCharacterNumber(characterNumber), "Invalid characterNumber = {0}", characterNumber);
        }

        #endregion Public Static Methods

        /// <summary>This classes BattleDebugLogger instance.</summary>
        private static BattleDebugLogger s_debugLogger;
    }
}
