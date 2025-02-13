using Photon.Deterministic;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class PlayerShieldSystem : SystemMainThreadFilter<PlayerShieldSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Transform2D* Transform;
            public PhysicsCollider2D* PhysicsCollider2D;
            public PlayerData* PlayerData;
            public PlayerShieldData* PlayerShieldData;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            // try to find a teammate, if not found exit method
            if (!filter.PlayerShieldData->TeamMateSet && !FindTeamMate(f, ref filter)) return;

            // get teamMatePosition and calculate distance
            FPVector2 teamMatePosition = f.Unsafe.GetPointer<Transform2D>(filter.PlayerShieldData->TeamMate)->Position;
            FP DistanceToTeamMate = FPVector2.Distance(filter.Transform->Position, teamMatePosition);

            // toggle shield on and off depending on the distance to teammate
            bool ShieldBool = DistanceToTeamMate >= FP._3;
            filter.PhysicsCollider2D->Enabled = ShieldBool;
            f.Events.ToggleShield(filter.Entity, ShieldBool);
        }

        private bool FindTeamMate(Frame f, ref Filter filter)
        {
            int teamMateIndex = (int)filter.PlayerData->Player switch
            {
                0 => 1,
                1 => 0,
                2 => 3,
                3 => 2,
                _ => -1,
            };

            foreach (EntityComponentPointerPair<PlayerData> pair in f.Unsafe.GetComponentBlockIterator<PlayerData>())
            {
                if (pair.Component->Player == teamMateIndex)
                {
                    filter.PlayerShieldData->TeamMate = pair.Entity;
                    filter.PlayerShieldData->TeamMateSet = true;
                    return true;
                }
            }

            return false;
        }
    }
}
