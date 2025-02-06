using Altzone.Scripts.Model.Poco.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.CharacterGallery
{
    public class CharacterSlot : MonoBehaviour, IGalleryCharacterData
    {
        [SerializeField] public GalleryCharacter Character;

        private CharacterID _id;
        [SerializeField] private Image _spriteImage;
        [SerializeField] private Image _backgroundSpriteImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Animator _animator;

        public CharacterID Id { get => _id; }

        public void SetInfo(Sprite sprite, Color bgColor, Color bgAltColor, string name, CharacterID id)
        {
            _spriteImage.sprite = sprite;
            _nameText.text = name;
            _id = id;
            _backgroundSpriteImage.color = new Color(bgColor.r - 0.5f, bgColor.g - 0.5f, bgColor.b - 0.5f);
            Character.SetInfo(sprite, bgColor, bgAltColor, name, id, this);
        }

        public void PlaySelectableAnimation()
        {
            _animator.Play("SelectableAnimation", -1, 0f);
        }
    }
}







