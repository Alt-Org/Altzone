/// @file BattlePlayerShieldClass400ViewController.cs
/// <summary>
/// Contains @cref{Battle.View.Player,BattlePlayerShieldClass400ViewController} class,
/// which is a <see cref="Battle.View.Player.BattlePlayerShieldClass400ViewController">shield class %view controller</see> for the Projector character class.
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
    /// <span class="brief-h">Projector <see cref="Battle.View.Player.BattlePlayerShieldClassBaseViewController">shield class %view controller</see>.</span><br/>
    /// Handles view logic for the Projector shield class
    /// </summary>
    ///
    /// @bigtext{See [{PlayerShieldClassViewController}](#page-concepts-player-view-shield-class-controller) for more info.}<br/>
    /// @bigtext{See [{Player Character Classes}](#page-concepts-player-characters-classes) for more info.}<br/>
    /// @bigtext{See [{Player Character Class 400 - Projector}](#page-concepts-player-class-400) for more info.}
    public class BattlePlayerShieldClass400ViewController : BattlePlayerShieldClassBaseViewController
    {
        /// @anchor BattlePlayerShieldClass400ViewController-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <summary>Reference to the moving shield object.</summary>
        /// @ref BattlePlayerShieldClass400ViewController-SerializeFields
        [Tooltip("Reference to the moving shield object.")]
        [SerializeField] private GameObject _movingShield;

        /// <summary>Indicates if the moving shield should be flipped.</summary>
        /// @ref BattlePlayerShieldClass400ViewController-SerializeFields
        [Tooltip("Indicates if the moving shield should be flipped.")]
        [SerializeField] private bool _movingShieldFlipped;

        /// @}

        /// <summary>
        /// Gets the character class associated with this Class.<br/>
        /// Always returns <see cref="Quantum.BattlePlayerCharacterClass.Class400">BattlePlayerCharacterClass.Class400</see>.
        /// </summary>
        public override BattlePlayerCharacterClass Class => BattlePlayerCharacterClass.Class400;

        /// <summary>
        /// Called when the shield takes damage.<br/>
        /// Deactivating moving shield when defence value reaches 0.
        /// </summary>
        ///
        /// <param name="e">The shield damage event data.</param>
        public override void OnShieldTakeDamage(EventBattleShieldTakeDamage e)
        {
            if (e.DefenceValue <= FP._0)
            {
                _movingShield.SetActive(false);
            }
        }

        /// <summary>
        /// Updates the shield view.<br/>
        /// Customized view logic where the shield tracks projectile in an arc.
        /// </summary>
        public override void OnUpdateView()
        {
            if (_movingShield == null) return;

            GameObject projectileRef = BattleGameViewController.ProjectileReference;
            if (projectileRef == null) return;

            Vector3 origin = transform.position;
            Vector3 toProjectile = projectileRef.transform.position - origin;

            if (toProjectile.sqrMagnitude < 0.0001f) return;

            toProjectile.y = 0f;

            if (toProjectile.sqrMagnitude > 0.0001f)
            {
                Quaternion yawRot = Quaternion.LookRotation(toProjectile, Vector3.up);

                if (_movingShieldFlipped)
                {
                    yawRot *= Quaternion.Euler(0f, 180f, 0f);
                }

                Vector3 currentAngle = _movingShield.transform.rotation.eulerAngles;
                _movingShield.transform.rotation = Quaternion.Euler(currentAngle.x, yawRot.eulerAngles.y, currentAngle.z);
            }

            float radius = Vector3.Distance(_movingShield.transform.position, origin);
            _movingShield.transform.position = origin + toProjectile.normalized * radius;
        }
    }
}
