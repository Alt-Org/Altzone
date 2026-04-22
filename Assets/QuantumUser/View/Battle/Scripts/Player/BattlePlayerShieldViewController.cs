/// @file BattlePlayerShieldViewController.cs
/// <summary>
/// Contains @cref{Battle.View.Player,BattlePlayerShieldViewController} class which handles shield sprites and particles.
/// </summary>
///
/// This script:<br/>
/// Handles shield sprites and particles

// System usings
using System;
using System.Runtime.CompilerServices;

// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;

// Battle QSimulation usings
using Battle.QSimulation;

// Battle Player usings
using SpriteSheetMap = Battle.View.Player.BattlePlayerCharacterViewController.SpriteSheetMap;

namespace Battle.View.Player
{
    /// <summary>
    /// <span class="brief-h">%Player shield's <a href="https://doc-api.photonengine.com/en/quantum/current/class_quantum_1_1_quantum_entity_view_component.html">QuantumEntityViewComponent@u-exlink</a>.</span><br/>
    /// Handles player shield sprites and animations.
    /// </summary>
    public unsafe class BattlePlayerShieldViewController : QuantumEntityViewComponent
    {
        /// @anchor BattlePlayerShieldViewController-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{
        #region SerializeFields

        [Header("References")]

#if UNITY_EDITOR
        /// <summary>[SerializeField] Reference to a struct that holds the character's spritesheet. Only for use in the Unity Editor.</summary>
        /// @ref BattlePlayerShieldViewController-SerializeFields
        [Tooltip("Reference to a struct that holds the character's spritesheet. Only for use in the Unity Editor")]
        [SerializeField] private BattleSpriteSheet _spriteSheet;
#endif

        /// <summary>[SerializeField] Shield <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObject@u-exlink</a>.</summary>
        /// @ref BattlePlayerShieldViewController-SerializeFields
        [Tooltip("Shield GameObject")]
        [SerializeField] private SpriteRenderer _shieldSpriteRenderer;

        /// <summary>[SerializeField] Reference to the shield hit particle system.</summary>
        /// @ref BattlePlayerShieldViewController-SerializeFields
        [Tooltip("Reference to the shield hit particle system")]
        [SerializeField] private ParticleSystem _shieldHitParticle;

        /// <summary>[SerializeField] Reference to an override character class view controller.</summary>
        /// @ref BattlePlayerShieldViewController-SerializeFields
        [Tooltip("Reference to an override character class view controller")]
        [SerializeField] private BattlePlayerShieldClassBaseViewController _classViewControllerOverride;

        [Header("Settings")]

        /// <summary>[SerializeField] Reference to the shield's shieldNumber.</summary>
        /// @ref BattlePlayerShieldViewController-SerializeFields
        [Tooltip("Reference to the shield's shieldNumber")]
        [SerializeField] private int _shieldNumber;

        #endregion
        /// @}

        #region Public

        /// <summary>Enum that tells which side of the map the shield is on.</summary>
        public enum ShieldSide
        {
            Top    = 0,
            Bottom = 1
        }

        #region Public - Sprite Control Methods

