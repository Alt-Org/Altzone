/// @file BattleCollisionQSystem.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Game,BattleCollisionQSystem} [Quantum System](https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems) which handles all collisions in the game.
/// </summary>

// System usings
using System.Runtime.CompilerServices;

// Unity usings
using UnityEngine.Scripting;

// Quantum usings
using Quantum;

// Battle QSimulation usings
using Battle.QSimulation.Diamond;
using Battle.QSimulation.Goal;
using Battle.QSimulation.Player;
using Battle.QSimulation.Projectile;
using Battle.QSimulation.SoulWall;

namespace Battle.QSimulation.Game
{
    /// <summary>
    /// <span class="brief-h">Collision <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum SystemSignalsOnly@u-exlink</a> @systemslink</span><br/>
    /// Handles all collisions in the game. Reacts only when it receives a signal upon collision.
    /// </summary>
    ///
    /// This system reacts to ISignalOnTrigger2D signals. Depending on which entities are colliding, the appropriate methods in other systems are called.
    [Preserve]
    public unsafe class BattleCollisionQSystem : SystemSignalsOnly, ISignalOnTrigger2D
    {
        public struct ProjectileCollisionData
        {
            public BattleProjectileQComponent* Projectile;
            public EntityRef ProjectileEntity;
            public EntityRef OtherEntity;
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
            public EntityRef ProjectileEntity;
            public BattleGoalQComponent* Goal;
        }

        /// <summary>
        /// Initializes this classes BattleDebugLogger instance.<br/>
        /// This method is exclusively for debug logging purposes.
        /// </summary>
        public static void Init()
        {
            s_debugLogger = BattleDebugLogger.Create<BattleCollisionQSystem>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BattleCollisionTriggerQComponent CreateCollisionTriggerComponent(BattleCollisionTriggerType triggerType)
        {
            BattleCollisionTriggerQComponent component = new BattleCollisionTriggerQComponent();
            component.Type = triggerType;

            return component;
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

                ProjectileCollisionData projectileCollisionData = new()
                {
                    Projectile = projectile,
                    ProjectileEntity = info.Entity,
                    OtherEntity = info.Other
                };

                switch (collisionTrigger->Type)
                {
                    case BattleCollisionTriggerType.ArenaBorder:
                        {
                            s_debugLogger.Log(f, "Projectile hit ArenaBorder");
                            if (BattleProjectileQSystem.IsCollisionFlagSet(f, projectile, BattleProjectileCollisionFlags.Projectile)) break;

                            ArenaBorderCollisionData arenaBorderCollisionData = new()
                            {
                                ArenaBorder = f.Unsafe.GetPointer<BattleArenaBorderQComponent>(info.Other)
                            };
                            //f.Events.PlaySoundEvent(SoundEffect.SideWallHit);
                            BattleProjectileQSystem.OnProjectileCollision(f, &projectileCollisionData, &arenaBorderCollisionData, BattleCollisionTriggerType.ArenaBorder);
                            break;
                        }

                    case BattleCollisionTriggerType.SoulWall:
                        {
                            s_debugLogger.Log(f, "Projectile hit SoulWall");
                            if (BattleProjectileQSystem.IsCollisionFlagSet(f, projectile, BattleProjectileCollisionFlags.Projectile)) break;
                            if (BattleProjectileQSystem.IsCollisionFlagSet(f, projectile, BattleProjectileCollisionFlags.SoulWall)) break;

                            SoulWallCollisionData soulWallCollisionData = new()
                            {
                                SoulWall = f.Unsafe.GetPointer<BattleSoulWallQComponent>(info.Other)
                            };
                            BattleProjectileQSystem.OnProjectileCollision(f, &projectileCollisionData, &soulWallCollisionData, BattleCollisionTriggerType.SoulWall);
                            BattleDiamondQSystem.OnProjectileHitSoulWall(f, &projectileCollisionData, &soulWallCollisionData);
                            BattleSoulWallQSystem.OnProjectileHitSoulWall(f, &projectileCollisionData, &soulWallCollisionData);
                            break;
                        }

                    case BattleCollisionTriggerType.Player:
                        {
                            s_debugLogger.Log(f, "Projectile hit Player Character");
                            if (BattleProjectileQSystem.IsCollisionFlagSet(f, projectile, BattleProjectileCollisionFlags.Player)) break;

                            PlayerCharacterCollisionData playerCollisionData = new()
                            {
                                PlayerCharacterHitbox = f.Unsafe.GetPointer<BattlePlayerHitboxQComponent>(info.Other)
                            };
                            //f.Events.PlaySoundEvent(SoundEffect.SideWallHit);
                            BattleProjectileQSystem.OnProjectileCollision(f, &projectileCollisionData, &playerCollisionData, BattleCollisionTriggerType.Player);
                            BattlePlayerQSystem.OnProjectileHitPlayerCharacter(f, &projectileCollisionData, &playerCollisionData);
                            BattlePlayerClassManager.OnProjectileHitPlayerCharacter(f, &projectileCollisionData, &playerCollisionData);
                            break;
                        }

                    case BattleCollisionTriggerType.Shield:
                        {
                            s_debugLogger.Log(f, "Projectile hit Player Shield");
                            if (BattleProjectileQSystem.IsCollisionFlagSet(f, projectile, BattleProjectileCollisionFlags.Projectile)) break;
                            if (BattleProjectileQSystem.IsCollisionFlagSet(f, projectile, BattleProjectileCollisionFlags.Player)) break;

                            PlayerShieldCollisionData shieldCollisionData = new()
                            {
                                PlayerShieldHitbox = f.Unsafe.GetPointer<BattlePlayerHitboxQComponent>(info.Other),
                                IsLoveProjectileCollision = false
                            };
                            BattleProjectileQSystem.OnProjectileCollision(f, &projectileCollisionData, &shieldCollisionData, BattleCollisionTriggerType.Shield);
                            BattlePlayerQSystem.OnProjectileHitPlayerShield(f, &projectileCollisionData, &shieldCollisionData);
                            BattlePlayerClassManager.OnProjectileHitPlayerShield(f, &projectileCollisionData, &shieldCollisionData);
                            break;
                        }

                    case BattleCollisionTriggerType.Goal:
                        {
                            s_debugLogger.Log(f, "Projectile hit Goal");

                            GoalCollisionData goalCollisionData = new()
                            {
                                Projectile = projectile,
                                ProjectileEntity = info.Entity,
                                Goal = f.Unsafe.GetPointer<BattleGoalQComponent>(info.Other)
                            };
                            BattleGoalQSystem.OnProjectileHitGoal(f, &goalCollisionData);
                            break;
                        }
                }
            }

            // if diamond
            else if (f.Unsafe.TryGetPointer(info.Entity, out BattleDiamondDataQComponent* diamond))
            {
                if (f.Unsafe.TryGetPointer(info.Other, out BattlePlayerHitboxQComponent* playerHitbox))
                {
                    s_debugLogger.Log(f, "Diamond hit player");
                    f.Signals.BattleOnDiamondHitPlayer(diamond, info.Entity, playerHitbox, info.Other);
                }
                else if (f.Unsafe.TryGetPointer(info.Other, out BattleArenaBorderQComponent* arenaBorder))
                {
                    s_debugLogger.Log(f, "Diamond hit ArenaBorder");
                    f.Signals.BattleOnDiamondHitArenaBorder(diamond, info.Entity, arenaBorder, info.Other);
                }
            }
        }

        /// <summary>This classes BattleDebugLogger instance.</summary>
        private static BattleDebugLogger s_debugLogger;
    }
}
