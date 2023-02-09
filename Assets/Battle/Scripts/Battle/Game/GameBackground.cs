using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Battle.Game
{
    /// <summary>
    /// Helper for game background to find it and change it if required.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    internal class GameBackground : MonoBehaviour, IBattleBackground
    {
        [Header("Settings"), SerializeField] private Sprite[] _backgroundSprites;
        private SpriteRenderer _renderer;
        private Transform _transform;

        bool IBattleBackground.IsRotated => transform.rotation.z != 0f;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _transform = GetComponent<Transform>();
        }

        private void Start()
        {
            // Set default background image on start.
            ((IBattleBackground)this).SetBackgroundImageByIndex(0);
        }

        void IBattleBackground.Rotate(bool isUpsideDown)
        {
            _transform.Rotate(isUpsideDown);
        }

        void IBattleBackground.SetBackgroundImageByIndex(int index)
        {
            Assert.IsTrue(index >= 0 && index < _backgroundSprites.Length, "index >= 0 && index < _backgroundSprites.Length");
            _renderer.sprite = _backgroundSprites[index];
        }
    }
}
