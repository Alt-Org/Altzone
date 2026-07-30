using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Altzone.Scripts.Settings;

public class TopBarToggleDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform _clampArea;
    [SerializeField] private ScrollRect _scrollRect;

    private bool _isScrolling;
    [SerializeField] private float _scrollSpeed = 10f;

    public Action OnDropped;

    private RectTransform _dragHandle;
    private RectTransform _row;
    private RectTransform _rectTransform, _listContainer, _boundsTransform;
    private LayoutElement _layoutElement;
    private CanvasGroup _canvasGroup;
    private Canvas _rootCanvas;
    private Transform _originalParent;
    private GameObject _placeholderObject;
    private Vector2 _pointerOffsetInRootCanvas;
    private float _lockedXInRootCanvas;

    private const bool DebugOn = true;

    private void Awake()
    {
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarToggleDrag : Awake()");

        if (_scrollRect == null)
            _scrollRect = GetComponentInParent<ScrollRect>();

        Debug.Log($"[TopBarDebugScroll] ScrollRect found: {_scrollRect != null}");

        TopBarToggleHandler handler = GetComponentInParent<TopBarToggleHandler>();
        _row = handler != null
            ? handler.transform as RectTransform
            : transform.parent as RectTransform;

        _dragHandle = GetComponent<RectTransform>();
        //_row = transform.parent as RectTransform;
        _layoutElement = _row.GetComponent<LayoutElement>();
        _canvasGroup = _row.GetComponent<CanvasGroup>();
        _rootCanvas = GetComponentInParent<Canvas>();
    }

    private void OnEnable()
    {
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarToggleDrag : OnEnable()");
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarToggleDrag : OnBeginDrag()");

        if (!DragAllowed())
            return;

        if (_rootCanvas == null) return;

        _originalParent = _row.parent;
        _listContainer = (RectTransform)_originalParent;
        _boundsTransform = (_clampArea != null) ? _clampArea : _listContainer;
        _rectTransform = _row;

        _placeholderObject = new GameObject("Placeholder", typeof(RectTransform), typeof(LayoutElement));
        _placeholderObject.transform.SetParent(_originalParent, false);

        LayoutElement ple = _placeholderObject.GetComponent<LayoutElement>();
        ple.preferredHeight = (_layoutElement != null && _layoutElement.preferredHeight > 1f)
            ? _layoutElement.preferredHeight
            : _rectTransform.rect.height;

        _placeholderObject.transform.SetSiblingIndex(_row.GetSiblingIndex());

        if (_layoutElement != null) _layoutElement.ignoreLayout = true;
        if (_canvasGroup != null) _canvasGroup.blocksRaycasts = false;

        LayoutRebuilder.ForceRebuildLayoutImmediate(_listContainer);
        Canvas.ForceUpdateCanvases();

        _row.SetParent(_rootCanvas.transform, true);
        _row.SetAsLastSibling();

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

    public void OnDrag(PointerEventData e)
    {
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarToggleDrag : OnDrag()");

        if (!DragAllowed())
            return;

        FollowPointer(e);
        UpdatePlaceholderIndex(e);
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarToggleDrag : OnEndDrag()");

        if (!DragAllowed())
            return;

        _row.SetParent(_originalParent, false);

        if (_placeholderObject != null)
        {
            _row.SetSiblingIndex(_placeholderObject.transform
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

    private void UpdatePlaceholderIndex(PointerEventData e)
    {
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarToggleDrag : UpdatePlaceholderIndex()");

        if (_placeholderObject == null || _listContainer == null) return;

        List<RectTransform> rows = new List<RectTransform>();

        foreach (Transform child in _listContainer)
        {
            if (child == _placeholderObject.transform) continue;

            RectTransform row = child as RectTransform;
            if (row == null) continue;

            if (row.GetComponentInChildren<TopBarToggleHandler>(true) == null)
                continue;

            if (ClanTileIsOn() && IsClanSubItem(row))
                continue;

            rows.Add(row);
        }

        if (rows.Count == 0) return;

        int insertSiblingIndex = 0;
        Vector3[] corners = new Vector3[4];

        for (int i = 0; i < rows.Count; i++)
        {
            RectTransform row = rows[i];

            row.GetWorldCorners(corners);

            float rowMiddleY = (corners[1].y + corners[0].y) * 0.5f;

            // Debug.Log(
            //     $"[TopBarDebug] row={row.name}, " +
            //     $"middleY={rowMiddleY}, " +
            //     $"pointerY={e.position.y}, " +
            //     $"i={i}"
            // );

            if (e.position.y < rowMiddleY)
                insertSiblingIndex = row.GetSiblingIndex() + 1;
            else
                break;
        }

        insertSiblingIndex = GetSafeSiblingIndex(insertSiblingIndex);
        insertSiblingIndex = Mathf.Clamp(insertSiblingIndex, 0, _listContainer.childCount - 1);

        _placeholderObject.transform.SetSiblingIndex(insertSiblingIndex);

        Debug.Log($"[TopBarDebug] insertSiblingIndex={insertSiblingIndex}, " +
                  $"childCount={_listContainer.childCount}, " +
                  $"dragging={_row.name}");
    }

    private bool DragAllowed()
    {
        TopBarToggleHandler handler = GetComponentInParent<TopBarToggleHandler>();
        if (handler == null) return true;

        bool clanTileOn = PlayerPrefs.GetInt(
            TopBarDefs.Key(TopBarDefs.TopBarItem.ClanTile) + "_" + SettingsCarrier.Instance.TopBarStyleSetting,
            1
        ) != 0;

        if (!clanTileOn) return true;

        return handler.item != TopBarDefs.TopBarItem.Leaderboard
               && handler.item != TopBarDefs.TopBarItem.Coins
               && handler.item != TopBarDefs.TopBarItem.ClanLogo
               && handler.item != TopBarDefs.TopBarItem.ClanTextContainer;
    }

    private bool IsClanSubItem(RectTransform row)
    {
        TopBarToggleHandler handler =
            row.GetComponentInChildren<TopBarToggleHandler>(true);

        if (handler == null) return false;

        return handler.item == TopBarDefs.TopBarItem.Leaderboard
               || handler.item == TopBarDefs.TopBarItem.Coins
               || handler.item == TopBarDefs.TopBarItem.ClanLogo
               || handler.item == TopBarDefs.TopBarItem.ClanTextContainer;
    }

    private bool ClanTileIsOn()
    {
        if (SettingsCarrier.Instance == null) return false;

        string key =
            TopBarDefs.Key(TopBarDefs.TopBarItem.ClanTile)
            + "_"
            + SettingsCarrier.Instance.TopBarStyleSetting;

        return PlayerPrefs.GetInt(key, 1) != 0;
    }

    private int GetSafeSiblingIndex(int index)
    {
        if (!ClanTileIsOn() || _listContainer == null)
            return index;

        int clanTileIndex = -1;
        int lastClanSubIndex = -1;

        foreach (Transform child in _listContainer)
        {
            if (child == _placeholderObject.transform) continue;

            TopBarToggleHandler handler =
                child.GetComponentInChildren<TopBarToggleHandler>(true);

            if (handler == null) continue;

            int sibling = child.GetSiblingIndex();

            if (handler.item == TopBarDefs.TopBarItem.ClanTile)
                clanTileIndex = sibling;

            if (handler.item == TopBarDefs.TopBarItem.Leaderboard ||
                handler.item == TopBarDefs.TopBarItem.Coins ||
                handler.item == TopBarDefs.TopBarItem.ClanLogo ||
                handler.item == TopBarDefs.TopBarItem.ClanTextContainer)
            {
                if (sibling > lastClanSubIndex)
                    lastClanSubIndex = sibling;
            }
        }

        if (clanTileIndex < 0 || lastClanSubIndex < 0)
            return index;

        if (index > clanTileIndex && index <= lastClanSubIndex + 1)
            return lastClanSubIndex + 1;

        return index;
    }
}
