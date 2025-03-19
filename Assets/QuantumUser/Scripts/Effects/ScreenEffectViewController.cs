using UnityEngine;

namespace Quantum
{
    public class ScreenEffectViewController : MonoBehaviour
    {
        [SerializeField] private GameObject[] _particleSources;
        [SerializeField] private Color[] _colors;

        public bool IsActive {get ; private set ; } = false;

        public void SetActive(bool active)
        {
            foreach (GameObject particleSource in _particleSources) particleSource.SetActive(active);
            _spriteRenderer.enabled = active;
            IsActive = active;
        }

        public void ChangeColor(int colorIndex)
        {
            _spriteRenderer.color = _colors[colorIndex];
        }

        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
}
