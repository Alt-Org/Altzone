using System;
using Quantum;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.View.UI
{
    public class BattleUiCharacterFrameComponent : MonoBehaviour
    {
        [SerializeField] private Image _frameImage;

        [SerializeField] private Image _backgroundImage;

        [SerializeField] private FrameGraphics[] _frameGraphics;

        public void SetCharacterFrame(BattlePlayerCharacterClass characterClass)
        {
            int index = characterClass switch
            {
                BattlePlayerCharacterClass.Desensitizer     => 0,
                BattlePlayerCharacterClass.Trickster        => 1,
                BattlePlayerCharacterClass.Obedient         => 2,
                BattlePlayerCharacterClass.Projector        => 3,
                BattlePlayerCharacterClass.Retroflector     => 4,
                BattlePlayerCharacterClass.Confluent        => 5,
                BattlePlayerCharacterClass.Intellectualizer => 6,

                _ => -1,
            };

            if (index == -1) return;

            _frameImage.gameObject.SetActive(true);
            _frameImage.sprite = _frameGraphics[index].FrameSprite;
            _backgroundImage.color = _frameGraphics[index].FrameBackgroundColor;
        }

        [Serializable]
        private struct FrameGraphics
        {
            public Sprite FrameSprite;
            public Color FrameBackgroundColor;
        }
    }
}
