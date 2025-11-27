using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ScrollRect that forwards unused events up to containing objects.
/// BASED ON: The original ScrollRect component supplied by Unity in the UnityEngine.UI namespace
/// INSPIRED BY: https://forum.unity3d.com/threads/nested-scrollrect.268551/#post-1906953
/// </summary>
public class BaseScrollRect : UIBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler, ICanvasElement, ILayoutElement, ILayoutGroup
{
    public enum MovementType
    {
        Unrestricted, // Unrestricted movement -- can scroll forever
        Elastic, // Restricted but flexible -- can go past the edges, but springs back in place
        Clamped, // Restricted movement where it's not possible to go past the edges
    }

    public enum ScrollbarVisibility
    {
        Permanent,
        AutoHide,
        AutoHideAndExpandViewport,
    }

    [Serializable]
    public class ScrollRectEvent : UnityEvent<Vector2> { }

    [SerializeField]
    private RectTransform content;
    public RectTransform Content
    {
        get { return content; }
        set { content = value; }
    }

    [SerializeField]
    private bool horizontallyScrollable = true;
    public virtual bool HorizontallyScrollable
    {
        get { return horizontallyScrollable; }
        set { horizontallyScrollable = value; }
    }

    [SerializeField]
    private bool verticallyScrollable = true;
    public virtual bool VerticallyScrollable
    {
        get { return verticallyScrollable; }
        set { verticallyScrollable = value; }
    }

    [SerializeField]
    private MovementType scrollingMovementType = MovementType.Elastic;
    public MovementType ScrollingMovementType
    {
        get { return scrollingMovementType; }
        set { scrollingMovementType = value; }
    }

    [SerializeField]
    private float elasticity = 0.1f; // Only used for MovementType.Elastic
    public float Elasticity
    {
        get { return elasticity; }
        set { elasticity = value; }
    }

    [SerializeField]
    private bool inertia = true;
    public bool Inertia
    {
        get { return inertia; }
        set { inertia = value; }
    }

    [SerializeField]
    private float decelerationRate = 0.135f; // Only used when inertia is enabled
    public float DecelerationRate
    {
        get { return decelerationRate; }
        set { decelerationRate = value; }
    }

    [SerializeField]
    private float scrollSensitivity = 1.0f;
    public float ScrollSensitivity
    {
        get { return scrollSensitivity; }
        set { scrollSensitivity = value; }
    }

    [SerializeField]
    private RectTransform viewportRectTransform;
    public RectTransform ViewportRectTransform
    {
        get { return viewportRectTransform; }
        set { viewportRectTransform = value; SetDirtyCaching(); }
    }

    [SerializeField]
    private Scrollbar horizontalScrollbar;
    public virtual Scrollbar HorizontalScrollbar
    {
        get
        {
            return horizontalScrollbar;
        }
        set
        {
            if (horizontalScrollbar)
                horizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
            horizontalScrollbar = value;
            if (horizontalScrollbar)
                horizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
            SetDirtyCaching();
        }
    }

