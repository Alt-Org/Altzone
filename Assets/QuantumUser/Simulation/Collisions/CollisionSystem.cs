using System.Collections;
using System.Collections.Generic;
using Quantum.QuantumUser.Simulation.Goal;
using UnityEngine;
using UnityEngine.Scripting;

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

        public void OnTrigger2D(Frame f, TriggerInfo2D info)
        {
            Debug.Log("Trigger detected");

            // Try to get the Goal component
            if (f.Unsafe.TryGetPointer<Goal>(info.Other, out var goal))
            {
                Debug.Log("Goal component found");

                // Resolve the GoalConfig asset using the AssetRef
                var goalConfig = f.FindAsset<GoalConfig>(goal->goalConfig);

                if (goalConfig != null)
                {
                    if (goalConfig.goal == QuantumUser.Simulation.Goal.GoalType.TopGoal )
                    {
                        Debug.Log("Top Goal triggered");
                    }
                    else if (goalConfig.goal == QuantumUser.Simulation.Goal.GoalType.BottomGoal)
                    {
                        Debug.Log("Bottom Goal triggered");
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
