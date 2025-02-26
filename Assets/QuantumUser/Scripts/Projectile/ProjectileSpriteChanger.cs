using UnityEngine;

namespace Quantum
{
    public class ProjectileSpriteChanger : QuantumCallbacks
    {
        [Tooltip("Sprite 0: Aggression\nSprite 1: Joy\nSprite 2: Love\nSprite 3: Playful\nSprite 4: Sadness")]
        [SerializeField] private Sprite[] _sprites;

        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            QuantumEvent.Subscribe<EventChangeProjectileSprite>(this, OnChangeProjectileSprite);
        }

        private void OnChangeProjectileSprite(EventChangeProjectileSprite e)
        {
            _spriteRenderer.sprite = _sprites[e.SpriteIndex];
        }
    }
}
