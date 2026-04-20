/// @file BattlePlayerCharacterViewController.cs
/// <summary>
/// Contains @cref{Battle.View.Player,BattlePlayerCharacterViewController} class which handles player character view logic.
/// </summary>

// System usings
using System.Collections;
using System.Runtime.CompilerServices;

// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;

// Battle QSimulation usings
using Battle.QSimulation;
using Battle.QSimulation.Player;

// Battle View usings
using Battle.View.Game;

namespace Battle.View.Player
{
    /// <summary>
    /// <span class="brief-h">%Player character's <a href="https://doc-api.photonengine.com/en/quantum/current/class_quantum_1_1_quantum_entity_view_component.html">QuantumEntityViewComponent@u-exlink</a>.</span><br/>
    /// Handles player character view logic.
    /// </summary>
    ///
    /// [{Player Overview}](#page-concepts-player-overview)<br/>
    /// [{Player View Code Overview}](#page-concepts-player-view-overview)
    public unsafe class BattlePlayerCharacterViewController : QuantumEntityViewComponent
    {
        /// <summary>
        /// Struct that holds a map for the player's spritesheet and handles getting a sprite from the spritesheet.
        /// </summary>
        ///
        /// See [{BattleSpriteSheetMap}](page-concepts-battle-sprite-sheet-sprite-sheet-map) for more info.
        public struct SpriteSheetMap : IBattleSpriteSheetMap
        {
            /// <summary>
            /// Constant that defines the exact sprite count of the spritesheet.
            /// </summary>
            public const int Count = 64;

            /// <summary>
            /// Enum that maps a Sprite name to its index on the player's spritesheet.
            /// </summary>
            public enum Enum
            {
                /// <summary>Index: 00</summary>
                Base = 0,

                /// <summary>Index: 01</summary>
                BaseHands = 1,

                /// <summary>Index: 02</summary>
                ScaredHands = 2,

                /// <summary>Index: 07</summary>
                Shadow = 7,

                /// <summary>Index: 08</summary>
                Head1 = 8,

                /// <summary>Index: 09</summary>
                Head2 = 9,

                /// <summary>Index: 10</summary>
                Head3 = 10,

                /// <summary>Index: 11</summary>
                Head4 = 11,

                /// <summary>Index: 12</summary>
                Body1 = 12,

                /// <summary>Index: 13</summary>
                Body2 = 13,

                /// <summary>Index: 14</summary>
                Body3 = 14,

                /// <summary>Index: 15</summary>
                Body4 = 15,

                /// <summary>Index: 16</summary>
                HandsShieldUp1 = 16,

                /// <summary>Index: 17</summary>
                HandsShieldUp2 = 17,

                /// <summary>Index: 18</summary>
                HandsShieldUp3 = 18,

                /// <summary>Index: 19</summary>
                HandsShieldUp4 = 19,

                /// <summary>Index: 20</summary>
                BaseShoes = 20,

                /// <summary>Index: 21</summary>
                RunningShoes1 = 21,

                /// <summary>Index: 22</summary>
                RunningShoes2 = 22,

                /// <summary>Index: 24</summary>
                HandsShieldDown1 = 24,

                /// <summary>Index: 25</summary>
                HandsShieldDown2 = 25,

                /// <summary>Index: 26</summary>
                HandsShieldDown3 = 26,

                /// <summary>Index: 27</summary>
                HandsShieldDown4 = 27,

                /// <summary>Index: 32</summary>
                ShieldUp1 = 32,

                /// <summary>Index: 33</summary>
                ShieldUp2 = 33,

                /// <summary>Index: 34</summary>
                ShieldUp3 = 34,

                /// <summary>Index: 35</summary>
                ShieldUp4 = 35,

                /// <summary>Index: 36</summary>
                ShieldUpHit1 = 36,

                /// <summary>Index: 37</summary>
                ShieldUpHit2 = 37,

                /// <summary>Index: 38</summary>
                ShieldUpHit3 = 38,

                /// <summary>Index: 39</summary>
                ShieldUpHit4 = 39,

