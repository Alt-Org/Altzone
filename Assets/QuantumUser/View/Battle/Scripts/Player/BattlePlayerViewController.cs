/// @file BattlePlayerViewController.cs
/// <summary>
/// Handles player sprites and animations.
/// </summary>

using System.Collections;
using UnityEngine;
using Quantum;
using Battle.View.Game;
using Battle.QSimulation.Player;

namespace Battle.View.Player
{
    /// <summary>
    /// %Player's entityview script.<br/>
    /// Handles player sprites and animations.
    /// </summary>
    public unsafe class BattlePlayerViewController : QuantumEntityViewComponent
    {
        /// <value>[SerializeField] Animator GameObject that handles player animations.</value>
        [SerializeField] private Animator _animator;

        /// <value>[SerializeField] %Player's ChildObject where heart sprite is located.</value>
        [SerializeField] private GameObject _heart;
        [SerializeField] private GameObject[] _characterGameObjects;
        [SerializeField] private GameObject _localPlayerIndicator;

        [SerializeField] private float _transparencyEffectRange;
        [SerializeField] private float _transparencyEffectTransitionRate;
        [SerializeField] private float _transparencyEffectMinimumAlpha;

        [SerializeField] private float _damageFlashDuration = 1f;
        [SerializeField] private int _damageFlashAmount = 5;

        private Coroutine _damageFlashCoroutine = null;

        public override void OnActivate(Frame _) => QuantumEvent.Subscribe(this, (EventBattlePlayerViewInit e) => {
            if (EntityRef != e.Entity) return;
            if (!PredictedFrame.Exists(e.Entity)) return;

            float scale = (float)e.ModelScale;
            transform.localScale = new Vector3(scale, scale, scale);

            if (BattlePlayerManager.PlayerHandle.GetTeamNumber(e.Slot) == BattleGameViewController.LocalPlayerTeam)
            {
                GameObject _characterGameObject = _characterGameObjects[0];
                _characterGameObject.SetActive(true);
                _spriteRenderer = _characterGameObject.GetComponent<SpriteRenderer>();
            }
            else
            {
                GameObject _characterGameObject = _characterGameObjects[1];
                _characterGameObject.SetActive(true);
                _heart.SetActive(false);
                _spriteRenderer = _characterGameObject.GetComponent<SpriteRenderer>();
            }

            if (e.Slot == BattleGameViewController.LocalPlayerSlot)
            {
                _localPlayerIndicator.SetActive(true);
            }

            QuantumEvent.Subscribe<EventBattleCharacterTakeDamage>(this, QEventOnCharacterTakeDamage);
        });

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
        }

        private SpriteRenderer _spriteRenderer;

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

        private void QEventOnCharacterTakeDamage(EventBattleCharacterTakeDamage e)
        {
            if (EntityRef != e.Entity) return;

            if (_damageFlashCoroutine != null)
            {
                StopCoroutine(_damageFlashCoroutine);
            }
            _damageFlashCoroutine = StartCoroutine(DamageFlashCoroutine());
        }

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
