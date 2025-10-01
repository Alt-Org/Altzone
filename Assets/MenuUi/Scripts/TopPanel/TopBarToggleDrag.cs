using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TopBarToggleDrag : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Bounds (optional)")]
    [SerializeField] private RectTransform _clampArea;

    public System.Action OnDropped;

    // Cached
    private RectTransform _rectTransform, _listContainer, _boundsTransform;
    private LayoutElement _layoutElement;
    private CanvasGroup _canvasGroup;
    private Canvas _rootCanvas;
    private Transform _originalParent;

    // Drag state
    private GameObject _placeholderObject;
    private Vector2 _pointerOffsetInRootCanvas;
    private float _lockedXInRootCanvas;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _layoutElement = GetComponent<LayoutElement>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _rootCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_rootCanvas == null) return;

        _originalParent = transform.parent;
        _listContainer = (RectTransform)_originalParent;
        _boundsTransform = _clampArea ? _clampArea : _listContainer;

        CreatePlaceholder();
        SetDragging(true);

        var rootRT = (RectTransform)_rootCanvas.transform;
        _lockedXInRootCanvas = _rectTransform.localPosition.x;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rootRT, eventData.position, eventData.pressEventCamera, out var p);
        _pointerOffsetInRootCanvas = (Vector2)_rectTransform.localPosition - p;

        FollowPointer(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        FollowPointer(eventData);
        UpdatePlaceholderIndex(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(_originalParent, false);
        if (_placeholderObject) transform.SetSiblingIndex(_placeholderObject.transform.GetSiblingIndex());
        SetDragging(false);
        DestroyPlaceholder();
        OnDropped?.Invoke();
    }

    private void SetDragging(bool isDragging)
    {
        if (_layoutElement) _layoutElement.ignoreLayout = isDragging;
        _canvasGroup.blocksRaycasts = !isDragging;
        _canvasGroup.alpha = isDragging ? 0.75f : 1f;
        transform.SetParent(isDragging ? _rootCanvas.transform : _originalParent, true);
    }

    private void CreatePlaceholder()
    {
        _placeholderObject = new GameObject("Placeholder", typeof(RectTransform), typeof(LayoutElement));
        _placeholderObject.transform.SetParent(_originalParent, false);
        var ple = _placeholderObject.GetComponent<LayoutElement>();
        ple.preferredHeight = (_layoutElement && _layoutElement.preferredHeight > 1f)
            ? _layoutElement.preferredHeight
            : _rectTransform.rect.height;
        _placeholderObject.transform.SetSiblingIndex(transform.GetSiblingIndex());
    }

    private void DestroyPlaceholder()
    {
        if (!_placeholderObject) return;
        Destroy(_placeholderObject);
        _placeholderObject = null;
    }

    private void FollowPointer(PointerEventData eventData)
    {
        var rootRT = (RectTransform)_rootCanvas.transform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rootRT, eventData.position, eventData.pressEventCamera, out var p);
        Vector2 desired = p + _pointerOffsetInRootCanvas; desired.x = _lockedXInRootCanvas;

        if (_boundsTransform)
        {
            Vector2 inBounds = (Vector2)_boundsTransform.InverseTransformPoint(rootRT.TransformPoint(desired));
            Rect br = _boundsTransform.rect;
            float hw = _rectTransform.rect.width * 0.5f, hh = _rectTransform.rect.height * 0.5f;
            inBounds.x = Mathf.Clamp(inBounds.x, br.xMin + hw, br.xMax - hw);
            inBounds.y = Mathf.Clamp(inBounds.y, br.yMin + hh, br.yMax - hh);
            desired = (Vector2)rootRT.InverseTransformPoint(_boundsTransform.TransformPoint(inBounds));
        }

        _rectTransform.localPosition = new Vector2(_lockedXInRootCanvas, desired.y);
    }

    private void UpdatePlaceholderIndex(PointerEventData eventData)
    {
        if (!_placeholderObject || !_listContainer) return;

        // Find the row by walking up via Unity's internal parent search
        var hit = eventData.pointerCurrentRaycast.gameObject;
        var hitRow = hit ? hit.GetComponentInParent<TopBarToggleDrag>() : null;
        var rowTransform = hitRow ? hitRow.transform : null;

        if (rowTransform && rowTransform != transform && rowTransform.gameObject != _placeholderObject)
        {
            int idx = rowTransform.GetSiblingIndex()
                    + (_rectTransform.position.y < ((RectTransform)rowTransform).position.y ? 1 : 0);

            int ph = _placeholderObject.transform.GetSiblingIndex();
            _placeholderObject.transform.SetSiblingIndex(Mathf.Clamp(ph < idx ? idx - 1 : idx, 0, _listContainer.childCount));
            return;
        }

        // No valid row: snap to edges
        Vector2 local = _listContainer.InverseTransformPoint(_rectTransform.position);
        Rect lr = _listContainer.rect;
        int edgeIdx = (local.y > lr.yMax) ? 0
                   : (local.y < lr.yMin) ? _listContainer.childCount
                   : _placeholderObject.transform.GetSiblingIndex();

        _placeholderObject.transform.SetSiblingIndex(edgeIdx);
    }
}
