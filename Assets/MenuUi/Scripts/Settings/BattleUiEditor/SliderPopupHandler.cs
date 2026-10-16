using System;
using UnityEngine;
using UnityEngine.EventSystems;


public class SliderPopupHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public event Action<Transform, Transform, Transform> DragStarted;
    public event Action DragEnded;
    [SerializeField] private Transform _valueField;
    [SerializeField] private Transform _title;

    public void OnPointerDown(PointerEventData eventData)
    {
        DragStarted?.Invoke(transform, _valueField, _title);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        DragEnded?.Invoke();
    }
}
