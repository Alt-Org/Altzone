using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;


public class SliderPopupHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public delegate void HideOtherComponents(Transform transform, Transform valueField, Transform title);
    public static event HideOtherComponents OnHideOtherComponents;

    public delegate void RestoreOtherComponents();
    public static event RestoreOtherComponents OnRestoreOtherComponents;

    [SerializeField] private Transform _valueField;
    [SerializeField] private Transform _title;

    public void OnPointerDown(PointerEventData eventData)
    {
        OnHideOtherComponents?.Invoke(transform, _valueField, _title);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnRestoreOtherComponents?.Invoke();
    }
}
