/// @file OnPointerDownButton.cs
/// <summary>
/// Has a helper class OnPointerDownButton for invoking a UnityEvent on pointer down.
/// </summary>
///
/// This script:<br/>
/// Handles invoking a UnityEvent on pointer down.

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// Handles invoking a UnityEvent on pointer down.<br/>
/// Implements IPointerDownHandler interface to detect pointer down.
/// </summary>
public class OnPointerDownButton : MonoBehaviour, IPointerDownHandler
{
    /// <value>Public field for the UnityEvent.</value>
    public UnityEvent onClick;

    /// <summary>
    /// Handler method required by IPointerDownHandler interface. Invokes #onClick UnityEvent.
    /// </summary>
    /// <param name="eventData">The event data.</param>
    public void OnPointerDown(PointerEventData eventData)
    {
        onClick.Invoke();
    }
}
