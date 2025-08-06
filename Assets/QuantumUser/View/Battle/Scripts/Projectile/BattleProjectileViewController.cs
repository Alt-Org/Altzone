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
        [Tooltip("Sprite 0: Sadness\nSprite 1: Joy\nSprite 2: Playful\nSprite 3: Aggression\nSprite 4: Love")]
        /// <value>[SerializeField] An array of projectile sprites.</value>
        [SerializeField] private Sprite[] _sprites;

        /// <value>[SerializeField] An array of gradient colors for projectile's trail.</value>
        [SerializeField] private Gradient[] _colorGradients;

        /// <value>SpriteRenderer for projectile's sprite.</value>
        private SpriteRenderer _spriteRenderer;

        /// <value>TrailRenderer for projectile's trail.</value>
        private TrailRenderer _trailRenderer;

        /// <summary>
        /// Public method that is called when entity is activated upon its creation.<br/>
        /// Fetches needed components and subscribes to BattleChangeEmotionState
        /// <a href = "https://doc.photonengine.com/quantum/current/manual/quantum-ecs/game-events"> Quantum Event.@u-exlink</a>
        /// </summary>
        /// <param name="_">Current simulation frame.</param>
        public override void OnActivate(Frame _)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _trailRenderer = GetComponent<TrailRenderer>();

            if (BattleGameViewController.LocalPlayerTeam == BattleTeamNumber.TeamBeta)
            {
                transform.rotation = Quaternion.Euler(90, 180, 0);
            }

            QuantumEvent.Subscribe<EventBattleChangeEmotionState>(this, OnChangeEmotionState);

            BattleGameViewController.AssignProjectileReference(gameObject);
        }

        /// <summary>
        /// Private method that gets called by Quantum via BattleChangeEmotionState Event.<br/>
        /// Changes projectile's sprite and its trail's color.
        /// </summary>
        /// <param name="e">BattleChangeEmotionState Event</param>
        private void OnChangeEmotionState(EventBattleChangeEmotionState e)
        {
            _spriteRenderer.sprite = _sprites[(int)e.Emotion];
            _trailRenderer.colorGradient = _colorGradients[(int)e.Emotion];
        }
    }
}
