using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Game;

namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    /// <summary>
    /// Controls visual functionality of CharacterStatsPanel.
    /// </summary>
    public class StatsPanel : MonoBehaviour
    {
        [SerializeField] private Image _characterHeadImage;
        [SerializeField] private Image _lockImage;

        [SerializeField] private Image _characterImage;
        [SerializeField] private TMP_Text _characterDescription;
        [SerializeField] private TMP_Text _specialAbility;
        [SerializeField] private TMP_Text _wins;
        [SerializeField] private TMP_Text _losses;
        [SerializeField] private TMP_Text _className;

        [SerializeField] private BaseScrollRect _scrollRect;

        private StatsWindowController _controller;

        private GameObject previousPage;

        private void OnEnable()
        {
            if (_controller == null) _controller = FindObjectOfType<StatsWindowController>();
            _scrollRect.VerticalNormalizedPosition = 1;

            SetCharacterHeadImage();
            SetCharacterImage();
            SetCharacterDescription();
            SetWinsAndLosses();
            SetClassName();


            if (_controller.IsCurrentCharacterLocked())
            {
                _lockImage.gameObject.SetActive(true);
            }
            else
            {
                _lockImage.gameObject.SetActive(false);
            }
        }

        private void SetClassName()
        {
            _className.text = _controller.GetCurrentCharacterClassName();
        }
        private void SetCharacterHeadImage()
        {
            Sprite sprite = _controller.GetCurrentCharacterSprite();

            if (sprite != null)
            {
                _characterHeadImage.sprite = sprite;
            }
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

        public void ClosePopup()
        {
            gameObject.SetActive(false);
        }
    }
}
