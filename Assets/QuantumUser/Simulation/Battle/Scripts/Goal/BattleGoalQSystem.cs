/// @file BattleGoalQSystem.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Goal,BattleGoalQSystem} [Quantum System](https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems) which triggers the end of the game when the projectile hits a goal.
/// </summary>

// Unity usings
using UnityEngine.Scripting;

// Quantum usings
using Quantum;

// Battle QSimulation usings
using Battle.QSimulation.Game;

namespace Battle.QSimulation.Goal
{
    /// <summary>
    /// <span class="brief-h">%Goal <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum SystemSignalsOnly@u-exlink</a> @systemslink</span><br/>
    /// Triggers the end of the game when the projectile hits a goal.
    /// </summary>
    ///
    /// This system is activated by BattleCollisionQSystem when projectile hits a goal.
    /// System then changes BattleGameState to "GameOver" to initiate game over procedures.
    [Preserve]
    public unsafe class BattleGoalQSystem : SystemSignalsOnly
    {
        /// <summary>
        /// Initializes this classes BattleDebugLogger instance.<br/>
        /// This method is exclusively for debug logging purposes.
        /// </summary>
        public static void Init()
        {
            s_debugLogger = BattleDebugLogger.Create<BattleGoalQSystem>();
        }

        /// <summary>
        /// Called by BattleCollisionQSystem. If the goal has not already been triggered and projectile is not in held state, calls the OnGameOver method in BattleGameControlQSystem.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="goalCollisionData">Collision data.</param>
        public static void OnProjectileHitGoal(Frame f, BattleCollisionQSystem.GoalCollisionData* goalCollisionData)
        {
            if (goalCollisionData->Goal->HasTriggered) return;
            if (goalCollisionData->Projectile->IsHeld) return;

            BattleTeamNumber winningTeam = goalCollisionData->Goal->TeamNumber switch
            {
                BattleTeamNumber.TeamAlpha => BattleTeamNumber.TeamBeta,
                BattleTeamNumber.TeamBeta  => BattleTeamNumber.TeamAlpha,

                _ => BattleTeamNumber.NoTeam
            };

            BattleGameControlQSystem.OnGameOver(f, winningTeam);

            goalCollisionData->Goal->HasTriggered = true;

            s_debugLogger.LogFormat(f, "GameOver {0} Goal", winningTeam.ToString());
        }

        /// <summary>This classes BattleDebugLogger instance.</summary>
        private static BattleDebugLogger s_debugLogger;
    }
}
