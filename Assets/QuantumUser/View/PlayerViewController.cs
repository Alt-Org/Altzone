using UnityEngine;
using Quantum;
using Photon.Deterministic;
public unsafe class PlayerViewController : QuantumEntityViewComponent
{

    [SerializeField] private Animator _animator;
    [SerializeField] private GameObject _heart;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    public override void OnActivate(Frame _) => QuantumEvent.Subscribe(this, (EventPlayerViewInit e) => {
        if (EntityRef != e.Entity) return;
        if (!PredictedFrame.Exists(e.Entity)) return;

        float scale = (float)e.ModelScale;
        transform.localScale = new Vector3(scale, scale, scale);
    });

    public override void OnUpdateView()
    {
        if (!PredictedFrame.Exists(EntityRef)) return;
        PlayerData* playerData = PredictedFrame.Unsafe.GetPointer<PlayerData>(EntityRef);
        if (playerData->PlayerRef == PlayerRef.None) return;

        UpdateAnimator(playerData);

    }

    private void UpdateAnimator(PlayerData* playerData)
    {
        int animationState = 0;
        bool flipX = false;
        
        if (transform.position.ToFPVector2() != playerData->TargetPosition)
        {
            FPVector2 movement = playerData->TargetPosition - transform.position.ToFPVector2();
            if((movement.X < -FP._0_25 || movement.X > FP._0_25) || (movement.Y < -FP._0_25 || movement.Y > FP._0_25))
            {
                if(FPMath.Abs(movement.X) >= FPMath.Abs(movement.Y))
                {
                    flipX = movement.X < 0;
                    animationState = 1;
                } else
                {
                    animationState = 2;
                }
            } else
            {
                animationState = 0;
            }
        }

        _spriteRenderer.flipX = flipX;
        _animator.SetInteger("state", animationState);
    }
}