    [SerializeField]
    private Scrollbar verticalScrollbar;
    public virtual Scrollbar VerticalScrollbar
    {
        get
        {
            return verticalScrollbar;
        }
        set
        {
            if (verticalScrollbar)
                verticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);
            verticalScrollbar = value;
            if (verticalScrollbar)
                verticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);
            SetDirtyCaching();
        }
    }

    [SerializeField]
    private ScrollbarVisibility horizontalScrollbarVisibility;
    public virtual ScrollbarVisibility HorizontalScrollbarVisibilityMode
    {
        get { return horizontalScrollbarVisibility; }
        set { horizontalScrollbarVisibility = value; SetDirtyCaching(); }
    }

    [SerializeField]
    private ScrollbarVisibility verticalScrollbarVisibility;
    public virtual ScrollbarVisibility VerticalScrollbarVisibilityMode
    {
        get { return verticalScrollbarVisibility; }
        set { verticalScrollbarVisibility = value; SetDirtyCaching(); }
    }

    [SerializeField]
    private float horizontalScrollbarSpacing;
    public virtual float HorizontalScrollbarSpacing
    {
        get { return horizontalScrollbarSpacing; }
        set { horizontalScrollbarSpacing = value; SetDirty(); }
    }

    [SerializeField]
    private float verticalScrollbarSpacing;
    public virtual float VerticalScrollbarSpacing
    {
        get { return verticalScrollbarSpacing; }
        set { verticalScrollbarSpacing = value; SetDirty(); }
    }

    [SerializeField]
    private bool forwardUnusedEventsToContainer;
    public bool ForwardUnusedEventsToContainer
    {
        get { return forwardUnusedEventsToContainer; }
        set { forwardUnusedEventsToContainer = value; }
    }

    [SerializeField]
    private ScrollRectEvent onValueChanged = new ScrollRectEvent();
    public ScrollRectEvent OnValueChanged
    {
        get { return onValueChanged; }
        set { onValueChanged = value; }
    }

    // The offset from handle position to mouse down position
    private Vector2 pointerStartLocalCursor = Vector2.zero;
    private Vector2 contentStartPosition = Vector2.zero;

    private RectTransform viewRect;
    protected RectTransform ViewRect
    {
        get
        {
            if (viewRect == null)
                viewRect = viewportRectTransform;
            if (viewRect == null)
                viewRect = (RectTransform)transform;
            return viewRect;
        }
    }

    private Bounds contentBounds;
    private Bounds viewBounds;

    private Vector2 velocity;
    public Vector2 Velocity
    {
        get { return velocity; }
        set { velocity = value; }
    }

    private bool dragging;

    private Vector2 prevPosition = Vector2.zero;
    private Bounds prevContentBounds;
    private Bounds prevViewBounds;
    [NonSerialized]
    private bool hasRebuiltLayout = false;

    private bool hSliderExpand;
    private bool vSliderExpand;
    private float hSliderHeight;
    private float vSliderWidth;

    [NonSerialized]
    private RectTransform rectTransform;
    private RectTransform RectTransform
    {
        get
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
            return rectTransform;
        }
    }

    private RectTransform horizontalScrollbarRect;
    private RectTransform verticalScrollbarRect;

    private DrivenRectTransformTracker tracker;

    private IInitializePotentialDragHandler parentInitializePotentialDragHandler;
    private IBeginDragHandler parentBeginDragHandler;
    private IDragHandler parentDragHandler;
    private IEndDragHandler parentEndDragHandler;
    private IScrollHandler parentScrollHandler;

    private bool routeToParent;

    private bool m_DraggingByTouch;

    protected BaseScrollRect()
    {
        flexibleWidth = -1;
    }

    public virtual void Rebuild(CanvasUpdate executing)
    {
        if (executing == CanvasUpdate.Prelayout)
        {
            UpdateCachedData();
        }

        if (executing == CanvasUpdate.PostLayout)
        {
            UpdateBounds();
            UpdateScrollbars(Vector2.zero);
            UpdatePrevData();

            hasRebuiltLayout = true;
        }
    }

    public virtual void LayoutComplete()
    { }

    public virtual void GraphicUpdateComplete()
    { }

    private void UpdateCachedData()
    {
        Transform transform = this.transform;
        horizontalScrollbarRect = horizontalScrollbar == null ? null : horizontalScrollbar.transform as RectTransform;
        verticalScrollbarRect = verticalScrollbar == null ? null : verticalScrollbar.transform as RectTransform;

        // These are true if either the elements are children, or they don't exist at all.
        bool viewIsChild = (ViewRect.parent == transform);
        bool hScrollbarIsChild = (!horizontalScrollbarRect || horizontalScrollbarRect.parent == transform);
        bool vScrollbarIsChild = (!verticalScrollbarRect || verticalScrollbarRect.parent == transform);
        bool allAreChildren = (viewIsChild && hScrollbarIsChild && vScrollbarIsChild);

        hSliderExpand = allAreChildren && horizontalScrollbarRect && HorizontalScrollbarVisibilityMode == ScrollbarVisibility.AutoHideAndExpandViewport;
        vSliderExpand = allAreChildren && verticalScrollbarRect && VerticalScrollbarVisibilityMode == ScrollbarVisibility.AutoHideAndExpandViewport;

        hSliderHeight = (horizontalScrollbarRect == null ? 0 : horizontalScrollbarRect.rect.height);
        vSliderWidth = (verticalScrollbarRect == null ? 0 : verticalScrollbarRect.rect.width);
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

    protected override void OnTransformParentChanged()
    {
        base.OnTransformParentChanged();
        CacheParentContainerComponents();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (horizontalScrollbar)
            horizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
        if (verticalScrollbar)
            verticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);

        CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
        UpdateCachedData();
        CacheParentContainerComponents();
    }

    protected override void OnDisable()
    {
        CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

        if (horizontalScrollbar)
            horizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
        if (verticalScrollbar)
            verticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);

        hasRebuiltLayout = false;
        tracker.Clear();
        velocity = Vector2.zero;
        LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
        UpdateCachedData();
        base.OnDisable();
    }

    public override bool IsActive()
    {
        return base.IsActive() && content != null;
    }

    private void EnsureLayoutHasRebuilt()
    {
        if (!hasRebuiltLayout && !CanvasUpdateRegistry.IsRebuildingLayout())
            Canvas.ForceUpdateCanvases();
    }

    public virtual void StopMovement()
    {
        velocity = Vector2.zero;
    }

    public virtual void OnInitializePotentialDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        velocity = Vector2.zero;

        // Always route initialize potential drag event to parent
        if (parentInitializePotentialDragHandler != null)
            parentInitializePotentialDragHandler.OnInitializePotentialDrag(eventData);
    }

    private void EvaluateRouteToParent(Vector2 delta, bool isXInverted, bool isYInverted)
    {
        routeToParent = false;

        if (!forwardUnusedEventsToContainer)
            return;

        if (Math.Abs(delta.x) > Math.Abs(delta.y))
        {
            if (horizontallyScrollable)
            {
                if (!HScrollingNeeded)
                    routeToParent = true;
                else if (HorizontalNormalizedPosition == 0 && ((delta.x > 0 && isXInverted) || (delta.x < 0 && !isXInverted)))
                    routeToParent = true;
                else if (HorizontalNormalizedPosition == 1 && ((delta.x < 0 && isXInverted) || (delta.x > 0 && !isXInverted)))
                    routeToParent = true;
            }
            else
                routeToParent = true;
        }
        else if (Math.Abs(delta.x) < Math.Abs(delta.y))
        {
            if (verticallyScrollable)
            {
                if (!VScrollingNeeded)
                    routeToParent = true;
                else if (VerticalNormalizedPosition == 0 && ((delta.y > 0 && isYInverted) || (delta.y < 0 && !isYInverted)))
                    routeToParent = true;
                else if (VerticalNormalizedPosition == 1 && ((delta.y < 0 && isYInverted) || (delta.y > 0 && !isYInverted)))
                    routeToParent = true;
            }
            else
                routeToParent = true;
        }
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        EvaluateRouteToParent(eventData.delta, true, true);

        if (routeToParent && parentBeginDragHandler != null)
            parentBeginDragHandler.OnBeginDrag(eventData);
        else
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            m_DraggingByTouch = eventData.pointerId != -1;

            UpdateBounds();

            pointerStartLocalCursor = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(ViewRect, eventData.position, eventData.pressEventCamera, out pointerStartLocalCursor);
            contentStartPosition = content.anchoredPosition;
            dragging = true;
        }
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (routeToParent && parentEndDragHandler != null)
            parentEndDragHandler.OnEndDrag(eventData);
        else
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            dragging = false;
        }

        routeToParent = false;
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (routeToParent && parentDragHandler != null)
            parentDragHandler.OnDrag(eventData);
        else
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(ViewRect, eventData.position, eventData.pressEventCamera, out localCursor))
                return;

            UpdateBounds();

            var pointerDelta = localCursor - pointerStartLocalCursor;
            Vector2 position = contentStartPosition + pointerDelta;

            // Offset to get content into place in the view.
            Vector2 offset = CalculateOffset(position - content.anchoredPosition);
            position += offset;
            if (scrollingMovementType == MovementType.Elastic)
            {
                if (offset.x != 0)
                    position.x = position.x - RubberDelta(offset.x, viewBounds.size.x);
                if (offset.y != 0)
                    position.y = position.y - RubberDelta(offset.y, viewBounds.size.y);
            }

            SetContentAnchoredPosition(position);
        }
    }

    public virtual void OnScroll(PointerEventData eventData)
    {
        EvaluateRouteToParent(eventData.scrollDelta, false, false);

        if (routeToParent && parentScrollHandler != null)
            parentScrollHandler.OnScroll(eventData);
        else
        {
            if (!IsActive())
                return;

            EnsureLayoutHasRebuilt();
            UpdateBounds();

            Vector2 delta = eventData.scrollDelta;
            // Down is positive for scroll events, while in UI system up is positive.
            delta.y *= -1;
            if (VerticallyScrollable && !HorizontallyScrollable)
            {
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    delta.y = delta.x;
                delta.x = 0;
            }
            if (HorizontallyScrollable && !VerticallyScrollable)
            {
                if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
                    delta.x = delta.y;
                delta.y = 0;
            }

            Vector2 position = content.anchoredPosition;
            position += delta * scrollSensitivity;
            if (scrollingMovementType == MovementType.Clamped)
                position += CalculateOffset(position - content.anchoredPosition);

            SetContentAnchoredPosition(position);
            UpdateBounds();
        }
    }

    protected virtual void SetContentAnchoredPosition(Vector2 position)
    {
        if (!horizontallyScrollable)
            position.x = content.anchoredPosition.x;
        if (!verticallyScrollable)
            position.y = content.anchoredPosition.y;

        if (position != content.anchoredPosition)
        {
            content.anchoredPosition = position;
            UpdateBounds();
        }
    }

    protected virtual void LateUpdate()
    {
        if (!content)
            return;

        EnsureLayoutHasRebuilt();
        UpdateScrollbarVisibility();
        UpdateBounds();
        float deltaTime = Time.unscaledDeltaTime;
        Vector2 offset = CalculateOffset(Vector2.zero);
        if (!dragging && (offset != Vector2.zero || velocity != Vector2.zero))
        {
            Vector2 position = content.anchoredPosition;
            for (int axis = 0; axis < 2; axis++)
            {
                // Apply spring physics if movement is elastic and content has an offset from the view.
                if (scrollingMovementType == MovementType.Elastic && offset[axis] != 0)
                {
                    float speed = velocity[axis];
                    position[axis] = Mathf.SmoothDamp(content.anchoredPosition[axis], content.anchoredPosition[axis] + offset[axis], ref speed, elasticity, Mathf.Infinity, deltaTime);
                    velocity[axis] = speed;
                }
                // Else move content according to velocity with deceleration applied.
                else if (inertia)
                {
                    velocity[axis] *= Mathf.Pow(decelerationRate, deltaTime);
                    if (Mathf.Abs(velocity[axis]) < 1)
                        velocity[axis] = 0;
                    position[axis] += velocity[axis] * deltaTime;
                }
                // If we have neither elaticity or friction, there shouldn't be any velocity.
                else
                {
                    velocity[axis] = 0;
                }
            }

            if (velocity != Vector2.zero)
            {
                if (scrollingMovementType == MovementType.Clamped)
                {
                    offset = CalculateOffset(position - content.anchoredPosition);
                    position += offset;
                }

                SetContentAnchoredPosition(position);
            }
        }

        if (dragging && inertia)
        {
            Vector3 newVelocity = (content.anchoredPosition - prevPosition) / deltaTime;
            velocity = m_DraggingByTouch ? newVelocity : Vector3.Lerp(velocity, newVelocity, deltaTime * 10);
        }

        if (viewBounds != prevViewBounds || contentBounds != prevContentBounds || content.anchoredPosition != prevPosition)
        {
            UpdateScrollbars(offset);
            onValueChanged.Invoke(NormalizedPosition);
            UpdatePrevData();
        }
    }

    private void UpdatePrevData()
    {
        if (content == null)
            prevPosition = Vector2.zero;
        else
            prevPosition = content.anchoredPosition;
        prevViewBounds = viewBounds;
        prevContentBounds = contentBounds;
    }

    private void UpdateScrollbars(Vector2 offset)
    {
        if (horizontalScrollbar)
        {
            if (contentBounds.size.x > 0)
                horizontalScrollbar.size = Mathf.Clamp01((viewBounds.size.x - Mathf.Abs(offset.x)) / contentBounds.size.x);
            else
                horizontalScrollbar.size = 1;

            horizontalScrollbar.value = HorizontalNormalizedPosition;
        }

        if (verticalScrollbar)
        {
            if (contentBounds.size.y > 0)
                verticalScrollbar.size = Mathf.Clamp01((viewBounds.size.y - Mathf.Abs(offset.y)) / contentBounds.size.y);
            else
                verticalScrollbar.size = 1;

            verticalScrollbar.value = VerticalNormalizedPosition;
        }
    }

    public Vector2 NormalizedPosition
    {
        get
        {
            return new Vector2(HorizontalNormalizedPosition, VerticalNormalizedPosition);
        }
        set
        {
            SetNormalizedPosition(value.x, 0);
            SetNormalizedPosition(value.y, 1);
        }
    }

    public float HorizontalNormalizedPosition
    {
        get
        {
            UpdateBounds();
            if (contentBounds.size.x <= viewBounds.size.x)
                return (viewBounds.min.x > contentBounds.min.x) ? 1 : 0;
            return (viewBounds.min.x - contentBounds.min.x) / (contentBounds.size.x - viewBounds.size.x);
        }
        set
        {
            SetNormalizedPosition(value, 0);
        }
    }

    public float VerticalNormalizedPosition
    {
        get
        {
            UpdateBounds();
            if (contentBounds.size.y <= viewBounds.size.y)
                return (viewBounds.min.y > contentBounds.min.y) ? 1 : 0;
            ;
            return (viewBounds.min.y - contentBounds.min.y) / (contentBounds.size.y - viewBounds.size.y);
        }
        set
        {
            SetNormalizedPosition(value, 1);
        }
    }

    private void SetHorizontalNormalizedPosition(float value) { SetNormalizedPosition(value, 0); }
    private void SetVerticalNormalizedPosition(float value) { SetNormalizedPosition(value, 1); }

    private void SetNormalizedPosition(float value, int axis)
    {
        EnsureLayoutHasRebuilt();
        UpdateBounds();
        // How much the content is larger than the view.
        float hiddenLength = contentBounds.size[axis] - viewBounds.size[axis];
        // Where the position of the lower left corner of the content bounds should be, in the space of the view.
        float contentBoundsMinPosition = viewBounds.min[axis] - value * hiddenLength;
        // The new content localPosition, in the space of the view.
        float newLocalPosition = content.localPosition[axis] + contentBoundsMinPosition - contentBounds.min[axis];

        Vector3 localPosition = content.localPosition;
        if (Mathf.Abs(localPosition[axis] - newLocalPosition) > 0.01f)
        {
            localPosition[axis] = newLocalPosition;
            content.localPosition = localPosition;
            velocity[axis] = 0;
            UpdateBounds();
        }
    }

    private static float RubberDelta(float overStretching, float viewSize)
    {
        return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
    }

    protected override void OnRectTransformDimensionsChange()
    {
        SetDirty();
    }

    private bool HScrollingNeeded
    {
        get
        {
            if (Application.isPlaying)
                return contentBounds.size.x > viewBounds.size.x + 0.01f;
            return true;
        }
    }
    private bool VScrollingNeeded
    {
        get
        {
            if (Application.isPlaying)
                return contentBounds.size.y > viewBounds.size.y + 0.01f;
            return true;
        }
    }

    public virtual void CalculateLayoutInputHorizontal() { }
    public virtual void CalculateLayoutInputVertical() { }

    public virtual float minWidth { get { return -1; } }
    public virtual float preferredWidth { get { return -1; } }
    public virtual float flexibleWidth { get; private set; }

    public virtual float minHeight { get { return -1; } }
    public virtual float preferredHeight { get { return -1; } }
    public virtual float flexibleHeight { get { return -1; } }

    public virtual int layoutPriority { get { return -1; } }

    public virtual void SetLayoutHorizontal()
    {
        tracker.Clear();

        if (hSliderExpand || vSliderExpand)
        {
            tracker.Add(this, ViewRect,
                DrivenTransformProperties.Anchors |
                DrivenTransformProperties.SizeDelta |
                DrivenTransformProperties.AnchoredPosition);

            // Make view full size to see if content fits.
            ViewRect.anchorMin = Vector2.zero;
            ViewRect.anchorMax = Vector2.one;
            ViewRect.sizeDelta = Vector2.zero;
            ViewRect.anchoredPosition = Vector2.zero;

            // Recalculate content layout with this size to see if it fits when there are no scrollbars.
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            viewBounds = new Bounds(ViewRect.rect.center, ViewRect.rect.size);
            contentBounds = GetBounds();
        }

        // If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
        if (vSliderExpand && VScrollingNeeded)
        {
            ViewRect.sizeDelta = new Vector2(-(vSliderWidth + verticalScrollbarSpacing), ViewRect.sizeDelta.y);

            // Recalculate content layout with this size to see if it fits vertically
            // when there is a vertical scrollbar (which may reflowed the content to make it taller).
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            viewBounds = new Bounds(ViewRect.rect.center, ViewRect.rect.size);
            contentBounds = GetBounds();
        }

        // If it doesn't fit horizontally, enable horizontal scrollbar and shrink view vertically to make room for it.
        if (hSliderExpand && HScrollingNeeded)
        {
            ViewRect.sizeDelta = new Vector2(ViewRect.sizeDelta.x, -(hSliderHeight + horizontalScrollbarSpacing));
            viewBounds = new Bounds(ViewRect.rect.center, ViewRect.rect.size);
            contentBounds = GetBounds();
        }

        // If the vertical slider didn't kick in the first time, and the horizontal one did,
        // we need to check again if the vertical slider now needs to kick in.
        // If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
        if (vSliderExpand && VScrollingNeeded && ViewRect.sizeDelta.x == 0 && ViewRect.sizeDelta.y < 0)
        {
            ViewRect.sizeDelta = new Vector2(-(vSliderWidth + verticalScrollbarSpacing), ViewRect.sizeDelta.y);
        }
    }

    public virtual void SetLayoutVertical()
    {
        UpdateScrollbarLayout();
        viewBounds = new Bounds(ViewRect.rect.center, ViewRect.rect.size);
        contentBounds = GetBounds();
    }

    private void UpdateScrollbarVisibility()
    {
        if (verticalScrollbar && verticalScrollbarVisibility != ScrollbarVisibility.Permanent && verticalScrollbar.gameObject.activeSelf != VScrollingNeeded)
            verticalScrollbar.gameObject.SetActive(VScrollingNeeded);

        if (horizontalScrollbar && horizontalScrollbarVisibility != ScrollbarVisibility.Permanent && horizontalScrollbar.gameObject.activeSelf != HScrollingNeeded)
            horizontalScrollbar.gameObject.SetActive(HScrollingNeeded);
    }

    private void UpdateScrollbarLayout()
    {
        if (vSliderExpand && horizontalScrollbar)
        {
            tracker.Add(this, horizontalScrollbarRect,
                DrivenTransformProperties.AnchorMinX |
                DrivenTransformProperties.AnchorMaxX |
                DrivenTransformProperties.SizeDeltaX |
                DrivenTransformProperties.AnchoredPositionX);
            horizontalScrollbarRect.anchorMin = new Vector2(0, horizontalScrollbarRect.anchorMin.y);
            horizontalScrollbarRect.anchorMax = new Vector2(1, horizontalScrollbarRect.anchorMax.y);
            horizontalScrollbarRect.anchoredPosition = new Vector2(0, horizontalScrollbarRect.anchoredPosition.y);
            if (VScrollingNeeded)
                horizontalScrollbarRect.sizeDelta = new Vector2(-(vSliderWidth + verticalScrollbarSpacing), horizontalScrollbarRect.sizeDelta.y);
            else
                horizontalScrollbarRect.sizeDelta = new Vector2(0, horizontalScrollbarRect.sizeDelta.y);
        }

        if (hSliderExpand && verticalScrollbar)
        {
            tracker.Add(this, verticalScrollbarRect,
                DrivenTransformProperties.AnchorMinY |
                DrivenTransformProperties.AnchorMaxY |
                DrivenTransformProperties.SizeDeltaY |
                DrivenTransformProperties.AnchoredPositionY);
            verticalScrollbarRect.anchorMin = new Vector2(verticalScrollbarRect.anchorMin.x, 0);
            verticalScrollbarRect.anchorMax = new Vector2(verticalScrollbarRect.anchorMax.x, 1);
            verticalScrollbarRect.anchoredPosition = new Vector2(verticalScrollbarRect.anchoredPosition.x, 0);
            if (HScrollingNeeded)
                verticalScrollbarRect.sizeDelta = new Vector2(verticalScrollbarRect.sizeDelta.x, -(hSliderHeight + horizontalScrollbarSpacing));
            else
                verticalScrollbarRect.sizeDelta = new Vector2(verticalScrollbarRect.sizeDelta.x, 0);
        }
    }

    private void UpdateBounds()
    {
        viewBounds = new Bounds(ViewRect.rect.center, ViewRect.rect.size);
        contentBounds = GetBounds();

        if (content == null)
            return;

        // Make sure content bounds are at least as large as view by adding padding if not.
        // One might think at first that if the content is smaller than the view, scrolling should be allowed.
        // However, that's not how scroll views normally work.
        // Scrolling is *only* possible when content is *larger* than view.
        // We use the pivot of the content rect to decide in which directions the content bounds should be expanded.
        // E.g. if pivot is at top, bounds are expanded downwards.
        // This also works nicely when ContentSizeFitter is used on the content.
        Vector3 contentSize = contentBounds.size;
        Vector3 contentPos = contentBounds.center;
        Vector3 excess = viewBounds.size - contentSize;
        if (excess.x > 0)
        {
            contentPos.x -= excess.x * (content.pivot.x - 0.5f);
            contentSize.x = viewBounds.size.x;
        }
        if (excess.y > 0)
        {
            contentPos.y -= excess.y * (content.pivot.y - 0.5f);
            contentSize.y = viewBounds.size.y;
        }

        contentBounds.size = contentSize;
        contentBounds.center = contentPos;
    }

    private readonly Vector3[] m_Corners = new Vector3[4];
    private Bounds GetBounds()
    {
        if (content == null)
            return new Bounds();

        var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        var toLocal = ViewRect.worldToLocalMatrix;
        content.GetWorldCorners(m_Corners);
        for (int j = 0; j < 4; j++)
        {
            Vector3 v = toLocal.MultiplyPoint3x4(m_Corners[j]);
            vMin = Vector3.Min(v, vMin);
            vMax = Vector3.Max(v, vMax);
        }

        var bounds = new Bounds(vMin, Vector3.zero);
        bounds.Encapsulate(vMax);
        return bounds;
    }

    private Vector2 CalculateOffset(Vector2 delta)
    {
        Vector2 offset = Vector2.zero;
        if (scrollingMovementType == MovementType.Unrestricted)
            return offset;

        Vector2 min = contentBounds.min;
        Vector2 max = contentBounds.max;

        if (horizontallyScrollable)
        {
            min.x += delta.x;
            max.x += delta.x;
            if (min.x > viewBounds.min.x)
                offset.x = viewBounds.min.x - min.x;
            else if (max.x < viewBounds.max.x)
                offset.x = viewBounds.max.x - max.x;
        }

        if (verticallyScrollable)
        {
            min.y += delta.y;
            max.y += delta.y;
            if (max.y < viewBounds.max.y)
                offset.y = viewBounds.max.y - max.y;
            else if (min.y > viewBounds.min.y)
                offset.y = viewBounds.min.y - min.y;
        }

        return offset;
    }

    protected void SetDirty()
    {
        if (!IsActive())
            return;

        LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
    }

    protected void SetDirtyCaching()
    {
        if (!IsActive())
            return;

        CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
        LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        SetDirtyCaching();
    }
#endif
}
