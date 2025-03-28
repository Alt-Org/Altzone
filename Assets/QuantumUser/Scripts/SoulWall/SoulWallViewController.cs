using UnityEngine;

namespace Quantum
{
    public class SoulWallViewController : QuantumCallbacks
    {
        [SerializeField] private GameObject _soulWallEntityGameObject;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private SpriteRenderer _emotionIndicatorSpriteRenderer;
        [SerializeField] private Color[] _colors;
        [SerializeField] private Color[] _emotionIndicatorColors;


        public void OnViewInit() => QuantumEvent.Subscribe<EventSoulWallViewInit>(this, (EventSoulWallViewInit e)=>
        {
            EntityRef entityRef = _soulWallEntityGameObject.GetComponent<QuantumEntityView>().EntityRef;
            if (entityRef != e.Entity) return;

            //scale gameobject
            float scale = (float)e.ModelScale;
            transform.localScale = new Vector3(scale, scale, scale);

            //color cement area
            _spriteRenderer.color = _colors[e.ColorIndex];
            _emotionIndicatorSpriteRenderer.color = _emotionIndicatorColors[e.EmotionIndicatorColorIndex];
        });

    }
}
