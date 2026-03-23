/// @file BattlePlayerShieldViewController.cs
/// <summary>
/// Contains @cref{Battle.View.Player,BattlePlayerShieldViewController} class which handles shield sprites and particles.
/// </summary>
///
/// This script:<br/>
/// Handles shield sprites and particles

// System usings
using System.Runtime.CompilerServices;
// Unity usings
using UnityEngine;
// Battle QSimulation usings
using Battle.QSimulation;
// Battle Player usings
using static Battle.View.Player.BattlePlayerCharacterViewController;
// Quantum usings
using Quantum;

namespace Battle.View.Player
{
    /// <summary>
    /// <span class="brief-h">%Player shield's <a href="https://doc-api.photonengine.com/en/quantum/current/class_quantum_1_1_quantum_entity_view_component.html">QuantumEntityViewComponent@u-exlink</a>.</span><br/>
    /// Handles player shield sprites and animations.
    /// </summary>
    public unsafe class BattlePlayerShieldViewController : QuantumEntityViewComponent
    {
        #if UNITY_EDITOR
        [SerializeField] private BattleSpriteSheet _spriteSheet;
        #endif
        /// @anchor BattlePlayerShieldViewController-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        [Header("References")]

        /// <summary>[SerializeField] Shield <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObject@u-exlink</a>.</summary>
        /// @ref BattlePlayerShieldViewController-SerializeFields
        [SerializeField] private GameObject _shieldGameObject;

        /// <summary>[SerializeField] Reference to the shield hit particle system.</summary>
        /// @ref BattlePlayerShieldViewController-SerializeFields
        [SerializeField] private ParticleSystem _shieldHitParticle;

        /// <summary>[SerializeField] Reference to an override character class view controller.</summary>
        /// @ref BattlePlayerShieldViewController-SerializeFields
        [SerializeField] private BattlePlayerShieldClassBaseViewController _classViewControllerOverride;

        [SerializeField] private int _shieldNumber;

        /// @}

        /// <summary>
        /// Public method that is called when entity is activated upon its creation.<br/>
        /// Calls <see cref="PreInitSetup"/> and subscribes to <see cref="Quantum.EventBattlePlayerCharacterViewInit">EventBattlePlayerViewInit</see> event with a lambda, which
        /// sets the shield model scale and active <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObjects@u-exlink</a>.
        /// Handles subscribing to QuantumEvents and registering to BattleViewRegistry.
        /// </summary>
        ///
        /// <param name="_">Current simulation frame.</param>
        public override void OnActivate(Frame _) { PreInitSetup(); QuantumEvent.Subscribe(this, (EventBattlePlayerShieldViewInit e) =>
        {
            if (EntityRef != e.ERef) return;
            if (!PredictedFrame.Exists(e.ERef)) return;

            _characterRef = e.CharacterRef;

            BattleDebugLogger.DevAssert(nameof(BattlePlayerShieldViewController), _characterRef != null, "Character ref is null");

            if (!_isRegistered)
            {
                BattleViewRegistry.Register(EntityRef, this);
                _isRegistered = true;
                BattleViewRegistry.WhenRegistered(_characterRef, OnCharacterRegistered);
            }

            float scale = (float)e.ModelScale;
            transform.localScale = new Vector3(scale, scale, scale);
            _shieldHitParticle.transform.localScale = new Vector3(scale, scale, scale);

            if (_classViewControllerOverride != null)
            {
                if (_classViewControllerOverride.Class == e.Class)
                {
                    Destroy(_classViewController);
                    _classViewController = _classViewControllerOverride;
                }
                else
                {
                    _debugLogger.ErrorFormat("Class view controller missmatch! Expected {0}, got {1}", e.Class, _classViewControllerOverride.Class);
                    Destroy(_classViewControllerOverride);
                }
            }

            _shieldGameObject.SetActive(true);

            QuantumEvent.Subscribe<EventBattleInPlayStateUpdate>(this, QEventOnPlayStateUpdate);
        });}

