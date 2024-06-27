using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ExitGames.Client.Photon;

namespace MenuUi.Scripts.CharacterGallery
{
    public class DraggableCharacter : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image image;

        private Button button;
        private ColorBlock originalColors;

        [HideInInspector] public Transform parentAfterDrag;
        private Transform previousParent;

        public Transform allowedSlot;
        public Transform initialSlot;

        [SerializeField] ModelView _modelView;

        public delegate void ParentChangedEventHandler(Transform newParent);
        public event ParentChangedEventHandler OnParentChanged;

        private void Start()
        {
            button = GetComponent<Button>();
            originalColors = button.colors;

            if (initialSlot == null)
            {
                initialSlot = transform.parent;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            parentAfterDrag = transform.parent;
            previousParent = transform.parent.parent.parent;
            transform.SetParent(transform.parent.parent.parent.parent);
            transform.SetAsLastSibling();
            image.raycastTarget = false;

            button.interactable = false;

            // Set the button colors to make the background transparent during dragging
            ColorBlock transparentColors = originalColors;
            transparentColors.disabledColor = new Color(0, 0, 0, 0); 
            button.colors = transparentColors;

            GameObject.Find("EventSystem").GetComponent<EventSystem>().SetSelectedGameObject(null);
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position;
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
                transform.SetParent(initialSlot);
                transform.position = initialSlot.position;
            }
            else
            {
                transform.SetParent(droppedSlot);
                transform.position = droppedSlot.position;
            }

            image.raycastTarget = true;
            button.interactable = true;

            // Reset the button colors to the original colors
            button.colors = originalColors;

            if (transform.parent != previousParent)
            {
                previousParent = transform.parent;
                HandleParentChange(previousParent);
            }
        }

        private void HandleParentChange(Transform newParent)
        {
            if (newParent == _modelView._CurSelectedCharacterSlot[0].transform)
            {
                OnParentChanged?.Invoke(newParent);
            }
        }
    }
}










