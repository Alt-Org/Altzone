using UnityEngine;

namespace Quantum
{
    public class ProjectileSpriteChanger : QuantumCallbacks
    {
        [Tooltip("Sprite 0: Sadness\nSprite 1: Joy\nSprite 2: Playful\nSprite 3: Aggression\nSprite 4: Love")]
        [SerializeField] private Sprite[] _sprites;

        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            QuantumEvent.Subscribe<EventChangeMoodState>(this, OnChangeMoodState);
        }

        private void OnChangeMoodState(EventChangeMoodState e)
        {
            _spriteRenderer.sprite = _sprites[(int)e.Mood];
        }
    }
}
