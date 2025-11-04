/// @file BattleDiamondViewController.cs
/// <summary>
/// Contains @cref{Battle.View.Diamond,BattleDiamondViewController} class which handles diamonds visual functionality.
/// </summary>
///
/// This script:<br/>
/// Handles diamonds visual functionality.

// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;

// Battle View usings
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

            _spriteRenderer = GetComponent<SpriteRenderer>();

            Color color = _spriteRenderer.color;
            color.a = 0.5f;
            _spriteRenderer.color = color;

            QuantumEvent.Subscribe<EventBattleDiamondLanded>(this, QEventOnDiamondLanded);
        }

        /// <summary>
        /// Private handler method for EventBattleDiamondLanded QuantumEvent.<br/>
        /// Sets the alpha of the diamond sprite to 1.
        /// </summary>
        /// 
        /// <param name="e">The event data.</param>
        private void QEventOnDiamondLanded(EventBattleDiamondLanded e)
        {
            if (e.Entity != EntityRef) return;

            Color color = _spriteRenderer.color;
            color.a = 1f;
            _spriteRenderer.color = color;
        }

        private SpriteRenderer _spriteRenderer;
    }
}
