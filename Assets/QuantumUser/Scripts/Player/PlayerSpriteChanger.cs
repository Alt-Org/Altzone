using UnityEngine;

namespace Quantum
{
    public class PlayerSpriteChanger : QuantumCallbacks
    {
        [Tooltip("Sprite 0: shield on\nSprite 1: shield off")]
        [SerializeField] private Sprite[] _sprites;

        private QuantumEntityView _entityView;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _entityView = GetComponent<QuantumEntityView>();
            _spriteRenderer = transform.GetChild(1).GetComponent<SpriteRenderer>();

            QuantumEvent.Subscribe<EventToggleShield>(this, OnToggleShield);
        }

        private void OnToggleShield(EventToggleShield e)
        {
            if (e.PlayerEntity != _entityView.EntityRef) return;

            _spriteRenderer.sprite = _sprites[e.ShieldBool ? 0 : 1];
        }
    }
}