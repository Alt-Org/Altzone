/// @file BattleGoalQSystem.cs
/// <summary>
/// Handles goals.
/// </summary>
///
/// This system listens to ISignalBattleOnProjectileHitGoal signal that is sent when projectile hits a goal.
/// System then changes BattleGameState to "GameOver" to initiate game over procedures.


using UnityEngine;
using UnityEngine.Scripting;

using Quantum;
using Battle.QSimulation.Game;

namespace Battle.QSimulation.Goal
{
    /// <summary>
    /// <span class="brief-h">%Goal <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum SystemSignalsOnly@u-exlink</a> @systemslink</span><br/>
    /// Triggers the end of the game when it receives signal
    /// </summary>
    [Preserve]
    public unsafe class BattleGoalQSystem : SystemSignalsOnly, ISignalBattleOnProjectileHitGoal
    {
        /// <summary>
        /// <span class="brief-h"><a href = "https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems" > Quantum System Signal method@u-exlink</a>
        /// that gets called when <see cref="Quantum.ISignalBattleOnProjectileHitGoal">ISignalBattleOnProjectileHitGoal</see> is sent.</span><br/>
        /// Signal handler for when a projectile hits a goal.<br/>
        /// Triggers the end of the game by setting the GameState to GameOver.
        /// @warning
        /// This method should only be called via Quantum signal.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="projectileEntity">EntityRef of the projectile.</param>
        /// <param name="goal">Pointer to the goal component.</param>
        /// <param name="goalEntity">EntityRef of the goal.</param>
        public void BattleOnProjectileHitGoal(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattleGoalQComponent* goal, EntityRef goalEntity)
        {
            if (goal->HasTriggered) return;

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
