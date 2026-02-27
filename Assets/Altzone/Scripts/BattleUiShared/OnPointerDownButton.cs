/// @file OnPointerDownButton.cs
/// <summary>
/// Contains @cref{OnPointerDownButton} helper class for invoking a UnityEvent on pointer down.
/// </summary>
///
/// This script:<br/>
/// Handles invoking a UnityEvent on pointer down.

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// <span class="brief-h">%OnPointerDownButton <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>.</span><br/>
/// Helper class which handles invoking a UnityEvent on pointer down. Implements IPointerDownHandler interface.
/// </summary>
public class OnPointerDownButton : MonoBehaviour, IPointerDownHandler
{
    /// <value>Public field for the UnityEvent.</value>
    public UnityEvent onClick;

    /// <summary>
    /// Handler method required by IPointerDownHandler interface. Invokes #onClick UnityEvent.
    /// </summary>
    ///
    /// <param name="eventData">The event data.</param>
    public void OnPointerDown(PointerEventData eventData)
    {
        onClick.Invoke();
    }
}
