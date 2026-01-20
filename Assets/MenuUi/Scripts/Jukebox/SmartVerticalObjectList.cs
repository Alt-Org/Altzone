using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Altzone.Scripts.Audio;
using UnityEngine.Assertions.Comparers;
using Object = System.Object;

/// <summary>
/// Recycles a limited amount of gameobjects by moving and repurposing out of bounds gameobjects for the other end that is coming out of the invisible area.
/// </summary>
public class SmartVerticalObjectList : AltMonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private bool _elastic = true;
    [SerializeField] private float _elasticityRange = 20f;
    [SerializeField] private float _slowdownTime = 1.5f;
    [Space]
    [SerializeField] private RectTransform _content;
    [Space]
    [SerializeField] private List<RectTransform> _uniqueGameObjectsAtTop = new();
    [SerializeField] private GameObject _contentPrefab;
    [SerializeField] private List<RectTransform> _uniqueGameObjectsAtBottom = new();
    [Space]
    [SerializeField] private float _smartItemTopStrenghtMultiplier = 0f;
    [SerializeField] private float _smartItemBottomStrenghtMultiplier = 2f;
    //[Space]
    //[SerializeField] private int _horizontalPadding = 10;
    [SerializeField] private int _verticalPadding = 10;

    private bool _dragActive = false;

    private List<float> _uniqueGameObjectsAtTopStartPositions = new();
    private List<float> _uniqueGameObjectsAtBottomStartPositions = new();

    private readonly List<SmartListItem> _smartListItems = new();
    //private int _smartScrollIndex = 0;

    private int _contentListLenght = -1;
    private int _smartListBottomIndex = -1;
    private int _smartListTopIndex = -1; //-1 and below are for _uniqueGameObjectsAtTop.

    private float _velocity = 0f;
    private Coroutine _velocityCoroutine;
    private Vector2 _previousUpdatePosition;
    private Vector2 _startPosition;

    private float _contentBorderTop = 0f;
    private float _contentBorderBottom = 0f;
    private int _amountToFillContentList = 0;
    private float _itemWithPadding = 0f;

    public delegate void NewDataRequested(int targetIndex);
    public event NewDataRequested OnNewDataRequested;

    // private bool _alwaysShowTopObjects = false;
    // public bool AlwaysShowTopObjects { get { return _alwaysShowTopObjects; } set { _alwaysShowTopObjects = value; } }

    private void Awake()
    {
        if (_smartListItems.Count == 0) CreatePool();
    }

    private void OnEnable()
    {

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _startPosition = eventData.position;

        foreach (var smartListItem in _smartListItems) smartListItem.UpdateStartPosition();

        SetUniqueStartPositions(_uniqueGameObjectsAtTop, ref _uniqueGameObjectsAtTopStartPositions);
        SetUniqueStartPositions(_uniqueGameObjectsAtBottom, ref _uniqueGameObjectsAtBottomStartPositions);

        _dragActive = true;

        if (_velocityCoroutine == null) return;

        StopCoroutine(_velocityCoroutine);
        _velocityCoroutine = null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        _velocity = eventData.position.y - _previousUpdatePosition.y;
        MoveItems((eventData.position.y - _startPosition.y));
        _previousUpdatePosition = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _dragActive = false;
        _startPosition = eventData.position;

        foreach (var smartListItem in _smartListItems) smartListItem.UpdateStartPosition();

        SetUniqueStartPositions(_uniqueGameObjectsAtTop, ref _uniqueGameObjectsAtTopStartPositions);
        SetUniqueStartPositions(_uniqueGameObjectsAtBottom, ref _uniqueGameObjectsAtBottomStartPositions);

        _velocityCoroutine = StartCoroutine(HandleVelocity());
    }

    public void Setup<T>(List<T> data)
    {
        _contentListLenght = data.Count;

        _contentBorderTop = HalfHeight(_content);
        _contentBorderBottom = -HalfHeight(_content);
        _smartListTopIndex = -_uniqueGameObjectsAtTop.Count;
        _smartListBottomIndex = _smartListItems.Count;

        if (_smartListItems.Count != _contentListLenght) CreatePool();

        UpdateContents<T>(data);
    }

    public void UpdateContent<T>(int index, T data)
    {
        int smartIndex = index % _smartListItems.Count;

        _smartListItems[smartIndex].SetVisibility(true);
        _smartListItems[smartIndex].SetData<T>(data);
    }

    public void UpdateContents<T>(List<T> data)
    {
        //Set positions.
        for (int mainIndex = -_uniqueGameObjectsAtTop.Count; mainIndex < _contentListLenght + _uniqueGameObjectsAtBottom.Count; mainIndex++)
        {
            int bottomIndex = mainIndex - _contentListLenght;

            if (mainIndex < 0) //Set top unique items.
            {
                int topIndex = _uniqueGameObjectsAtTop.Count + mainIndex;

                if (mainIndex < _smartListTopIndex || mainIndex > _smartListBottomIndex)
                {
                    /*if (!_alwaysShowTopObjects)*/ _uniqueGameObjectsAtTop[topIndex].gameObject.SetActive(false);

                    continue;
                }

                SetPosition(_uniqueGameObjectsAtTop[topIndex], topIndex);
            }
            else if (mainIndex < _contentListLenght) //Set smart items.
            {
                int smartIndex = mainIndex % _smartListItems.Count;

                if (mainIndex < _smartListTopIndex || mainIndex > _smartListBottomIndex)
                {
                    //_smartListItems[smartIndex].SetVisibility(false);
                    continue;
                }

                int positionIndex = mainIndex + _uniqueGameObjectsAtTop.Count - 1;

                _smartListItems[smartIndex].SetData<T>(data[mainIndex]);
                SetPosition(_smartListItems[smartIndex].SelfRectTransform, positionIndex);
            }
            else if (bottomIndex < _uniqueGameObjectsAtBottom.Count) //Set bottom unique items.
            {
                if (mainIndex < _smartListTopIndex || mainIndex > _smartListBottomIndex)
                {
                    _uniqueGameObjectsAtBottom[bottomIndex].gameObject.SetActive(false);
                    continue;
                }

                SetPosition(_uniqueGameObjectsAtBottom[bottomIndex], bottomIndex);
            }
        }
    }

    public void Clear()
    {
        foreach (var smartListItem in _smartListItems) smartListItem.ClearData();
    }

    private void CreatePool()
    {
        //Create first SmartListItem to calculate how many of it can fit inside based on the height.
        if (_smartListItems.Count == 0)
        {
            SmartListItem firstSmartListItem = Instantiate(_contentPrefab, _content).GetComponent<SmartListItem>();

            _itemWithPadding = firstSmartListItem.SelfRectTransform.rect.height + _verticalPadding;
            firstSmartListItem.SelfRectTransform.sizeDelta = new Vector2(_content.rect.width,
                firstSmartListItem.SelfRectTransform.sizeDelta.y);

            _smartListItems.Add(firstSmartListItem);
        }
        else
            _itemWithPadding = _smartListItems[0].SelfRectTransform.rect.height + _verticalPadding;

        _amountToFillContentList = (int)MathF.Ceiling((_content.rect.height / _itemWithPadding)) + 1;

        if (_amountToFillContentList <= _smartListItems.Count) return;

        _amountToFillContentList -= _smartListItems.Count;

        //Create the rest of the SmartListItem's.
        for (int i = 0; i < _amountToFillContentList; i++)
        {
            SmartListItem smartListItem = Instantiate(_contentPrefab, _content).GetComponent<SmartListItem>();

            smartListItem.SelfRectTransform.sizeDelta = new Vector2(_content.rect.width,
                smartListItem.SelfRectTransform.sizeDelta.y);

            _smartListItems.Add(smartListItem);
        }
    }

    private static void SetUniqueStartPositions(List<RectTransform> rectTransforms, ref List<float> startPositions)
    {
        for (int i = 0; i < rectTransforms.Count; i++)
        {
            if (i >= startPositions.Count)
            {
                startPositions.Add(rectTransforms[i].position.y);
                continue;
            }

            startPositions[i] = rectTransforms[i].position.y;
        }
    }

    #region Movement
    private IEnumerator HandleVelocity()
    {
        float totalDistance = 0f;
        float timer = 0f;

        while (timer < _slowdownTime)
        {
            totalDistance += Mathf.Lerp(_velocity, 0f, timer / _slowdownTime);

            MoveItems(totalDistance);
            yield return null;
            timer += Time.deltaTime;
        }
    }

    private void MoveItems(float distance) //TODO: Add elasticity and limits and velocity handling.
    {
        //Unique top objects
        if (_smartListTopIndex < 0)
            for (int i = 0; i < _uniqueGameObjectsAtTop.Count; i++)
            {
                MoveRectTransform(_uniqueGameObjectsAtTop[i], _uniqueGameObjectsAtTopStartPositions[i] + distance);
                _uniqueGameObjectsAtTop[i].gameObject.SetActive(!OutOfBounds(_uniqueGameObjectsAtTop[i]));
            }

        //Smart list items
        foreach (SmartListItem smartListItem in _smartListItems) MoveSmartItem(smartListItem, distance);

        //Unique bottom objects
        if (_smartListBottomIndex < _contentListLenght) return;

        for (int i = 0; i < _uniqueGameObjectsAtBottom.Count; i++)
        {
            MoveRectTransform(_uniqueGameObjectsAtBottom[i], _uniqueGameObjectsAtBottomStartPositions[i] + distance);
            _uniqueGameObjectsAtBottom[i].gameObject.SetActive(!OutOfBounds(_uniqueGameObjectsAtBottom[i]));
        }
    }

    private void MoveSmartItem(SmartListItem smartListItem, float distance)
    {
        MoveRectTransform(smartListItem.SelfRectTransform, smartListItem.YStartPosition + distance);

        //Out of bounds calculations.
        float smartItemBorderTop = smartListItem.SelfRectTransform.localPosition.y +
                                   HalfHeight(smartListItem.SelfRectTransform) * _smartItemTopStrenghtMultiplier;
        float smartItemBorderBottom = smartListItem.SelfRectTransform.localPosition.y -
                                      HalfHeight(smartListItem.SelfRectTransform) * _smartItemBottomStrenghtMultiplier;

        bool overTop = _contentBorderTop < smartItemBorderBottom && _velocity > 0;
        bool overBottom = _contentBorderBottom > smartItemBorderTop && _velocity < 0;
        bool outOfBounds = (overTop || overBottom);

        if (!outOfBounds) return;

        //Move item to opposite end.
        int outOfBoundsDirection = (_contentBorderTop < smartItemBorderBottom ? -1 : 1 );
        Vector3 oldStartPosition = smartListItem.SelfRectTransform.position;

        smartListItem.SelfRectTransform.anchoredPosition =
            new Vector2(smartListItem.SelfRectTransform.anchoredPosition.x,
                smartListItem.SelfRectTransform.anchoredPosition.y +
                (_itemWithPadding * (_smartListItems.Count)) * outOfBoundsDirection);

        smartListItem.UpdateStartPosition(smartListItem.YStartPosition + (Vector3.Distance(oldStartPosition, smartListItem.SelfRectTransform.position)) * outOfBoundsDirection);

        _smartListTopIndex += outOfBoundsDirection * -1;
        _smartListBottomIndex += outOfBoundsDirection * -1;

        int targetIndex = overTop ? _smartListBottomIndex : _smartListTopIndex;

        if (targetIndex >= 0 && targetIndex < _contentListLenght)
            OnNewDataRequested?.Invoke(targetIndex);
        else
            smartListItem.SetVisibility(false);
    }
    #endregion

    #region Helper Functions

    private static void MoveRectTransform(RectTransform rectTransform, float newPosition)
    {
        rectTransform.position = new Vector2(rectTransform.position.x, newPosition);
    }

    private static float HalfHeight(RectTransform rectTransform)
    {
        return rectTransform.rect.height * 0.5f;
    }

    private void SetPosition(RectTransform rectTransform, int number)
    {
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -_itemWithPadding * number);

        if (!rectTransform.gameObject.activeSelf) rectTransform.gameObject.SetActive(true);
    }

    private bool OutOfBounds(RectTransform rectTransform, float topMultiplier = 0f, float bottomMultiplier = 2f)
    {
        float smartItemBorderTop = rectTransform.localPosition.y +
                                   HalfHeight(rectTransform) * topMultiplier;
        float smartItemBorderBottom = rectTransform.localPosition.y -
                                      HalfHeight(rectTransform) * bottomMultiplier;

        bool overTop = _contentBorderTop < smartItemBorderBottom && _velocity > 0;
        bool overBottom = _contentBorderBottom > smartItemBorderTop && _velocity < 0;

        return overTop || overBottom;
    }
    #endregion
}
