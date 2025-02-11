using System;
using System.Diagnostics.Tracing;
using Photon.Deterministic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class ShieldSystem : SystemMainThreadFilter<ShieldSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Transform2D* Transform;
            public PhysicsCollider2D* PhysicsCollider2D;
            //public SpriteRenderer* SpriteRenderer;
            public PlayerData* PlayerData;
            public ShieldData* ShieldData;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            //tries to find a teammate, if not found exits method
            if (!filter.ShieldData->TeamMateSet && !FindTeamMate(f, ref filter)) return;

            //gets teammates position
            FPVector2 teamMatePosition = f.Unsafe.GetPointer<Transform2D>(filter.ShieldData->TeamMate)->Position;
            FP DistanceToTeamMate = FPVector2.Distance(filter.Transform->Position, teamMatePosition);
            Debug.Log("[ShieldSystem] Distance between player 0 and 1: " + DistanceToTeamMate);

            //enable or disable shield depending on the distance
            if (DistanceToTeamMate >= FP._3)
            {
                filter.PhysicsCollider2D->Enabled = true;
            }

            else
            {
                filter.PhysicsCollider2D->Enabled = false;
            }
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
            //Debug.LogFormat("[ShieldSystem] Searching player with playerref: {}", teamMateIndex);

            foreach (EntityComponentPointerPair<PlayerData> pair in f.Unsafe.GetComponentBlockIterator<PlayerData>())
            {
                if (pair.Component->Player == teamMateIndex)
                {
                    filter.ShieldData->TeamMate = pair.Entity;
                    filter.ShieldData->TeamMateSet = true;
                    //Debug.LogFormat("[ShieldSystem]Teammate found. Teammates playerref: {0} Teammates Entityref: {1}", playerdata->Player, entityref);
                    return true;
                }
            }

            //Debug.LogFormat("[ShieldSystem]Teammate not found for player: {}", filter.PlayerData->Player);
            return false;
        }
    }
}
