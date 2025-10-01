/// @file BattlePlayerClassProjectorViewController.cs
/// <summary>
/// Contains @cref{Battle.View.Player,BattlePlayerClassProjectorViewController} class,
/// which is a <see cref="Battle.View.Player.BattlePlayerClassProjectorViewController">class %view controller</see> for the Projector character class.
/// </summary>

using Battle.View.Game;
using Photon.Deterministic;
using Quantum;
using UnityEngine;

namespace Battle.View.Player
{
    /// <summary>
    /// <span class="brief-h">Projector <see cref="Battle.View.Player.BattlePlayerClassBaseViewController">class %view controller</see>.</span><br/>
    /// Handles view logic for the Projector character class
    /// </summary>
    ///
    /// @bigtext{See [{Player Character Class Projector}](#page-wip-concepts-playerclass-projector) for more info.}
    public class BattlePlayerClassProjectorViewController : BattlePlayerClassBaseViewController
    {
        /// @anchor BattlePlayerClassProjectorViewController-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <summary>Reference to the moving shield object.</summary>
        /// @ref BattlePlayerClassProjectorViewController-SerializeFields
        [Tooltip("Reference to the moving shield object.")]
        [SerializeField] private GameObject _movingShield;

        /// <summary>Indicates if the moving shield should be flipped.</summary>
        /// @ref BattlePlayerClassProjectorViewController-SerializeFields
        [Tooltip("Indicates if the moving shield should be flipped.")]
        [SerializeField] private bool _movingShieldFlipped;

        /// @}

        /// <summary>
        /// Gets the character class associated with this Class.<br/>
        /// Always returns <see cref="Quantum.BattlePlayerCharacterClass.Projector">BattlePlayerCharacterClass.Projector</see>.
        /// </summary>
        public override BattlePlayerCharacterClass Class => BattlePlayerCharacterClass.Projector;

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
