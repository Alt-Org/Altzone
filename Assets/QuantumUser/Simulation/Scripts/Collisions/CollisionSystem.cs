using System.Collections;
using System.Collections.Generic;
using Photon.Deterministic;
using UnityEngine;
using UnityEngine.Scripting;

/***
 * If the collision side grows larger, will split the trigger to its own system. But for now can be here
 * despite somewhat misleading name. If Stays here, might rename this to something more accurate down the line
 *
 */

namespace Quantum
{
    [Preserve]
    public unsafe class CollisionsSystem : SystemSignalsOnly, /*ISignalOnCollisionEnter2D,*/ ISignalOnTrigger2D
    {
        /*
        public void OnCollisionEnter2D(Frame f, CollisionInfo2D info)
        {
            // Projectile is colliding with something
            if (f.Unsafe.TryGetPointer<Projectile>(info.Entity, out Projectile* projectile))
            {
                ProjectileConfig projectileConfig = f.FindAsset<ProjectileConfig>(projectile->ProjectileConfig);

                // Check if the projectile is on cooldown
                if (projectileConfig.Cooldown > 0)
                {
                    return; // Skip processing if cooldown is active
                }
                else
                {
                    // Apply a cooldown to the projectile
                    projectileConfig.Cooldown= FP.FromFloat_UNSAFE(0.05f); // 0.1 seconds
                }

                if (f.Unsafe.TryGetPointer<SoulWall>(info.Other, out SoulWall* soulWall))
                {
                    // Projectile Hit SoulWall
                    Debug.Log("[CollisionSystem] SoulWall hit");
                    f.Events.PlaySoundEvent(SoundEffect.WallBroken);
                    f.Signals.OnCollisionProjectileHitSoulWall(info, projectile, soulWall);

                }
                else
                {
                    //projectile hit a side wall or player
                    Debug.Log("[CollisionSystem] Player or sidewall hit");
                    f.Events.PlaySoundEvent(SoundEffect.SideWallHit);
                    f.Signals.OnCollisionProjectileHitSomething(info, projectile);
                }

            }
        }
        */

        //Handles all triggers in the game
        public void OnTrigger2D(Frame f, TriggerInfo2D info)
        {
            // if projectile
            if (f.Unsafe.TryGetPointer<Projectile>(info.Entity, out Projectile* projectile))
            {
                // if projectile hits soul wall
                if (f.Unsafe.TryGetPointer<SoulWall>(info.Other, out SoulWall* soulWall))
                {
                    // Projectile Hit SoulWall
                    Debug.Log("[CollisionSystem] SoulWall hit");
                    if (projectile->CoolDown <= 0) f.Events.PlaySoundEvent(SoundEffect.WallBroken);
                    f.Signals.OnTriggerProjectileHitSoulWall(projectile, info.Entity, soulWall, info.Other);
                    projectile->CoolDown = FP._0_10;
                }

                // if projectile hits arena border
                else if (f.Unsafe.TryGetPointer<ArenaBorder>(info.Other, out ArenaBorder* arenaBorder))
                {
                    //projectile hit a side wall
                    Debug.Log("[CollisionSystem] Sidewall hit");
                    //f.Events.PlaySoundEvent(SoundEffect.SideWallHit);
                    f.Signals.OnTriggerProjectileHitArenaBorder(projectile, info.Entity, arenaBorder, info.Other);
                }

                // if projectile hits player
                else if (f.Unsafe.TryGetPointer<PlayerData>(info.Other, out PlayerData* playerData))
                {
                    //projectile hit a player
                    Debug.Log("[CollisionSystem] Player hit");
                    //f.Events.PlaySoundEvent(SoundEffect.SideWallHit);
                    f.Signals.OnTriggerProjectileHitPlayer(projectile, info.Entity, playerData, info.Other);
                }

                // if projectile hits goals
                else if (f.Unsafe.TryGetPointer<Goal>(info.Other, out Goal* goal))
                {
                    // Resolve the GoalConfig asset using the AssetRef
                    GoalConfig goalConfig = f.FindAsset<GoalConfig>(goal->goalConfig);

                    // Check if the sound has already been played
                    if (goal->hasTriggered)
                    {
                        return; // Exit if the sound was already played
                    }

                    // Mark the sound as played
                    goal->hasTriggered = true;

                    if (goalConfig != null)
                    {
                        f.Events.PlaySoundEvent(SoundEffect.GoalHit);

                        if (goalConfig.goal == GoalType.TopGoal)
                        {
                            f.Signals.OnTriggerTopGoal();
                        }
                        else if (goalConfig.goal == GoalType.BottomGoal)
                        {
                            f.Signals.OnTriggerBottomGoal();
                        }

                    }
                    else
                    {
                        Debug.Log("GoalConfig asset not found");
                    }
                }
                else
                {
                    Debug.Log("Goal component not found");
                }
            }
        }
    }
}
