using UnityEngine;
using UnityEngine.Scripting;
using Quantum;

using Battle.QSimulation.Projectile;
using Battle.QSimulation.Goal;
using Battle.QSimulation.Player;
using Battle.QSimulation.Diamond;
using Battle.QSimulation.SoulWall;

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

                switch (collisionTrigger->Type)
                {
                    case BattleCollisionTriggerType.ArenaBorder:
                        {
                            BattleArenaBorderQComponent* arenaBorder = f.Unsafe.GetPointer<BattleArenaBorderQComponent>(info.Other);
                            Debug.Log("[CollisionSystem] Projectile hit ArenaBorder");
                            //f.Events.PlaySoundEvent(SoundEffect.SideWallHit);
                            BattleProjectileQSystem.OnProjectileCollision(f, projectile, info.Entity, info.Other, arenaBorder, BattleCollisionTriggerType.ArenaBorder);
                            break;
                        }

                    case BattleCollisionTriggerType.SoulWall:
                        {
                            BattleSoulWallQComponent* soulWall = f.Unsafe.GetPointer<BattleSoulWallQComponent>(info.Other);
                            Debug.Log("[CollisionSystem] Projectile hit SoulWall");
                            BattleProjectileQSystem.OnProjectileCollision(f, projectile, info.Entity, info.Other, soulWall, BattleCollisionTriggerType.SoulWall);
                            BattleDiamondQSystem.OnProjectileHitSoulWall(f, projectile, info.Entity, soulWall);
                            BattleSoulWallQSystem.OnProjectileHitSoulWall(f, projectile, soulWall, info.Other);
                            break;
                        }

                    case BattleCollisionTriggerType.Player:
                        {
                            BattlePlayerHitboxQComponent*  playerHitbox = f.Unsafe.GetPointer<BattlePlayerHitboxQComponent>(info.Other);
                            Debug.Log("[CollisionSystem] Projectile hit PlayerHitbox");
                            //f.Events.PlaySoundEvent(SoundEffect.SideWallHit);
                            BattlePlayerQSystem.OnProjectileHitPlayerHitbox(f, projectile, info.Entity, playerHitbox, info.Other);
                            break;
                        }

                    case BattleCollisionTriggerType.Shield:
                        {
                            BattlePlayerHitboxQComponent* playerHitbox = f.Unsafe.GetPointer<BattlePlayerHitboxQComponent>(info.Other);
                            Debug.Log("[CollisionSystem] Projectile hit Player Shield");
                            BattleProjectileQSystem.OnProjectileCollision(f, projectile, info.Entity, info.Other, playerHitbox, BattleCollisionTriggerType.Shield);
                            BattlePlayerQSystem.OnProjectileHitPlayerShield(f, projectile, info.Entity, playerHitbox, info.Other);
                            break;
                        }

                    case BattleCollisionTriggerType.Goal:
                        {
                            BattleGoalQComponent* goal = f.Unsafe.GetPointer<BattleGoalQComponent>(info.Other);
                            Debug.Log("[CollisionSystem] Projectile hit Goal");
                            BattleGoalQSystem.OnProjectileHitGoal(f, projectile, info.Entity, goal);
                            break;
                        }
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