        /// <summary>
        /// Handles changing the sprite for the shield gameobject.
        /// </summary>
        ///
        /// <param name="shieldNumber">ShieldNumber of the shield.</param>
        /// <param name="side">The side the shield is on.</param>
        /// <param name="hit">Whether the shield was hit or not.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetShieldSprite(int shieldNumber, ShieldSide side, bool hit)
        {
            const int StateCount = 2;
            const int ShieldCount = 4;
            const int ShieldSpriteStart = 32;

            int index = ShieldSpriteStart + (int)side * (StateCount * ShieldCount) + Convert.ToInt32(hit) * ShieldCount + shieldNumber;

            SpriteSheetMap sprite = SpriteSheetMap.FromInt(index);

            _debugLogger.DevAssertFormat(
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

            _shieldSpriteRenderer.sprite = _spriteSheet.GetSprite(sprite);
        }

        #endregion Public - Sprite Control Methods

        #region Public - Gameflow Methods

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

            _characterEntityRef = e.CharacterRef;

            if (e.ShieldNumber != _shieldNumber)
            {
                _debugLogger.ErrorFormat(
                    nameof(BattlePlayerShieldViewController),
                    "Expected shield number: {0}. Given shield number: {1}. Changing shield number to the correct one",
                    e.ShieldNumber , _shieldNumber
                );
                _shieldNumber = e.ShieldNumber;
            }

            _debugLogger.DevAssert(_characterEntityRef != null, "Character ref is null");

            // initialize visuals
            float scale = (float)e.ModelScale;
            transform.localScale = new Vector3(scale, scale, scale);
            _shieldHitParticle.transform.localScale = new Vector3(scale, scale, scale);

            //{ initialize class view controller

            if (_classViewControllerOverride != null)
            {
                if (_classViewControllerOverride.Class == e.CharacterClass)
                {
                    Destroy(_classViewController);
                    _classViewController = _classViewControllerOverride;
                }
                else
                {
                    _debugLogger.ErrorFormat("Class view controller missmatch! Expected {0}, got {1}", e.CharacterClass, _classViewControllerOverride.Class);
                    Destroy(_classViewControllerOverride);
                }
            }

            _classViewController.OnViewInit(this, e.ERef, e.Slot, e.CharacterId, e.ShieldNumber);

            //} initialize class view controller

            // register shield view controller
            BattleViewRegistry.Register(EntityRef, this);

            // subscribe to view registry callbacks
            BattleViewRegistry.WhenRegistered(_characterEntityRef, OnCharacterRegistered);

            // subscribe to quantum events
            QuantumEvent.Subscribe<EventBattlePlayStateUpdate>(this, QEventOnPlayStateUpdate);
        });}

        public override void OnUpdateView()
        {
            if (!_isInPlay) return;

            _classViewController.OnUpdateView();
        }

        #endregion Public - Gameflow Methods

        #region Public - Forwarded Quantum Event Handlers

        /// <summary>
        /// Handler method for EventBattleShieldHit QuantumEvent.
        /// </summary>
        ///
        /// <param name="e">The event data.</param>
        public void OnShieldHit(EventBattleShieldHit e)
        {
            if (EntityRef != e.ERef) return;

            if (_shieldHitParticle != null)
            {
                _shieldHitParticle.Play();
            }

            _classViewController.OnShieldHit(e);
        }

        #endregion Public - Forwarded Quantum Event Handlers

        #endregion Public

        #region Private

        /// <summary>This classes BattleDebugLogger instance.</summary>
        private BattleDebugLogger _debugLogger;

        /// <summary>EntityRef for the character this shield is assigned to.</summary>
        private EntityRef _characterEntityRef;

        /// <summary>Boolean that tells whether the Quantum Entity this ViewController is attached to is in play.</summary>
        private bool _isInPlay;

        /// <summary>Character view controller this shield view controller is bound to.</summary>
        private BattlePlayerCharacterViewController _characterViewController;

        /// <value>Reference to the active shield class view controller.</value>
        private BattlePlayerShieldClassBaseViewController _classViewController;

        #region Private - Gameflow Methods

        /// <summary>
        /// Handles setup that needs to happen before <see cref="Quantum.EventBattlePlayerShieldViewInit">EventBattlePlayerShieldViewInit</see> event is received.<br/>
        /// Currently this is needed for initializing shield's class as none.
        /// </summary>
        private void PreInitSetup()
        {
            _debugLogger = BattleDebugLogger.Create<BattlePlayerShieldViewController>();

            _classViewController = gameObject.AddComponent<BattlePlayerShieldClassNoneViewController>();
        }

        #endregion Private - Gameflow Methods

        #region Private - View Registry Callback Handlers

        /// <summary>Handles binding this shield view controller to the character view controller and vice versa.</summary>
        private bool OnCharacterRegistered()
        {
            BattlePlayerCharacterViewController characterViewController = BattleViewRegistry.GetObject<BattlePlayerCharacterViewController>(_characterEntityRef);
            if (characterViewController == null) return false;

            _characterViewController = characterViewController;
            _characterViewController.BindShield(this, _shieldNumber);

            return true;
        }

        #endregion Private - View Registry Callback Handlers

        #region Private - QuantumEvent Handlers

        /// <summary>
        /// Handler method for EventBattleInPlayStateUpdate QuantumEvent.<br/>
        /// Updates the _isInPlay bool.
        /// </summary>
        ///
        /// <param name="e">The event data.</param>
        private void QEventOnPlayStateUpdate(EventBattlePlayStateUpdate e)
        {
            if (e.ERef != EntityRef) return;
            _isInPlay = e.IsInPlay;
        }

        #endregion Private - QuantumEvent Handlers

        #endregion Private
    }
}
