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
        [SerializeField] private GameObject _shieldSprite;

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
            if (e.DefencePercentage <= FP._0)
            {
                _shieldSprite.SetActive(false);
            }
        }

        /// <summary>
        /// Updates the shield view.<br/>
        /// Customized view logic where the shield tracks projectile in an arc.
        /// </summary>
        public override void OnUpdateView()
        {
            GameObject projectileRef = BattleGameViewController.ProjectileReference;

            if (projectileRef == null) return;

            Vector3 toProjectileVec3 = projectileRef.transform.position - transform.position;
            Vector2 toProjectileVec2 = new(toProjectileVec3.x, toProjectileVec3.z);

            //if (toProjectileVec2.sqrMagnitude < 0.0001f) return;

            float angle = -Vector2.SignedAngle(Vector2.up, toProjectileVec2);

            if (_movingShieldFlipped)
            {
                angle += 180;
            }

            Vector3 currentAngle = _shieldSprite.transform.rotation.eulerAngles;
            _shieldSprite.transform.rotation = Quaternion.Euler(currentAngle.x, angle, currentAngle.z);

            float radius = Vector3.Distance(_shieldSprite.transform.position, transform.position);
            _shieldSprite.transform.position = transform.position + toProjectileVec3.normalized * radius;
        }
    }
}
