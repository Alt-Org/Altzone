using UnityEngine;
using UnityEngine.UI;

using TMPro;

using Altzone.Scripts.Model.Poco.Game;

namespace MenuUi.Scripts.CharacterGallery
{
    /// <summary>
    /// Handles the visual functionality of CharacterSlot.
    /// Has a reference to GalleryCharacter, and the info to GalleryCharacter is also set through SetInfo function.
    /// Inherits SlotBase for editing selected characters.
    /// </summary>
    public class CharacterSlot : SlotBase, IGalleryCharacterData
    {
        [SerializeField] public GalleryCharacter Character;

        private CharacterID _id;
        [SerializeField] private Image _spriteImage;
        [SerializeField] private Image _backgroundLowerImage;
        [SerializeField] private Image _backgroundUpperImage;
        [SerializeField] private TextMeshProUGUI _nameText;

        public CharacterID Id { get => _id; }
        [HideInInspector] public bool IsLocked = false;
        public void SetInfo(Sprite sprite, Color bgColor, Color bgAltColor, string name, CharacterID id)
        {
            _spriteImage.sprite = sprite;
            _nameText.text = name;
            _id = id;
            _backgroundLowerImage.color = bgColor;
            _backgroundUpperImage.color = new Color(bgAltColor.r - 0.4f, bgAltColor.g - 0.4f, bgAltColor.b - 0.4f);
            Character.SetInfo(sprite, bgColor, bgAltColor, name, id, this);
        }
    }
}
