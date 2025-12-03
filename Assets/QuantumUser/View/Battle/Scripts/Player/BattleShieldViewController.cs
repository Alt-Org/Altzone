/// @file BattleShieldViewController.cs
/// <summary>
/// Contains @cref{Battle.View.Player,BattleShieldViewController} class which handles shield sprites and particles.
/// </summary>
///
/// This script:<br/>
/// Handles shield sprites and particles

// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;

// Battle QSimulation usings
using Battle.QSimulation.Player;

// Battle View usings
using Battle.View.Game;

namespace Battle.View.Player
{
    /// <summary>
    /// <span class="brief-h">%Player's <a href="https://doc-api.photonengine.com/en/quantum/current/class_quantum_1_1_quantum_entity_view_component.html">QuantumEntityViewComponent@u-exlink</a>.</span><br/>
    /// Handles player sprites and animations.
    /// </summary>
    public unsafe class BattleShieldViewController : QuantumEntityViewComponent
    {
        /// @anchor BattlePlayerShieldViewController-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        [Header("References")]

        /// <summary>[SerializeField] Array of character <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObjects@u-exlink</a>.</summary>
        /// @ref BattlePlayerShieldViewController-SerializeFields
        [SerializeField] private GameObject[] _shieldGameObjects;

        /// <summary>[SerializeField] Reference to the shield hit particle system.</summary>
        /// @ref BattlePlayerShieldViewController-SerializeFields
        [SerializeField] private ParticleSystem _shieldHitParticle;

        /// @}

        /// <summary>
        /// Public method that is called when entity is activated upon its creation.<br/>
        /// Calls <see cref="PreInitSetup"/> and subscribes to <see cref="Quantum.EventBattlePlayerViewInit">EventBattlePlayerViewInit</see> event with a lambda, which
        /// sets the shield model scale and active <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObjects@u-exlink</a>. Handles subscribing to QuantumEvents.
        /// </summary>
        ///
        /// <param name="_">Current simulation frame.</param>
        public override void OnActivate(Frame _) { QuantumEvent.Subscribe(this, (EventBattleShieldViewInit e) =>
        {
            if (EntityRef != e.Entity) return;
            if (!PredictedFrame.Exists(e.Entity)) return;

            float scale = (float)e.ModelScale;
            transform.localScale = new Vector3(scale, scale, scale);
            _shieldHitParticle.transform.localScale = new Vector3(scale, scale, scale);

            if (BattlePlayerManager.PlayerHandle.GetTeamNumber(e.Slot) == BattleGameViewController.LocalPlayerTeam)
            {
                GameObject shieldGameObject = _shieldGameObjects[0];
                shieldGameObject.SetActive(true);
            }
            else
            {
                GameObject shieldGameObject = _shieldGameObjects[1];
                shieldGameObject.SetActive(true);
            }

            QuantumEvent.Subscribe<EventBattleShieldTakeDamage>(this, QEventOnShieldTakeDamage);
        });}

        /// <summary>
        /// Handler method for EventBattleShieldTakeDamage QuantumEvent.<br/>
        /// </summary>
        ///
        /// <param name="e">The event data.</param>
        private void QEventOnShieldTakeDamage(EventBattleShieldTakeDamage e)
        {
            if (EntityRef != e.Entity) return;
            if (!PredictedFrame.Exists(e.Entity)) return;

            if (_shieldHitParticle != null)
            {
                _shieldHitParticle.Play();
            }
        }
    }
}
