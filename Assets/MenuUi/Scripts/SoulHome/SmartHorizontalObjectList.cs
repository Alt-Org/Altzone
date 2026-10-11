using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Altzone.Scripts.Audio;

/// <summary>
/// Horizontal list that only creates enough slots to fill the viewport and reuses them while scrolling.
/// Scrolling, inertia and the edge limits are handled by a <see cref="GestureScrollRect"/>; this only sets the content
/// width and places each slot at its data index based on the scroll position.
/// </summary>
[RequireComponent(typeof(GestureScrollRect))]
public class SmartHorizontalObjectList : MonoBehaviour
{
    [SerializeField] private GestureScrollRect _scrollRect;
    [SerializeField] private RectTransform _viewport;
    [SerializeField] private RectTransform _content;
    [SerializeField] private GameObject _contentPrefab;
    [Tooltip("Slots created on top of what fits in the viewport. At least one is needed while slots at both edges are partially visible.")]
    [SerializeField] private int _extraSmartListItems = 2;
    [SerializeField] private int _horizontalPadding = 10;

    private readonly List<SmartListItem> _pool = new();
    private readonly List<int> _poolBoundIndexes = new(); //The data index each pool slot currently shows, -1 if none.
    private Vector2 _slotSize;
    private float _step; //Slot width + padding.
    private float _poolViewportWidth = -1f;

    private int _count;
    private Action<SmartListItem, int> _bindItem;
    private int _firstIndex = -1;
    private Action _pendingSetup; //Setup called while disabled, run on enable when the layout is valid.

    public enum DragIntent { Pending, Scroll, Furniture }
    private Vector2 _gestureStartPosition;
    private DragIntent _dragIntent = DragIntent.Pending;
    private bool _scrollEnabled = true;

    public bool scroll_enabled
    {
        get => _scrollEnabled;
        set
        {
            _scrollEnabled = value;
            if (!value) _scrollRect.CancelDrag();
        }
    }

    private void Awake()
    {
        if (!_scrollRect) _scrollRect = GetComponent<GestureScrollRect>();

        _scrollRect.content = _content;
        _scrollRect.viewport = _viewport;
        _scrollRect.horizontal = true;
        _scrollRect.vertical = false;
        _scrollRect.CanBeginDrag = eventData => _scrollEnabled && ResolveGesture(eventData.position) == DragIntent.Scroll;
        _scrollRect.PotentialDragStarted += eventData => BeginGesture(eventData.pressPosition);

        //Content grows to the right from the left edge, its width is set from the data count.
        _content.anchorMin = new Vector2(0f, _content.anchorMin.y);
        _content.anchorMax = new Vector2(0f, _content.anchorMax.y);
        _content.pivot = new Vector2(0f, _content.pivot.y);

        if (_pool.Count == 0) CreatePool();
    }

    private void OnEnable()
    {
        _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);

        if (_pendingSetup == null) return;

