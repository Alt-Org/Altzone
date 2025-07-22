using UnityEngine;
using UnityEngine.Scripting;

using Quantum;

namespace Battle.QSimulation.Game
{
    [Preserve]
    public unsafe class BattleCollisionQSystem : SystemSignalsOnly, ISignalOnTrigger2D
    {
        // Handles all triggers in the game
        public void OnTrigger2D(Frame f, TriggerInfo2D info)
        {
            // if projectile
            if (f.Unsafe.TryGetPointer(info.Entity, out BattleProjectileQComponent* projectile))
            {
                if(!f.Unsafe.TryGetPointer(info.Other, out BattleCollisionTriggerQComponent* collisionTrigger)) return;

                BattlePlayerHitboxQComponent* playerHitbox;

                switch (collisionTrigger->Type)
                {
                    case BattleCollisionTriggerType.ArenaBorder:
                        BattleArenaBorderQComponent* arenaBorder = f.Unsafe.GetPointer<BattleArenaBorderQComponent>(info.Other);
                        Debug.Log("[CollisionSystem] Projectile hit ArenaBorder");
                        //f.Events.PlaySoundEvent(SoundEffect.SideWallHit);
                        f.Signals.BattleOnProjectileHitArenaBorder(projectile, info.Entity, arenaBorder, info.Other);
                        break;

                    case BattleCollisionTriggerType.SoulWall:
                        BattleSoulWallQComponent* soulWall = f.Unsafe.GetPointer<BattleSoulWallQComponent>(info.Other);
                        Debug.Log("[CollisionSystem] Projectile hit SoulWall");
                        f.Signals.BattleOnProjectileHitSoulWall(projectile, info.Entity, soulWall, info.Other);
                        break;

                    case BattleCollisionTriggerType.Player:
                        playerHitbox = f.Unsafe.GetPointer<BattlePlayerHitboxQComponent>(info.Other);
                        Debug.Log("[CollisionSystem] Projectile hit PlayerHitbox");
                        //f.Events.PlaySoundEvent(SoundEffect.SideWallHit);
                        f.Signals.BattleOnProjectileHitPlayerHitbox(projectile, info.Entity, playerHitbox, info.Other);
                        break;

                    case BattleCollisionTriggerType.Shield:
                        playerHitbox = f.Unsafe.GetPointer<BattlePlayerHitboxQComponent>(info.Other);
                        Debug.Log("[CollisionSystem] Projectile hit Player Shield");
                        f.Signals.BattleOnProjectileHitPlayerShield(projectile, info.Entity, playerHitbox, info.Other);
                        break;

                    case BattleCollisionTriggerType.Goal:
                        BattleGoalQComponent* goal = f.Unsafe.GetPointer<BattleGoalQComponent>(info.Other);
                        Debug.Log("[CollisionSystem] Projectile hit Goal");
                        f.Signals.BattleOnProjectileHitGoal(projectile, info.Entity, goal, info.Other);
                        break;
                }
            }

            // if diamond
            else if (f.Unsafe.TryGetPointer(info.Entity, out BattleDiamondDataQComponent* diamond))
            {
                if (f.Unsafe.TryGetPointer(info.Other, out BattlePlayerHitboxQComponent* playerHitbox))
                {
                    Debug.Log("[CollisionSystem] Diamond hit player");
                    f.Signals.BattleOnDiamondHitPlayer(diamond, info.Entity, playerHitbox, info.Other);
                }
            }
        }
    }
}
