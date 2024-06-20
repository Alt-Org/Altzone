using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MenuUi.Scripts.CharacterGallery
{
    public class DraggableCharacter : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        // Image to be dragged
        [SerializeField] private Image image;
        // Parent tranform after drag
        [HideInInspector] public Transform parentAfterDrag;
        // Previous parent transform
        private Transform previousParent;

        public Transform allowedSlot;
        // Starting slot
        public Transform initialSlot; 

        [SerializeField] ModelView _modelView;

        public delegate void ParentChangedEventHandler(Transform newParent);
        // Event triggered when parent changes
        public event ParentChangedEventHandler OnParentChanged;

        private void Start()
        {
            // Set starting slot if null
            if (initialSlot == null)
            {
                initialSlot = transform.parent;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            parentAfterDrag = transform.parent;
            transform.SetParent(transform.parent.parent.parent);
            transform.SetAsLastSibling();
            image.raycastTarget = false;
            previousParent = transform.parent;
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position;
            GetComponent<Button>().enabled = false;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Transform droppedSlot = null;

            if (eventData.pointerEnter != null)
            {
                CharacterSlot characterSlot = eventData.pointerEnter.GetComponent<CharacterSlot>();
                if (characterSlot != null)
                {
                    droppedSlot = characterSlot.transform;
                }
            }

            if (droppedSlot == null || (droppedSlot != allowedSlot && droppedSlot.tag != "Topslot"))
            {
                // If no slot is allowed, return to the original slot
                transform.SetParent(initialSlot);
                transform.position = initialSlot.position;
            }
            else
            {
                // If slot is allowed, move here
                transform.SetParent(droppedSlot);
                transform.position = droppedSlot.position;
            }

            image.raycastTarget = true;

            if (transform.parent != previousParent)
            {
                previousParent = transform.parent;
                HandleParentChange(previousParent);
            }

            GetComponent<Button>().enabled = true;
        }

        private void HandleParentChange(Transform newParent)
        {
            // Check if the new parent is the first slot of the horizontal character slot
            if (newParent == _modelView._CurSelectedCharacterSlot[0].transform)
            {
                // Change parent
                OnParentChanged?.Invoke(newParent);
            }
        }
    }
}




