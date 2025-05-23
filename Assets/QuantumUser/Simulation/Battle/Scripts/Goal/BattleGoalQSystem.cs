using UnityEngine;
using UnityEngine.Scripting;

using Quantum;
using Battle.QSimulation.Game;

namespace Battle.QSimulation.Goal
{
    [Preserve]
    public unsafe class BattleGoalQSystem : SystemSignalsOnly, ISignalBattleOnProjectileHitGoal
    {
        public void BattleOnProjectileHitGoal(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattleGoalQComponent* goal, EntityRef goalEntity)
        {
            if (goal->HasTriggered) return;

            BattleGameControlQSystem.OnGameOver(f, goal->TeamNumber, projectile, projectileEntity);

            goal->HasTriggered = true;

            Debug.LogFormat("[BattleGoalQSystem] GameOver {0} Goal", goal->TeamNumber.ToString());
        }
    }
}