        /// <summary>
        /// Handler method for EventBattleInPlayStateUpdate QuantumEvent.<br/>
        /// Updates the _isInPlay bool.
        /// </summary>
        /// <param name="e">The event data.</param>
        private void QEventOnPlayStateUpdate(EventBattleInPlayStateUpdate e)
        {
            if (e.ERef != EntityRef) return;
            _isInPlay = e.IsInPlay;
        }

        /// <summary>
        /// Handler method for EventBattleShieldTakeDamage QuantumEvent.
        /// </summary>
        ///
        /// <param name="e">The event data.</param>
        public void OnShieldTakeDamage(EventBattleShieldTakeDamage e)
        {
            if (EntityRef != e.ERef) return;
            if (!PredictedFrame.Exists(e.ERef)) return;

            if (_shieldHitParticle != null)
            {
                _shieldHitParticle.Play();
            }

            _classViewController.OnShieldTakeDamage(e);
        }

        /// <summary>
        /// Handles changing the sprite for the shield gameobject.
        /// </summary>
        /// <param name="sprite">sprite that the shield sprite is being changed to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ShieldSprite(SpriteSheetMap sprite)
        {
            BattleDebugLogger.DevAssertFormat(nameof(BattlePlayerShieldViewController),
                sprite.EnumValue is
                    SpriteSheetMap.Enum.ShieldUp1 or
                    SpriteSheetMap.Enum.ShieldUp2 or
                    SpriteSheetMap.Enum.ShieldUp3 or
                    SpriteSheetMap.Enum.ShieldUp4 or
                    SpriteSheetMap.Enum.ShieldUpHit1 or
                    SpriteSheetMap.Enum.ShieldUpHit2 or
                    SpriteSheetMap.Enum.ShieldUpHit3 or
                    SpriteSheetMap.Enum.ShieldUpHit4 or
                    SpriteSheetMap.Enum.ShieldDown1 or
                    SpriteSheetMap.Enum.ShieldDown2 or
                    SpriteSheetMap.Enum.ShieldDown3 or
                    SpriteSheetMap.Enum.ShieldDown4 or
                    SpriteSheetMap.Enum.ShieldDownHit1 or
                    SpriteSheetMap.Enum.ShieldDownHit2 or
                    SpriteSheetMap.Enum.ShieldDownHit3 or
                    SpriteSheetMap.Enum.ShieldDownHit4,
                "{0} Sprite is not a shield sprite", sprite
            );
            _shieldGameObject.GetComponent<SpriteRenderer>().sprite = _spriteSheet.GetSprite(sprite);
        }

        /// <summary>This classes BattleDebugLogger instance.</summary>
        private BattleDebugLogger _debugLogger;

        /// <summary>EntityRef for the character this shield is assigned to.</summary>
        private EntityRef _characterRef;

        /// <summary>Character view controller this shield view controller is bound to.</summary>
        private BattlePlayerCharacterViewController _characterViewController;

        /// <summary>Boolean that prevents this shield view controller from being registered multiple times to the BattleViewRegistry.</summary>
        private bool _isRegistered = false;

        /// <summary>Boolean that tells whether the Quantum Entity this ViewController is attached to is in play.</summary>
        private bool _isInPlay;

        /// <value>Reference to the active shield class view controller.</value>
        private BattlePlayerShieldClassBaseViewController _classViewController;

        /// <summary>
        /// Handles setup that needs to happen before <see cref="Quantum.EventBattlePlayerShieldViewInit">EventBattlePlayerCharacterViewInit</see> event is received.<br/>
        /// Currently this is needed for initializing character's class as none.
        /// </summary>
        private void PreInitSetup()
        {
            _debugLogger = BattleDebugLogger.Create<BattlePlayerShieldViewController>();

            _classViewController = gameObject.AddComponent<BattlePlayerShieldClassNoneViewController>();
        }

        /// <summary>Handles binding this shield view controller to the character view controller and vice versa.</summary>
        private bool OnCharacterRegistered()
        {
            BattlePlayerCharacterViewController characterViewController = BattleViewRegistry.GetObject<BattlePlayerCharacterViewController>(_characterRef);
            if (characterViewController == null) return false;

            _characterViewController = characterViewController;
            _characterViewController.BindShield(this);

            return true;
        }
    }
}