                /// <summary>Index: 40</summary>
                ShieldDown1 = 40,

                /// <summary>Index: 41</summary>
                ShieldDown2 = 41,

                /// <summary>Index: 42</summary>
                ShieldDown3 = 42,

                /// <summary>Index: 43</summary>
                ShieldDown4 = 43,

                /// <summary>Index: 44</summary>
                ShieldDownHit1 = 44,

                /// <summary>Index: 45</summary>
                ShieldDownHit2 = 45,

                /// <summary>Index: 46</summary>
                ShieldDownHit3 = 46,

                /// <summary>Index: 47</summary>
                ShieldDownHit4 = 47,

                /// <summary>Index: 48</summary>
                Joy = 48,

                /// <summary>Index: 49</summary>
                Sadness = 49,

                /// <summary>Index: 50</summary>
                Playful = 50,

                /// <summary>Index: 51</summary>
                Agression = 51,

                /// <summary>Index: 52</summary>
                Love = 52,

                /// <summary>Index: 56</summary>
                ShieldBroken = 56,

                /// <summary>Index: 57</summary>
                Defenseless = 57,

                /// <summary>Index: 58</summary>
                Death1 = 58,

                /// <summary>Index: 59</summary>
                Death2 = 59,

                /// <summary>Index: 60</summary>
                DeadOnTheGround = 60
            }

            /// <summary>
            /// Converts int to %SpriteSheetMap
            /// </summary>
            ///
            /// <param name="index">Int to be converted</param>
            ///
            /// <returns>EnumValue of the given index.</returns>
            public static SpriteSheetMap FromInt(int index) => new() { EnumValue = (Enum)index };

            /// <summary>
            /// Implicit conversion from PlayerSpriteSheetMap to Enum.
            /// </summary>
            ///
            /// <param name="playerSpriteSheetMap">PlayerSpriteSheetMap thats being converted.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Enum(SpriteSheetMap playerSpriteSheetMap) => playerSpriteSheetMap.EnumValue;

            /// <summary>
            /// Implicit conversion from Enum to PlayerSpriteSheetMap.
            /// </summary>
            ///
            /// <param name="enumValue">Enum thats being converted.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator SpriteSheetMap(Enum enumValue) => new() {EnumValue = enumValue };

            /// <summary>
            /// Validates that the given <paramref name="spriteSheet"/> has the expected Sprite <see cref="Count"/>.
            /// </summary>
            ///
            /// Wrapper for @cref{Battle.View,IBattleSpriteSheetMap.ValidateCount}.
            ///
            /// <param name="spriteSheet">Spritesheet that is being validated.</param>
            ///
            /// <returns>True if the spritesheet has the correct number of sprites.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool Validate(BattleSpriteSheet spriteSheet) => IBattleSpriteSheetMap.ValidateCount(Count, spriteSheet.Count);

            /// <summary>
            /// Helper Enum for conversions.
            /// </summary>
            public Enum EnumValue;

            /// <summary>
            /// Method for getting the index of the correct sprite in a %BattleSpriteSheet.
            /// </summary>
            ///
            /// <returns>The index of the sprite.</returns>
            public readonly int GetIndex() => (int)EnumValue;
        }

        /// @anchor BattlePlayerCharacterViewController-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{
        #region SerializeFields

        [Header("References")]

        /// <summary>[SerializeField] Reference to an override character class view controller.</summary>
        /// @ref BattlePlayerCharacterViewController-SerializeFields
        [SerializeField] private BattlePlayerCharacterClassBaseViewController _classViewControllerOverride;

        /// <summary>[SerializeField] Reference to a struct that holds the character's spritesheet.</summary>
        /// @ref BattlePlayerCharacterViewController-SerializeFields
        [SerializeField] private BattleSpriteSheet _spriteSheet;

        /// <summary>[SerializeField] %Player's child <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObject@u-exlink</a> where heart sprite is located.</summary>
        /// @ref BattlePlayerCharacterViewController-SerializeFields
        //[SerializeField] private GameObject _heart;

