using UnityEngine.UI;
using UnityEngine;
using TMPro;
using MenuUi.Scripts.Signals;
using Altzone.Scripts.Model.Poco.Game;
using MenuUi.Scripts.DefenceScreen.CharacterGallery;

namespace MenuUi.Scripts.CharacterGallery
{
    public class SelectedCharSlot : SlotBase
    {
        [SerializeField] private Image _characterImage;
        [SerializeField] private TMP_Text _className;
        [SerializeField] private TMP_Text _characterNameText;

        [Space, SerializeField] private Image _backgroundLowerImage;
        [SerializeField] private Image _backgroundUpperImage;

        [Space, SerializeField] private PieChartPreview _pieChartPreview;

        [Space, SerializeField] private Button _statPopupButton;

        [Space, SerializeField] private GameObject _characterCard;
        [SerializeField] private TMP_Text _hpText;
        [SerializeField] private TMP_Text _impactForceText;
        [SerializeField] private TMP_Text _speedText;
        [SerializeField] private TMP_Text _defenceText;
        [SerializeField] private TMP_Text _charSizeText;

        private void Awake()
        {
            if (_slotButton != null) _slotButton.onClick.AddListener(SignalBus.OnDefenceGalleryEditPanelRequestedSignal);
        }

        private void OnDestroy()
        {
            _slotButton.onClick.RemoveAllListeners();
        }

        public void SetInfo(CustomCharacter customCharacter, Sprite sprite, Color bgColor, Color bgAltColor, string name, string className)
        {
            _characterImage.sprite = sprite;
            _characterNameText.text = name;
            _className.text = className;
            _backgroundLowerImage.color = bgColor;
            _backgroundUpperImage.color = bgAltColor;
            if (customCharacter != null)
            {
                _pieChartPreview.UpdateChart(customCharacter.Id);
                _id = customCharacter.Id;

                _hpText.text = customCharacter.Hp.ToString();
                _impactForceText.text = customCharacter.Attack.ToString();
                _speedText.text = customCharacter.Speed.ToString();
                _defenceText.text = customCharacter.Defence.ToString();
                _charSizeText.text = customCharacter.CharacterSize.ToString();
            }
        }

        public void SetCharacterVisibility(bool visible)
        {
           _characterCard.SetActive(visible);
           _statPopupButton.gameObject.SetActive(visible);
        }
    }
}
