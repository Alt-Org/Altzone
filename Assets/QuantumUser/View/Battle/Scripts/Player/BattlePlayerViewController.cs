/// @file BattlePlayerViewController.cs
/// <summary>
/// Contains @cref{Battle.View.Player,BattlePlayerViewController} class which handles player view logic.
/// </summary>

// System usings
using System.Collections;

// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;
using Photon.Deterministic;

// Battle QSimulation usings
using Battle.QSimulation.Player;

// Battle View usings
using Battle.View.Game;

namespace Battle.View.Player
{
    /// <summary>
    /// <span class="brief-h">%Player's <a href="https://doc-api.photonengine.com/en/quantum/current/class_quantum_1_1_quantum_entity_view_component.html">QuantumEntityViewComponent@u-exlink</a>.</span><br/>
    /// Handles player view logic.
    /// </summary>
    ///
    /// [{Player Overview}](#page-concepts-player-overview)<br/>
    /// [{Player View Code Overview}](#page-concepts-player-view-overview)
    public unsafe class BattlePlayerViewController : QuantumEntityViewComponent
    {
        /// @anchor BattlePlayerViewController-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        [Header("References")]

        /// <summary>[SerializeField] Reference to an override class view controller.</summary>
        /// @ref BattlePlayerViewController-SerializeFields
        [SerializeField] private BattlePlayerClassBaseViewController _classViewControllerOverride;

        /// <summary>[SerializeField] Animator <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObject@u-exlink</a> that handles player animations.</summary>
        /// @ref BattlePlayerViewController-SerializeFields
        [SerializeField] private Animator _animator;

        /// <summary>[SerializeField] %Player's child <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObject@u-exlink</a> where heart sprite is located.</summary>
        /// @ref BattlePlayerViewController-SerializeFields
        //[SerializeField] private GameObject _heart;

        /// <summary>[SerializeField] Array of character <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObjects@u-exlink</a>.</summary>
        /// @ref BattlePlayerViewController-SerializeFields
        [SerializeField] private GameObject[] _characterGameObjects;

        /// <summary>[SerializeField] %Player's local player indicator <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObject@u-exlink</a>.</summary>
        /// @ref BattlePlayerViewController-SerializeFields
        [SerializeField] private GameObject _localPlayerIndicator;

        /// <summary>[SerializeField] Reference to a character sprite without shield.</summary>
        /// @ref BattlePlayerViewController-SerializeFields
        [SerializeField] private Sprite _noShieldSprite;

        /// <summary>[SerializeField] Reference to the shield hit particle system.</summary>
        /// @ref BattlePlayerViewController-SerializeFields
        [SerializeField] private ParticleSystem _shieldHitParticle;

        [Header("Settings")]

        /// <summary>[SerializeField] The transparency effect's range.</summary>
        /// @ref BattlePlayerViewController-SerializeFields
        [SerializeField] private float _transparencyEffectRange;

        /// <summary>[SerializeField] The transparency effect's transition rate.</summary>
        /// @ref BattlePlayerViewController-SerializeFields
        [SerializeField] private float _transparencyEffectTransitionRate;

        /// <summary>[SerializeField] The transparency effect's minimum alpha value..</summary>
        /// @ref BattlePlayerViewController-SerializeFields
        [SerializeField] private float _transparencyEffectMinimumAlpha;

        /// <summary>[SerializeField] The damage flash animation's duration.</summary>
        /// @ref BattlePlayerViewController-SerializeFields
        [SerializeField] private float _damageFlashDuration = 1f;

        /// <summary>[SerializeField] The amount of damage flashes.</summary>
        /// @ref BattlePlayerViewController-SerializeFields
        [SerializeField] private int _damageFlashAmount = 5;

        /// @}

