using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MenuUi.Scripts.CharacterGallery
{
    public class CharacterSlot : MonoBehaviour, IDropHandler
    {
        // Called when an object is dropped onto the character slot
        public void OnDrop(PointerEventData eventData)
        {
            // Check if the character slot is empty
            if (transform.childCount == 0)
            {
                // If it's empty, set the dropped object as a child of this character slot
                GameObject dropped = eventData.pointerDrag;
                DraggableCharacter draggableItem = dropped.GetComponent<DraggableCharacter>();
                draggableItem.parentAfterDrag = transform;
            }

            // If it's not empty, swap the positions of the characters
            else
            {
                
                GameObject dropped = eventData.pointerDrag;
                DraggableCharacter draggableItem = dropped.GetComponent<DraggableCharacter>();

                GameObject current = transform.GetChild(0).gameObject;
                DraggableCharacter currentDraggable = current.GetComponent<DraggableCharacter>();

                currentDraggable.transform.SetParent(draggableItem.parentAfterDrag);
                draggableItem.parentAfterDrag = transform;
            }
        }
            
      
    }
}
