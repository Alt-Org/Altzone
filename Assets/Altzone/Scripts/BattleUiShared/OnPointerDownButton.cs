using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OnPointerDownButton : MonoBehaviour, IPointerDownHandler
{
    public UnityEvent onClick;

    public void OnPointerDown(PointerEventData eventData)
    {
        onClick.Invoke();
    }
}
