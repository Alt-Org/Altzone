using Altzone.Scripts.Model.Poco.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.CharacterGallery
{
    public class CharacterSlot : SlotBase, IGalleryCharacterData
    {
        [SerializeField] public GalleryCharacter Character;

        private CharacterID _id;
        [SerializeField] private Image _spriteImage;
        [SerializeField] private Image _backgroundSpriteImage;
        [SerializeField] private Image _contentsSpriteImage;
        [SerializeField] private TextMeshProUGUI _nameText;

        public CharacterID Id { get => _id; }

        public void SetInfo(Sprite sprite, Color bgColor, Color bgAltColor, string name, CharacterID id)
        {
            _spriteImage.sprite = sprite;
            _nameText.text = name;
            _id = id;
            _backgroundSpriteImage.color = bgColor;
            //_contentsSpriteImage.color = new Color(bgAltColor.r - 0.4f, bgAltColor.g - 0.4f, bgAltColor.b - 0.4f);
            Character.SetInfo(sprite, bgColor, bgAltColor, name, id, this);
        }
    }
}







