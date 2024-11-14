using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MenuUi.Scripts.CharacterGallery
{
    public class CharacterSlot : MonoBehaviour, IGalleryCharacterData, IDropHandler
    {
        [SerializeField] public DraggableCharacter _character;

        private CharacterID _id;
        [SerializeField] private Image _spriteImage;
        [SerializeField] private TextMeshProUGUI _nameText;

        public CharacterID Id { get => _id; }

        // Called when an object is dropped onto the character slot
        public void OnDrop(PointerEventData eventData)
        {
            // Check that dropped object is DraggableCharacter
            GameObject dropped = eventData.pointerDrag;
            DraggableCharacter draggableItem = dropped.GetComponent<DraggableCharacter>();

            if (draggableItem == null)
            {
                return;
            }

            // Check if this slot is allowed slot
            if (draggableItem.allowedSlot != transform && transform.tag != "Topslot")
            {
                // If not, let OnEndDrag handle the return
                return;
            }

            // If this slot is allowed
            if (transform.childCount == 0)
            {
                // If the slot is empty, set the dropped object as a child of this slot
                draggableItem.parentAfterDrag = transform;
            }
            else
            {
                // If the slot is not null, switch the characters
                GameObject current = transform.GetChild(0).gameObject;
                DraggableCharacter currentDraggable = current.GetComponent<DraggableCharacter>();              
            }
            ModelView modelView = GetComponent<ModelView>();
            modelView.CheckSelectedCharacterSlotText();
        }

        public void SetCharacterDown()
        {
            GameObject current = transform.GetChild(0).gameObject;
            DraggableCharacter currentCharacter = current.GetComponent<DraggableCharacter>();

            Transform currentInitialSlot = currentCharacter.initialSlot;
            currentCharacter.transform.SetParent(currentInitialSlot);
            currentCharacter.transform.position = currentInitialSlot.position;
            ModelView modelView = GetComponent<ModelView>();
            modelView.CheckSelectedCharacterSlotText();
        }

        public void SetInfo(Sprite sprite, string name, CharacterID id, ModelView view)
        {
            _spriteImage.sprite = sprite;
            _nameText.text = name;
            _id = id;
            _character.SetInfo(sprite, name, id, view);
        }
    }
}







