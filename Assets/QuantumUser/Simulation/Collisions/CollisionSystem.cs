using System.Collections;
using System.Collections.Generic;
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
            if (f.Unsafe.TryGetPointer<Projectile>(info.Entity, out  var projectile))
            {

                if (f.Unsafe.TryGetPointer<SoulWall>(info.Other, out  var soulWall))
                {
                    // Projectile Hit SoulWall
                    Debug.Log("SoulWall hit - CollisionSystem");
                    f.Signals.OnCollisionProjectileHitSoulWall(info,projectile,soulWall);
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
                   // f.Signals.OnCollisionProjectileHitSoulWall(info,projectile,soulWall);
                }
            }

        }

        public void OnTrigger2D(Frame f, TriggerInfo2D info)
        {
            Debug.Log("Something hit something Trigger");
        }
    }
}
