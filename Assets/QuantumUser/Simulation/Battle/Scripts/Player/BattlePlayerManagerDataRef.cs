using System.Runtime.CompilerServices;
using Quantum;

namespace Battle.QSimulation.Player
{
    public unsafe partial class BattlePlayerManager
    {
        private static class Data
        {
            public struct Ref
            {
                public ref int PlayerCount
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => ref Ptr->PlayerCount;
                }

                public BattlePlayerManagerDataQSingleton* Ptr;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Ref GetRef(Frame f)
            {
                if (!f.Unsafe.TryGetPointerSingleton(out BattlePlayerManagerDataQSingleton* playerManagerData))
                {
                    s_debugLogger.Error(f, "PlayerManagerData singleton not found!");
                }

                return new Ref() { Ptr = playerManagerData };
            }

            #region PlayerIndex Methods

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int GetPlayerIndex(BattlePlayerSlot slot)
            {
                int index = slot switch
                {
                    BattlePlayerSlot.Slot1 => 0,
                    BattlePlayerSlot.Slot2 => 1,
                    BattlePlayerSlot.Slot3 => 2,
                    BattlePlayerSlot.Slot4 => 3,

                    _ => -1
                };

                return index;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int GetPlayerIndex(Ref @ref, PlayerRef playerRef)
            {
                for (int i = 0; i < Constants.BATTLE_PLAYER_SLOT_COUNT; i++)
                {
                    if (@ref.Ptr->PlayerArray[i].PRef == playerRef) return i;
                }
                return -1;
            }

            #endregion PlayerIndex Methods

            #region Slot and Team Methods

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static BattlePlayerSlot GetSlot(Ref @ref, PlayerRef playerRef)
            {
                return GetPlayerSlot(GetPlayerIndex(@ref, playerRef));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static BattlePlayerSlot GetPlayerSlot(int index)
            {
                return index switch
                {
                    0 => BattlePlayerSlot.Slot1,
                    1 => BattlePlayerSlot.Slot2,
                    2 => BattlePlayerSlot.Slot3,
                    3 => BattlePlayerSlot.Slot4,

                    _ => BattlePlayerSlot.Spectator
                };
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static BattlePlayerSlot GetTeammateSlot(BattlePlayerSlot slot)
            {
                return slot switch
                {
                    BattlePlayerSlot.Slot1 => BattlePlayerSlot.Slot2,
                    BattlePlayerSlot.Slot2 => BattlePlayerSlot.Slot1,
                    BattlePlayerSlot.Slot3 => BattlePlayerSlot.Slot4,
                    BattlePlayerSlot.Slot4 => BattlePlayerSlot.Slot3
                };
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static BattleTeamNumber GetTeam(BattlePlayerSlot slot)
            {
                return slot switch
                {
                    BattlePlayerSlot.Slot1 => BattleTeamNumber.TeamAlpha,
                    BattlePlayerSlot.Slot2 => BattleTeamNumber.TeamAlpha,
                    BattlePlayerSlot.Slot3 => BattleTeamNumber.TeamBeta,
                    BattlePlayerSlot.Slot4 => BattleTeamNumber.TeamBeta,

                    _ => BattleTeamNumber.NoTeam
                };
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static BattleTeamNumber GetOpposingTeam(BattleTeamNumber team)
            {
                return team switch
                {
                    BattleTeamNumber.TeamAlpha => BattleTeamNumber.TeamBeta,
                    BattleTeamNumber.TeamBeta  => BattleTeamNumber.TeamAlpha,
                };
            }

            #endregion Slot and Team Methods

            #region PlayerData Methods

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static BattlePlayerData.Ref GetPlayerData(Ref @ref, int index)
            {
                return new BattlePlayerData.Ref(@ref.Ptr, index);
            }

            #endregion PlayerData Methods
        }
    }
}
