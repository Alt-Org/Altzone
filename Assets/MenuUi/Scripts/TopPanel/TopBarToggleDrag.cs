using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TopBarToggleDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform _clampArea;
    public Action OnDropped;

    private RectTransform _rectTransform, _listContainer, _boundsTransform;
    private LayoutElement _layoutElement;
    private CanvasGroup _canvasGroup;
    private Canvas _rootCanvas;
    private Transform _originalParent;
    private GameObject _placeholderObject;
    private Vector2 _pointerOffsetInRootCanvas;
    private float _lockedXInRootCanvas;

    private const bool DebugOn = false;

    // Cache required components used during drag.
    // Assumes all components exist on the same GameObject (except Canvas).
    private void Awake()
    {
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarToggleDrag : Awake()");

        _rectTransform = GetComponent<RectTransform>();
        _layoutElement = GetComponent<LayoutElement>();
        _canvasGroup = GetComponent<CanvasGroup>();

        _rootCanvas = GetComponentInParent<Canvas>();
    }


    // Handles drag initialization:
    // creates placeholder, removes row from layout, rebuilds UI,
    // moves row to root canvas and syncs it with pointer.
    // Order of operations is critical to avoid first-drag bugs.
    public void OnBeginDrag(PointerEventData e)
    {
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarToggleDrag : OnBeginDrag()");

        if (_rootCanvas == null) return;

        _originalParent = transform.parent;
        _listContainer = (RectTransform)_originalParent;
        _boundsTransform = (_clampArea != null) ? _clampArea : _listContainer;

        _placeholderObject = new GameObject("Placeholder", typeof(RectTransform), typeof(LayoutElement));
        _placeholderObject.transform.SetParent(_originalParent, false);

        LayoutElement ple = _placeholderObject.GetComponent<LayoutElement>();
        ple.preferredHeight = (_layoutElement != null && _layoutElement.preferredHeight > 1f)
            ? _layoutElement.preferredHeight
            : _rectTransform.rect.height;

        _placeholderObject.transform.SetSiblingIndex(transform.GetSiblingIndex());

        if (_layoutElement != null) _layoutElement.ignoreLayout = true;
        if (_canvasGroup != null) _canvasGroup.blocksRaycasts = false;

        LayoutRebuilder.ForceRebuildLayoutImmediate(_listContainer);
        Canvas.ForceUpdateCanvases();

        transform.SetParent(_rootCanvas.transform, true);
        _rectTransform.SetAsLastSibling();

        RectTransform rootRT = (RectTransform)_rootCanvas.transform;

        Vector2 lp;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootRT,
            e.position,
            e.pressEventCamera,
            out lp
        );

        _lockedXInRootCanvas = _rectTransform.localPosition.x;

        _pointerOffsetInRootCanvas = (Vector2)_rectTransform.localPosition - lp;
        _pointerOffsetInRootCanvas.x = 0f;

        FollowPointer(e);
        UpdatePlaceholderIndex(e);
    }

    // Handles dragging each frame: moves the row and updates its position in the list.
    public void OnDrag(PointerEventData e)
    {
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarToggleDrag : OnDrag()");

        FollowPointer(e);
        UpdatePlaceholderIndex(e);
    }

    // Finalizes drag: restores layout, applies new order and cleans up placeholder
    public void OnEndDrag(PointerEventData e)
    {
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarToggleDrag : OnEndDrag()");

        transform.SetParent(_originalParent, false);

        if (_placeholderObject != null)
        {
            transform.SetSiblingIndex(_placeholderObject.transform
                .GetSiblingIndex());
            Destroy(_placeholderObject);
            _placeholderObject = null;
        }

        if (_layoutElement != null) _layoutElement.ignoreLayout = false;

        if (_canvasGroup != null)
        {
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha = 1f;
        }

        OnDropped?.Invoke();
    }

    // Moves dragged row with pointer in root canvas
    // and clamps movement inside defined bounds: Clamp Area
    private void FollowPointer(PointerEventData e)
    {
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarToggleDrag : FollowPointer()");

        RectTransform rootRT = (RectTransform)_rootCanvas.transform;

        Vector2 lp;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootRT,
            e.position,
            e.pressEventCamera,
            out lp
        );

        float y = lp.y + _pointerOffsetInRootCanvas.y;

        if (_boundsTransform != null)
        {
            Vector3 minWorld = _boundsTransform.TransformPoint(new Vector3(0, _boundsTransform.rect.yMin, 0));
            Vector3 maxWorld = _boundsTransform.TransformPoint(new Vector3(0, _boundsTransform.rect.yMax, 0));

            float a = rootRT.InverseTransformPoint(minWorld).y + _rectTransform.rect.height * 0.5f;
            float b = rootRT.InverseTransformPoint(maxWorld).y - _rectTransform.rect.height * 0.5f;

            float minY = Mathf.Min(a, b);
            float maxY = Mathf.Max(a, b);

            y = Mathf.Clamp(y, minY, maxY);
        }

        _rectTransform.localPosition = new Vector2(_lockedXInRootCanvas, y);
    }

    // Calculates and updates placeholder index based on pointer position
    // Determines where the dragged row should be inserted during drag
    private void UpdatePlaceholderIndex(PointerEventData e)
    {
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarToggleDrag : UpdatePlaceholderIndex()");

        if (_placeholderObject == null || _listContainer == null) return;

        List<RectTransform> rows = new List<RectTransform>();

        foreach (Transform child in _listContainer)
        {
            if (child == _placeholderObject.transform) continue;

            if (child.GetComponent<TopBarToggleDrag>() != null)
                rows.Add((RectTransform)child);
        }

        if (rows.Count == 0) return;

        int insertIndex = 0;
        Vector3[] corners = new Vector3[4];

        for (int i = 0; i < rows.Count; i++)
        {
            RectTransform row = rows[i];

            row.GetWorldCorners(corners);

            float rowMiddleY = (corners[1].y + corners[0].y) * 0.5f;

            if (e.position.y < rowMiddleY)
                insertIndex = i + 1;
            else
                break;
        }


        _placeholderObject.transform.SetSiblingIndex(insertIndex);
    }
}
