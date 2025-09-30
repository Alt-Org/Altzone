/// @file BattleCollisionQSystem.cs
/// <summary>
/// Handles all collisions in the game.
/// </summary>
///
/// This system reacts to ISignalOnTrigger2D signals. Depending on which entities are colliding, the appropriate methods in other systems are called.

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
    /// <summary>
    /// <span class="brief-h">Collision <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum SystemSignalsOnly@u-exlink</a> @systemslink</span><br/>
    /// Handles all collisions in the game. Reacts only when it receives a signal upon collision.
    /// </summary>
    [Preserve]
    public unsafe class BattleCollisionQSystem : SystemSignalsOnly, ISignalOnTrigger2D
    {
        public struct ProjectileCollisionData
        {
            public BattleProjectileQComponent* Projectile;
            public EntityRef CollidingEntity;
            public EntityRef OtherEntity;
            public bool ProjectileHeld;
        }

        public struct ArenaBorderCollisionData
        {
            public BattleArenaBorderQComponent* ArenaBorder;
        }

        public struct SoulWallCollisionData
        {
            public BattleSoulWallQComponent* SoulWall;
        }

        public struct PlayerCharacterCollisionData
        {
            public BattlePlayerHitboxQComponent* PlayerCharacterHitbox;
        }

        public struct PlayerShieldCollisionData
        {
            public BattlePlayerHitboxQComponent* PlayerShieldHitbox;
            public bool IsLoveProjectileCollision;
        }

        public struct GoalCollisionData
        {
            public BattleProjectileQComponent* Projectile;
            public EntityRef CollidingEntity;
            public BattleGoalQComponent* Goal;
        }

        /// <summary>
        /// <span class="brief-h"><a href = "https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems" > Quantum System Signal method@u-exlink</a>
        /// that gets called when <a href="https://doc-api.photonengine.com/en/quantum/current/interface_quantum_1_1_i_signal_on_trigger2_d.html">ISignalOnTrigger2D@u-exlink</a> is sent.</span><br/>
        /// Handles all 2D trigger collisions in the game.<br/>
        /// Routes projectile collisions to the correct systems based on the type of object hit.
        /// @warning
        /// This method should only be called via Quantum signal.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="info">Trigger collision information.</param>
        public void OnTrigger2D(Frame f, TriggerInfo2D info)
        {
            // if projectile
            if (f.Unsafe.TryGetPointer(info.Entity, out BattleProjectileQComponent* projectile))
            {
                if(!f.Unsafe.TryGetPointer(info.Other, out BattleCollisionTriggerQComponent* collisionTrigger)) return;

                ProjectileCollisionData projectileData = new()
                {
                    Projectile = projectile,
                    CollidingEntity = info.Entity,
                    OtherEntity = info.Other,
                    ProjectileHeld = false
                };

                switch (collisionTrigger->Type)
                {
                    case BattleCollisionTriggerType.ArenaBorder:
                        {
                            Debug.Log("[CollisionSystem] Projectile hit ArenaBorder");
                            if (BattleProjectileQSystem.IsCollisionFlagSet(f, projectile, BattleProjectileCollisionFlags.Projectile)) break;

                            ArenaBorderCollisionData arenaBorderData = new()
                            {
                                ArenaBorder = f.Unsafe.GetPointer<BattleArenaBorderQComponent>(info.Other)
                            };
                            //f.Events.PlaySoundEvent(SoundEffect.SideWallHit);
                            BattleProjectileQSystem.OnProjectileCollision(f, &projectileData, &arenaBorderData, BattleCollisionTriggerType.ArenaBorder);
                            break;
                        }

                    case BattleCollisionTriggerType.SoulWall:
                        {
                            Debug.Log("[CollisionSystem] Projectile hit SoulWall");
                            if (BattleProjectileQSystem.IsCollisionFlagSet(f, projectile, BattleProjectileCollisionFlags.Projectile)) break;
                            if (BattleProjectileQSystem.IsCollisionFlagSet(f, projectile, BattleProjectileCollisionFlags.SoulWall)) break;

                            SoulWallCollisionData soulWallData = new()
                            {
                                SoulWall = f.Unsafe.GetPointer<BattleSoulWallQComponent>(info.Other)
                            };
                            BattleProjectileQSystem.OnProjectileCollision(f, &projectileData, &soulWallData, BattleCollisionTriggerType.SoulWall);
                            BattleDiamondQSystem.OnProjectileHitSoulWall(f, &projectileData, &soulWallData);
                            BattleSoulWallQSystem.OnProjectileHitSoulWall(f, &projectileData, &soulWallData);
                            break;
                        }

                    case BattleCollisionTriggerType.Player:
                        {
                            Debug.Log("[CollisionSystem] Projectile hit Player Character");
                            if (BattleProjectileQSystem.IsCollisionFlagSet(f, projectile, BattleProjectileCollisionFlags.Player)) break;

                            PlayerCharacterCollisionData playerData = new()
                            {
                                PlayerCharacterHitbox = f.Unsafe.GetPointer<BattlePlayerHitboxQComponent>(info.Other)
                            };
                            //f.Events.PlaySoundEvent(SoundEffect.SideWallHit);
                            BattlePlayerQSystem.OnProjectileHitPlayerCharacter(f, &projectileData, &playerData);
                            BattlePlayerClassManager.OnProjectileHitPlayerCharacter(f, &projectileData, &playerData);
                            break;
                        }

                    case BattleCollisionTriggerType.Shield:
                        {
                            Debug.Log("[CollisionSystem] Projectile hit Player Shield");
                            if (BattleProjectileQSystem.IsCollisionFlagSet(f, projectile, BattleProjectileCollisionFlags.Projectile)) break;
                            if (BattleProjectileQSystem.IsCollisionFlagSet(f, projectile, BattleProjectileCollisionFlags.Player)) break;

                            PlayerShieldCollisionData shieldData = new()
                            {
                                PlayerShieldHitbox = f.Unsafe.GetPointer<BattlePlayerHitboxQComponent>(info.Other),
                                IsLoveProjectileCollision = false
                            };
                            BattleProjectileQSystem.OnProjectileCollision(f, &projectileData, &shieldData, BattleCollisionTriggerType.Shield);
                            BattlePlayerQSystem.OnProjectileHitPlayerShield(f, &projectileData, &shieldData);
                            BattlePlayerClassManager.OnProjectileHitPlayerShield(f, &projectileData, &shieldData);
                            break;
                        }

                    case BattleCollisionTriggerType.Goal:
                        {
                            Debug.Log("[CollisionSystem] Projectile hit Goal");

                            GoalCollisionData goalData = new()
                            {
                                Projectile = projectile,
                                CollidingEntity = info.Entity,
                                Goal = f.Unsafe.GetPointer<BattleGoalQComponent>(info.Other)
                            };
                            BattleGoalQSystem.OnProjectileHitGoal(f, &goalData);
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
