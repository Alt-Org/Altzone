using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    public class InfoPanel : MonoBehaviour
    {
        [SerializeField] private StatsWindowController _controller;
        [SerializeField] private Image _characterImage;
        [SerializeField] private Image _specialAbilityImage;
        [SerializeField] private TMP_Text _characterDescription;
        [SerializeField] private TMP_Text _specialAbility;
        [SerializeField] private TMP_Text _wins;
        [SerializeField] private TMP_Text _losses;


        private void OnEnable()
        {
            SetCharacterImage();
            SetCharacterDescription();
            SetSpecialAbility();
            SetWinsAndLosses();
        }


        private void SetCharacterImage()
        {
            Sprite sprite = _controller.GetCurrentCharacterSprite();

            if (sprite != null)
            {
                _characterImage.sprite = sprite;
            }
        }


        private void SetCharacterDescription()
        {
            _characterDescription.text = _controller.GetCurrentCharacterDescription();
        }


        private void SetSpecialAbility()
        {
            _specialAbility.text = _controller.GetCurrentCharacterSpecialAbilityDescription();

            Sprite sprite = _controller.GetCurrentCharacterSpecialAbilitySprite();
            if (sprite != null && _specialAbilityImage != null)
            {
                _specialAbilityImage.sprite = sprite;
            }
        }


        private void SetWinsAndLosses()
        {
            _wins.text = _controller.GetCurrentCharacterWins().ToString();
            _losses.text = _controller.GetCurrentCharacterLosses().ToString();
        }
    }
}
