using UnityEngine;

namespace Battle.View.Effect
{
    public class BattleScreenEffectViewController : MonoBehaviour
    {
        [SerializeField] private GameObject _overlay;
        [SerializeField] private GameObject[] _particleSources;
        [SerializeField] private Color[] _colors;

        public bool IsActive {get ; private set ; } = false;

        public void SetActive(bool active)
        {
            foreach (GameObject particleSource in _particleSources) particleSource.SetActive(active);
            _overlay.SetActive(active);
            IsActive = active;
        }

        public void ChangeColor(int colorIndex)
        {
            _spriteRenderer.color = _colors[colorIndex];
        }

        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = _overlay.GetComponent<SpriteRenderer>();
        }
    }
}
