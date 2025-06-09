using UnityEngine.UI;
using UnityEngine;
using TMPro;
using MenuUi.Scripts.Signals;
using Altzone.Scripts.Model.Poco.Game;
using MenuUi.Scripts.DefenceScreen.CharacterGallery;

namespace MenuUi.Scripts.CharacterGallery
{
    public class SelectedCharSlot : SlotBase, IGalleryCharacterData
    {
        [SerializeField] private Image _characterImage;
        [SerializeField] private TMP_Text _className;
        [SerializeField] private TMP_Text _characterNameText;

        [SerializeField] private Image _backgroundLowerImage;
        [SerializeField] private Image _backgroundUpperImage;

        [SerializeField] private PieChartPreview _pieChartPreview;

        [SerializeField] private Button _editingPopupButton;

        [SerializeField] private GameObject _characterCard;

        private CharacterID _id;
        public CharacterID Id => _id;

        private void Awake()
        {
            if (_editingPopupButton != null) _editingPopupButton.onClick.AddListener(SignalBus.OnDefenceGalleryEditPanelRequestedSignal);
        }

        private void OnDestroy()
        {
            _editingPopupButton.onClick.RemoveAllListeners();
        }

        public void SetInfo(CustomCharacter customCharacter, Sprite sprite, Color bgColor, Color bgAltColor, string name, string className)
        {
            _characterImage.sprite = sprite;
            _characterNameText.text = name;
            _className.text = className;
            _backgroundLowerImage.color = bgColor;
            _backgroundUpperImage.color = bgAltColor;
            _pieChartPreview.UpdateChart(customCharacter.Id);
            _id = customCharacter.Id;
        }

        public void SetCharacterVisibility(bool visible)
        {
           _characterCard.SetActive(visible);
           _slotButton.enabled = visible;
        }
    }
}
