using Altzone.Scripts.Language;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ReferenceSheets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    /// <summary>
    /// Controls visual functionality of CharacterStatsPanel.
    /// </summary>
    public class StatsPanel : MonoBehaviour
    {
        [SerializeField] private Image _characterHeadImage;
        [SerializeField] private Image _lockImage;

        [SerializeField] private Image _characterImageTop;
        [SerializeField] private Image _characterImageBottom;
        [SerializeField] private TextLanguageSelectorCaller _characterDescription;
        [SerializeField] private TextLanguageSelectorCaller _specialAbility;
        [SerializeField] private TMP_Text _wins;
        [SerializeField] private TMP_Text _losses;
        [SerializeField] private Image _classNameIcon;
        [SerializeField] private Image _classIcon;
        [SerializeField] private ClassReference _classReference;
        [SerializeField] private Image _charPhotoSeries;

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
            SetClassNameIcon();
            SetClassIcon();
            SetCharPhotoSeries();


            if (_controller.IsCurrentCharacterLocked())
            {
                _lockImage.gameObject.SetActive(true);
            }
            else
            {
                _lockImage.gameObject.SetActive(false);
            }
        }

        private void SetClassNameIcon()
        {
             CharacterClassType classType = _controller.GetCurrentCharacterClass();
            _classNameIcon.sprite = _classReference.GetNameIcon(classType);
            _classNameIcon.enabled = _classNameIcon.sprite != null;
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
                _characterImageTop.sprite = sprite;
                _characterImageBottom.sprite = sprite;
            }
        }
        private void SetCharacterDescription()
        {
            _characterDescription.SetText(_controller.GetCurrentCharacterDescription());
            _specialAbility.SetText(_controller.GetCurrentCharacterSpecialAbilityDescription());

        }
        private void SetWinsAndLosses()
        {
            _wins.text = _controller.GetCurrentCharacterWins().ToString();
            _losses.text = _controller.GetCurrentCharacterLosses().ToString();
        }

        private void SetClassIcon()
        {
            if (!_classIcon || !_classReference || _controller == null) return;

            CharacterClassType classType = _controller.GetCurrentCharacterClass();
            var icon = _classReference.GetCornerIcon(classType);

            _classIcon.enabled = icon != null;
            if (icon != null) _classIcon.sprite = icon;
        }

        private void SetCharPhotoSeries ()
        {
            _charPhotoSeries.sprite = _controller.GetCurrentCharacterPhotoSeries();
            _charPhotoSeries.enabled = _charPhotoSeries.sprite != null;

        }

        public void ClosePopup()
        {
            gameObject.SetActive(false);
        }
    }
}
