using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ScrollLocalManager : UIBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler, ICanvasElement
{
    public enum MovementType
    {
        Unrestricted, // Unrestricted movement -- can scroll forever
        Elastic, // Restricted but flexible -- can go past the edges, but springs back in place
        Clamped, // Restricted movement where it's not possible to go past the edges
    }

    [Serializable] public class ScrollRectEvent : UnityEvent<Vector2> { }

    [SerializeField] private MovementType _scrollingMovementType = MovementType.Elastic;
    [SerializeField] private float _elasticity = 0.1f; // Only used for MovementType.Elastic
    [SerializeField] private bool _inertia = true;
    [SerializeField] private float _decelerationRate = 0.135f; // Only used when inertia is enabled
    [SerializeField] private float _scrollSensitivity = 1.0f;
    [SerializeField] private ScrollRectEvent _onValueChanged = new ScrollRectEvent();
    [SerializeField] private List<BaseScrollRectVariant> _baseScrollRectVariants = new List<BaseScrollRectVariant>();

    private Vector2 _pointerStartLocalCursor = Vector2.zero;
    private List<Vector2> _contentStartPosition = new List<Vector2>();
    private List<Vector2> _velocities;

    private bool _routeToParent;
    private List<bool> _DraggingByTouch = new List<bool>();
    private bool _dragging;

    private IInitializePotentialDragHandler _parentInitializePotentialDragHandler;
    private IBeginDragHandler _parentBeginDragHandler;
    private IDragHandler _parentDragHandler;
    private IEndDragHandler _parentEndDragHandler;
    private IScrollHandler _parentScrollHandler;

    private List<int> _activeScrollIndexes = new List<int>();
    [SerializeField] private RectTransform _pointerRect;

    protected override void Awake()
    {
        base.Awake();
        _velocities = new List<Vector2>();
        
        foreach (var baseScroll in _baseScrollRectVariants)
        {
            _velocities.Add(Vector2.zero);
            _contentStartPosition.Add(Vector2.zero);
            _DraggingByTouch.Add(false);
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
        CacheParentContainerComponents();
    }

    protected override void OnDisable()
    {
        CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

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
            Vector2 offset = _baseScrollRectVariants[i].CalculateOffset(Vector2.zero, _scrollingMovementType);

            if (!_dragging && (offset != Vector2.zero || _velocities[i] != Vector2.zero))
            {
                Vector2 position = _baseScrollRectVariants[i].Content.anchoredPosition;
                for (int axis = 0; axis < 2; axis++)
                {
                    Vector2 vector2 = _velocities[i];
                    // Apply spring physics if movement is elastic and content has an offset from the view.
                    if (_scrollingMovementType == MovementType.Elastic && offset[axis] != 0)
                    {
                        float speed = _velocities[i][axis];
                        position[axis] = Mathf.SmoothDamp(
                            _baseScrollRectVariants[i].Content.anchoredPosition[axis],
                            _baseScrollRectVariants[i].Content.anchoredPosition[axis] + offset[axis],
                            ref speed,
                            _elasticity,
                            Mathf.Infinity,
                            deltaTime);

                        vector2[axis] = speed;
                        _velocities[i] = vector2;
                    }
                    // Else move content according to velocity with deceleration applied.
                    else if (_inertia)
                    {
                        vector2[axis] *= Mathf.Pow(_decelerationRate, deltaTime);
                        _velocities[i] = vector2;

                        if (Mathf.Abs(_velocities[i][axis]) < 1)
                        {
                            vector2[axis] = 0;
                            _velocities[i] = vector2;
                        }

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
                    if (_scrollingMovementType == MovementType.Clamped)
                    {
                        offset = _baseScrollRectVariants[i].CalculateOffset(position - _baseScrollRectVariants[i].Content.anchoredPosition, _scrollingMovementType);
                        position += offset;
                    }

                    _baseScrollRectVariants[i].SetContentAnchoredPosition(position);
                }
            }

            if (_dragging && _inertia)
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

    #region Drag

    /// <summary>
    /// Returns list of indexes (used to get <c>_baseScrollRectVariants</c>)<br/>
    /// where the pointer is contained by the content rects.
    /// </summary>
    public List<int> GetSelectedBaseScrollVariants(PointerEventData eventData)
    {
        List<int> indexes = new List<int>();

        Vector2 pointerResult = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(this.transform as RectTransform, eventData.position, Camera.current, out pointerResult);
        
        _pointerRect.anchoredPosition = pointerResult; //TEST

        for (int i = 0; i < _baseScrollRectVariants.Count; i++)
        {
            Vector2 rectResult = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(this.transform as RectTransform, _baseScrollRectVariants[i].Content.position, eventData.pressEventCamera, out rectResult);

            Vector2 size = Vector2.Scale(_baseScrollRectVariants[i].Content.rect.size, _baseScrollRectVariants[i].Content.transform.lossyScale);
            Rect rect = new Rect(rectResult - (size * 0.5f), size);

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

        //// Always route initialize potential drag event to parent
        //if (_parentInitializePotentialDragHandler != null)
        //    _parentInitializePotentialDragHandler.OnInitializePotentialDrag(eventData);
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        _activeScrollIndexes = GetSelectedBaseScrollVariants(eventData);

        foreach (int index in _activeScrollIndexes)
        {
            _baseScrollRectVariants[index].EvaluateRouteToParent(eventData.delta, true, true);

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
        if (_routeToParent && _parentEndDragHandler != null)
            _parentEndDragHandler.OnEndDrag(eventData);
        else
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            _dragging = false;
        }

        _activeScrollIndexes.Clear();
        _routeToParent = false;
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        foreach (var index in _activeScrollIndexes)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            Vector2 localCursor;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(this.transform as RectTransform, eventData.position, eventData.pressEventCamera, out localCursor))
                return;

            _baseScrollRectVariants[index].UpdateBounds();

            var pointerDelta = localCursor - _pointerStartLocalCursor;
            Vector2 position = _contentStartPosition[index] + pointerDelta;

            // Offset to get content into place in the view.
            Vector2 offset = _baseScrollRectVariants[index].CalculateOffset(position - _baseScrollRectVariants[index].Content.anchoredPosition, _scrollingMovementType);
            position += offset;

            if (_scrollingMovementType == MovementType.Elastic)
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
            _baseScrollRectVariants[index].EvaluateRouteToParent(eventData.scrollDelta, false, false);

            if (!IsActive())
                return;

            _baseScrollRectVariants[index].Scroll(eventData.scrollDelta, _scrollSensitivity, _scrollingMovementType);
        }
    }

    #endregion

    private void CacheParentContainerComponents()
    {
        _parentInitializePotentialDragHandler = GetComponentOnlyInParents<IInitializePotentialDragHandler>();
        _parentBeginDragHandler = GetComponentOnlyInParents<IBeginDragHandler>();
        _parentDragHandler = GetComponentOnlyInParents<IDragHandler>();
        _parentEndDragHandler = GetComponentOnlyInParents<IEndDragHandler>();
        _parentScrollHandler = GetComponentOnlyInParents<IScrollHandler>();
    }

    private T GetComponentOnlyInParents<T>()
    {
        if (transform.parent != null)
            return transform.parent.GetComponentInParent<T>();

        return default(T);
    }

    private static float RubberDelta(float overStretching, float viewSize)
    {
        return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
    }

    public virtual void Rebuild(CanvasUpdate executing)
    {

    }

    public virtual void LayoutComplete()
    {

    }

    public virtual void GraphicUpdateComplete()
    {

    }
}
