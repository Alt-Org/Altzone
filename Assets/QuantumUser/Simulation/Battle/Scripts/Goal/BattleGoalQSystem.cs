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
