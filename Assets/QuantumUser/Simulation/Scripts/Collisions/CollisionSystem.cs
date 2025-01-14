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
    public unsafe class CollisionsSystem : SystemSignalsOnly, ISignalOnCollisionEnter2D, ISignalOnTrigger2D
    {
        public void OnCollisionEnter2D(Frame f, CollisionInfo2D info)
        {
            // Projectile is colliding with something
            if (f.Unsafe.TryGetPointer<Projectile>(info.Entity, out var projectile))
            {
                var projectileConfig = f.FindAsset<ProjectileConfig>(projectile->ProjectileConfig);

                // Check if the projectile is on cooldown
                if (projectileConfig.Cooldown > 0)
                {
                    return; // Skip processing if cooldown is active
                } else
                {
                    // Apply a cooldown to the projectile
                    projectileConfig.Cooldown= FP.FromFloat_UNSAFE(0.05f); // 0.1 seconds
                }

                if (f.Unsafe.TryGetPointer<SoulWall>(info.Other, out var soulWall))
                {
                    // Projectile Hit SoulWall
                    Debug.Log("SoulWall hit - CollisionSystem");
                    f.Events.PlaySoundEvent(SoundEffect.WallBroken);
                    f.Signals.OnCollisionProjectileHitSoulWall(info, projectile, soulWall);

                }
                else
                {
                    //projectile hit a side wall
                    //Debug.Log("Something hit, side wall probably - CollisionSystem");
                    f.Events.PlaySoundEvent(SoundEffect.SideWallHit);
                    f.Signals.OnCollisionProjectileHitSomething(info, projectile);
                }


            }
        }

        //Handles all triggers in the game; currently just the goals
        public void OnTrigger2D(Frame f, TriggerInfo2D info)
        {

            // Try to get the Goal component
            if (f.Unsafe.TryGetPointer<Goal>(info.Other, out var goal))
            {
                // Resolve the GoalConfig asset using the AssetRef
                var goalConfig = f.FindAsset<GoalConfig>(goal->goalConfig);

                // Check if the sound has already been played
                if (goalConfig.hasTriggered)
                {
                    return; // Exit if the sound was already played
                }

                // Mark the sound as played
                goalConfig.hasTriggered = true;

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
