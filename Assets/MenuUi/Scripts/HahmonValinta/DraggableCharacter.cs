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
        // parent transform after drag
        [HideInInspector] public Transform parentAfterDrag; 
        // previous parent transform
        private Transform previousParent; 

        [SerializeField] ModelView _modelView;

        public delegate void ParentChangedEventHandler(Transform newParent);
        // Event triggered when parent changes
        public event ParentChangedEventHandler OnParentChanged;


    public void OnBeginDrag(PointerEventData eventData)
        {
            parentAfterDrag = transform.parent;
            transform.SetParent(transform.parent.parent.parent.parent); 
            transform.SetAsLastSibling(); 
            image.raycastTarget = false; 
            previousParent = transform.parent; 
        }
        

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            transform.SetParent(parentAfterDrag);
            image.raycastTarget = true;
            // check if the parent changed
            if (transform.parent != previousParent)
            {
                previousParent = transform.parent;
                HandleParentChange(previousParent);
            }

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


