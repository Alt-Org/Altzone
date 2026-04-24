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

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _layoutElement = GetComponent<LayoutElement>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _rootCanvas = GetComponentInParent<Canvas>();
    }

   public void OnBeginDrag(PointerEventData e)
   {
    if (_rootCanvas == null) return; // turvatarkistus

    // V�likeston konteksti
    _originalParent = transform.parent;
    _listContainer = (RectTransform)_originalParent;
    _boundsTransform = (_clampArea != null) ? _clampArea : _listContainer;

    // Placeholder alkuper�iseen paikkaan
    _placeholderObject = new GameObject("Placeholder", typeof(RectTransform), typeof(LayoutElement));
    _placeholderObject.transform.SetParent(_originalParent, false);
    LayoutElement ple = _placeholderObject.GetComponent<LayoutElement>();
    ple.preferredHeight = (_layoutElement != null && _layoutElement.preferredHeight > 1f)
        ? _layoutElement.preferredHeight
        : _rectTransform.rect.height;
    _placeholderObject.transform.SetSiblingIndex(transform.GetSiblingIndex());

    // Vedon aikaiset asetukset
    if (_layoutElement != null) _layoutElement.ignoreLayout = true;
    if (_canvasGroup != null) _canvasGroup.blocksRaycasts = false;

    // Siirr� vedett�v� root-canvakseen
    transform.SetParent(_rootCanvas.transform, true);

    // Laske osoitinsiirto, jotta elementti ei "hypp��" tarttuessa
    RectTransform rootRT = (RectTransform)_rootCanvas.transform;
    _lockedXInRootCanvas = _rectTransform.localPosition.x;

    Vector2 lp;
    RectTransformUtility.ScreenPointToLocalPointInRectangle(
        rootRT, e.position, e.pressEventCamera, out lp);

    _pointerOffsetInRootCanvas = (Vector2)_rectTransform.localPosition - lp;

     FollowPointer(e);
   }

    public void OnDrag(PointerEventData e)
    {
      FollowPointer(e);
      UpdatePlaceholderIndex(e);
    }

    public void OnEndDrag(PointerEventData e)
    {

        transform.SetParent(_originalParent, false); // Palaa listaan ennen indeksin asettamista

        if (_placeholderObject != null)
        {
            transform.SetSiblingIndex(_placeholderObject.transform.GetSiblingIndex());  // Aseta lopullinen indeksi ja siivoa placeholder ()
            Destroy(_placeholderObject);
            _placeholderObject = null;
        }

        // Palauta layout & raycastit
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
        RectTransform rootRT = (RectTransform)_rootCanvas.transform;
        Vector2 lp; RectTransformUtility.ScreenPointToLocalPointInRectangle(rootRT, e.position, e.pressEventCamera, out lp);
        Vector2 desired = lp + _pointerOffsetInRootCanvas; desired.x = _lockedXInRootCanvas;
        if (_boundsTransform != null)
        {
            Vector2 inB = (Vector2)_boundsTransform.InverseTransformPoint(rootRT.TransformPoint(desired));
            Rect r = _boundsTransform.rect; float hw = _rectTransform.rect.width * 0.5f, hh = _rectTransform.rect.height * 0.5f;
            inB.x = Mathf.Clamp(inB.x, r.xMin + hw, r.xMax - hw); inB.y = Mathf.Clamp(inB.y, r.yMin + hh, r.yMax - hh);
            desired = (Vector2)rootRT.InverseTransformPoint(_boundsTransform.TransformPoint(inB));
        }
        _rectTransform.localPosition = new Vector2(_lockedXInRootCanvas, desired.y);
    }

    private void UpdatePlaceholderIndex(PointerEventData e)
    {
        if (_placeholderObject == null || _listContainer == null) return;

        List<TopBarToggleDrag> rows = new List<TopBarToggleDrag>();
        foreach (Transform child in _listContainer)
        {
            if (child == _placeholderObject.transform) continue;

            TopBarToggleDrag drag = child.GetComponent<TopBarToggleDrag>();
            if (drag != null)
                rows.Add(drag);
        }

        if (rows.Count == 0) return;

        float pointerY = e.position.y;

        RectTransform firstRow = rows[0].GetComponent<RectTransform>();
        RectTransform lastRow = rows[rows.Count - 1].GetComponent<RectTransform>();

        // IMPORTANT:
        // pointerY is from PointerEventData (screen space),
        // while RectTransform.position.y is world space.
        // This works in Screen Space Overlay canvases.
        // If using Screen Space Camera or World Space, convert both values to the same space.
        if (pointerY > firstRow.position.y)
        {
            _placeholderObject.transform.SetSiblingIndex(0);
            return;
        }

        if (pointerY < lastRow.position.y)
        {
            _placeholderObject.transform.SetSiblingIndex(_listContainer.childCount - 1);
            return;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            RectTransform rowRt = rows[i].GetComponent<RectTransform>();
            if (pointerY > rowRt.position.y)
            {
                _placeholderObject.transform.SetSiblingIndex(i);
                return;
            }
        }
    }
}
