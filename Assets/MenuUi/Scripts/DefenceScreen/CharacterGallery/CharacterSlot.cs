using Altzone.Scripts.Model.Poco.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.CharacterGallery
{
    public class CharacterSlot : MonoBehaviour, IGalleryCharacterData
    {
        [SerializeField] public DraggableCharacter _character;

        private CharacterID _id;
        [SerializeField] private Image _spriteImage;
        [SerializeField] private Image _backgroundSpriteImage;
        [SerializeField] private TextMeshProUGUI _nameText;

        public CharacterID Id { get => _id; }

        public void SetInfo(Sprite sprite, string name, CharacterID id, ModelView view)
        {
            _spriteImage.sprite = sprite;
            _nameText.text = name;
            _id = id;
            _character.SetInfo(sprite, name, id, view);
        }
    }
}







