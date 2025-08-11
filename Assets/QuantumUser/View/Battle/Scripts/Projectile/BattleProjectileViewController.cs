/// @file BattleProjectileViewController.cs
/// <summary>
/// Handles projectile's sprite changes and its trail's color.
/// </summary>

using UnityEngine;
using Quantum;

using Battle.View.Game;

namespace Battle.View.Projectile
{
    /// <summary>
    /// <span class="brief-h">%Projectile view <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>.</span><br/>
    /// Handles projectile's sprite changes and its trail's color.
    /// </summary>
    public class BattleProjectileViewController : QuantumEntityViewComponent
    {
        // old doc
        // <value>SpriteRenderer for projectile's sprite.</value>
        // <value>TrailRenderer for projectile's trail.</value>

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private SpriteRenderer _spriteGlowRenderer;
        [SerializeField] private TrailRenderer _trailRenderer;

        [Tooltip("Sprite 0: Sadness\nSprite 1: Joy\nSprite 2: Playful\nSprite 3: Aggression\nSprite 4: Love")]
        /// <value>[SerializeField] An array of projectile sprites.</value>
        [SerializeField] private Sprite[] _sprites;

        /// <value>[SerializeField] An array of gradient colors for projectile's trail.</value>
        [SerializeField] private Gradient[] _colorGradients;
        [SerializeField] private Color[] _colorGlows;

        /// <summary>
        /// Public method that is called when entity is activated upon its creation.<br/>
        /// Fetches needed components and subscribes to BattleChangeEmotionState
        /// <a href = "https://doc.photonengine.com/quantum/current/manual/quantum-ecs/game-events"> Quantum Event.@u-exlink</a>
        /// </summary>
        ///
        /// <param name="_">Current simulation frame.</param>
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

        /// <summary>
        /// Private method that gets called by Quantum via BattleChangeEmotionState Event.<br/>
        /// Changes projectile's sprite and its trail's color.
        /// </summary>
        ///
        /// <param name="e">BattleChangeEmotionState Event</param>
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
