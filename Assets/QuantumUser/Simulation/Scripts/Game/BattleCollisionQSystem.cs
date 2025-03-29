using UnityEngine;
using UnityEngine.Scripting;

using Quantum;

using Battle.QSimulation.Goal;

/***
 * If the collision side grows larger, will split the trigger to its own system. But for now can be here
 * despite somewhat misleading name. If Stays here, might rename this to something more accurate down the line
 *
 */

namespace Battle.QSimulation.Game
{
    [Preserve]
    public unsafe class BattleCollisionQSystem : SystemSignalsOnly, ISignalOnTrigger2D
    {
        //Handles all triggers in the game
        public void OnTrigger2D(Frame f, TriggerInfo2D info)
        {
            // if projectile
            if (f.Unsafe.TryGetPointer(info.Entity, out BattleProjectileQComponent* projectile))
            {
                // if projectile hits soul wall
                if (f.Unsafe.TryGetPointer(info.Other, out BattleSoulWallQComponent* soulWall))
                {
                    // Projectile Hit SoulWall
                    Debug.Log("[CollisionSystem] SoulWall hit");
                    f.Signals.OnTriggerProjectileHitSoulWall(projectile, info.Entity, soulWall, info.Other);
                }

                // if projectile hits arena border
                else if (f.Unsafe.TryGetPointer(info.Other, out BattleArenaBorderQComponent* arenaBorder))
                {
                    //projectile hit a side wall
                    Debug.Log("[CollisionSystem] Sidewall hit");
                    //f.Events.PlaySoundEvent(SoundEffect.SideWallHit);
                    f.Signals.OnTriggerProjectileHitArenaBorder(projectile, info.Entity, arenaBorder, info.Other);
                }

                // if projectile hits playerHitbox
                else if (f.Unsafe.TryGetPointer(info.Other, out BattlePlayerHitboxQComponent* playerHitbox))
                {
                    //projectile hit a player's shield
                    Debug.Log("[CollisionSystem] Player's shield hit");
                    //f.Events.PlaySoundEvent(SoundEffect.SideWallHit);
                    f.Signals.OnTriggerProjectileHitPlayerHitbox(projectile, info.Entity, playerHitbox, info.Other);
                }

                // if projectile hits goals
                else if (f.Unsafe.TryGetPointer(info.Other, out BattleGoalQComponent* goal))
                {
                    // Resolve the GoalConfig asset using the AssetRef
                    GoalConfig goalConfig = f.FindAsset(goal->goalConfig);

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
