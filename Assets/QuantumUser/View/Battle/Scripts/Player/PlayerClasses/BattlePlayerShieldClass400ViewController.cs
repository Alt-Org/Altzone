/// @file BattlePlayerShieldClass400ViewController.cs
/// <summary>
/// Contains @cref{Battle.View.Player,BattlePlayerShieldClass400ViewController} class,
/// which is a <see cref="Battle.View.Player.BattlePlayerShieldClass400ViewController">shield class %view controller</see> for the Projector character class.
/// </summary>

// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;

// Battle QSimulation usings
using Battle.QSimulation.Game;

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

        /// <summary>Reference to the shield GameObject.</summary>
        /// Part of @ref BattlePlayerShieldClass400ViewController-SerializeFields "SerializeFields"
        [Tooltip("Reference to the shield GameObject.")]
        [SerializeField] private GameObject _shieldSpriteGameObject;

        /// <summary>Indicates if the moving shield should be flipped.</summary>
        /// Part of @ref BattlePlayerShieldClass400ViewController-SerializeFields "SerializeFields"
        [Tooltip("Indicates if the moving shield should be flipped.")]
        [SerializeField] private bool _movingShieldFlipped;

        /// @}

        /// <summary>
        /// Gets the character class associated with this Class.<br/>
        /// Always returns <see cref="Quantum.BattlePlayerCharacterClass.Class400">BattlePlayerCharacterClass.Class400</see>.
        /// </summary>
        public override BattlePlayerCharacterClass Class => BattlePlayerCharacterClass.Class400;

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
            Vector3 currentAngles    = _shieldSpriteGameObject.transform.rotation.eulerAngles;

            if (toProjectileVec2.sqrMagnitude > _maxShieldRotateDistanceSqr)
            {
                _shieldSpriteGameObject.transform.SetLocalPositionAndRotation(
                    _shieldDefaultPosition,
                    _shieldDefaultRotation
                );
                return;
            }

            float angle = -Vector2.SignedAngle(Vector2.up, toProjectileVec2);

            if (_movingShieldFlipped)
            {
                angle += 180f;
            }

            _shieldSpriteGameObject.transform.SetPositionAndRotation(
                transform.position + toProjectileVec3.normalized * _shieldOffset,
                Quaternion.Euler(currentAngles.x, angle, currentAngles.z)
            );
        }

        /// <summary>
        /// Method that is called when view is initialized.<br/>
        /// Handles character class 400 initialization.
        /// </summary>
        ///
        /// <param name="slot">Slot of the player this shield is associated with.</param>
        /// <param name="characterId">CharacterID of the character this shield is associated with.</param>
        protected override void OnViewInitOverride(BattlePlayerSlot slot, BattlePlayerCharacterID characterId)
        {
            _maxShieldRotateDistanceSqr = 20f * (float)BattleGridManager.GridScaleFactor;
            _maxShieldRotateDistanceSqr *= _maxShieldRotateDistanceSqr;
            _shieldOffset = Vector3.Distance(_shieldSpriteGameObject.transform.position, transform.position);

            _shieldSpriteGameObject.transform.GetLocalPositionAndRotation(out _shieldDefaultPosition, out _shieldDefaultRotation);
        }

        /// <summary>
        /// Maximum distance where the shield will follow the projectile squared.
        /// </summary>
        private float _maxShieldRotateDistanceSqr;

        /// <summary>
        /// Offset of the <see cref="_shieldSpriteGameObject">_shieldSpriteGameObject</see>.
        /// </summary>
        private float _shieldOffset;

        /// <summary>
        /// Default position of the <see cref="_shieldSpriteGameObject">_shieldSpriteGameObject</see>.
        /// </summary>
        private Vector3 _shieldDefaultPosition;

        /// <summary>
        /// Default rotation of the <see cref="_shieldSpriteGameObject">_shieldSpriteGameObject</see>.
        /// </summary>
        private Quaternion _shieldDefaultRotation;
    }
}
