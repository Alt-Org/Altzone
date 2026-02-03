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
public class SmartVerticalObjectList : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private bool _elastic = true;
    [SerializeField] private float _elasticityRange = 20f;
    [SerializeField] private float _slowdownTime = 1.5f;
    //[SerializeField] private float _
    [Space]
    [SerializeField] private RectTransform _content;
    [Space]
    [SerializeField] private List<RectTransform> _uniqueGameObjectsAtTop = new();
    [SerializeField] private GameObject _contentPrefab;
    [SerializeField] private List<RectTransform> _uniqueGameObjectsAtBottom = new();
    [Space]
    [SerializeField] private float _smartItemTopStrenghtMultiplier = 0f;
    [SerializeField] private float _smartItemBottomStrenghtMultiplier = 2f;
    [Tooltip("Use to prevent pop in of the items.")]
    [SerializeField] private int _extraSmartListItems = 1;
    [SerializeField] private float _ignoreRemainingVelocityTriggerTime = 0.1f;
    [Space]
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
    private bool _smartIndexUpdated = false;

    private float _velocity = 0f;
    private Coroutine _velocityCoroutine;
    private Vector2 _previousUpdatePosition;
    private Vector2 _startPosition;
    private float _timeFromLastVelocityUpdate = 0f;

    private float _contentBorderTop = 0f;
    private float _contentBorderBottom = 0f;
    private int _amountToFillContentList = 0;
    private float _smartListItemWithPadding = 0f;

    public delegate void NewDataRequested(int targetIndex);
    public event NewDataRequested OnNewDataRequested;

    private enum _srollDirectionType
    {
        Up,
        Down
    }

    private void Awake()
    {
        if (_smartListItems.Count == 0) CreatePool();
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

        _timeFromLastVelocityUpdate = Time.time;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _dragActive = false;
        _startPosition = eventData.position;

        foreach (var smartListItem in _smartListItems) smartListItem.UpdateStartPosition();

        SetUniqueStartPositions(_uniqueGameObjectsAtTop, ref _uniqueGameObjectsAtTopStartPositions);
        SetUniqueStartPositions(_uniqueGameObjectsAtBottom, ref _uniqueGameObjectsAtBottomStartPositions);

        if (_ignoreRemainingVelocityTriggerTime > (Time.time - _timeFromLastVelocityUpdate)) _velocityCoroutine = StartCoroutine(HandleVelocity());
    }

    public void Setup<T>(List<T> data)
    {
        _contentListLenght = data.Count;

        _contentBorderTop = HalfHeight(_content);
        _contentBorderBottom = -HalfHeight(_content);
        _smartListTopIndex = -_uniqueGameObjectsAtTop.Count;
        _smartListBottomIndex = _smartListItems.Count - _extraSmartListItems;

        if (_smartListItems.Count != _contentListLenght) CreatePool();

        UpdateContents<T>(data);
    }

    public void UpdateContent<T>(int index, T data)
    {
        if (index < _smartListTopIndex || index > _smartListBottomIndex) return;

        int smartIndex = index % _smartListItems.Count;

        _smartListItems[smartIndex].SetVisibility(true);
        _smartListItems[smartIndex].SetData<T>(data);
    }

    private void UpdateContents<T>(List<T> data)
    {
        int smartPositionIndexesUsed = 0;

        //Set positions.
        for (int mainIndex = -_uniqueGameObjectsAtTop.Count; mainIndex < _contentListLenght + _uniqueGameObjectsAtBottom.Count; mainIndex++)
        {
            int bottomIndex = mainIndex - _contentListLenght;

            if (mainIndex < 0) //Set top unique items.
            {
                int topIndex = mainIndex + _uniqueGameObjectsAtTop.Count;

                if (mainIndex < _smartListTopIndex || mainIndex > _smartListBottomIndex)
                {
                    _uniqueGameObjectsAtTop[topIndex].gameObject.SetActive(false);
                    continue;
                }

                float heightWithPadding = _uniqueGameObjectsAtTop[topIndex].rect.height + _verticalPadding;

                SetAnchoredPosition(_uniqueGameObjectsAtTop[topIndex], heightWithPadding, topIndex);
                smartPositionIndexesUsed++;
            }
            else if (bottomIndex < _uniqueGameObjectsAtBottom.Count && mainIndex >= _contentListLenght) //Set bottom unique items.
            {
                if (mainIndex < _smartListTopIndex || mainIndex > _smartListBottomIndex)
                {
                    _uniqueGameObjectsAtBottom[bottomIndex].gameObject.SetActive(false);
                    continue;
                }

                int positionIndex = mainIndex + _uniqueGameObjectsAtTop.Count;
                float heightWithPadding = _uniqueGameObjectsAtBottom[bottomIndex].rect.height + _verticalPadding;

                SetAnchoredPosition(_uniqueGameObjectsAtBottom[bottomIndex], heightWithPadding, positionIndex);
            }
        }

        //Set all smart items.
        for (int i = 0; i < _smartListItems.Count; i++)
        {
            int smartIndex = i % _smartListItems.Count;
            int positionIndex = (i + _uniqueGameObjectsAtTop.Count) % _smartListItems.Count;

            SetAnchoredPosition(_smartListItems[smartIndex].SelfRectTransform, -_smartListItemWithPadding, positionIndex);
            //Debug.LogError("smartPositionIndexesUsed: " + smartPositionIndexesUsed);
            if (i < _smartListTopIndex || i > _smartListBottomIndex || smartPositionIndexesUsed >= _smartListItems.Count)
            {
                _smartListItems[smartIndex].SetVisibility(false);
                continue;
            }

            _smartListItems[smartIndex].SetData<T>(data[i]);
            smartPositionIndexesUsed++;
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

            _smartListItemWithPadding = firstSmartListItem.SelfRectTransform.rect.height + _verticalPadding;
            firstSmartListItem.SelfRectTransform.sizeDelta = new Vector2(_content.rect.width,
                firstSmartListItem.SelfRectTransform.sizeDelta.y);

            _smartListItems.Add(firstSmartListItem);
        }
        else
            _smartListItemWithPadding = _smartListItems[0].SelfRectTransform.rect.height + _verticalPadding;

        _amountToFillContentList = (int)MathF.Ceiling(_content.rect.height / _smartListItemWithPadding) + _extraSmartListItems;

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
        _smartIndexUpdated = false;

        //Unique top objects
        if (_smartListTopIndex <= 0)
            for (int i = 0; i < _uniqueGameObjectsAtTop.Count; i++)
            {
                if (i < (_uniqueGameObjectsAtTop.Count + _smartListTopIndex))
                {
                    _uniqueGameObjectsAtTop[i].gameObject.SetActive(false);
                    continue;
                }

                float height = _uniqueGameObjectsAtTop[i].rect.height;

                if (i == _uniqueGameObjectsAtTop.Count - 1 && _smartListItems.Count != 0)
                    height += _smartListItems[0].SelfRectTransform.rect.y + _smartListItemWithPadding;
                else
                    height += GetSafeEdgeRectPosition(_uniqueGameObjectsAtTop, _srollDirectionType.Down, i);

                MoveUniqueItem(_uniqueGameObjectsAtTop, i, height, _uniqueGameObjectsAtTopStartPositions,
                    distance);
            }

        //Smart list items
        foreach (SmartListItem smartListItem in _smartListItems) MoveSmartItem(smartListItem, distance);

        //Unique bottom objects
        // if (_smartListBottomIndex < _contentListLenght) return;
        //
        // for (int i = 0; i < _uniqueGameObjectsAtBottom.Count; i++)
        // {
        //     if (i >= (_smartListBottomIndex - _contentListLenght))
        //     {
        //         _uniqueGameObjectsAtBottom[i].gameObject.SetActive(false);
        //         continue;
        //     }
        //
        //     float heightWithPadding = _uniqueGameObjectsAtBottom[i].rect.height + _verticalPadding;
        //
        //     if (_smartListBottomIndex == _contentListLenght && i == 0)
        //     {
        //         if (_smartListItems.Count != 0)
        //             heightWithPadding += _smartListItems[0].SelfRectTransform.rect.height + _verticalPadding;
        //         else if (_uniqueGameObjectsAtTop.Count != 0)
        //             heightWithPadding += _uniqueGameObjectsAtTop[0].rect.height + _verticalPadding;
        //         else
        //             heightWithPadding += _verticalPadding;
        //     }
        //     else
        //         heightWithPadding = _uniqueGameObjectsAtTop[i - 1].rect.height + _verticalPadding;
        //
        //     MoveUniqueItem(_uniqueGameObjectsAtBottom, i, heightWithPadding, _uniqueGameObjectsAtBottomStartPositions,
        //         distance);
        //}
    }

    private void MoveUniqueItem(List<RectTransform> rectTransforms, int index, float heightWithPadding, List<float> startPositions, float distance)
    {
        if (!rectTransforms[index].gameObject.activeSelf)
        {
            Vector3 oldPosition = rectTransforms[index].position;

            SetAnchoredPosition(rectTransforms[index], heightWithPadding);

            float driftCorrection = 1f - (Mathf.Abs(_velocity * 0.5f) / _content.rect.height);

            startPositions[index] = rectTransforms[index].position.y + (distance * -1f) /** driftCorrection*/;
            Debug.LogError("startPositions[index]: " + startPositions[index]);
            Debug.LogError("distance: " + distance);
            Debug.LogError("fix: " + driftCorrection);
        }

        MoveRectTransformPosition(rectTransforms[index], startPositions[index] + distance);

        bool outOfBounds = OutOfBounds(rectTransforms[index]);

        rectTransforms[index].gameObject.SetActive(!outOfBounds);
    }

    private void MoveSmartItem(SmartListItem smartListItem, float distance)
    {
        MoveRectTransformPosition(smartListItem.SelfRectTransform, smartListItem.YStartPosition + distance);

        //Out of bounds calculations.
        float smartItemBorderTop = smartListItem.SelfRectTransform.localPosition.y +
                                   HalfHeight(smartListItem.SelfRectTransform) * _smartItemTopStrenghtMultiplier;
        float smartItemBorderBottom = smartListItem.SelfRectTransform.localPosition.y -
                                      HalfHeight(smartListItem.SelfRectTransform) * _smartItemBottomStrenghtMultiplier;

        bool overTop = _contentBorderTop < smartItemBorderBottom && _velocity > 0;
        bool overBottom = _contentBorderBottom > smartItemBorderTop && _velocity < 0;
        bool outOfBounds = (overTop || overBottom);
        bool outOfRange = ((overTop && _smartListBottomIndex >= _contentListLenght) ||
                           (overBottom && _smartListTopIndex < 0));

        if (!outOfBounds || outOfRange) return;

        //Move item to opposite end.
        int outOfBoundsDirection = (_contentBorderTop < smartItemBorderBottom ? -1 : 1 );
        Vector3 oldStartPosition = smartListItem.SelfRectTransform.position;

        smartListItem.SelfRectTransform.anchoredPosition =
            new Vector2(smartListItem.SelfRectTransform.anchoredPosition.x,
                smartListItem.SelfRectTransform.anchoredPosition.y +
                (_smartListItemWithPadding * (_smartListItems.Count)) * outOfBoundsDirection);

        //Update SmartListItem & data.
        smartListItem.UpdateStartPosition(smartListItem.YStartPosition +
                                          (Vector3.Distance(oldStartPosition, smartListItem.SelfRectTransform.position))
                                          * outOfBoundsDirection);

        UpdateEdgeIndexes(outOfBoundsDirection);

        int targetIndex = overTop ? _smartListBottomIndex - _uniqueGameObjectsAtTop.Count : _smartListTopIndex;

        //Set or clear smart list item.
        if (targetIndex >= 0 && targetIndex < _contentListLenght)
            OnNewDataRequested?.Invoke(targetIndex);
        else
            smartListItem.SetVisibility(false);
    }

    private void UpdateEdgeIndexes(int outOfBoundsDirection)
    {
        if (_smartIndexUpdated) return;

        _smartListTopIndex += outOfBoundsDirection * -1;
        _smartListBottomIndex += outOfBoundsDirection * -1;
        _smartIndexUpdated = true;
    }
    #endregion

    #region Helper Functions

    private static void MoveRectTransformPosition(RectTransform rectTransform, float newPosition)
    {
        rectTransform.position = new Vector2(rectTransform.position.x, newPosition);
    }

    private static float HalfHeight(RectTransform rectTransform)
    {
        return rectTransform.rect.height * 0.5f;
    }

    private static void SetAnchoredPosition(RectTransform rectTransform, float sizeWithPadding, int number = 1)
    {
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, sizeWithPadding * number);

        //if (!rectTransform.gameObject.activeSelf) rectTransform.gameObject.SetActive(true);
    }

    private bool OutOfBounds(RectTransform rectTransform, float topMultiplier = 0f, float bottomMultiplier = 2f)
    {
        float smartItemBorderTop = rectTransform.localPosition.y +
                                   HalfHeight(rectTransform) * topMultiplier;
        float smartItemBorderBottom = rectTransform.localPosition.y -
                                      HalfHeight(rectTransform) * bottomMultiplier;

        bool overTop = _contentBorderTop < smartItemBorderBottom && _velocity > 0;
        bool overBottom = _contentBorderBottom > smartItemBorderTop && _velocity < 0;
        bool outOfBounds = (overTop || overBottom);

        if (!outOfBounds) return false;

        int outOfBoundsDirection = (_contentBorderTop < smartItemBorderBottom ? -1 : 1 );

        if (rectTransform.gameObject.activeSelf) UpdateEdgeIndexes(outOfBoundsDirection);

        return true;
    }

    private float GetSafeEdgeRectPosition(List<RectTransform> rectTransforms, _srollDirectionType checkDirection,
        int startIndex)
    {
        float direction = checkDirection == _srollDirectionType.Up ? 1f : -1f;
        float totalDistance = 0f;
        int index = startIndex;

        while (checkDirection == _srollDirectionType.Up ? (index >= 0) : (index < rectTransforms.Count - 1))
        {
            totalDistance +=  rectTransforms[index].rect.height + _verticalPadding;
            index++;
        }

        return totalDistance * direction;
    }
    #endregion
}