        /// <summary>
        /// Public method that is called when entity is activated upon its creation.<br/>
        /// Calls <see cref="PreInitSetup"/> and subscribes to <see cref="Quantum.EventBattlePlayerViewInit">EventBattlePlayerViewInit</see> event with a lambda, which
        /// sets the player model scale and active <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObjects@u-exlink</a>. Handles subscribing to QuantumEvents.
        /// </summary>
        ///
        /// <param name="_">Current simulation frame.</param>
        public override void OnActivate(Frame _) { PreInitSetup(); QuantumEvent.Subscribe(this, (EventBattlePlayerViewInit e) =>
        {
            if (EntityRef != e.Entity) return;
            if (!PredictedFrame.Exists(e.Entity)) return;

            float scale = (float)e.ModelScale;
            transform.localScale = new Vector3(scale, scale, scale);
            _shieldHitParticle.transform.localScale = new Vector3(scale, scale, scale);

            if (BattlePlayerManager.PlayerHandle.GetTeamNumber(e.Slot) == BattleGameViewController.LocalPlayerTeam)
            {
                GameObject characterGameObjects = _characterGameObjects[0];
                characterGameObjects.SetActive(true);
                _spriteRenderer = characterGameObjects.GetComponent<SpriteRenderer>();
            }
            else
            {
                GameObject characterGameObjects = _characterGameObjects[1];
                characterGameObjects.SetActive(true);
                //_heart.SetActive(false);
                _spriteRenderer = characterGameObjects.GetComponent<SpriteRenderer>();
            }

            if (e.Slot == BattleGameViewController.LocalPlayerSlot)
            {
                _localPlayerIndicator.SetActive(true);
            }

            if (_classViewControllerOverride != null)
            {
                if(_classViewControllerOverride.Class == e.Class)
                {
                    Destroy(_classViewController);
                    _classViewController = _classViewControllerOverride;
                }
                else
                {
                    Debug.LogErrorFormat("[BattlePlayerViewController] Class view controller missmatch! Expected {0}, got {1}", e.Class, _classViewControllerOverride.Class);
                    Destroy(_classViewControllerOverride);
                }
            }
            _classViewController.OnViewInit(this, e.Entity, e.Slot, e.CharacterId);

            QuantumEvent.Subscribe<EventBattleCharacterTakeDamage>(this, QEventOnCharacterTakeDamage);
            QuantumEvent.Subscribe<EventBattleShieldTakeDamage>(this, QEventOnShieldTakeDamage);
        });}

        /// <summary>
        /// Public method that is called when the view should update.<br/>
        /// Calls #UpdateModelPositionAdjustment to update the player model's position
        /// </summary>
        public override void OnUpdateView()
        {
            if (!PredictedFrame.Exists(EntityRef)) return;
            BattlePlayerDataQComponent* playerData = PredictedFrame.Unsafe.GetPointer<BattlePlayerDataQComponent>(EntityRef);
            if (playerData->PlayerRef == PlayerRef.None) return;

            Vector3 targetPosition = playerData->TargetPosition.ToUnityVector3();
            BattleTeamNumber battleTeamNumber = playerData->TeamNumber;

            UpdateModelPositionAdjustment(&targetPosition);
            //UpdateAnimator(&targetPosition, battleTeamNumber);

            //if (BattleGameViewController.ProjectileReference != null)
            //{
            //    if (Vector3.Distance(gameObject.transform.position, BattleGameViewController.ProjectileReference.transform.position) <= _transparencyEffectRange && _spriteRenderer.color.a > _transparencyEffectMinimumAlpha)
            //    {
            //        Color tempColor = _spriteRenderer.color;
            //        tempColor.a = Mathf.Clamp(tempColor.a - _transparencyEffectTransitionRate * Time.deltaTime, _transparencyEffectMinimumAlpha, 1);
            //        _spriteRenderer.color = tempColor;
            //    }
            //    else if (_spriteRenderer.color.a < 1)
            //    {
            //        Color tempColor = _spriteRenderer.color;
            //        tempColor.a = Mathf.Clamp(tempColor.a + _transparencyEffectTransitionRate * Time.deltaTime, _transparencyEffectMinimumAlpha, 1);
            //        _spriteRenderer.color = tempColor;
            //    }
            //}

            _classViewController.OnUpdateView();
        }