        Action pendingSetup = _pendingSetup;
        _pendingSetup = null;
        pendingSetup();
    }

    private void OnDisable()
    {
        if (!_scrollRect) return; //Destroyed together with this object.
        _scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
        CancelGesture();
    }

    //The content position can also be set directly (ScrollToStart, ScrollRect clamping), so check it every frame too.
    private void LateUpdate() { Refresh(false); }

    private void OnScrollValueChanged(Vector2 _) { Refresh(false); }

    #region Gesture
    // Shared by the ScrollRect drag events and the controller's physics picking path.
    public void BeginGesture(Vector2 position)
    {
        StopMovement();
        _gestureStartPosition = position;
        _dragIntent = DragIntent.Pending;
    }

    public DragIntent ResolveGesture(Vector2 position)
    {
        if (_dragIntent != DragIntent.Pending) return _dragIntent;
        Vector2 delta = position - _gestureStartPosition;
        float threshold = Mathf.Max(1, EventSystem.current != null ? EventSystem.current.pixelDragThreshold : 10);
        if (delta.sqrMagnitude < threshold * threshold) return _dragIntent;
        _dragIntent = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y) ? DragIntent.Scroll : DragIntent.Furniture;
        return _dragIntent;
    }

    public void StopMovement() { _scrollRect.StopMovement(); }

    public void CancelGesture()
    {
        _scrollRect.CancelDrag();
        // Ignore the rest of this gesture until the next press initializes a new one.
        _dragIntent = DragIntent.Furniture;
    }
    #endregion

    #region Data
    /// <summary>Shows the given data. Keeps the current scroll position, use <see cref="ScrollToStart"/> to reset it.</summary>
    public void Setup<T>(List<T> data)
    {
        // When the furniture tray is loaded the list is disabled and its layout is not up to date,
        // so the slot size and pool size would be calculated from stale rects.
        if (!isActiveAndEnabled)
        {
            _pendingSetup = () => Setup(data);
            return;
        }
        _pendingSetup = null;

        if (_pool.Count == 0) CreatePool();

        _count = data.Count;
        _bindItem = (item, index) => item.SetData(data[index]);

        float contentWidth = _count > 0 ? _count * _step - _horizontalPadding : 0f;
        _content.sizeDelta = new Vector2(contentWidth, _content.sizeDelta.y);

        Refresh(true);
    }

    /// <summary>Sets the data again to the slots that are currently shown, e.g. after the data has changed.</summary>
    public void Refresh() { Refresh(true); }

    public void ScrollToStart()
    {
        _scrollRect.StopMovement();
        _content.anchoredPosition = new Vector2(0f, _content.anchoredPosition.y);
        Refresh(false);
    }

    private void Refresh(bool rebind)
    {
        if (_pool.Count == 0 || _step <= 0f) return;

        if (!Mathf.Approximately(GetViewportWidth(), _poolViewportWidth))
        {
            if (FillPool()) rebind = true; //Index to slot mapping changes when the pool grows.
        }

        int maxFirstIndex = Mathf.Max(0, _count - _pool.Count);
        int firstIndex = Mathf.Clamp(Mathf.FloorToInt(-_content.anchoredPosition.x / _step), 0, maxFirstIndex);

        if (firstIndex == _firstIndex && !rebind) return;
        _firstIndex = firstIndex;

        for (int i = 0; i < _pool.Count; i++)
        {
            int index = firstIndex + i;
            int poolIndex = index % _pool.Count;
            SmartListItem item = _pool[poolIndex];

            if (index >= _count)
            {
                if (_poolBoundIndexes[poolIndex] != -1) item.ClearData();
                item.SetVisibility(false);
                _poolBoundIndexes[poolIndex] = -1;
                continue;
            }

            item.SelfRectTransform.anchoredPosition = new Vector2(index * _step, 0f);
            item.SetVisibility(true);

            if (!rebind && _poolBoundIndexes[poolIndex] == index) continue;
            _bindItem(item, index);
            _poolBoundIndexes[poolIndex] = index;
        }
    }

    private void CreatePool()
    {
        //All slots use the same size: the prefab's aspect ratio scaled to the content height.
        RectTransform prefabRect = _contentPrefab.GetComponent<RectTransform>();
        float sizeRatio = prefabRect.sizeDelta.x / prefabRect.sizeDelta.y;
        float height = _content.rect.height;

        _slotSize = new Vector2(height * sizeRatio, height);
        _step = _slotSize.x + _horizontalPadding;

        FillPool();
    }

    /// <returns>True if new slots were created.</returns>
    private bool FillPool()
    {
        _poolViewportWidth = GetViewportWidth();
        if (_step <= 0f) return false;

        int amount = Mathf.CeilToInt(_poolViewportWidth / _step) + Mathf.Max(1, _extraSmartListItems);
        if (amount <= _pool.Count) return false;

        for (int i = _pool.Count; i < amount; i++)
        {
            SmartListItem item = Instantiate(_contentPrefab, _content).GetComponent<SmartListItem>();
            if (!item.SelfRectTransform) item.SetSelfRectTransform();

            item.SelfRectTransform.sizeDelta = _slotSize;
            item.ClearData();
            item.SetVisibility(false);

            _pool.Add(item);
            _poolBoundIndexes.Add(-1);
        }
        return true;
    }

    // Viewport width in the content's local units.
    private float GetViewportWidth()
    {
        Vector3 viewportWidth = _viewport.TransformVector(new Vector3(_viewport.rect.width, 0f, 0f));
        return Mathf.Abs(_content.InverseTransformVector(viewportWidth).x);
    }
    #endregion
}
