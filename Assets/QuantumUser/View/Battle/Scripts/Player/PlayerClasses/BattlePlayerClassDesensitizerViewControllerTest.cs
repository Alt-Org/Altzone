/// @file BattlePlayerClassDesensitizerViewControllerTest.cs
/// <summary>
/// Contains @cref{Battle.View.Player,BattlePlayerClassDesensitizerViewControllerTest} class,
/// which is a <see cref="Battle.View.Player.BattlePlayerClassDesensitizerViewControllerTest">class %view controller</see> for the Desensitizer character class.
/// </summary>

// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;
using Photon.Deterministic;

// Battle View usings
using Battle.View.Game;

namespace Battle.View.Player
{
    /// <summary>
    /// <span class="brief-h">Desensitizer <see cref="Battle.View.Player.BattlePlayerClassBaseViewControllerTest">class %view controller</see>.</span><br/>
    /// Handles view logic for the Desensitizer character class
    /// </summary>
    ///
    /// @bigtext{See [{PlayerClassViewController}](#page-concepts-player-view-class-controller) for more info.}<br/>
    /// @bigtext{See [{Player Character Classes}](#page-concepts-player-characters-classes) for more info.}<br/>
    /// @bigtext ADD CONCEPT LINK TO DESENSITIZER CLASS HERE}
    public class BattlePlayerClassDesensitizerViewControllerTest : BattlePlayerClassBaseViewController
    {
        /// @anchor BattlePlayerClassDesensitizerViewController-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <summary>Reference to the aim indicator object</summary>
        /// @ref BattlePlayerClassDesensitizerViewController-SerializeFields
        [Tooltip("Reference to the aim indicator object")]
        [SerializeField] private GameObject _aimIndicator;

        /// @}

        /// <summary>
        /// Gets the character class associated with this Class.<br/>
        /// Always returns <see cref="Quantum.BattlePlayerCharacterClass.Desensitizer">BattlePlayerCharacterClass.Desensitizer</see>.
        /// </summary>
        public override BattlePlayerCharacterClass Class => BattlePlayerCharacterClass.Desensitizer;

        /// <summary>
        /// Called when class is initialized.<br/>
        /// Registers this component to listen for <see cref="EventBattlePlayerClassDesensitizerAimIndicatorUpdate"/> event.
        /// </summary>
        ///
        /// <param name="slot"><see cref="BattlePlayerSlot"/> of the player (unused)</param>
        /// <param name="characterId">Id of the character (unused)</param>
        protected override void OnViewInitOverride(BattlePlayerSlot slot, int characterId)
        {
            QuantumEvent.Subscribe<EventBattlePlayerClassDesensitizerAimIndicatorUpdate>(_parent, QEventOnAimIndicatorUpdate);
        }

        /// <summary>
        /// Updates the aim indicator view.
        /// </summary>
        ///
        /// <param name="e">The aim indicator update event data</param>
        private void QEventOnAimIndicatorUpdate(EventBattlePlayerClassDesensitizerAimIndicatorUpdate e)
        {
            if (e.ERef != _entityRef) return;
            if (e.Slot != BattleGameViewController.LocalPlayerSlot) return;
            if (!e.Show)
            {
                _aimIndicator.SetActive(false);
                return;
            }
            // Set indicator active
            _aimIndicator.SetActive(true);
            // Rotate indicator in degrees
            float angleDeg = (float)(FPVector2.RadiansSigned(FPVector2.Up, e.Direction) * -FP.Rad2Deg);
            if (BattleGameViewController.LocalPlayerTeam == BattleTeamNumber.TeamBeta) angleDeg += 180;
            // Indicator's new rotation in degrees.
            _aimIndicator.transform.rotation = Quaternion.Euler(0, angleDeg, 0);
        }
    }
}
