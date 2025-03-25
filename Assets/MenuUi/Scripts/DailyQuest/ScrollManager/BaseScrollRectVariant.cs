using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BaseScrollRectVariant : UIBehaviour, ICanvasElement, ILayoutElement, ILayoutGroup
{
    [SerializeField] private bool _horizontallyScrollable = true;
    [SerializeField] private bool _verticallyScrollable = true;
    [SerializeField] private bool _forwardUnusedEventsToContainer;
    [SerializeField] private RectTransform _content;
    [SerializeField] private RectTransform _viewportRectTransform;

    private bool _hasRebuiltLayout = false;

    private RectTransform _viewRect;
    public RectTransform ViewRect
    {
        get
        {
            if (_viewRect == null)
                _viewRect = _viewportRectTransform;
            if (_viewRect == null)
                _viewRect = (RectTransform)transform;
            return _viewRect;
        }
    }

    [NonSerialized]
    private RectTransform _rectTransform;
    public RectTransform RectTransform
    {
        get
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();
            return _rectTransform;
        }
    }

    private Vector2 _previousPosition = Vector2.zero;

    private Bounds _viewBounds;
    private Bounds _contentBounds;
    private Bounds _previousViewBounds;
    private Bounds _previousContentBounds;

    protected BaseScrollRectVariant()
    {
        flexibleWidth = -1;
    }

    public Vector2 PreviousPosition { get { return _previousPosition; } }
    public Bounds ViewBounds { get { return _viewBounds; } }
    public Bounds ContentBounds { get { return _contentBounds; } }
    public Bounds PreviousViewBounds { get { return _previousViewBounds; } }
    public Bounds PreviousContentBounds { get { return _previousContentBounds; } }
    public RectTransform Content { get { return _content; } }

    #region Useless but mandatory.

    public virtual void LayoutComplete()
    { }

    public virtual void GraphicUpdateComplete()
    { }

    public virtual void SetLayoutHorizontal()
    {
        //_tracker.Clear();

        //if (_hSliderExpand || _vSliderExpand)
        //{
        //_tracker.Add(this, ViewRect,
        //    DrivenTransformProperties.Anchors |
        //    DrivenTransformProperties.SizeDelta |
        //    DrivenTransformProperties.AnchoredPosition);

        //// Make view full size to see if content fits.
        //ViewRect.anchorMin = Vector2.zero;
        //ViewRect.anchorMax = Vector2.one;
        //ViewRect.sizeDelta = Vector2.zero;
        //ViewRect.anchoredPosition = Vector2.zero;

        //// Recalculate content layout with this size to see if it fits when there are no scrollbars.
        //LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
        //_viewBounds = new Bounds(ViewRect.rect.center, ViewRect.rect.size);
        //_contentBounds = GetBounds();
        //}

        // If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
        //if (_vSliderExpand && VScrollingNeeded)
        //{
        //    ViewRect.sizeDelta = new Vector2(-(_vSliderWidth/* + verticalScrollbarSpacing*/), ViewRect.sizeDelta.y);

        //    // Recalculate content layout with this size to see if it fits vertically
        //    // when there is a vertical scrollbar (which may reflowed the content to make it taller).
        //    LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
        //    _viewBounds = new Bounds(ViewRect.rect.center, ViewRect.rect.size);
        //    _contentBounds = GetBounds();
        //}

        //// If it doesn't fit horizontally, enable horizontal scrollbar and shrink view vertically to make room for it.
        //if (_hSliderExpand && HScrollingNeeded)
        //{
        //    ViewRect.sizeDelta = new Vector2(ViewRect.sizeDelta.x, -(_hSliderHeight/* + horizontalScrollbarSpacing*/));
        //    _viewBounds = new Bounds(ViewRect.rect.center, ViewRect.rect.size);
        //    _contentBounds = GetBounds();
        //}

        //// If the vertical slider didn't kick in the first time, and the horizontal one did,
        //// we need to check again if the vertical slider now needs to kick in.
        //// If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
        //if (_vSliderExpand && VScrollingNeeded && ViewRect.sizeDelta.x == 0 && ViewRect.sizeDelta.y < 0)
        //{
        //    ViewRect.sizeDelta = new Vector2(-(_vSliderWidth/* + verticalScrollbarSpacing*/), ViewRect.sizeDelta.y);
        //}
    }

    #endregion

    #region Base methods

    protected override void OnEnable()
    {
        base.OnEnable();
        CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
    }

    protected override void OnDisable()
    {
        CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);
        _hasRebuiltLayout = false;
        LayoutRebuilder.MarkLayoutForRebuild(_rectTransform);
        base.OnDisable();
    }

    public override bool IsActive()
    {
        return base.IsActive() && _content != null;
    }

    protected override void OnRectTransformDimensionsChange()
    {
        SetDirty();
    }

    #endregion

    public virtual void Rebuild(CanvasUpdate executing)
    {
        if (executing == CanvasUpdate.PostLayout)
        {
            UpdateBounds();
            UpdatePrevData();
            _hasRebuiltLayout = true;
        }
    }

    public void EnsureLayoutHasRebuilt()
    {
        if (!_hasRebuiltLayout && !CanvasUpdateRegistry.IsRebuildingLayout())
            Canvas.ForceUpdateCanvases();
    }

    public virtual void SetContentAnchoredPosition(Vector2 position)
    {
        if (!_horizontallyScrollable)
            position.x = _content.anchoredPosition.x;
        if (!_verticallyScrollable)
            position.y = _content.anchoredPosition.y;

        if (position != _content.anchoredPosition)
        {
            _content.anchoredPosition = position;
            UpdateBounds();
        }
    }

    public bool EvaluateRouteToParent(Vector2 delta, bool isXInverted, bool isYInverted)
    {
        if (!_forwardUnusedEventsToContainer)
            return (false);

        if (Math.Abs(delta.x) > Math.Abs(delta.y))
        {
            if (_horizontallyScrollable)
                if (!HScrollingNeeded ||
                    (HorizontalNormalizedPosition == 0 && ((delta.x > 0 && isXInverted) || (delta.x < 0 && !isXInverted))) ||
                    (HorizontalNormalizedPosition == 1 && ((delta.x < 0 && isXInverted) || (delta.x > 0 && !isXInverted))))
                    return (true);//routeToParent = true;
            else
                return (true);
        }
        else if (Math.Abs(delta.x) < Math.Abs(delta.y))
        {
            if (_verticallyScrollable)
                if (!VScrollingNeeded ||
                    (VerticalNormalizedPosition == 0 && ((delta.y > 0 && isYInverted) || (delta.y < 0 && !isYInverted))) ||
                    (VerticalNormalizedPosition == 1 && ((delta.y < 0 && isYInverted) || (delta.y > 0 && !isYInverted))))
                    return (true);
            else
                return (true);
        }

        return (false);
    }

    public void UpdatePrevData()
    {
        if (_content == null)
            _previousPosition = Vector2.zero;
        else
            _previousPosition = _content.anchoredPosition;
        _previousViewBounds = _viewBounds;
        _previousContentBounds = _contentBounds;
    }

    public float HorizontalNormalizedPosition
    {
        get
        {
            UpdateBounds();
            if (_contentBounds.size.x <= _viewBounds.size.x)
                return (_viewBounds.min.x > _contentBounds.min.x) ? 1 : 0;
            return (_viewBounds.min.x - _contentBounds.min.x) / (_contentBounds.size.x - _viewBounds.size.x);
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
            if (_contentBounds.size.y <= _viewBounds.size.y)
                return (_viewBounds.min.y > _contentBounds.min.y) ? 1 : 0;
            ;
            return (_viewBounds.min.y - _contentBounds.min.y) / (_contentBounds.size.y - _viewBounds.size.y);
        }
        set
        {
            SetNormalizedPosition(value, 1);
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

    private void SetNormalizedPosition(float value, int axis)
    {
        EnsureLayoutHasRebuilt();
        UpdateBounds();
        // How much the content is larger than the view.
        float hiddenLength = _contentBounds.size[axis] - _viewBounds.size[axis];
        // Where the position of the lower left corner of the content bounds should be, in the space of the view.
        float contentBoundsMinPosition = _viewBounds.min[axis] - value * hiddenLength;
        // The new content localPosition, in the space of the view.
        float newLocalPosition = _content.localPosition[axis] + contentBoundsMinPosition - _contentBounds.min[axis];

        Vector3 localPosition = _content.localPosition;
        if (Mathf.Abs(localPosition[axis] - newLocalPosition) > 0.01f)
        {
            localPosition[axis] = newLocalPosition;
            _content.localPosition = localPosition;
            UpdateBounds();
        }
    }

    private bool HScrollingNeeded
    {
        get
        {
            if (Application.isPlaying)
                return _contentBounds.size.x > _viewBounds.size.x + 0.01f;
            return true;
        }
    }
    private bool VScrollingNeeded
    {
        get
        {
            if (Application.isPlaying)
                return _contentBounds.size.y > _viewBounds.size.y + 0.01f;
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

    public virtual void SetLayoutVertical()
    {
        //UpdateScrollbarLayout();
        _viewBounds = new Bounds(ViewRect.rect.center, ViewRect.rect.size);
        _contentBounds = GetBounds();
    }

    public void UpdateBounds()
    {
        _viewBounds = new Bounds(ViewRect.rect.center, ViewRect.rect.size);
        _contentBounds = GetBounds();

        if (_content == null)
            return;

        // Make sure content bounds are at least as large as view by adding padding if not.
        // One might think at first that if the content is smaller than the view, scrolling should be allowed.
        // However, that's not how scroll views normally work.
        // Scrolling is *only* possible when content is *larger* than view.
        // We use the pivot of the content rect to decide in which directions the content bounds should be expanded.
        // E.g. if pivot is at top, bounds are expanded downwards.
        // This also works nicely when ContentSizeFitter is used on the content.
        Vector3 contentSize = _contentBounds.size;
        Vector3 contentPos = _contentBounds.center;
        Vector3 excess = _viewBounds.size - contentSize;
        if (excess.x > 0)
        {
            contentPos.x -= excess.x * (_content.pivot.x - 0.5f);
            contentSize.x = _viewBounds.size.x;
        }
        if (excess.y > 0)
        {
            contentPos.y -= excess.y * (_content.pivot.y - 0.5f);
            contentSize.y = _viewBounds.size.y;
        }

        _contentBounds.size = contentSize;
        _contentBounds.center = contentPos;
    }

    private readonly Vector3[] m_Corners = new Vector3[4];

    private Bounds GetBounds()
    {
        if (_content == null)
            return new Bounds();

        var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        var toLocal = ViewRect.worldToLocalMatrix;

        _content.GetWorldCorners(m_Corners);

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

    public Vector2 CalculateOffset(Vector2 delta, ScrollLocalManager.MovementType scrollingMovementType)
    {
        Vector2 offset = Vector2.zero;

        if (scrollingMovementType == ScrollLocalManager.MovementType.Unrestricted)
            return offset;

        Vector2 min = _contentBounds.min;
        Vector2 max = _contentBounds.max;

        if (_horizontallyScrollable)
        {
            min.x += delta.x;
            max.x += delta.x;
            if (min.x > _viewBounds.min.x)
                offset.x = _viewBounds.min.x - min.x;
            else if (max.x < _viewBounds.max.x)
                offset.x = _viewBounds.max.x - max.x;
        }

        if (_verticallyScrollable)
        {
            min.y += delta.y;
            max.y += delta.y;
            if (max.y < _viewBounds.max.y)
                offset.y = _viewBounds.max.y - max.y;
            else if (min.y > _viewBounds.min.y)
                offset.y = _viewBounds.min.y - min.y;
        }

        return offset;
    }

    public void Scroll(Vector2 delta, float scrollSensitivity, ScrollLocalManager.MovementType scrollingMovementType)
    {
        EnsureLayoutHasRebuilt();
        UpdateBounds();

        // Down is positive for scroll events, while in UI system up is positive.
        delta.y *= -1;

        if (_verticallyScrollable && !_horizontallyScrollable)
        {
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                delta.y = delta.x;
            delta.x = 0;
        }

        if (_horizontallyScrollable && !_verticallyScrollable)
        {
            if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
                delta.x = delta.y;
            delta.y = 0;
        }

        Vector2 position = _content.anchoredPosition;
        position += delta * scrollSensitivity;
        if (scrollingMovementType == ScrollLocalManager.MovementType.Clamped)
            position += CalculateOffset(position - _content.anchoredPosition, scrollingMovementType);

        SetContentAnchoredPosition(position);
        UpdateBounds();
    }

    protected void SetDirty()
    {
        if (!IsActive())
            return;

        LayoutRebuilder.MarkLayoutForRebuild(_rectTransform);
    }

    protected void SetDirtyCaching()
    {
        if (!IsActive())
            return;

        CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
        LayoutRebuilder.MarkLayoutForRebuild(_rectTransform);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        SetDirtyCaching();
    }
#endif
}
