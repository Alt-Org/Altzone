using UnityEngine;

namespace Quantum
{
    public class ScreenEffectColorChanger : QuantumCallbacks
    {
        [SerializeField] private Color[] _colors;

        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();

            QuantumEvent.Subscribe<EventChangeEmotionState>(this, OnChangeEmotionState);
        }

        private void OnChangeEmotionState(EventChangeEmotionState e)
        {
            _spriteRenderer.color = _colors[(int)e.Emotion];
        }
    }
}
