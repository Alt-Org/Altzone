using UnityEngine;
using Quantum;
using Battle.View.Game;

namespace Battle.View.Diamond
{
    public class BattleDiamondViewController : QuantumEntityViewComponent
    {
        public override void OnActivate(Frame _)
        {
            if (BattleGameViewController.LocalPlayerTeam == BattleTeamNumber.TeamBeta)
            {
                transform.rotation = Quaternion.Euler(90, 180, 0);
            }
        }
    }
}