        /// <summary>[SerializeField] Array of character <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObjects@u-exlink</a>.</summary>
        /// @ref BattlePlayerCharacterViewController-SerializeFields
        [SerializeField] private GameObject[] _characterGameObjects;

        /// <summary>[SerializeField] %Player's local player indicator <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObject@u-exlink</a>.</summary>
        /// @ref BattlePlayerCharacterViewController-SerializeFields
        [SerializeField] private GameObject _localPlayerIndicator;

        /// <summary>[SerializeField] Reference to the shield hit particle system.</summary>
        /// @ref BattlePlayerCharacterViewController-SerializeFields

        [Header("Settings")]

        /// <summary>[SerializeField] The amount of damage flashes.</summary>
        /// @ref BattlePlayerCharacterViewController-SerializeFields
        [SerializeField] private int _stunFlashAmount;

        #endregion SerializeFields
        /// @}

        #region Public

        #region Public - Sprite Control Methods

        /// <summary>
        /// Handles changing the sprite to the base sprite.
        /// </summary>
        public void SetBaseSprite()
        {
            _bodypartSpriteRenderers[SpriteRendererHeadIndex]   .sprite = null;
            _bodypartSpriteRenderers[SpriteRendererBodyIndex]   .sprite = _spriteSheet.GetSprite<SpriteSheetMap>(SpriteSheetMap.Enum.Base);
            _bodypartSpriteRenderers[SpriteRendererHandsIndex]  .sprite = null;
            _bodypartSpriteRenderers[SpriteRendererFeetIndex]   .sprite = null;
            _bodypartSpriteRenderers[SpriteRendererShadowIndex] .sprite = null;
        }

        /// <summary>
        /// Handles changing the sprite for the head gameobject.
        /// </summary>
        ///
        /// <param name="sprite">Sprite that the head sprite is being changed to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetHeadSprite(SpriteSheetMap sprite)
        {
            BattleDebugLogger.DevAssertFormat(nameof(BattlePlayerCharacterViewController),
                sprite.EnumValue is
                    SpriteSheetMap.Enum.Head1 or
                    SpriteSheetMap.Enum.Head2 or
                    SpriteSheetMap.Enum.Head3 or
                    SpriteSheetMap.Enum.Head4 or
                    SpriteSheetMap.Enum.Joy or
                    SpriteSheetMap.Enum.Sadness or
                    SpriteSheetMap.Enum.Playful or
                    SpriteSheetMap.Enum.Agression or
                    SpriteSheetMap.Enum.Love,
                "{0} Sprite is not a head sprite", sprite
            );
            _bodypartSpriteRenderers[0].sprite = _spriteSheet.GetSprite(sprite);
        }

        /// <summary>
        /// Handles changing the sprite for the body gameobject.
        /// </summary>
        ///
        /// <param name="sprite">Sprite that the head sprite is being changed to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBodySprite(SpriteSheetMap sprite)
        {
            BattleDebugLogger.DevAssertFormat(nameof(BattlePlayerCharacterViewController),
                sprite.EnumValue is
                    SpriteSheetMap.Enum.Body1 or
                    SpriteSheetMap.Enum.Body2 or
                    SpriteSheetMap.Enum.Body3 or
                    SpriteSheetMap.Enum.Body4,
                "{0} Sprite is not a body sprite", sprite
            );
            _bodypartSpriteRenderers[1].sprite = _spriteSheet.GetSprite(sprite);
        }

        /// <summary>
        /// Handles changing the sprite for the hand gameobject.
        /// </summary>
        ///
        /// <param name="sprite">Sprite that the hand sprite is being changed to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetHandSprite(SpriteSheetMap sprite)
        {
            BattleDebugLogger.DevAssertFormat(nameof(BattlePlayerCharacterViewController),
                sprite.EnumValue is
                    SpriteSheetMap.Enum.BaseHands or
                    SpriteSheetMap.Enum.ScaredHands or
                    SpriteSheetMap.Enum.HandsShieldDown1 or
                    SpriteSheetMap.Enum.HandsShieldDown2 or
                    SpriteSheetMap.Enum.HandsShieldDown3 or
                    SpriteSheetMap.Enum.HandsShieldDown4 or
                    SpriteSheetMap.Enum.HandsShieldUp1 or
                    SpriteSheetMap.Enum.HandsShieldUp2 or
                    SpriteSheetMap.Enum.HandsShieldUp3 or
                    SpriteSheetMap.Enum.HandsShieldUp4,
                "{0} Sprite is not a hand sprite", sprite
            );
            _bodypartSpriteRenderers[2].sprite = _spriteSheet.GetSprite(sprite);
        }

