using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
///!------------NOTE------------!<br/>
///This script controlls<br/>
/// <c>BaseScrollRectVariant.cs</c><br/>
/// scripts!<br/>
///!------------------------------!<br/><br/>
/// Used to get free 2 dimensional movement.<br/>
/// BASED ON: <c>BaseScrollRect.cs</c> script.
/// </summary>
public class ScrollLocalManager : UIBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler
{
    #region Variables
    [Serializable] public class ScrollRectEvent : UnityEvent<Vector2> { }

    [SerializeField] private ScrollRectEvent _onValueChanged = new ScrollRectEvent();
    [SerializeField] private List<BaseScrollRectVariant> _baseScrollRectVariants = new List<BaseScrollRectVariant>();

    private Vector2 _pointerStartLocalCursor = Vector2.zero;
    private List<Vector2> _contentStartPosition = new List<Vector2>();
    private List<Vector2> _velocities;

    private List<bool> _DraggingByTouch = new List<bool>();
    private bool _dragging;

    private List<int> _activeScrollIndexes = new List<int>();

    private IInitializePotentialDragHandler parentInitializePotentialDragHandler;
    private IBeginDragHandler parentBeginDragHandler;
    private IDragHandler parentDragHandler;
    private IEndDragHandler parentEndDragHandler;
    private IScrollHandler parentScrollHandler;

    private List<bool> _routeToParent;

    [SerializeField]
    private bool _forwardUnusedEventsToContainer;
    public bool ForwardUnusedEventsToContainer
    {
        get { return _forwardUnusedEventsToContainer; }
        set { _forwardUnusedEventsToContainer = value; }
    }

    [SerializeField]
    private bool _horizontallyScrollable = true;
    public bool HorizontallyScrollable
    {
        get { return _horizontallyScrollable; }
        set { _horizontallyScrollable = value; }
    }

    [SerializeField]
    private bool _verticallyScrollable = true;
    public bool VerticallyScrollable
    {
        get { return _verticallyScrollable; }
        set { _verticallyScrollable = value; }
    }

    #endregion

