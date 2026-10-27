using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndDropSpot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("on drop");
        if (eventData.pointerDrag != null)
        {
            eventData.pointerDrag.transform.position = transform.position;
        }
    }
}
