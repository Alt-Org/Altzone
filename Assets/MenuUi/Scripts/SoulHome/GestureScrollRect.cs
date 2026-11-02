using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ScrollRect that asks <see cref="CanBeginDrag"/> before it starts dragging, so the same pointer can either scroll
/// the list or be used for something else (like dragging furniture out of the tray). Only one pointer drags at a time.
/// </summary>
public class GestureScrollRect : ScrollRect
{
    /// <summary>Called when a pointer is pressed down on the scroll view, before any drag has started.</summary>
    public event Action<PointerEventData> PotentialDragStarted;
    /// <summary>Return false to ignore the drag. Called once per drag, after the EventSystem drag threshold is passed.</summary>
    public Func<PointerEventData, bool> CanBeginDrag;

    private int? _dragPointerId;
    private PointerEventData _dragEventData;

    public bool IsDragging => _dragPointerId.HasValue;

    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        if (_dragPointerId.HasValue) return; //A second finger must not stop the current drag.
        PotentialDragStarted?.Invoke(eventData);
        base.OnInitializePotentialDrag(eventData);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (_dragPointerId.HasValue || eventData.button != PointerEventData.InputButton.Left) return;
        if (CanBeginDrag != null && !CanBeginDrag(eventData)) return;

        _dragPointerId = eventData.pointerId;
        _dragEventData = eventData;
        base.OnBeginDrag(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (_dragPointerId != eventData.pointerId) return;
        base.OnDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (_dragPointerId != eventData.pointerId) return;
        _dragPointerId = null;
        _dragEventData = null;
        base.OnEndDrag(eventData);
    }

    /// <summary>Ends the current drag and stops the movement. The remaining drag events of that pointer are ignored.</summary>
    public void CancelDrag()
    {
        if (_dragEventData != null) base.OnEndDrag(_dragEventData);
        _dragPointerId = null;
        _dragEventData = null;
        StopMovement();
    }

    protected override void OnDisable()
    {
        CancelDrag();
        base.OnDisable();
    }
}
