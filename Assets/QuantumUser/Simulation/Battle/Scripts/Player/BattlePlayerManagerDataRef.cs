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

            public static Ref GetRef(Frame f)
            {
                if (!f.Unsafe.TryGetPointerSingleton(out BattlePlayerManagerDataQSingleton* playerManagerData))
                {
                    s_debugLogger.Error(f, "PlayerManagerData singleton not found!");
                }

                return new Ref() { Ptr = playerManagerData };
            }

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

            public static int GetPlayerIndex(Ref @ref, PlayerRef playerRef)
            {
                for (int i = 0; i < Constants.BATTLE_PLAYER_SLOT_COUNT; i++)
                {
                    if (@ref.Ptr->PlayerArray[i].PRef == playerRef) return i;
                }
                return -1;
            }

            public static BattlePlayerSlot GetSlot(Ref @ref, PlayerRef playerRef)
            {
                return GetPlayerSlot(GetPlayerIndex(@ref, playerRef));
            }

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

            public static BattleTeamNumber GetOpposingTeam(BattleTeamNumber team)
            {
                return team switch
                {
                    BattleTeamNumber.TeamAlpha => BattleTeamNumber.TeamBeta,
                    BattleTeamNumber.TeamBeta  => BattleTeamNumber.TeamAlpha,
                };
            }

            public static BattlePlayerData.Ref GetPlayerData(Ref @ref, int index)
            {
                return new BattlePlayerData.Ref(@ref.Ptr, index);
            }
        }
    }
}