        /// <summary>
        /// Handles changing the sprite for the feet gameobject.
        /// </summary>
        ///
        /// <param name="sprite">Sprite that the hand sprite is being changed to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFeetSprite(SpriteSheetMap sprite)
        {
            BattleDebugLogger.DevAssertFormat(nameof(BattlePlayerCharacterViewController),
                sprite.EnumValue is
                    SpriteSheetMap.Enum.BaseShoes or
                    SpriteSheetMap.Enum.RunningShoes1 or
                    SpriteSheetMap.Enum.RunningShoes2,
                "{0} Sprite is not a feet sprite", sprite
            );
            _bodypartSpriteRenderers[3].sprite = _spriteSheet.GetSprite(sprite);
        }

        #endregion Public - Sprite Control Methods

        /// <summary>
        /// Binds the shield view controller to the _playerShieldViewControllers dictionary to be able to call on it later.
        /// </summary>
        ///
        /// <param name="shieldViewController">Pointer to a shield view controller associated with the character.</param>
        /// <param name="shieldNumber">ShieldNumber of the shield being bound.</param>
        public void BindShield(BattlePlayerShieldViewController shieldViewController, int shieldNumber)
        {
            _playerShieldViewControllers[shieldNumber] = shieldViewController;
        }

        #region Public - Gameflow Methods

