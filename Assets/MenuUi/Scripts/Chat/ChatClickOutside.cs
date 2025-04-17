using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChatClickOutside : MonoBehaviour, IPointerClickHandler
{
    [Header("Chat Reference")]
    [SerializeField] private Chat _chatScript;

    public void OnPointerClick(PointerEventData eventData)
    {
        // Checks for clicks outside of the quick message and send button panels. Closes them if so.
        if (!((PointerEventData)eventData).pointerCurrentRaycast.gameObject.Equals(_chatScript.quickMessages) && !((PointerEventData)eventData).pointerCurrentRaycast.gameObject.Equals(_chatScript.sendButtons))
        {
            _chatScript.MinimizeOptions();
        }

        if (_chatScript.selectedMessage != null)
        {
            // Checks for clicks outside of the selected message. Deselects the selected message if so.
            if (!((PointerEventData)eventData).pointerCurrentRaycast.gameObject.Equals(_chatScript.selectedMessage))
            {
                _chatScript.DeselectMessage(_chatScript.selectedMessage);
            }
        }

    }
}
