using UnityEngine.UI;
using UnityEngine;
using TMPro;
using MenuUi.Scripts.Signals;

namespace MenuUi.Scripts.CharacterGallery
{
    public class SelectedCharSlot : SlotBase
    {
        [SerializeField] private Image _characterHeadImage;
        [SerializeField] private TMP_Text _className;

        private void Awake()
        {
            if (_slotButton != null) _slotButton.onClick.AddListener(SignalBus.OnDefenceGalleryEditPanelRequestedSignal);
        }

        void OnEnable()
        {
            SetCharacterHeadImage();
            SetClassName();
        }

        private void SetCharacterHeadImage()
        {
            Sprite sprite = null;

            if (sprite != null)
            {
                _characterHeadImage.sprite = sprite;
            }
        }
        private void SetClassName()
        {
            _className.text = "";
        }
    }
}