        /// <summary>
        /// Public method that is called when entity is activated upon its creation.<br/>
        /// Calls <see cref="PreInitSetup"/> and subscribes to <see cref="Quantum.EventBattlePlayerCharacterViewInit">EventBattlePlayerCharacterViewInit</see> event with a lambda, which
        /// sets the player character model scale and active <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObjects@u-exlink</a>.
        /// Handles subscribing to QuantumEvents and registering to BattleViewRegistry.
        /// </summary>
        ///
        /// <param name="_">Current simulation frame.</param>
        public override void OnActivate(Frame _) { PreInitSetup(); QuantumEvent.Subscribe(this, (EventBattlePlayerCharacterViewInit e) =>
        {
            if (EntityRef != e.ERef) return;
            if (!PredictedFrame.Exists(e.ERef)) return;

            //{ initialize visuals

            float scale = (float)e.ModelScale;
            transform.localScale = new Vector3(scale, scale, scale);

            if (BattlePlayerManager.PlayerHandle.GetTeamNumber(e.Slot) == BattleGameViewController.LocalPlayerTeam)
            {
                GameObject characterGameObject = _characterGameObjects[0];
                characterGameObject.SetActive(true);
                _bodypartSpriteRenderers[SpriteRendererHeadIndex]   = characterGameObject.transform.Find("Head").GetComponent<SpriteRenderer>();
                _bodypartSpriteRenderers[SpriteRendererBodyIndex]   = characterGameObject.transform.Find("Body").GetComponent<SpriteRenderer>();
                _bodypartSpriteRenderers[SpriteRendererHandsIndex]  = characterGameObject.transform.Find("Hands").GetComponent<SpriteRenderer>();
                _bodypartSpriteRenderers[SpriteRendererFeetIndex]   = characterGameObject.transform.Find("Feet").GetComponent<SpriteRenderer>();
                _bodypartSpriteRenderers[SpriteRendererShadowIndex] = characterGameObject.transform.Find("Shadow").GetComponent<SpriteRenderer>();
            }
            else
            {
                GameObject characterGameObject = _characterGameObjects[1];
                characterGameObject.SetActive(true);
                //_heart.SetActive(false);
                _bodypartSpriteRenderers[SpriteRendererHeadIndex]   = characterGameObject.transform.Find("Head").GetComponent<SpriteRenderer>();
                _bodypartSpriteRenderers[SpriteRendererBodyIndex]   = characterGameObject.transform.Find("Body").GetComponent<SpriteRenderer>();
                _bodypartSpriteRenderers[SpriteRendererHandsIndex]  = characterGameObject.transform.Find("Hands").GetComponent<SpriteRenderer>();
                _bodypartSpriteRenderers[SpriteRendererFeetIndex]   = characterGameObject.transform.Find("Feet").GetComponent<SpriteRenderer>();
                _bodypartSpriteRenderers[SpriteRendererShadowIndex] = characterGameObject.transform.Find("Shadow").GetComponent<SpriteRenderer>();
            }

            SetBaseSprite();

            if (e.Slot == BattleGameViewController.LocalPlayerSlot)
            {
                _localPlayerIndicator.SetActive(true);
            }

            //} initialize visuals

            //{ initialize class view controller

            if (_classViewControllerOverride != null)
            {
                if(_classViewControllerOverride.Class == e.CharacterClass)
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

            _classViewController.OnViewInit(this, e.ERef, e.Slot, e.CharacterId);

            //} initialize class view controller

            // initialize shield view controller reference array
            _playerShieldViewControllers = new BattlePlayerShieldViewController[e.ShieldCount];

            // register character view controller
            BattleViewRegistry.Register(EntityRef, this);

            // subscribe to quantum events
            QuantumEvent.Subscribe<EventBattlePlayStateUpdate>(this, QEventOnPlayStateUpdate);
            QuantumEvent.Subscribe<EventBattleCharacterHit>(this, QEventOnCharacterHit);
            QuantumEvent.Subscribe<EventBattleShieldHit>(this, QEventOnShieldHit);
        });}

        /// <summary>
        /// Public method that is called when the view should update.<br/>
        /// Calls <see cref="BattlePlayerCharacterViewController.UpdateModelPositionAdjustment">UpdateModelPositionAdjustment</see> to update the player character model's position
        /// </summary>
        public override void OnUpdateView()
        {
            if (!_isInPlay) return;
            if (!PredictedFrame.Exists(EntityRef)) return;
            BattlePlayerDataQComponent* playerData = PredictedFrame.Unsafe.GetPointer<BattlePlayerDataQComponent>(EntityRef);
            if (playerData->PlayerRef == PlayerRef.None) return;

            Vector3 targetPosition = playerData->TargetPosition.ToUnityVector3();
            BattleTeamNumber battleTeamNumber = playerData->TeamNumber;

            UpdateModelPositionAdjustment(&targetPosition);

            _classViewController.OnUpdateView();
        }

        #endregion Public - Gameflow Methods

        #endregion Public

        #region Private

        /// <summary>Constant for the head sprite renderer's index.</summary>
        private const int SpriteRendererHeadIndex = 0;
        /// <summary>Constant for the body sprite renderer's index.</summary>
        private const int SpriteRendererBodyIndex = 1;
        /// <summary>Constant for the hands sprite renderer's index.</summary>
        private const int SpriteRendererHandsIndex = 2;
        /// <summary>Constant for the feet sprite renderer's index.</summary>
        private const int SpriteRendererFeetIndex = 3;
        /// <summary>Constant for the shadow sprite renderer's index.</summary>
        private const int SpriteRendererShadowIndex = 4;

        /// <summary>This classes BattleDebugLogger instance.</summary>
        private BattleDebugLogger _debugLogger;

        /// <summary>Boolean that tells whether the Quantum Entity this ViewController is attached to is in play.</summary>
        private bool _isInPlay;

        /// <value>Reference to the active character class view controller.</value>
        private BattlePlayerCharacterClassBaseViewController _classViewController;

        /// <summary>Array that holds the SpriteRenderer components of each body part gameobject.</summary>
        private readonly SpriteRenderer[] _bodypartSpriteRenderers = new SpriteRenderer[5];

        /// <summary>Array that holds references to the shield view controllers associated with this character view controller.</summary>
        private BattlePlayerShieldViewController[] _playerShieldViewControllers;

        /// <value>Holder variable for the damage flash coroutine.</value>
        private Coroutine _damageFlashCoroutine = null;

        #region Private Gameflow Methods

        /// <summary>
        /// Handles setup that needs to happen before <see cref="Quantum.EventBattlePlayerCharacterViewInit">EventBattlePlayerCharacterViewInit</see> event is received.<br/>
        /// Currently this is needed for initializing character's class as none.
        /// </summary>
        private void PreInitSetup()
        {
            _debugLogger = BattleDebugLogger.Create<BattlePlayerCharacterViewController>();

            _classViewController = gameObject.AddComponent<BattlePlayerCharacterClassNoneViewController>();
        }

        #endregion Private Gameflow Methods

        #region Private QuantumEvent Handlers

        /// <summary>
        /// Handler method for EventBattleInPlayStateUpdate QuantumEvent.<br/>
        /// Updates the _isInPlay bool.
        /// </summary>
        /// <param name="e">The event data.</param>
        private void QEventOnPlayStateUpdate(EventBattlePlayStateUpdate e)
        {
            if (e.ERef != EntityRef) return;
            _isInPlay = e.IsInPlay;
        }

        /// <summary>
        /// Handler method for EventBattleCharacterHit QuantumEvent.<br/>
        /// Starts <see cref="BattlePlayerCharacterViewController.StunFlashCoroutine">DamageFlashCoroutine</see>.
        /// </summary>
        ///
        /// <param name="e">The event data.</param>
        private void QEventOnCharacterHit(EventBattleCharacterHit e)
        {
            if (EntityRef != e.ERef) return;

            if (_damageFlashCoroutine != null)
            {
                StopCoroutine(_damageFlashCoroutine);
            }
            _damageFlashCoroutine = StartCoroutine(StunFlashCoroutine((float)e.StunFlashDurationSec));

            _classViewController.OnCharacterHit(e);
        }

        /// <summary>
        /// Forwards the parameter event to every shield view controller bound to this character view controller.
        /// </summary>
        ///
        /// <param name="e">Shield take damage event that needs to be forwarded.</param>
        private void QEventOnShieldHit(EventBattleShieldHit e)
        {
            foreach (BattlePlayerShieldViewController shield in _playerShieldViewControllers)
            {
                shield.OnShieldHit(e);
            }
        }

        #endregion Private QuantumEvent Handlers

        /// <summary>
        /// Updates the player character model's position.
        /// </summary>
        ///
        /// <param name="targetPosition">Target position Vector3.</param>
        private void UpdateModelPositionAdjustment(Vector3* targetPosition)
        {
            const float adjustmentDistance = 0.25f;
            Vector3 distanceToTargetPosition = *targetPosition - transform.position;
            if (distanceToTargetPosition.sqrMagnitude < adjustmentDistance * adjustmentDistance)
            {
                transform.position = *targetPosition;
            }
            else
            {
                transform.localPosition = Vector3.zero;
            }
        }

        /// <summary>
        /// Coroutine which plays the stun flash animation.
        /// </summary>
        ///
        /// <returns>Coroutine IEnumerator.</returns>
        private IEnumerator StunFlashCoroutine(float stunFlashDurationSec)
        {
            Color tempColor;
            float singleFlashDuration = stunFlashDurationSec / (_stunFlashAmount * 2 - 1);
            for (int i = 0; i < _stunFlashAmount; i++)
            {
                foreach (SpriteRenderer sprite in _bodypartSpriteRenderers)
                {
                    tempColor = sprite.color;
                    tempColor.a = 0;
                    sprite.color = tempColor;
                }

                yield return new WaitForSeconds(singleFlashDuration);

                foreach (SpriteRenderer sprite in _bodypartSpriteRenderers)
                {
                    tempColor = sprite.color;
                    tempColor.a = 1;
                    sprite.color = tempColor;
                }

                yield return new WaitForSeconds(singleFlashDuration);
            }
        }

        #endregion Private
    }
}
