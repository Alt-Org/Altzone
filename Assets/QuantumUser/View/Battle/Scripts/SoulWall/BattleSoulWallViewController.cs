using UnityEngine;
using Quantum;

namespace Battle.View.SoulWall
{
    public class BattleSoulWallViewController : QuantumEntityViewComponent
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private SpriteRenderer _emotionIndicatorSpriteRenderer;
        [SerializeField] private SpriteRenderer _debugSpriteRenderer;
        [SerializeField] private Color[] _emotionIndicatorColors;
        [SerializeField] private bool _useDebugSprites;
        [SerializeField] private Color[] _debugColors;

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
