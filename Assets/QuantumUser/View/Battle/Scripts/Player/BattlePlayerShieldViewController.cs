/// @file BattlePlayerShieldViewController.cs
/// <summary>
/// Contains @cref{Battle.View.Player,BattlePlayerShieldViewController} class which handles shield sprites and particles.
/// </summary>

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
    ///
    /// See [{PlayerShieldViewController}](#page-concepts-player-shield-view-controller) for more info.<br/>
    /// See [{Player Overview}](#page-concepts-player-overview) for more info.<br/>
    /// See [{Player View Code Overview}](#page-concepts-player-view-overview) for more info.
    public unsafe class BattlePlayerShieldViewController : QuantumEntityViewComponent
    {
        /// @anchor BattlePlayerShieldViewController-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{
        #region SerializeFields

        //{ editor

#if UNITY_EDITOR
        /// <summary>[SerializeField] Used to set the shield's sprite in the Unity Editor. @note Only for use in the Unity Editor.</summary>
        ///
        /// Part of @ref BattlePlayerShieldViewController-SerializeFields "SerializeFields"
        ///
        /// See [{BattleSpriteSheet}](#page-concepts-battle-sprite-sheet-sprite-sheet) for more info.
        [Header("Editor")]
        [Tooltip("Sets the shield's sprite in the editor.\nOnly for use in the Unity Editor")]
        [SerializeField] private BattleSpriteSheet _spriteSheet;

        /// <summary>[SerializeField] Used to disable the automatic sprite setting in the editor. @note Only for use in the Unity Editor.</summary>
        ///
        /// Part of @ref BattlePlayerShieldViewController-SerializeFields "SerializeFields"
        [Tooltip("Disables the automatic sprite setting\nOnly for use in the Unity Editor")]
        [SerializeField] private bool _autoSpriteDisable;
#endif

        //} editor

        //{ references

        /// <summary>[SerializeField] Reference to an override character class view controller.</summary>
        /// Part of @ref BattlePlayerShieldViewController-SerializeFields "SerializeFields"
        ///
        /// See [{PlayerShieldClassViewControllers}](#page-concepts-player-view-shield-class-controller) for more info.
        [Header("References")]
        [Tooltip("Reference to an override character class view controller")]
        [SerializeField] private BattlePlayerShieldClassBaseViewController _classViewControllerOverride;

        /// <summary>[SerializeField] Reference to the shield's <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SpriteRenderer.html">SpriteRenderer@u-exlink</a>.</summary>
        /// Part of @ref BattlePlayerShieldViewController-SerializeFields "SerializeFields"
        [Tooltip("Reference to the shield's SpriteRenderer")]
        [SerializeField] private SpriteRenderer _shieldSpriteRenderer;

        /// <summary>[SerializeField] Reference to the shield hit particle system.</summary>
        /// Part of @ref BattlePlayerShieldViewController-SerializeFields "SerializeFields"
        [Tooltip("Reference to the shield hit particle system")]
        [SerializeField] private ParticleSystem _shieldHitParticle;

        //} references

        //{ settings

        /// <summary>[SerializeField] Shield's shieldNumber.</summary>
        /// Part of @ref BattlePlayerShieldViewController-SerializeFields "SerializeFields"
        ///
        /// See [{ShieldNumber}](#page-concepts-player-character-entity-shield-number) for more info.
        [Header("Settings")]
        [Tooltip("Shield's shieldNumber")]
        [SerializeField] private int _shieldNumber;

        //} settings

        #endregion
        /// @}

        #region Public

        /// <summary>Enum that tells which side of the map the shield is on.</summary>
        public enum ShieldSide
        {
            Top    = 0,
            Bottom = 1
        }

        /// @anchor BattlePlayerShieldViewController-Public-SpriteControlMethods
        /// @name Public Sprite Control Methods
        /// Methods for controlling sprites.
        /// @{
        #region Public - Sprite Control Methods

        /// <summary>
        /// Handles changing the sprite of the shield gameobject based on <paramref name="shieldNumber"/>, <paramref name="side"/> and <paramref name="isHit"/>.
        /// </summary>
        ///
        /// Part of @ref BattlePlayerShieldViewController-Public-SpriteControlMethods "Public Sprite Control Methods"
        ///
        /// See [{ShieldNumber}](#page-concepts-player-character-entity-shield-number) for more info.
        ///
        /// <param name="shieldNumber">ShieldNumber of the shield.</param>
        /// <param name="side">The side the shield is on.</param>
        /// <param name="isHit">Whether the shield has been hit or not.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetShieldSprite(int shieldNumber, ShieldSide side, bool isHit)
        {
            const int StateCount = 2;
            const int ShieldCount = 4;
            const int ShieldSpriteStart = 32;

            int index = ShieldSpriteStart + (int)side * (StateCount * ShieldCount) + Convert.ToInt32(isHit) * ShieldCount + shieldNumber;

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

            _shieldSpriteRenderer.sprite = _characterViewController.SpriteSheet.GetSprite(sprite);
        }

        #endregion Public - Sprite Control Methods
        /// @}

        /// @anchor BattlePlayerShieldViewController-Public-GameflowMethods
        /// @name Public Gameflow Methods
        /// @{
        #region Public - Gameflow Methods

        /// <summary>
        /// Public method that is called when entity is activated upon its creation.<br/>
        /// Calls <see cref="PreInitSetup"/> and subscribes to <see cref="Quantum.EventBattlePlayerShieldViewInit">EventBattlePlayerShieldViewInit</see> event with a lambda, which
        /// sets the shield model scale and active <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObjects@u-exlink</a>.<br/>
        /// Handles subscribing to QuantumEvents and registering to BattleViewRegistry.
        /// </summary>
        ///
        /// Part of @ref BattlePlayerShieldViewController-Public-GameflowMethods "Public Gameflow Methods"
        ///
        /// <param name="_">Current simulation frame.</param>
        public override void OnActivate(Frame _) { PreInitSetup(); QuantumEvent.Subscribe(this, (EventBattlePlayerShieldViewInit e) =>
        {
            if (EntityRef != e.ERef) return;

            if (e.ShieldNumber != _shieldNumber)
            {
                _debugLogger.ErrorFormat(
                    nameof(BattlePlayerShieldViewController),
                    "Expected shield number: {0}. Given shield number: {1}. Changing shield number to the correct one",
                    e.ShieldNumber , _shieldNumber
                );
                _shieldNumber = e.ShieldNumber;
            }

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

            _characterEntityRef = e.CharacterRef;
            _debugLogger.DevAssert(_characterEntityRef != null, "Character ref is null");

            // register shield view controller
            BattleViewRegistry.Register(EntityRef, this);

            // subscribe to view registry callbacks
            BattleViewRegistry.WhenRegistered(_characterEntityRef, OnCharacterRegistered);

            // subscribe to quantum events
            QuantumEvent.Subscribe<EventBattlePlayStateUpdate>(this, QEventOnPlayStateUpdate);
        });}

        /// <summary>
        /// Public method that is called when the view should update.
        /// </summary>
        ///
        /// Part of @ref BattlePlayerShieldViewController-Public-GameflowMethods "Public Gameflow Methods"
        public override void OnUpdateView()
        {
            if (!_isInPlay) return;

            _classViewController.OnUpdateView();
        }

        #endregion Public - Gameflow Methods
        /// @}

        /// @anchor BattlePlayerShieldViewController-Public-ForwardedQuantumEventHandlers
        /// @name Public Forwarded QuantumEvent Handlers
        /// Methods for handling forwarded QuantumEvents.
        /// @{
        #region Public - Forwarded QuantumEvent Handlers

        /// <summary>
        /// Handler method for <see cref="Quantum.EventBattleShieldHit">EventBattleShieldHit</see> QuantumEvent.
        /// </summary>
        ///
        /// Part of @ref BattlePlayerShieldViewController-Public-ForwardedQuantumEventHandlers "Public Forwarded QuantumEvent Handlers"
        ///
        /// <param name="e">The event data.</param>
        public void OnShieldHit(EventBattleShieldHit e)
        {
            if (_shieldHitParticle != null)
            {
                _shieldHitParticle.Play();
            }

            _classViewController.OnShieldHit(e);
        }

        /// <summary>
        /// Handler method for <see cref="Quantum.EventBattleCharacterHit">EventBattleCharacterHit</see> QuantumEvent.
        /// </summary>
        ///
        /// Part of @ref BattlePlayerShieldViewController-Public-ForwardedQuantumEventHandlers "Public Forwarded QuantumEvent Handlers"
        ///
        /// <param name="e">The event data.</param>
        public void OnCharacterHit(EventBattleCharacterHit e)
        {
            _classViewController.OnCharacterHit(e);
        }

        #endregion Public - Forwarded QuantumEvent Handlers
        /// @}

        #endregion Public

        #region Private

        /// <summary>This classes %BattleDebugLogger instance.</summary>
        private BattleDebugLogger _debugLogger;

        /// <summary>%Boolean that tells whether the %Quantum Entity this ViewController is attached to is in play.</summary>
        ///
        /// See [{PlayState}](#page-concepts-entity-management-registered-entities-playstate) for more info.
        private bool _isInPlay;

        /// <summary>Reference to the active shield class view controller.</summary>
        ///
        /// See [{PlayerShieldClassViewControllers}](#page-concepts-player-view-shield-class-controller) for more info.
        private BattlePlayerShieldClassBaseViewController _classViewController;

        /// <summary>EntityRef for the character this shield is assigned to.</summary>
        private EntityRef _characterEntityRef;

        /// <summary>Character view controller this shield view controller is bound to.</summary>
        ///
        /// See [{PlayerCharacterViewController}](#page-concepts-player-character-view-controller) for more info.
        private BattlePlayerCharacterViewController _characterViewController;

        /// @anchor BattlePlayerShieldViewController-Private-GameflowMethods
        /// @name Private Gameflow Methods
        /// @{
        #region Private - Gameflow Methods

        /// <summary>
        /// Handles setup that needs to happen before <see cref="Quantum.EventBattlePlayerShieldViewInit">EventBattlePlayerShieldViewInit</see> event is received.<br/>
        /// Currently this is needed for initializing shield's class as none.
        /// </summary>
        ///
        /// Part of @ref BattlePlayerShieldViewController-Private-GameflowMethods "Private Gameflow Methods"
        private void PreInitSetup()
        {
            _debugLogger = BattleDebugLogger.Create<BattlePlayerShieldViewController>();

            _classViewController = gameObject.AddComponent<BattlePlayerShieldClassNoneViewController>();
        }

        #endregion Private - Gameflow Methods
        /// @}

        /// @anchor BattlePlayerShieldViewController-Private-ViewRegistryCallbackHandlers
        /// @name Private ViewRegistry Callback Handlers
        /// Methods for handling ViewRegistry callbacks.
        /// @{
        #region Private - ViewRegistry Callback Handlers

        /// <summary>
        /// Handles binding this shield view controller to the character view controller and vice versa.
        /// </summary>
        ///
        /// Part of @ref BattlePlayerShieldViewController-Private-ViewRegistryCallbackHandlers "Private ViewRegistry Callback Handlers"
        private bool OnCharacterRegistered()
        {
            BattlePlayerCharacterViewController characterViewController = BattleViewRegistry.GetObject<BattlePlayerCharacterViewController>(_characterEntityRef);
            if (characterViewController == null) return false;

            _characterViewController = characterViewController;
            _characterViewController.BindShield(this, _shieldNumber);

            return true;
        }

        #endregion Private - ViewRegistry Callback Handlers
        /// @}

        /// @anchor BattlePlayerShieldViewController-Private-QuantumEventHandlers
        /// @name Private QuantumEvent Handlers
        /// Methods for handling QuantumEvents.
        /// @{
        #region Private - QuantumEvent Handlers

        /// <summary>
        /// Handler method for <see cref="Quantum.EventBattlePlayStateUpdate">EventBattlePlayStateUpdate</see> QuantumEvent.<br/>
        /// Updates the <see cref="_isInPlay">_isInPlay</see> bool.
        /// </summary>
        ///
        /// Part of @ref BattlePlayerShieldViewController-Private-QuantumEventHandlers "Private QuantumEvent Handlers"
        ///
        /// See [{PlayState}](#page-concepts-entity-management-registered-entities-playstate) for more info.
        ///
        /// <param name="e">The event data.</param>
        private void QEventOnPlayStateUpdate(EventBattlePlayStateUpdate e)
        {
            if (e.ERef != EntityRef) return;
            _isInPlay = e.IsInPlay;

            _classViewController.OnPlayStateUpdate(e);
        }

        #endregion Private - QuantumEvent Handlers
        /// @}

        #endregion Private
    }
}
