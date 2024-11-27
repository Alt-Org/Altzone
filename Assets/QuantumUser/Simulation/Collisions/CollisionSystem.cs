using System.Collections;
using System.Collections.Generic;
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
                if (f.Unsafe.TryGetPointer<SoulWall>(info.Other, out var soulWall))
                {
                    // Projectile Hit SoulWall
                    Debug.Log("SoulWall hit - CollisionSystem");
                    f.Signals.OnCollisionProjectileHitSoulWall(info, projectile, soulWall);
                }
                else if (f.Unsafe.TryGetPointer<Goal>(info.Other, out var asteroid))
                {
                    // projectile Hit Goal
                    Debug.Log("Goal hit - CollisionSystem");
                }
                else
                {
                    //projectile hit a side wall
                    //Debug.Log("Something hit, side wall probably - CollisionSystem");
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

                if (goalConfig != null)
                {
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
