using UnityEngine;
using UnityEngine.Scripting;

using Quantum;

namespace Battle.QSimulation.Goal
{
    /// <summary>
    /// Goal <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">SystemSignalsOnly Quantum System</a>.<br/>
    /// Triggers the end of the game when it receives signal
    /// </summary>
    [Preserve]
    public unsafe class BattleGoalQSystem : SystemSignalsOnly, ISignalBattleOnProjectileHitGoal
    {
        /// <summary>
        /// Signal handler for when a projectile hits a goal.
        /// Triggers the end of the game by setting the state to GameOver.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="projectileEntity">EntityRef of the projectile.</param>
        /// <param name="goal">Pointer to the goal component.</param>
        /// <param name="goalEntity">EntityRef of the goal.</param>
        public void BattleOnProjectileHitGoal(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattleGoalQComponent* goal, EntityRef goalEntity)
        {
            if (goal->HasTriggered) return;

            BattleGameSessionQSingleton* gameSession = f.Unsafe.GetPointerSingleton<BattleGameSessionQSingleton>();
            gameSession->State = BattleGameState.GameOver;

            goal->HasTriggered = true;

            Debug.LogFormat("[BattleGoalQSystem] GameOver {0} Goal", goal->TeamNumber.ToString());
        }
    }
}
