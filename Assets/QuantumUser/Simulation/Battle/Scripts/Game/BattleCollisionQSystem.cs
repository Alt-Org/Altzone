/// @file BattleCollisionQSystem.cs
/// <summary>
/// Handles all collisions in the game.
/// </summary>
///
/// This system reacts to ISignalOnTrigger2D signals. Depending on which entities are colliding, a new signal is sent for other systems to react to.

using UnityEngine;
using UnityEngine.Scripting;

using Quantum;

namespace Battle.QSimulation.Game
{
    /// <summary>
    /// <span class="brief-h">Collision <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum SystemSignalsOnly@u-exlink</a> @systemslink</span><br/>
    /// Handles all collisions in the game. Reacts only when it receives a signal upon collision.
    /// </summary>
    [Preserve]
    public unsafe class BattleCollisionQSystem : SystemSignalsOnly, ISignalOnTrigger2D
    {
        /// <summary>
        /// <span class="brief-h"><a href = "https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems" > Quantum System Signal method@u-exlink</a>
        /// that gets called when <a href="https://doc-api.photonengine.com/en/quantum/current/interface_quantum_1_1_i_signal_on_trigger2_d.html">ISignalOnTrigger2D@u-exlink</a> is sent.</span><br/>
        /// Handles all 2D trigger collisions in the game.
        /// Routes projectile collisions to the correct signal handler based on the type of object hit.
        /// @warning
        /// This method should only be called via Quantum signal.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <param name="info">Trigger collision information.</param>
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
                            f.Signals.BattleOnProjectileHitArenaBorder(projectile, info.Entity, arenaBorder, info.Other);
                            break;
                        }

                    case BattleCollisionTriggerType.SoulWall:
                        {
                            BattleSoulWallQComponent* soulWall = f.Unsafe.GetPointer<BattleSoulWallQComponent>(info.Other);
                            Debug.Log("[CollisionSystem] Projectile hit SoulWall");
                            f.Signals.BattleOnProjectileHitSoulWall(projectile, info.Entity, soulWall, info.Other);
                            break;
                        }

                    case BattleCollisionTriggerType.Player:
                        {
                            BattlePlayerHitboxQComponent*  playerHitbox = f.Unsafe.GetPointer<BattlePlayerHitboxQComponent>(info.Other);
                            Debug.Log("[CollisionSystem] Projectile hit PlayerHitbox");
                            //f.Events.PlaySoundEvent(SoundEffect.SideWallHit);
                            f.Signals.BattleOnProjectileHitPlayerHitbox(projectile, info.Entity, playerHitbox, info.Other);
                            break;
                        }

                    case BattleCollisionTriggerType.Shield:
                        {
                            BattlePlayerHitboxQComponent* playerHitbox = f.Unsafe.GetPointer<BattlePlayerHitboxQComponent>(info.Other);
                            Debug.Log("[CollisionSystem] Projectile hit Player Shield");
                            f.Signals.BattleOnProjectileHitPlayerShield(projectile, info.Entity, playerHitbox, info.Other);
                            break;
                        }

                    case BattleCollisionTriggerType.Goal:
                        {
                            BattleGoalQComponent* goal = f.Unsafe.GetPointer<BattleGoalQComponent>(info.Other);
                            Debug.Log("[CollisionSystem] Projectile hit Goal");
                            f.Signals.BattleOnProjectileHitGoal(projectile, info.Entity, goal, info.Other);
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
