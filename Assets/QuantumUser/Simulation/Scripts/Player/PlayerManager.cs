using Photon.Deterministic;

namespace Quantum
{
    public static unsafe class PlayerManager
    {

        public static void Init()
        {
            // Create or find PlayerManagerData singleton and set default values (if not set)
        }

        public static void InitPlayer(Frame f, PlayerRef player)
        {
            var playerManagerData = f.Unsafe.GetPointerSingleton<PlayerManagerData>();

        }

        public static void SpawnPlayer(Frame f, BattlePlayerSlot slot, int characterIndex)
        {
            // if Character already in position?
        }

        public static void SpawnPlayer(Frame f, BattlePlayerSlot slot, int characterIndex, FPVector2 worldPosition) // position, 
        {
            // if Character already in position?
        }

        public static void DespawnPlayer(Frame f, BattlePlayerSlot slot)
        {

        }

        public static EntityRef GetPlayerEntity(Frame f, BattlePlayerSlot slot)
        {
            return default;
        }

        public static EntityRef GetTeammateEnity(Frame f, BattlePlayerSlot slot)
        {
            return default;
        }

        public static void SwitchCharacter()
        {

        }


    }
}
