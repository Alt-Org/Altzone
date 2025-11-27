/// @file BattleSoulWallViewController.cs
/// <summary>
/// Handles SoulWall graphics. If debug mode is active, uses colored boxes instead of sprites.
/// </summary>

// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;
using Battle.QSimulation;

namespace Battle.View.SoulWall
{
    /// <summary>
    /// <span class="brief-h">%SoulWall's <a href="https://doc-api.photonengine.com/en/quantum/current/class_quantum_1_1_quantum_entity_view_component.html">QuantumEntityViewComponent@u-exlink</a>.</span><br/>
    /// Handles %SoulWall's sprites and debug colors.
    /// </summary>
    public class BattleSoulWallViewController : QuantumEntityViewComponent
    {
        /// @anchor BattleSoulWallViewController-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <summary>[SerializeField] SpriteRenderer for SoulWall sprites.</summary>
        /// @ref BattleSoulWallViewController-SerializeFields
        [SerializeField] private SpriteRenderer _spriteRenderer;

        /// <summary>[SerializeField] SpriteRenderer for the cement under SoulWall sprite.</summary>
        /// @ref BattleSoulWallViewController-SerializeFields
        [SerializeField] private SpriteRenderer _emotionIndicatorSpriteRenderer;

        /// <summary>[SerializeField] SpriteRender for debug mode box sprite. Disabled by default.</summary>
        /// @ref BattleSoulWallViewController-SerializeFields
        [SerializeField] private SpriteRenderer _debugSpriteRenderer;

        /// <summary>[SerializeField] An array of colors that show which emotion projectile takes after destroying the SoulWall segment in question.</summary>
        /// @ref BattleSoulWallViewController-SerializeFields
        [SerializeField] private Color[] _emotionIndicatorColors;

        /// <summary>[SerializeField] Bool for debug mode.</summary>
        /// @ref BattleSoulWallViewController-SerializeFields
        [SerializeField] private bool _useDebugSprites;

        /// <summary>[SerializeField] An array of colors for debug mode.</summary>
        /// @ref BattleSoulWallViewController-SerializeFields
        [SerializeField] private Color[] _debugColors;

        /// @}

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

            BattleDebugLogger debugLogger = BattleDebugLogger.Create<BattleSoulWallViewController>();

            _spriteRenderer.enabled = true;
            _emotionIndicatorSpriteRenderer.enabled = true;

            // scale gameobject
            float scale = (float)e.ModelScale;
            transform.localScale = new Vector3(scale, scale, scale);

            debugLogger.LogFormat("SoulWall view initialized with scale {0}", scale);

            // color emotionIndicator
            //_emotionIndicatorSpriteRenderer.color = _emotionIndicatorColors[e.EmotionIndicatorColorIndex];
            _emotionIndicatorSpriteRenderer.color = Color.gray;

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
