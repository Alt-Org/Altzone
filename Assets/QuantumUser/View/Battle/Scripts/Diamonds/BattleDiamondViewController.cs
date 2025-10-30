/// @file BattleDiamondViewController.cs
/// <summary>
/// Contains @cref{Battle.View.Diamond,BattleDiamondViewController} class which handles diamonds visual functionality.
/// </summary>
///
/// This script:<br/>
/// Handles diamonds visual functionality.

using UnityEngine;
using Quantum;
using Battle.View.Game;

namespace Battle.View.Diamond
{
    /// <summary>
    /// <span class="brief-h">Diamond's <a href="https://doc-api.photonengine.com/en/quantum/current/class_quantum_1_1_quantum_entity_view_component.html">QuantumEntityViewComponent@u-exlink</a>.</span><br/>
    /// Handles diamonds visual functionality.
    /// </summary>
    public class BattleDiamondViewController : QuantumEntityViewComponent
    {
        /// <summary>
        /// Public method that is called when entity is activated upon its creation.<br/>
        /// Rotates the diamond visuals 180 degrees for the beta team.
        /// </summary>
        ///
        /// <param name="_">Current simulation frame.</param>
        public override void OnActivate(Frame _)
        {
            if (BattleGameViewController.LocalPlayerTeam == BattleTeamNumber.TeamBeta)
            {
                transform.rotation = Quaternion.Euler(90, 180, 0);
            }
        }
    }
}
