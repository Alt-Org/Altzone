using UnityEngine;
using Quantum;

namespace Battle.View.SoulWall
{
    public class BattleSoulWallViewController : QuantumCallbacks
    {
        [SerializeField] private GameObject _soulWallEntityGameObject;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private SpriteRenderer _emotionIndicatorSpriteRenderer;
        [SerializeField] private SpriteRenderer _debugSpriteRenderer;
        [SerializeField] private Color[] _emotionIndicatorColors;
        [SerializeField] private bool _useDebugSprites;
        [SerializeField] private Color[] _debugColors;

        public void OnViewInit() => QuantumEvent.Subscribe(this, (EventBattleSoulWallViewInit e)=>
        {
            EntityRef entityRef = _soulWallEntityGameObject.GetComponent<QuantumEntityView>().EntityRef;
            if (entityRef != e.Entity) return;

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