        /// <value>Reference to the active character's <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SpriteRenderer.html">SpriteRenderer@u-exlink</a>.</value>
        private SpriteRenderer _spriteRenderer;

        /// <value>Holder variable for the damage flash coroutine.</value>
        private Coroutine _damageFlashCoroutine = null;

        /// <value>Reference to the active class view controller.</value>
        private BattlePlayerClassBaseViewController _classViewController;

        /// <summary>
        /// Handles setup that needs to happen before <see cref="Quantum.EventBattlePlayerViewInit">EventBattlePlayerViewInit</see> event is received.<br/>
        /// Currently this is needed for initializing character's class as none.
        /// </summary>
        private void PreInitSetup()
        {
            _classViewController = gameObject.AddComponent<BattlePlayerClassNoneViewController>();
        }

        /// <summary>
        /// Updates the player model's position.
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
        /// Updates the player model's animator.
        /// </summary>
        ///
        /// <param name="targetPosition">Target position Vector3.</param>
        /// <param name="battleTeamNumber">The BattleTeamNumber for the player.</param>
        private void UpdateAnimator(Vector3* targetPosition, BattleTeamNumber battleTeamNumber)
        {
            int animationState = 0;
            bool flipX = false;

            if (transform.position != *targetPosition)
            {
                Vector3 movement = *targetPosition - transform.position;
                if (Mathf.Abs(movement.x) >= Mathf.Abs(movement.z))
                {
                    flipX = (battleTeamNumber == BattleTeamNumber.TeamBeta) ^ (movement.x < 0f);
                    animationState = 1;
                }
                else
                {
                    animationState = 2;
                }
            }

            _spriteRenderer.flipX = flipX;
            _animator.SetInteger("state", animationState);
        }

        /// <summary>
        /// Handler method for EventBattleCharacterTakeDamage QuantumEvent.<br/>
        /// Starts #DamageFlashCoroutine.
        /// </summary>
        ///
        /// <param name="e">The event data.</param>
        private void QEventOnCharacterTakeDamage(EventBattleCharacterTakeDamage e)
        {
            if (EntityRef != e.Entity) return;

            if (_damageFlashCoroutine != null)
            {
                StopCoroutine(_damageFlashCoroutine);
            }
            _damageFlashCoroutine = StartCoroutine(DamageFlashCoroutine());

            _classViewController.OnCharacterTakeDamage(e);
        }

        /// <summary>
        /// Handler method for EventBattleShieldTakeDamage QuantumEvent.<br/>
        /// </summary>
        ///
        /// <param name="e">The event data.</param>
        private void QEventOnShieldTakeDamage(EventBattleShieldTakeDamage e)
        {
            if (EntityRef != e.Entity) return;
            if (!PredictedFrame.Exists(e.Entity)) return;

            if (e.DefenceValue <= FP._0 && _noShieldSprite != null)
            {
                _spriteRenderer.sprite = _noShieldSprite;
            }

            if (_shieldHitParticle != null)
            {
                _shieldHitParticle.Play();
            }

            _classViewController.OnShieldTakeDamage(e);
        }

        /// <summary>
        /// Coroutine which plays the damage flash animation.
        /// </summary>
        ///
        /// <returns>Coroutine IEnumerator.</returns>
        private IEnumerator DamageFlashCoroutine()
        {
            Color tempColor;
            for (int i = 0; i < _damageFlashAmount; i++)
            {
                tempColor = _spriteRenderer.color;
                tempColor.a = 0;
                _spriteRenderer.color = tempColor;

                yield return new WaitForSeconds(_damageFlashDuration / (2 * _damageFlashAmount));

                tempColor = _spriteRenderer.color;
                tempColor.a = 1;
                _spriteRenderer.color = tempColor;

                yield return new WaitForSeconds(_damageFlashDuration / (2 * _damageFlashAmount));
            }
        }
    }
}
