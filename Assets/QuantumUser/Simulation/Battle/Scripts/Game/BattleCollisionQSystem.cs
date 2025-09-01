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

                BattleProjectileQSystem.CollisionData projectileCollisionData = new BattleProjectileQSystem.CollisionData();

                switch (collisionTrigger->Type)
                {
                    case BattleCollisionTriggerType.ArenaBorder:
                        {
                            BattleArenaBorderQComponent* arenaBorder = f.Unsafe.GetPointer<BattleArenaBorderQComponent>(info.Other);
                            Debug.Log("[CollisionSystem] Projectile hit ArenaBorder");
                            //f.Events.PlaySoundEvent(SoundEffect.SideWallHit);
                            projectileCollisionData.arenaBorder = arenaBorder;
                            projectileCollisionData.arenaBorderEntity = info.Other;
                            BattleProjectileQSystem.OnProjectileCollision(f, projectile, info.Entity, BattleCollisionTriggerType.ArenaBorder, projectileCollisionData);
                            break;
                        }

                    case BattleCollisionTriggerType.SoulWall:
                        {
                            BattleSoulWallQComponent* soulWall = f.Unsafe.GetPointer<BattleSoulWallQComponent>(info.Other);
                            Debug.Log("[CollisionSystem] Projectile hit SoulWall");
                            projectileCollisionData.soulWall = soulWall;
                            projectileCollisionData.soulWallEntity = info.Other;
                            BattleProjectileQSystem.OnProjectileCollision(f, projectile, info.Entity, BattleCollisionTriggerType.SoulWall, projectileCollisionData);
                            BattleSoulWallQSystem.OnProjectileHitSoulWall(f, projectile, info.Entity, soulWall, info.Other);
                            BattleDiamondQSystem.OnProjectileHitSoulWall(f, projectile, info.Entity, soulWall, info.Other);
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
                            projectileCollisionData.playerHitbox = playerHitbox;
                            projectileCollisionData.playerHitboxEntity = info.Other;
                            BattleProjectileQSystem.OnProjectileCollision(f, projectile, info.Entity, BattleCollisionTriggerType.Shield, projectileCollisionData);
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
