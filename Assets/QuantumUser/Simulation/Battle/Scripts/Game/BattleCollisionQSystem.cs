using UnityEngine;
using UnityEngine.Scripting;

using Quantum;

namespace Battle.QSimulation.Game
{
    /// <summary>
    /// Collision <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">SystemSignalsOnly Quantum System</a>.<br/>
    /// Handles all collisions in the game. Reacts only when it receives a signal upon collision.
    /// </summary>
    [Preserve]
    public unsafe class BattleCollisionQSystem : SystemSignalsOnly, ISignalOnTrigger2D
    {
        /// <summary>
        /// Handles all 2D trigger collisions in the game.
        /// Routes projectile collisions to the correct signal handler based on the type of object hit.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <param name="info">Trigger collision information.</param>
        public void OnTrigger2D(Frame f, TriggerInfo2D info)
        {
            // if projectile
            if (f.Unsafe.TryGetPointer(info.Entity, out BattleProjectileQComponent* projectile))
            {
                // if projectile hits soulWall
                if (f.Unsafe.TryGetPointer(info.Other, out BattleSoulWallQComponent* soulWall))
                {
                    Debug.Log("[CollisionSystem] Projectile hit SoulWall");
                    f.Signals.BattleOnProjectileHitSoulWall(projectile, info.Entity, soulWall, info.Other);
                }

                // if projectile hits arenaBorder
                else if (f.Unsafe.TryGetPointer(info.Other, out BattleArenaBorderQComponent* arenaBorder))
                {
                    Debug.Log("[CollisionSystem] Projectile hit ArenaBorder");
                    //f.Events.PlaySoundEvent(SoundEffect.SideWallHit);
                    f.Signals.BattleOnProjectileHitArenaBorder(projectile, info.Entity, arenaBorder, info.Other);
                }

                // if projectile hits playerHitbox
                else if (f.Unsafe.TryGetPointer(info.Other, out BattlePlayerHitboxQComponent* playerHitbox))
                {
                    Debug.Log("[CollisionSystem] Projectile hit PlayerHitbox");
                    //f.Events.PlaySoundEvent(SoundEffect.SideWallHit);
                    f.Signals.BattleOnProjectileHitPlayerHitbox(projectile, info.Entity, playerHitbox, info.Other);
                }

                // if projectile hits goal
                else if (f.Unsafe.TryGetPointer(info.Other, out BattleGoalQComponent* goal))
                {
                    Debug.Log("[CollisionSystem] Projectile hit Goal");
                    f.Signals.BattleOnProjectileHitGoal(projectile, info.Entity, goal, info.Other);
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
