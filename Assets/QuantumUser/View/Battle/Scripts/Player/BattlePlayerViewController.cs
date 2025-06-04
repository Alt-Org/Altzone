using UnityEngine;
using Quantum;
using Battle.View.Game;
using Battle.QSimulation.Player;

namespace Battle.View.Player
{
    public unsafe class BattlePlayerViewController : QuantumEntityViewComponent
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private GameObject _heart;
        [SerializeField] private GameObject[] _characterGameObjects;
        [SerializeField] private GameObject _localPlayerIndicator;

        [SerializeField] private float _transparencyEffectRange;
        [SerializeField] private float _transparencyEffectTransitionRate;
        [SerializeField] private float _transparencyEffectMinimumAlpha;

        private BattlePlayerRotationDirection _rotationDirection;
        private float _playerRotation;

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

            QuantumEvent.Subscribe<EventBattlePlayerRotationAction>(this, PlayerRotationAction);
        });

        private void PlayerRotationAction(EventBattlePlayerRotationAction e)
        {
            if (EntityRef != e.PlayerEntity) return;

            _playerRotation = 0;
            _rotationDirection = e.RotationDirection;
        }

        public override void OnUpdateView()
        {
            if (!PredictedFrame.Exists(EntityRef)) return;
            BattlePlayerDataQComponent* playerData = PredictedFrame.Unsafe.GetPointer<BattlePlayerDataQComponent>(EntityRef);
            if (playerData->PlayerRef == PlayerRef.None) return;

            Vector3 targetPosition = playerData->TargetPosition.ToUnityVector3();
            BattleTeamNumber battleTeamNumber = playerData->TeamNumber;

            //UpdateAnimator(&targetPosition, battleTeamNumber);

            switch (_rotationDirection)
            {
                case BattlePlayerRotationDirection.Left:
                    _playerRotation -= 720f * Time.deltaTime;
                    break;
                case BattlePlayerRotationDirection.Right:
                    _playerRotation += 720f * Time.deltaTime;
                    break;
            }

            if (_rotationDirection != BattlePlayerRotationDirection.None)
            {
                if (Mathf.Abs(_playerRotation) >= 360f)
                {
                    _playerRotation = 0;
                    _rotationDirection = BattlePlayerRotationDirection.None;
                }
            }

            UpdateModelAdjustment(&targetPosition);

            if (BattleGameViewController.ProjectileReference != null)
            {
                if (Vector3.Distance(gameObject.transform.position, BattleGameViewController.ProjectileReference.transform.position) <= _transparencyEffectRange && _spriteRenderer.color.a > _transparencyEffectMinimumAlpha)
                {
                    Color tempColor = _spriteRenderer.color;
                    tempColor.a = Mathf.Clamp(tempColor.a - _transparencyEffectTransitionRate * Time.deltaTime, _transparencyEffectMinimumAlpha, 1);
                    _spriteRenderer.color = tempColor;
                }
                else if (_spriteRenderer.color.a < 1)
                {
                    Color tempColor = _spriteRenderer.color;
                    tempColor.a = Mathf.Clamp(tempColor.a + _transparencyEffectTransitionRate * Time.deltaTime, _transparencyEffectMinimumAlpha, 1);
                    _spriteRenderer.color = tempColor;
                }
            }
        }

        private SpriteRenderer _spriteRenderer;

        private void UpdateModelAdjustment(Vector3* targetPosition)
        {
            const float adjustmentDistance = 0.25f;
            Vector3 distanceToTargetPosition = *targetPosition - transform.position;
            if (distanceToTargetPosition.sqrMagnitude < adjustmentDistance * adjustmentDistance)
            {
                transform.position = *targetPosition;
            }

            transform.localRotation = Quaternion.Euler(0, _playerRotation, 0);
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
    }
}
