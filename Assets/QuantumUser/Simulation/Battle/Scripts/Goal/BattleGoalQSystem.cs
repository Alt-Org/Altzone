using UnityEngine;
using UnityEngine.Scripting;

using Quantum;

namespace Battle.QSimulation.Goal
{
    [Preserve]
    public unsafe class BattleGoalQSystem : SystemSignalsOnly, ISignalBattleOnProjectileHitGoal
    {
        /// <summary>
        /// Signal handler for when a projectile hits a goal.
        /// Triggers the end of the game by setting the state to GameOver.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="projectileEntity">Entity of the projectile.</param>
        /// <param name="goal">Pointer to the goal component.</param>
        /// <param name="goalEntity">Entity of the goal.</param>
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
