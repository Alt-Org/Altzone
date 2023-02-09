using UnityEngine;
using UnityEngine.Assertions;

namespace Battle0.Scripts.Battle.Game
{
    /// <summary>
    /// Helper for game background to find it and change it if required.
    /// </summary>
    internal class GameBackground : MonoBehaviour, IBattleBackground
    {
        [Header("Settings"), SerializeField] private GameObject _gameBackground;
        [SerializeField] private Sprite[] _backgroundSprites;
        [SerializeField] private SpriteRenderer _renderer;
        private Transform _transform;

        public bool IsRotated => _gameBackground.transform.rotation.z != 0f;

        private void Awake()
        {
            Assert.IsNotNull(_gameBackground, "_gameBackground must be assigned in Editor");
            _transform = GetComponent<Transform>();
        }

        private void Start()
        {
            // Set default background image on start.
            SetBackgroundImageByIndex(0);
        }

        public void Rotate(bool isUpsideDown)
        {
            _transform.Rotate(isUpsideDown);
        }

        public void SetBackgroundImageByIndex(int index)
        {
            Assert.IsTrue(index >= 0 && index < _backgroundSprites.Length, "index >= 0 && index < _backgroundSprites.Length");
            _renderer.sprite = _backgroundSprites[index];
        }
    }
}