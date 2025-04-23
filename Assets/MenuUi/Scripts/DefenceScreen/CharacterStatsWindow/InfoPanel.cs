using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    /// <summary>
    /// Set visual info to info panel
    /// </summary>
    public class InfoPanel : MonoBehaviour
    {
        [SerializeField] private Image _characterImage;
        [SerializeField] private TMP_Text _characterDescription;
        [SerializeField] private TMP_Text _specialAbility;
        [SerializeField] private TMP_Text _wins;
        [SerializeField] private TMP_Text _losses;

        [SerializeField] private GameObject currentPage;
        [SerializeField] private GameObject targetPage;

        private StatsWindowController _controller;

        private void OnEnable()
        {
            if (_controller == null) _controller = FindObjectOfType<StatsWindowController>();
            SetCharacterImage();
            SetCharacterDescription();
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


        private void SetWinsAndLosses()
        {
            _wins.text = _controller.GetCurrentCharacterWins().ToString();
            _losses.text = _controller.GetCurrentCharacterLosses().ToString();
        }

        public void SwitchPage()
        {
            if (currentPage != null) currentPage.SetActive(false);
            if (targetPage != null) targetPage.SetActive(true);
        }
        public void ClosePopup()
        {
            gameObject.SetActive(false);
        }
    }
}
