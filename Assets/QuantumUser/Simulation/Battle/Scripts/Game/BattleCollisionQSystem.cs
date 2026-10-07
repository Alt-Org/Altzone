/// @file BattleCollisionQSystem.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Game,BattleCollisionQSystem} [Quantum System](https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems) which handles all collisions in the game.
/// </summary>

// System usings
using System.Runtime.CompilerServices;

// Unity usings
using Battle.QSimulation.Diamond;
using Battle.QSimulation.Goal;
using Battle.QSimulation.Player;

// Battle QSimulation usings
using Battle.QSimulation.Projectile;
using Battle.QSimulation.SoulWall;

// Quantum usings
using Quantum;
using UnityEngine.Scripting;

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
            public BattleProjectileHandle Projectile;
            public BattleEmotionState ProjectileEmotionBase;
            public BattleEmotionState ProjectileEmotionCurrent;
            public EntityRef OtherEntityRef;
            public bool IsLoveProjectileCollision;
        }

        public struct PlayerClass100ProjectileCollisionData
        {
            public BattlePlayerClass100ProjectileQComponent* Projectile;
            public EntityRef ProjectileEntityRef;
            public EntityRef OtherEntityRef;
        }

        public struct GoalCollisionData
        {
            public BattleProjectileQComponent* Projectile;
            public EntityRef ProjectileEntityRef;
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

        /// <summary>
        /// Creates a BattleCollisionTriggerQComponent with the given @cref{Quantum,BattleCollisionTriggerType}.
        /// </summary>
        ///
        /// <param name="triggerType">BattleCollisionTriggerType the component needs to be.</param>
        ///
        /// <returns>The newly created component.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BattleCollisionTriggerQComponent CreateCollisionTriggerComponent(BattleCollisionTriggerType triggerType)
        {
            BattleCollisionTriggerQComponent component = new();
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
            if (!f.Unsafe.TryGetPointer(info.Entity, out BattleCollisionColliderQComponent* collisionCollider)) return;
            if (!f.Unsafe.TryGetPointer(info.Other, out BattleCollisionTriggerQComponent* collisionTrigger)) return;

            switch (collisionCollider->Type)
            {
                case BattleCollisionColliderType.Projectile:
                {
                    BattleProjectileHandle projectile = BattleProjectileHandle.Create(f, info.Entity);

                    ProjectileCollisionData projectileCollisionData = new()
                    {
                        Projectile         = projectile,
                        ProjectileEmotionBase    = projectile.Data->EmotionBase,
                        ProjectileEmotionCurrent = projectile.Data->EmotionCurrent,
                        OtherEntityRef              = info.Other
                    };

                    switch (collisionTrigger->Type)
                    {
                        case BattleCollisionTriggerType.ArenaBorder:
                        {
                            s_debugLogger.Log(f, "Projectile hit ArenaBorder");
                            if (BattleProjectileQSystem.IsCollisionFlagSet(f, projectile.Data, BattleProjectileCollisionFlags.Projectile)) break;

                            BattleArenaBorderQComponent* arenaBorder = f.Unsafe.GetPointer<BattleArenaBorderQComponent>(info.Other);

                            //f.Events.PlaySoundEvent(SoundEffect.SideWallHit);
                            BattleProjectileQSystem.OnProjectileCollision(f, &projectileCollisionData, arenaBorder, BattleCollisionTriggerType.ArenaBorder);
                            break;
                        }

                        case BattleCollisionTriggerType.SoulWall:
                        {
                            s_debugLogger.Log(f, "Projectile hit SoulWall");
                            if (BattleProjectileQSystem.IsCollisionFlagSet(f, projectile.Data, BattleProjectileCollisionFlags.Projectile)) break;
                            if (BattleProjectileQSystem.IsCollisionFlagSet(f, projectile.Data, BattleProjectileCollisionFlags.SoulWall)) break;

                            BattleSoulWallQComponent* soulWall = f.Unsafe.GetPointer<BattleSoulWallQComponent>(info.Other);

                            BattleProjectileQSystem.OnProjectileCollision(f, &projectileCollisionData, soulWall, BattleCollisionTriggerType.SoulWall);
                            BattleDiamondQSystem.OnProjectileHitSoulWall(f, &projectileCollisionData, soulWall);
                            BattleSoulWallQSystem.OnProjectileHitSoulWall(f, &projectileCollisionData, soulWall);
                            break;
                        }

                        case BattleCollisionTriggerType.Player:
                        {
                            s_debugLogger.Log(f, "Projectile hit Player Character");
                            if (BattleProjectileQSystem.IsCollisionFlagSet(f, projectile.Data, BattleProjectileCollisionFlags.Player)) break;

                            BattlePlayerHitboxQComponent* playerCharacterHitbox = f.Unsafe.GetPointer<BattlePlayerHitboxQComponent>(info.Other);

                            //f.Events.PlaySoundEvent(SoundEffect.SideWallHit);
                            BattleProjectileQSystem.OnProjectileCollision(f, &projectileCollisionData, playerCharacterHitbox, BattleCollisionTriggerType.Player);
                            BattlePlayerQSystem.OnProjectileHitPlayerCharacter(f, &projectileCollisionData);
                            BattlePlayerClassManager.OnProjectileHitPlayerCharacter(f, &projectileCollisionData);
                            break;
                        }

                        case BattleCollisionTriggerType.Shield:
                        {
                            s_debugLogger.Log(f, "Projectile hit Player Shield");
                            if (BattleProjectileQSystem.IsCollisionFlagSet(f, projectile.Data, BattleProjectileCollisionFlags.Projectile)) break;
                            if (BattleProjectileQSystem.IsCollisionFlagSet(f, projectile.Data, BattleProjectileCollisionFlags.Player)) break;

                            BattlePlayerHitboxQComponent* playerShieldHitbox = f.Unsafe.GetPointer<BattlePlayerHitboxQComponent>(info.Other);

                            projectileCollisionData.IsLoveProjectileCollision = false;

                            BattleProjectileQSystem.OnProjectileCollision(f, &projectileCollisionData, playerShieldHitbox, BattleCollisionTriggerType.Shield);
                            BattlePlayerQSystem.OnProjectileHitPlayerShield(f, &projectileCollisionData);
                            BattlePlayerClassManager.OnProjectileHitPlayerShield(f, &projectileCollisionData);
                            break;
                        }

                        case BattleCollisionTriggerType.Goal:
                        {
                            s_debugLogger.Log(f, "Projectile hit Goal");

                            GoalCollisionData goalCollisionData = new()
                            {
                                Projectile       = projectile.Data,
                                ProjectileEntityRef = info.Entity,
                                Goal             = f.Unsafe.GetPointer<BattleGoalQComponent>(info.Other)
                            };

                            BattleGoalQSystem.OnProjectileHitGoal(f, &goalCollisionData);
                            break;
                        }
                    }
                    break;
                }

                case BattleCollisionColliderType.Diamond:
                {
                    BattleDiamondDataQComponent* diamond = f.Unsafe.GetPointer<BattleDiamondDataQComponent>(info.Entity);

                    switch (collisionTrigger->Type)
                    {
                        case BattleCollisionTriggerType.Player:
                        {
                            s_debugLogger.Log(f, "Diamond hit player");

                            BattlePlayerHitboxQComponent* playerHitbox = f.Unsafe.GetPointer<BattlePlayerHitboxQComponent>(info.Other);

                            f.Signals.BattleOnDiamondHitPlayer(diamond, info.Entity, info.Other);
                            break;
                        }

                        case BattleCollisionTriggerType.ArenaBorder:
                        {
                            s_debugLogger.Log(f, "Diamond hit ArenaBorder");

                            BattleArenaBorderQComponent* arenaBorder = f.Unsafe.GetPointer<BattleArenaBorderQComponent>(info.Other);

                            f.Signals.BattleOnDiamondHitArenaBorder(diamond, info.Entity, arenaBorder, info.Other);
                            break;
                        }
                    }
                    break;
                }

                case BattleCollisionColliderType.PlayerClass100Projectile:
                {
                    BattlePlayerClass100ProjectileQComponent* playerClass100Projectile = f.Unsafe.GetPointer<BattlePlayerClass100ProjectileQComponent>(info.Entity);

                    PlayerClass100ProjectileCollisionData playerClass100ProjectileCollisionData = new()
                    {
                        Projectile          = playerClass100Projectile,
                        ProjectileEntityRef = info.Entity,
                        OtherEntityRef      = info.Other
                    };

                    switch (collisionTrigger->Type)
                    {
                        case BattleCollisionTriggerType.Projectile:
                        {
                            s_debugLogger.Log("Player class 100 projectile hit the Emotion Projectile");

                            BattleProjectileLinkQComponent* trigger           = f.Unsafe.GetPointer<BattleProjectileLinkQComponent>(info.Other);
                            BattleProjectileQComponent*     emotionProjectile = f.Unsafe.GetPointer<BattleProjectileQComponent>(trigger->ERef);

                            BattlePlayerClass100ProjectileQSystem.OnProjectileHitEmotionProjectile(f, playerClass100Projectile, info.Entity, emotionProjectile);
                            break;
                        }

                        case BattleCollisionTriggerType.Goal:
                        {
                            s_debugLogger.Log("Player class 100 projectile hit the Goal zone");
                            BattlePlayerClass100ProjectileQSystem.OnProjectileHitObstacle(f, info.Entity);
                            break;
                        }

                        case BattleCollisionTriggerType.SoulWall:
                        {
                            s_debugLogger.Log("Player class 100 projectile hit the Soul Wall");
                            BattlePlayerClass100ProjectileQSystem.OnProjectileHitObstacle(f, info.Entity);
                            break;
                        }

                        case BattleCollisionTriggerType.ArenaBorder:
                        {
                            s_debugLogger.Log("Player class 100 projectile hit the Arena Border");

                            BattleArenaBorderQComponent* arenaBorder = f.Unsafe.GetPointer<BattleArenaBorderQComponent>(info.Other);

                            BattlePlayerClass100ProjectileQSystem.OnProjectileHitArenaBorder(f, arenaBorder, &playerClass100ProjectileCollisionData);
                            break;
                        }
                    }
                    break;
                }
            }
        }
        /// <summary>This classes BattleDebugLogger instance.</summary>
        private static BattleDebugLogger s_debugLogger;
    }
}
