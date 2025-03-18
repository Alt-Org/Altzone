using UnityEngine;

namespace Quantum
{
    public class ScreenEffectColorChanger : QuantumCallbacks
    {
        private readonly Color[] _colors = new Color[5]
        {
            new(0.09f, 0.4f , 0.7f, 1f), //blue
            new(0.9f, 0.7f , 0.1f, 1f),  //yellow
            new(0.7f, 0.2f , 0f, 1f),    //orange
            new(0.8f, 0f , 0f, 1f),      //red
            new(1f, 0.5f , 0.7f, 1f)     //pink
        };

        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();

            QuantumEvent.Subscribe<EventChangeMoodState>(this, OnChangeMoodState);
        }

        private void OnChangeMoodState(EventChangeMoodState e)
        {
            _spriteRenderer.color = _colors[(int)e.Mood];
        }
    }
}
