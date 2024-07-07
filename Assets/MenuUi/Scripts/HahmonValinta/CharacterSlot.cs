using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MenuUi.Scripts.CharacterGallery
{
    public class CharacterSlot : MonoBehaviour, IDropHandler
    {
        [SerializeField] public DraggableCharacter _character;

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

                currentDraggable.transform.SetParent(draggableItem.parentAfterDrag);
                draggableItem.parentAfterDrag = transform;
            }
        }
    }
}







