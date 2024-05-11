using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Game;
using System.Linq;
using Altzone.Scripts;
using Prg.Scripts.Common.Photon;
using UnityEngine.TextCore.Text;


namespace Battle0.Scripts.Lobby.InChooseModel
{
    // <summary>
    // Allows Dragging characters and tells modelview script if the character has changed
    // </summary>
    public class DraggableCharacter : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image image;
        [HideInInspector] public Transform parentAfterDrag;
        private Transform previousParent;

        [SerializeField] ModelView _modelView;

        public delegate void ParentChangedEventHandler(Transform newParent);
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


