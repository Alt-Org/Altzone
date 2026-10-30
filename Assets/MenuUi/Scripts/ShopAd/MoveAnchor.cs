using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveAnchor : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{
    [SerializeField] private RectTransform adTextHolder;
    [SerializeField] private AnchorType anchorType;
    private enum AnchorType { TopLeft, BottomRight}

    private CanvasGroup canvasGroup;

    private Vector2 minAnc;
    private Vector2 maxAnc;

    private Vector3 mouseStartPosition;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        minAnc = adTextHolder.anchorMin;
        maxAnc = adTextHolder.anchorMax;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("on pointer down");
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("on drag");

        transform.position = eventData.position;

        if (anchorType == AnchorType.TopLeft)
        {
            minAnc.x += (mouseStartPosition.x + eventData.delta.x) * 0.001f;
            maxAnc.y += (mouseStartPosition.y + eventData.delta.y) * 0.001f;
        }
        if (anchorType == AnchorType.BottomRight)
        {
            maxAnc.x += (mouseStartPosition.x + eventData.delta.x) * 0.001f;
            minAnc.y += (mouseStartPosition.y + eventData.delta.y) * 0.001f;
        }

        adTextHolder.anchorMin = minAnc;
        adTextHolder.anchorMax = maxAnc;

        adTextHolder.offsetMin = new Vector2(0f, 0f);
        adTextHolder.offsetMax = new Vector2(0f, 0f);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("on begin drag");
        canvasGroup.alpha = 0.7f;
        canvasGroup.blocksRaycasts = false;

        mouseStartPosition = eventData.delta;

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("on end drag");
        canvasGroup.alpha = 1.0f;
        canvasGroup.blocksRaycasts = true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("on drop");
    }
}