    protected override void Awake()
    {
        base.Awake();
        _velocities = new List<Vector2>();
        _routeToParent = new List<bool>();
        
        foreach (var baseScroll in _baseScrollRectVariants)
        {
            _velocities.Add(Vector2.zero);
            _contentStartPosition.Add(Vector2.zero);
            _DraggingByTouch.Add(false);
            _routeToParent.Add(false);
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        CacheParentContainerComponents();
    }

    protected override void OnDisable()
    {
        for (int i = 0; i < _velocities.Count; i++)
            _velocities[i] = Vector2.zero;

        base.OnDisable();
    }

    protected override void OnTransformParentChanged()
    {
        base.OnTransformParentChanged();
        CacheParentContainerComponents();
    }

    protected virtual void LateUpdate()
    {
        if (_baseScrollRectVariants.Count == 0)
            return;

        for (int i = 0; i < _baseScrollRectVariants.Count; i++)
        {
            _baseScrollRectVariants[i].EnsureLayoutHasRebuilt();
            _baseScrollRectVariants[i].UpdateBounds();

            float deltaTime = Time.unscaledDeltaTime;
            Vector2 offset = _baseScrollRectVariants[i].CalculateOffset(Vector2.zero);

            if (!_dragging && (offset != Vector2.zero || _velocities[i] != Vector2.zero))
            {
                Vector2 position = _baseScrollRectVariants[i].Content.anchoredPosition;
                for (int axis = 0; axis < 2; axis++)
                {
                    Vector2 vector2 = _velocities[i];
                    // Apply spring physics if movement is elastic and content has an offset from the view.
                    if (_baseScrollRectVariants[i].ScrollingMovementType == BaseScrollRectVariant.MovementType.Elastic && offset[axis] != 0)
                    {
                        float speed = _velocities[i][axis];
                        position[axis] = Mathf.SmoothDamp(
                            _baseScrollRectVariants[i].Content.anchoredPosition[axis],
                            _baseScrollRectVariants[i].Content.anchoredPosition[axis] + offset[axis],
                            ref speed,
                            _baseScrollRectVariants[i].Elasticity,
                            Mathf.Infinity,
                            deltaTime);

                        vector2[axis] = speed;
                        _velocities[i] = vector2;
                    }
                    // Else move content according to velocity with deceleration applied.
                    else if (_baseScrollRectVariants[i].Inertia)
                    {
                        vector2[axis] *= Mathf.Pow(_baseScrollRectVariants[i].DecelerationRate, deltaTime);

                        if (Mathf.Abs(vector2[axis]) < 1)
                            vector2[axis] = 0;

                        _velocities[i] = vector2;
                        position[axis] += _velocities[i][axis] * deltaTime;
                    }
                    // If we have neither elaticity or friction, there shouldn't be any velocity.
                    else
                    {
                        vector2[axis] = 0;
                        _velocities[i] = vector2;
                    }
                }

                if (_velocities[i] != Vector2.zero)
                {
                    if (_baseScrollRectVariants[i].ScrollingMovementType == BaseScrollRectVariant.MovementType.Clamped)
                        position += _baseScrollRectVariants[i].CalculateOffset(position - _baseScrollRectVariants[i].Content.anchoredPosition);

                    _baseScrollRectVariants[i].SetContentAnchoredPosition(position);
                }
            }

            if (_dragging && _baseScrollRectVariants[i].Inertia)
            {
                Vector3 newVelocity = (_baseScrollRectVariants[i].Content.anchoredPosition - _baseScrollRectVariants[i].PreviousPosition) / deltaTime;
                _velocities[i] = _DraggingByTouch[i] ? newVelocity : Vector3.Lerp(_velocities[i], newVelocity, deltaTime * 10);
            }

            if (_baseScrollRectVariants[i].ViewBounds != _baseScrollRectVariants[i].PreviousViewBounds ||
                _baseScrollRectVariants[i].ContentBounds != _baseScrollRectVariants[i].PreviousContentBounds ||
                _baseScrollRectVariants[i].Content.anchoredPosition != _baseScrollRectVariants[i].PreviousPosition)
            {
                _onValueChanged.Invoke(_baseScrollRectVariants[i].NormalizedPosition);
                _baseScrollRectVariants[i].UpdatePrevData();
            }
        }
    }

    private T GetComponentOnlyInParents<T>()
    {
        if (transform.parent != null)
            return transform.parent.GetComponentInParent<T>();

        return default(T);
    }

    private void CacheParentContainerComponents()
    {
        parentInitializePotentialDragHandler = GetComponentOnlyInParents<IInitializePotentialDragHandler>();
        parentBeginDragHandler = GetComponentOnlyInParents<IBeginDragHandler>();
        parentDragHandler = GetComponentOnlyInParents<IDragHandler>();
        parentEndDragHandler = GetComponentOnlyInParents<IEndDragHandler>();
        parentScrollHandler = GetComponentOnlyInParents<IScrollHandler>();
    }

    private void EvaluateRouteToParent(Vector2 delta, bool isXInverted, bool isYInverted, int index)
    {
        _routeToParent[index] = false;

        if (!_forwardUnusedEventsToContainer)
            return;
        Debug.LogError("asdasda");
        if (Math.Abs(delta.x) > Math.Abs(delta.y))
        {
            if (_horizontallyScrollable)
            {
                if ((!_baseScrollRectVariants[index].HScrollingNeeded) ||
                    (_baseScrollRectVariants[index].HorizontalNormalizedPosition == 0 && ((delta.x > 0 && isXInverted) || (delta.x < 0 && !isXInverted))) ||
                    (_baseScrollRectVariants[index].HorizontalNormalizedPosition == 1 && ((delta.x < 0 && isXInverted) || (delta.x > 0 && !isXInverted))))
                    _routeToParent[index] = true;
            }
            else
                _routeToParent[index] = true;
        }
        else if (Math.Abs(delta.x) < Math.Abs(delta.y))
        {
            if (_verticallyScrollable)
            {
                if ((!_baseScrollRectVariants[index].VScrollingNeeded) ||
                    (_baseScrollRectVariants[index].VerticalNormalizedPosition == 0 && ((delta.y > 0 && isYInverted) || (delta.y < 0 && !isYInverted))) ||
                    (_baseScrollRectVariants[index].VerticalNormalizedPosition == 1 && ((delta.y < 0 && isYInverted) || (delta.y > 0 && !isYInverted))))
                    _routeToParent[index] = true;
            }
            else
                _routeToParent[index] = true;
        }
    }

    #region Drag

    /// <summary>
    /// Returns list of indexes (used to get <c>_baseScrollRectVariants</c>)<br/>
    /// where the pointer is contained by the content rects.
    /// </summary>
    public List<int> GetSelectedBaseScrollVariants(PointerEventData eventData)
    {
        List<int> indexes = new List<int>();

        Vector2 pointerResult = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(this.transform as RectTransform, eventData.position, eventData.pressEventCamera, out pointerResult);

        for (int i = 0; i < _baseScrollRectVariants.Count; i++)
        {
            Vector2 rectResult = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(this.transform as RectTransform, _baseScrollRectVariants[i].Content.position, eventData.pressEventCamera, out rectResult);

            Vector2 size = Vector2.Scale(_baseScrollRectVariants[i].Content.rect.size, _baseScrollRectVariants[i].Content.transform.lossyScale);
            Vector2 rectPivot = _baseScrollRectVariants[i].Content.pivot;
            Rect rect = new Rect(rectResult - new Vector2(size.x * rectPivot.x, size.y * rectPivot.y), size);

            if (rect.Contains(pointerResult))
                indexes.Add(i);
        }

        return (indexes);
    }

    public virtual void OnInitializePotentialDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        for (int i = 0; i < _velocities.Count; i++)
            _velocities[i] = Vector2.zero;
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        _activeScrollIndexes = GetSelectedBaseScrollVariants(eventData);

        foreach (int index in _activeScrollIndexes)
        {
            EvaluateRouteToParent(eventData.delta, true, true, index);

            if (_routeToParent[index] && parentBeginDragHandler != null)
            {
                parentBeginDragHandler.OnBeginDrag(eventData);
                continue;
            }

            if (!IsActive() || (eventData.button != PointerEventData.InputButton.Left))
                return;

            _DraggingByTouch[index] = eventData.pointerId != -1;
            _baseScrollRectVariants[index].UpdateBounds();
            _pointerStartLocalCursor = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(this.transform as RectTransform, eventData.position, eventData.pressEventCamera, out _pointerStartLocalCursor);
            _contentStartPosition[index] = _baseScrollRectVariants[index].Content.anchoredPosition;
            _dragging = true;
        }
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {

        foreach (int index in _activeScrollIndexes)
        {
            if (_routeToParent[index] && parentEndDragHandler != null)
            {
                parentBeginDragHandler.OnBeginDrag(eventData);
                return;
            }
            else
            {
                if (eventData.button != PointerEventData.InputButton.Left)
                    return;

                _dragging = false;
            }

            _routeToParent[index] = false;
        }

        _activeScrollIndexes.Clear();
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        foreach (var index in _activeScrollIndexes)
        {
            if (_routeToParent[index] && parentDragHandler != null)
            {
                parentBeginDragHandler.OnBeginDrag(eventData);
                continue;
            }

            if ((eventData.button != PointerEventData.InputButton.Left) || !IsActive())
                return;

            Vector2 localCursor;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(this.transform as RectTransform, eventData.position, eventData.pressEventCamera, out localCursor))
                return;

            _baseScrollRectVariants[index].UpdateBounds();
            Vector2 pointerDelta = localCursor - _pointerStartLocalCursor;
            Vector2 position = _contentStartPosition[index] + pointerDelta;

            // Offset to get content into place in the view.
            Vector2 offset = _baseScrollRectVariants[index].CalculateOffset(position - _baseScrollRectVariants[index].Content.anchoredPosition);
            position += offset;

            if (_baseScrollRectVariants[index].ScrollingMovementType == BaseScrollRectVariant.MovementType.Elastic)
            {
                if (offset.x != 0)
                    position.x = position.x - RubberDelta(offset.x, _baseScrollRectVariants[index].ViewBounds.size.x);
                if (offset.y != 0)
                    position.y = position.y - RubberDelta(offset.y, _baseScrollRectVariants[index].ViewBounds.size.y);
            }

            _baseScrollRectVariants[index].SetContentAnchoredPosition(position);
        }
    }

    public virtual void OnScroll(PointerEventData eventData)
    {
        foreach (var index in _activeScrollIndexes)
        {
            EvaluateRouteToParent(eventData.delta, false, false, index);

            if (_routeToParent[index] && parentScrollHandler != null)
            {
                parentBeginDragHandler.OnBeginDrag(eventData);
                return;
            }

            if (!IsActive())
                return;

            _baseScrollRectVariants[index].Scroll(eventData.scrollDelta);
        }
    }

    #endregion

    private static float RubberDelta(float overStretching, float viewSize)
    {
        return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
    }
}
