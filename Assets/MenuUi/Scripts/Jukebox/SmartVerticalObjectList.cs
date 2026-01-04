using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Recycles a limited amount of gameobjects by moving and repurposing out of bounds gameobjects for the other end that is coming out of the invisible area.
/// </summary>
public class SmartVerticalObjectList : AltMonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private bool _elastic = true;
    [Space]
    [SerializeField] private RectTransform _content;
    [Space]
    [SerializeField] private List<RectTransform> _uniqueGameObjectsAtTop = new List<RectTransform>();
    [SerializeField] private GameObject _contentPrefab;
    [SerializeField] private List<RectTransform> _uniqueGameObjectsAtBottom = new List<RectTransform>();
    //[Space]
    //[SerializeField] private int _horizontalPadding = 10;
    //[SerializeField] private int _verticalPadding = 10;

    private List<SmartListItem> _dataItemsList = new List<SmartListItem>();

    private List<RectTransform> _visibleItems = new List<RectTransform>();

    //private int _lastIndex = -1;
    //private int _firstIndex = -1;

    private Vector2 _startPosition;
    private Camera _currentCamera;

    private void OnEnable()
    {
        _currentCamera = Camera.current;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _scrollItemDatas.Add(new ScrollItemData(_uniqueGameObjectsAtTop[0].position.y));

        _startPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        MoveItems((eventData.position.y - _startPosition.y));
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _scrollItemDatas.Clear();
    }

    public void Setup()
    {

    }

    private void CreatePool()
    {

    }

    private void MoveItems(float distance)
    {
        foreach (RectTransform rectTransform in _uniqueGameObjectsAtTop) MoveItem(rectTransform, distance);
    }

    private void MoveItem(RectTransform rectTransform, float distance)
    {
        Debug.LogError(distance);
        rectTransform.position = new Vector2(rectTransform.position.x, _scrollItemDatas[0].YStartPosition + distance);
    }
}

public class SmartListItem : MonoBehaviour
{
    public float YStartPosition;

    public SmartListItem(float yStartPosition)
    {
        YStartPosition = yStartPosition;
    }

    public virtual void Set() { }


}
