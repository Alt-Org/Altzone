/// @file BattleGoalQSystem.cs
/// <summary>
/// Handles goals.
/// </summary>
///
/// This system is activated by BattleCollisionQSystem when projectile hits a goal.
/// System then changes BattleGameState to "GameOver" to initiate game over procedures.


using UnityEngine;
using UnityEngine.Scripting;
using Quantum;

using Battle.QSimulation.Game;

namespace Battle.QSimulation.Goal
{
    /// <summary>
    /// <span class="brief-h">%Goal <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum SystemSignalsOnly@u-exlink</a> @systemslink</span><br/>
    /// Triggers the end of the game when the projectile hits a goal
    /// </summary>
    [Preserve]
    public unsafe class BattleGoalQSystem : SystemSignalsOnly
    {
        /// <summary>
        /// Called by BattleCollisionQSystem. If the goal has not already been triggered and projectile is not in held state, calls the OnGameOver method in BattleGameControlQSystem.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="projectileEntity">EntityRef of the projectile.</param>
        /// <param name="goal">Pointer to the goal component.</param>
        public static void OnProjectileHitGoal(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattleGoalQComponent* goal)
        {
            if (goal->HasTriggered) return;
            if (projectile->IsHeld) return;

            BattleTeamNumber winningTeam = goal->TeamNumber switch
            {
                BattleTeamNumber.TeamAlpha => BattleTeamNumber.TeamBeta,
                BattleTeamNumber.TeamBeta  => BattleTeamNumber.TeamAlpha,

                _ => BattleTeamNumber.NoTeam
            };

            BattleGameControlQSystem.OnGameOver(f, winningTeam, projectile, projectileEntity);

            goal->HasTriggered = true;

            Debug.LogFormat("[BattleGoalQSystem] GameOver {0} Goal", winningTeam.ToString());
        }
    }
}
