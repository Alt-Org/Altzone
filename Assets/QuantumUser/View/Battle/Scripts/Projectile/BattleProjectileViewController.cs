using UnityEngine;
using Quantum;

namespace Battle.View.Projectile
{
    public class BattleProjectileViewController : QuantumEntityViewComponent
    {
        [Tooltip("Sprite 0: Sadness\nSprite 1: Joy\nSprite 2: Playful\nSprite 3: Aggression\nSprite 4: Love")]
        [SerializeField] private Sprite[] _sprites;
        [SerializeField] private Gradient[] _colorGradients;

        private SpriteRenderer _spriteRenderer;
        private TrailRenderer _trailRenderer;

        public override void OnActivate(Frame _)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _trailRenderer = GetComponent<TrailRenderer>();

            QuantumEvent.Subscribe<EventBattleChangeEmotionState>(this, OnChangeEmotionState);
        }

        private void OnChangeEmotionState(EventBattleChangeEmotionState e)
        {
            _spriteRenderer.sprite = _sprites[(int)e.Emotion];
            _trailRenderer.colorGradient = _colorGradients[(int)e.Emotion];
        }
    }
}
