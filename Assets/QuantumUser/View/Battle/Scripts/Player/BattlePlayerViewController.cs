using UnityEngine;
using Quantum;

namespace Battle.View.Player
{
    public unsafe class BattlePlayerViewController : QuantumEntityViewComponent
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private GameObject _heart;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private GameObject _localPlayerIndicator;

        public override void OnActivate(Frame _) => QuantumEvent.Subscribe(this, (EventBattlePlayerViewInit e) => {
            if (EntityRef != e.Entity) return;
            if (!PredictedFrame.Exists(e.Entity)) return;

            float scale = (float)e.ModelScale;
            transform.localScale = new Vector3(scale, scale, scale);
        });

        public override void OnUpdateView()
        {
            if (!PredictedFrame.Exists(EntityRef)) return;
            BattlePlayerDataQComponent* playerData = PredictedFrame.Unsafe.GetPointer<BattlePlayerDataQComponent>(EntityRef);
            if (playerData->PlayerRef == PlayerRef.None) return;

            Vector3 targetPosition = playerData->TargetPosition.ToUnityVector3();
            BattleTeamNumber battleTeamNumber = playerData->TeamNumber;

            UpdateModelPositionAdjustment(&targetPosition);
            UpdateAnimator(&targetPosition, battleTeamNumber);
        }

        private void UpdateModelPositionAdjustment(Vector3* targetPosition)
        {
            const float adjustmentDistance = 0.25f;
            Vector3 distanceToTargetPosition = *targetPosition - transform.position;
            if (distanceToTargetPosition.sqrMagnitude < adjustmentDistance * adjustmentDistance)
            {
                transform.position = *targetPosition;
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
    }
}
