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
        /// @anchor BattleProjectileViewController-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <summary>[SerializeField] SpriteRenderer reference for projectile's sprite.</summary>
        /// @ref BattleProjectileViewController-SerializeFields
        [SerializeField] private SpriteRenderer _spriteRenderer;

        /// <summary>[SerializeField] SpriteRenderer reference for projectile's glow.</summary>
        /// @ref BattleProjectileViewController-SerializeFields
        [SerializeField] private SpriteRenderer _spriteGlowRenderer;

        /// <summary>[SerializeField] TrailRenderer reference for projectile's trail.</summary>
        /// @ref BattleProjectileViewController-SerializeFields
        [SerializeField] private TrailRenderer _trailRenderer;

        [Tooltip("Sprite 0: Sadness\nSprite 1: Joy\nSprite 2: Playful\nSprite 3: Aggression\nSprite 4: Love")]
        /// <summary>[SerializeField] An array of projectile sprites.</summary>
        /// @ref BattleProjectileViewController-SerializeFields
        [SerializeField] private Sprite[] _sprites;

        /// <summary>[SerializeField] An array of gradient colors for projectile's trail.</summary>
        /// @ref BattleProjectileViewController-SerializeFields
        [SerializeField] private Gradient[] _colorGradients;

        /// <summary>[SerializeField] An array of glow colors for projectile's glow.</summary>
        /// @ref BattleProjectileViewController-SerializeFields
        [SerializeField] private Color[] _colorGlows;

        /// @}

        /// <summary>
        /// Public method that is called when entity is activated upon its creation.<br/>
        /// Fetches needed components and subscribes to BattleChangeEmotionState and BattleProjectileChangeGlowStrength.
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

        /// <value>Holder variable for the projectile's glow value.</value>
        private float _glowStrength;

        /// <summary>
        /// Private method that gets called by Quantum via BattleChangeEmotionState Event.<br/>
        /// Changes projectile's sprite, glow strength and its trail's color.
        /// </summary>
        ///
        /// <param name="e">BattleChangeEmotionState Event</param>
        private void OnChangeEmotionState(EventBattleChangeEmotionState e)
        {
            _spriteRenderer.sprite = _sprites[(int)e.Emotion];
            _spriteGlowRenderer.color = _colorGlows[(int)e.Emotion].Alpha(_glowStrength);
            _trailRenderer.colorGradient = _colorGradients[(int)e.Emotion];
        }
        /// <summary>
        /// Private method that gets called by Quantum via BattleProjectileChangeGlowStrength.<br/>
        /// Changes projectile's glow strength.
        /// </summary>
        ///
        /// <param name="e">BattleProjectileChangeGlowStrength Event</param>
        private void OnProjectileChangeGlowStrength(EventBattleProjectileChangeGlowStrength e)
        {
            _glowStrength = (float)e.Strength;
            _spriteGlowRenderer.color = _spriteGlowRenderer.color.Alpha(_glowStrength);
        }
    }
}
