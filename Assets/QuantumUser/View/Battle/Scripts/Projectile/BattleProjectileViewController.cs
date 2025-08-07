using Battle.QSimulation.Game;
using Battle.View.Game;
using Prg.Scripts.Common.PubSub;
using Quantum;
using UnityEngine;

namespace Battle.View.Projectile
{
    public class BattleProjectileViewController : QuantumEntityViewComponent
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private SpriteRenderer _spriteGlowRenderer;
        [SerializeField] private TrailRenderer _trailRenderer;

        [Tooltip("Sprite 0: Sadness\nSprite 1: Joy\nSprite 2: Playful\nSprite 3: Aggression\nSprite 4: Love")]
        [SerializeField] private Sprite[] _sprites;
        [SerializeField] private Gradient[] _colorGradients;
        [SerializeField] private Color[] _colorGlows;


        public override void OnActivate(Frame _)
        {
            if (BattleGameViewController.LocalPlayerTeam == BattleTeamNumber.TeamBeta)
            {
                transform.rotation = Quaternion.Euler(90, 180, 0);
            }

            QuantumEvent.Subscribe<EventBattleChangeEmotionState>(this, OnChangeEmotionState);
            QuantumEvent.Subscribe<EventBattleProjectileChangeGlowStrength>(this, OnProjectileChangeGlowStrength);

            BattleGameViewController.AssignProjectileReference(gameObject);
        }

        private float _glowStrength;

        private void OnChangeEmotionState(EventBattleChangeEmotionState e)
        {
            _spriteRenderer.sprite = _sprites[(int)e.Emotion];
            _spriteGlowRenderer.color = _colorGlows[(int)e.Emotion].Alpha(_glowStrength);
            _trailRenderer.colorGradient = _colorGradients[(int)e.Emotion];
        }
        private void OnProjectileChangeGlowStrength(EventBattleProjectileChangeGlowStrength e)
        {
            _glowStrength = (float)e.Strength;
            _spriteGlowRenderer.color = _spriteGlowRenderer.color.Alpha(_glowStrength);
        }
    }
}
