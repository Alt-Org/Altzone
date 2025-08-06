/// @file BattleSoulWallViewController.cs
/// <summary>
/// Handles SoulWall graphics. If debug mode is active, uses colored boxes instead of sprites.
/// </summary>

using UnityEngine;
using Quantum;

namespace Battle.View.SoulWall
{
    /// <summary>
    /// <span class="brief-h">%SoulWall's <a href="https://doc-api.photonengine.com/en/quantum/current/class_quantum_1_1_quantum_entity_view_component.html">QuantumEntityViewComponent@u-exlink</a>.</span><br/>
    /// Handles %SoulWall's sprites and debug colors.
    /// </summary>
    public class BattleSoulWallViewController : QuantumEntityViewComponent
    {
        /// <value>[SerializeField] SpriteRenderer for SoulWall sprites.</value>
        [SerializeField] private SpriteRenderer _spriteRenderer;

        /// <value>[SerializeField] SpriteRenderer for the cement under SoulWall sprite.</value>
        [SerializeField] private SpriteRenderer _emotionIndicatorSpriteRenderer;

        /// <value>[SerializeField] SpriteRender for debug mode box sprite. Disabled by default.</value>
        [SerializeField] private SpriteRenderer _debugSpriteRenderer;

        /// <value>[SerializeField] An array of colors that show which emotion projectile takes after destroying the SoulWall segment in question.</value>
        [SerializeField] private Color[] _emotionIndicatorColors;

        /// <value>[SerializeField] Bool for debug mode.</value>
        [SerializeField] private bool _useDebugSprites;

        /// <value>[SerializeField] An array of colors for debug mode.</value>
        [SerializeField] private Color[] _debugColors;

        /// <summary>
        /// Public method that is called when entity is activated upon its creation.<br/>
        /// Scales SoulWall GameObject and changes cement layer's color.
        /// If debug mode is on, uses box sprites instead of regular SoulWall sprites.
        /// </summary>
        /// 
        /// <param name="_">Current simulation frame.</param>
        public override void OnActivate(Frame _) => QuantumEvent.Subscribe(this, (EventBattleSoulWallViewInit e) =>
        {
            if (EntityRef != e.Entity) return;

            // scale gameobject
            float scale = (float)e.ModelScale;
            transform.localScale = new Vector3(scale, scale, scale);

            // color emotionIndicator
            _emotionIndicatorSpriteRenderer.color = _emotionIndicatorColors[e.EmotionIndicatorColorIndex];

            // debug
            if (_useDebugSprites)
            {
                _spriteRenderer.enabled = false;
                _debugSpriteRenderer.enabled = true;
                _debugSpriteRenderer.color = _debugColors[e.DebugColorIndex];
            }
        });
    }
}
