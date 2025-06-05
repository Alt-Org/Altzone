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

        [SerializeField] private Image _backgroundLowerImage;
        [SerializeField] private Image _backgroundUpperImage;

        [SerializeField] private PieChartPreview _pieChartPreview;

        [SerializeField] private GameObject _characterCard;
        private void Awake()
        {
            if (_slotButton != null) _slotButton.onClick.AddListener(SignalBus.OnDefenceGalleryEditPanelRequestedSignal);
        }

        public void SetInfo(Sprite sprite, Color bgColor, Color bgAltColor, string name, string className, CharacterID id)
        {
            _characterImage.sprite = sprite;
            _characterNameText.text = name;
            _className.text = className;
            _backgroundLowerImage.color = bgColor;
            _backgroundUpperImage.color = bgAltColor;
            _pieChartPreview.UpdateChart(id);
        }

        public void SetCharacterVisibility(bool visible)
        {
           _characterCard.SetActive(visible);
        }
    }
}
