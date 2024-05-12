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


namespace MenuUi.Scripts.CharacterGallery
{
    // <summary>
    // Manages slots for draggableitems, DraggableCharacter.cs
    // </summary>
    public class CharacterSlot : MonoBehaviour, IDropHandler
    {
        [SerializeField] private bool isCharacterGoingToBattleSelectionSlot = false;
        public void OnDrop(PointerEventData eventData)
        {
            if (transform.childCount == 0 && isCharacterGoingToBattleSelectionSlot)
            {
                GameObject dropped = eventData.pointerDrag;
                DraggableCharacter draggableItem = dropped.GetComponent<DraggableCharacter>();
                draggableItem.parentAfterDrag = transform;
            }
            else if (isCharacterGoingToBattleSelectionSlot)
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
