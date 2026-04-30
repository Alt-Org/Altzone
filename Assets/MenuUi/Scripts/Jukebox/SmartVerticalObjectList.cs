using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Altzone.Scripts.Audio;

/// <summary>
/// Recycles a limited amount of gameobjects by moving and repurposing out of bounds gameobjects for the other end that is coming out of the invisible area.
/// </summary>
public class SmartVerticalObjectList : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private float _anchoredVelocityLimit = 100f;
    [SerializeField] private float _worldVelocityLimit = 100f;
    private bool _elastic = true; //Not in use!
    private float _elasticityRange = 20f; //Not in use!
    [SerializeField] private float _slowdownTime = 1.5f;
    [Space]
    [SerializeField] private RectTransform _viewport;
    [SerializeField] private RectTransform _content;
    [Space]
    [SerializeField] private List<RectTransform> _uniqueGameObjectsAtTop = new();
    [SerializeField] private GameObject _contentPrefab;
    private List<RectTransform> _uniqueGameObjectsAtBottom = new(); //Not in use!
    [Space]
    [SerializeField] private float _smartItemTopStrenghtMultiplier = 0f;
    [SerializeField] private float _smartItemBottomStrenghtMultiplier = 2f;
    [Tooltip("Use to prevent pop in of the items.")]
    [SerializeField] private int _extraSmartListItems = 1;
    [SerializeField] private float _ignoreRemainingVelocityTriggerTime = 0.1f;
    [Space]
    //[SerializeField] private int _horizontalPadding = 10;
    [SerializeField] private int _verticalPadding = 10;

    //private bool _dragActive = false;

    private readonly List<SmartListItem> _smartListItems = new();

    private int _contentListLenght = -1;
    private int _smartListBottomIndex = -1;
    private int _smartListTopIndex = -1; //-1 and below are for _uniqueGameObjectsAtTop.

    private float _viewportBottomAnchoredBorder = 0f;
    private float _viewportTopAnchoredBorder = 0f;
    private float _viewportBottomWorldBorder = 0f;
    private float _viewportTopWorldBorder = 0f;

    private float _velocity = 0f;
    private float _scrollDiffCompensation = 0f;
    private Coroutine _velocityCoroutine;
    private Vector2 _previousUpdatePosition;
    private Vector2 _pointerStartPosition;
    private Vector2 _contentStartAnchoredPosition;
    private Vector2 _contentStartWorldPosition;
    private float _timeFromLastVelocityUpdate = 0f;

    private int _amountToFillContentList = 0;
    private float _smartListItemLocalHeightWithPadding = 0f;
    private float _bottomItemWorldHeightWithPadding = 0f;

    private VerticalDirectionType _outOfBoundDirection = VerticalDirectionType.Neutral;

    private RectTransform _locationHelper;

    private bool _buildOnEnable = false;

    public delegate void NewDataRequested(int targetIndex);
    public event NewDataRequested OnNewDataRequested; //Used to tell the host that new content data is needed.

    public delegate void LateDataRequest();
    public event LateDataRequest OnLateDataRequest; //Used to get data for the list when this list is enabled.

    private enum VerticalDirectionType
    {
        Neutral = 0,
        Up = 1,
        Down = -1
    }

    private void Awake()
    {
        if (_smartListItems.Count == 0) CreatePool();

        _contentStartAnchoredPosition = _content.anchoredPosition;
    }

    private void Start() { if (!_locationHelper) CreateLocationHelper(); }

    private void OnEnable()
    {
        if (!_buildOnEnable) return;

        _buildOnEnable = false;
        OnLateDataRequest?.Invoke();
    }

    private void CreateLocationHelper()
    {
        GameObject locationHelper = new GameObject("LocationHelper");

        _locationHelper = locationHelper.AddComponent<RectTransform>();
        _locationHelper.SetParent(_viewport);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _pointerStartPosition = eventData.position;
        _contentStartWorldPosition = _content.position;
        _scrollDiffCompensation = 0f;

        if (_velocityCoroutine == null) return;

        StopCoroutine(_velocityCoroutine);
        _velocityCoroutine = null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        _velocity = eventData.position.y - _previousUpdatePosition.y;

        if (Mathf.Abs(_velocity) > _anchoredVelocityLimit) _velocity = _anchoredVelocityLimit * (float.IsNegative(_velocity) ? -1f : 1f);

        ScrollHandling(eventData.position.y - _pointerStartPosition.y + _scrollDiffCompensation);
        _previousUpdatePosition = eventData.position;

        if (_outOfBoundDirection != VerticalDirectionType.Neutral)
        {
            _pointerStartPosition = eventData.position;
            _contentStartWorldPosition = _content.position;
        }

        _timeFromLastVelocityUpdate = Time.time;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _pointerStartPosition = eventData.position;
        _contentStartWorldPosition = _content.position;
        _scrollDiffCompensation = 0f;

        if (_ignoreRemainingVelocityTriggerTime > (Time.time - _timeFromLastVelocityUpdate)) _velocityCoroutine = StartCoroutine(HandleVelocity());
    }

    #region Data
    public void Setup<T>(List<T> data)
    {
        if (!isActiveAndEnabled || _buildOnEnable)
        {
            _buildOnEnable = true;
            return;
        }

        if (!_locationHelper) CreateLocationHelper();

        _contentListLenght = data.Count;

        _viewportTopAnchoredBorder = HalfHeight(_viewport);
        _viewportBottomAnchoredBorder = -HalfHeight(_viewport);

        _locationHelper.anchoredPosition = new Vector2(0f, _viewportTopAnchoredBorder);
        _viewportTopWorldBorder = _locationHelper.position.y;

        _locationHelper.anchoredPosition = new Vector2(0f, _viewportBottomAnchoredBorder);
        _viewportBottomWorldBorder = _locationHelper.position.y;

        _smartListTopIndex = -_uniqueGameObjectsAtTop.Count;
        _smartListBottomIndex = _smartListItems.Count - _extraSmartListItems - _uniqueGameObjectsAtTop.Count;

        if (_smartListItems.Count != _contentListLenght) CreatePool();

        UpdateContents<T>(data);

        RectTransform rectTransform = GetBottomItemRectTransform();

        if (rectTransform)
        {
            _locationHelper.anchoredPosition = new Vector2(0f, rectTransform.anchoredPosition.y + rectTransform.rect.height + _verticalPadding);
            float worldBorderTopPosition = _locationHelper.position.y;

            _locationHelper.anchoredPosition = new Vector2(0f, rectTransform.anchoredPosition.y);
            float worldBorderBottomPosition = _locationHelper.position.y;

            _bottomItemWorldHeightWithPadding = worldBorderTopPosition - worldBorderBottomPosition;
        }
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

        //Set all generated smart items.
        for (int i = 0; i < _smartListItems.Count; i++)
        {
            int smartIndex = i % _smartListItems.Count;
            int positionIndex = (i + _uniqueGameObjectsAtTop.Count) % _smartListItems.Count;

            SetAnchoredPosition(_smartListItems[smartIndex].SelfRectTransform, -_smartListItemLocalHeightWithPadding, positionIndex);

            if (i < _smartListTopIndex || i > _smartListBottomIndex || smartPositionIndexesUsed >= _smartListItems.Count || i >= data.Count)
            {
                _smartListItems[smartIndex].SetVisibility(false);
                continue;
            }

            _smartListItems[smartIndex].SetData<T>(data[i]);
            smartPositionIndexesUsed++;
        }
    }

    public void Clear() { foreach (var smartListItem in _smartListItems) smartListItem.ClearData(); }

    private void CreatePool()
    {
        //Create first SmartListItem to calculate how many of it can fit inside based on the height + padding.
        if (_smartListItems.Count == 0)
        {
            SmartListItem firstSmartListItem = Instantiate(_contentPrefab, _content).GetComponent<SmartListItem>();

            if (!firstSmartListItem.SelfRectTransform) firstSmartListItem.SetSelfRectTransform();

            _smartListItemLocalHeightWithPadding = firstSmartListItem.SelfRectTransform.rect.height + _verticalPadding;
            firstSmartListItem.SelfRectTransform.sizeDelta = new Vector2(_content.rect.width,
                firstSmartListItem.SelfRectTransform.sizeDelta.y);

            firstSmartListItem.ClearData();
            _smartListItems.Add(firstSmartListItem);
        }
        else
        {
            _smartListItemLocalHeightWithPadding = _smartListItems[0].SelfRectTransform.rect.height + _verticalPadding;
        }

        _amountToFillContentList = (int)MathF.Ceiling(_content.rect.height / _smartListItemLocalHeightWithPadding) + _extraSmartListItems;

        if (_amountToFillContentList <= _smartListItems.Count) return;

        _amountToFillContentList -= _smartListItems.Count;

        //Create the rest of the SmartListItem's.
        for (int i = 0; i < _amountToFillContentList; i++)
        {
            SmartListItem smartListItem = Instantiate(_contentPrefab, _content).GetComponent<SmartListItem>();

            if (!smartListItem.SelfRectTransform) smartListItem.SetSelfRectTransform();

            smartListItem.SelfRectTransform.sizeDelta = new Vector2(_content.rect.width,
                smartListItem.SelfRectTransform.sizeDelta.y);

            smartListItem.ClearData();
            _smartListItems.Add(smartListItem);
        }
    }
    #endregion

    #region Movement
    private IEnumerator HandleVelocity()
    {
        float totalDistance = 0f;
        float timer = 0f;

        while (timer < _slowdownTime)
        {
            totalDistance += Mathf.Lerp(_velocity, 0f, timer / _slowdownTime);

            ScrollHandling(totalDistance);
            yield return null;
            timer += Time.deltaTime;
        }
    }

    private void ScrollHandling(float distance) //TODO: Add elasticity.
    {
        if (_smartListItems.Count > _contentListLenght) return; //Nothing or not enough content to scroll.

        bool movementCheck = ((_outOfBoundDirection == VerticalDirectionType.Up && _velocity < 0) ||
                              (_outOfBoundDirection == VerticalDirectionType.Down && _velocity > 0));

        if (!movementCheck) _content.position = new Vector2(_content.position.x, _contentStartWorldPosition.y + distance);

        float anchoredDistance = _content.anchoredPosition.y - _contentStartAnchoredPosition.y;

        if (!movementCheck) _outOfBoundDirection = ScrollLimiter(distance, anchoredDistance);

        //Out of bounds checks.
        //Unique top objects
        if (_smartListTopIndex <= 0)
            for (int i = _uniqueGameObjectsAtTop.Count - 1; i >= 0; i--)
            {
                if (i < (_uniqueGameObjectsAtTop.Count + _smartListTopIndex))
                {
                    _uniqueGameObjectsAtTop[i].gameObject.SetActive(false);
                    continue;
                }

                CheckUniqueItemVisibility(_uniqueGameObjectsAtTop, i, VerticalDirectionType.Up);
            }

        //Smart list items
        foreach (SmartListItem smartListItem in _smartListItems) CheckSmartItemVisibility(smartListItem, anchoredDistance);

        //TODO: WIP
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

    /// <summary>
    /// Sets content to the edge position that was crossed over.
    /// </summary>
    /// <returns>VerticalDirectionType in which there was no more content to be displayed.</returns>
    private VerticalDirectionType ScrollLimiter(float worldDistance, float anchoredDistance)
    {
        if (_smartListTopIndex * -1 - _uniqueGameObjectsAtTop.Count == 0 && _velocity < 0) //Over top check.
        {
            float topItemTopEdge = GetTopItemEdgeLocalPositionY() + anchoredDistance;

            if (_viewportTopAnchoredBorder < topItemTopEdge) return VerticalDirectionType.Neutral;

            float correction = (_viewportTopWorldBorder - GetTopItemWorldPositionY());

            _content.position = new Vector2(_content.position.x, _contentStartWorldPosition.y + worldDistance + correction);

            return VerticalDirectionType.Up;
        }
        if (_smartListBottomIndex == (_contentListLenght + _uniqueGameObjectsAtBottom.Count) && _velocity > 0) //Over bottom check.
        {
            float bottomItemTopEdge = GetBottomItemEdgeLocalPositionY() + anchoredDistance - _content.rect.height / 2f;

            if (_viewportBottomAnchoredBorder > bottomItemTopEdge) return VerticalDirectionType.Neutral;

            float correction = GetBottomItemWorldPositionY() - _viewportBottomWorldBorder -
                               _bottomItemWorldHeightWithPadding - _verticalPadding;

            _content.position = new Vector2(_content.position.x, _contentStartWorldPosition.y + worldDistance - correction);

            return VerticalDirectionType.Down;
        }

        return VerticalDirectionType.Neutral;
    }

    private RectTransform GetBottomItemRectTransform()
    {
        if (_uniqueGameObjectsAtBottom.Count != 0) return _uniqueGameObjectsAtBottom[^1];

        if (_smartListItems.Count != 0) return _smartListItems[^1].SelfRectTransform;

        if (_uniqueGameObjectsAtTop.Count != 0) return _uniqueGameObjectsAtTop[^1];

        return null;
    }

    private float GetTopItemEdgeLocalPositionY()
    {
        if (_uniqueGameObjectsAtTop.Count != 0)
            return _uniqueGameObjectsAtTop[0].localPosition.y + HalfHeight(_uniqueGameObjectsAtTop[0]) * _smartItemTopStrenghtMultiplier;
        if (_smartListItems.Count != 0)
            return _smartListItems[0].SelfRectTransform.localPosition.y + HalfHeight(_smartListItems[0].SelfRectTransform) * _smartItemTopStrenghtMultiplier;
        if (_uniqueGameObjectsAtBottom.Count != 0)
            return _uniqueGameObjectsAtBottom[0].localPosition.y + HalfHeight(_uniqueGameObjectsAtBottom[0]) * _smartItemTopStrenghtMultiplier;

        return 0f;
    }

    private float GetTopItemWorldPositionY()
    {
        if (_uniqueGameObjectsAtTop.Count != 0)
            return _uniqueGameObjectsAtTop[0].position.y;
        if (_smartListItems.Count != 0)
            return _smartListItems[0].SelfRectTransform.position.y;
        if (_uniqueGameObjectsAtBottom.Count != 0)
            return _uniqueGameObjectsAtBottom[0].position.y;

        return 0f;
    }

    private float GetBottomItemEdgeLocalPositionY()
    {
        if (_uniqueGameObjectsAtBottom.Count != 0)
            return _uniqueGameObjectsAtBottom[^1].localPosition.y + _uniqueGameObjectsAtBottom[^1].rect.height;
        if (_smartListItems.Count != 0)
        {
            int smartIndex = (_smartListBottomIndex - _extraSmartListItems - 1) % _smartListItems.Count;

            return _smartListItems[smartIndex].SelfRectTransform.localPosition.y + _smartListItems[smartIndex].SelfRectTransform.rect.height;
        }
        if (_uniqueGameObjectsAtTop.Count != 0)
            return _uniqueGameObjectsAtTop[^1].localPosition.y + _uniqueGameObjectsAtTop[^1].rect.height;

        return 0f;
    }

    private float GetBottomItemWorldPositionY()
    {
        if (_uniqueGameObjectsAtBottom.Count != 0)
            return _uniqueGameObjectsAtBottom[^1].position.y;

        if (_smartListItems.Count != 0)
        {
            int smartIndex = (_smartListItems.Count > _smartListBottomIndex ? _smartListBottomIndex :
                _smartListBottomIndex - _extraSmartListItems) % _smartListItems.Count;

            return _smartListItems[smartIndex].SelfRectTransform.position.y;
        }
        if (_uniqueGameObjectsAtTop.Count != 0)
            return _uniqueGameObjectsAtTop[^1].position.y;

        return 0f;
    }

    private void CheckUniqueItemVisibility(List<RectTransform> rectTransforms, int index, VerticalDirectionType listDirection)
    {
        VerticalDirectionType outOfBoundsDirection = OutOfBoundsVerticalCheck(rectTransforms[index]);

        if (outOfBoundsDirection == VerticalDirectionType.Neutral)
        {
            if (!rectTransforms[index].gameObject.activeSelf)
            {
                float adjacentItem = GetAdjacentItemVerticalLocation(listDirection);

                rectTransforms[index].anchoredPosition = new Vector2(rectTransforms[index].anchoredPosition.x,
                    adjacentItem + (_smartListItemLocalHeightWithPadding * (_smartListItems.Count)) * (int)outOfBoundsDirection);
            }

            rectTransforms[index].gameObject.SetActive(true);
            return;
        }

        if (rectTransforms[index].gameObject.activeSelf) UpdateEdgeIndexes(outOfBoundsDirection);

        rectTransforms[index].gameObject.SetActive(false);
    }

    private float GetAdjacentItemVerticalLocation(VerticalDirectionType selfDirection)
    {
        if (selfDirection == VerticalDirectionType.Up)
        {
            if (_smartListItems.Count != 0 && _smartListTopIndex == 0)
                return _smartListItems[0].SelfRectTransform.anchoredPosition.y;

            if (_smartListTopIndex >= 0 && _uniqueGameObjectsAtBottom.Count != 0)
                return _uniqueGameObjectsAtBottom[0].anchoredPosition.y;

            return _uniqueGameObjectsAtTop[_smartListTopIndex + _uniqueGameObjectsAtTop.Count].anchoredPosition.y;
        }

        //Down
        if (_smartListItems.Count != 0 && _smartListBottomIndex == _contentListLenght - 1)
            return _smartListItems[^1].SelfRectTransform.anchoredPosition.y;

        if (_smartListBottomIndex == _contentListLenght - 1 && _uniqueGameObjectsAtTop.Count != 0)
            return _uniqueGameObjectsAtTop[^1].anchoredPosition.y;

        return _uniqueGameObjectsAtBottom[_smartListBottomIndex].anchoredPosition.y;
    }

    private float GetCurrentEdgeItemHeight(VerticalDirectionType scrollDirection)
    {
        if (scrollDirection == VerticalDirectionType.Up)
        {
            if (_smartListItems.Count != 0 && _smartListBottomIndex < _smartListItems.Count)
                return _smartListItems[_smartListBottomIndex].SelfRectTransform.rect.height;

            if (_uniqueGameObjectsAtBottom.Count != 0 && _smartListBottomIndex >= _contentListLenght)
                return _uniqueGameObjectsAtBottom[_smartListBottomIndex - _contentListLenght].rect.height;

            return 0f;
        }

        if (_smartListItems.Count != 0 && _smartListTopIndex >= 0 && _smartListTopIndex < _smartListItems.Count)
            return _smartListItems[_smartListTopIndex].SelfRectTransform.rect.height;

        if (_uniqueGameObjectsAtTop.Count != 0 && _smartListTopIndex < 0)
            return _uniqueGameObjectsAtTop[_smartListTopIndex + _uniqueGameObjectsAtTop.Count].rect.height;

        return 0f;
    }

    private void CheckSmartItemVisibility(SmartListItem smartListItem, float anchoredDistance)
    {
        RectTransform rectTransform = smartListItem.SelfRectTransform;

        //Out of bounds calculations.
        float smartItemBorderTop = rectTransform.localPosition.y + anchoredDistance;
        float smartItemBorderBottom = rectTransform.localPosition.y - rectTransform.rect.height + anchoredDistance;

        bool overTop = _viewportTopAnchoredBorder < smartItemBorderBottom && _velocity > 0;
        bool overBottom = _viewportBottomAnchoredBorder > smartItemBorderTop && _velocity < 0;
        bool outOfBounds = (overTop || overBottom);
        bool outOfRange = ((overTop && _smartListBottomIndex >= _contentListLenght) ||
                           (overBottom && _smartListTopIndex < 0));

        if (outOfBounds && !outOfRange) smartListItem.SetVisibility(false);

        if (!outOfBounds || outOfRange) return;

        //Move item to opposite end.
        VerticalDirectionType outOfBoundsDirection =
            (_viewportTopAnchoredBorder < smartItemBorderBottom ? VerticalDirectionType.Down : VerticalDirectionType.Up);

        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y +
            (_smartListItemLocalHeightWithPadding * (_smartListItems.Count)) * (int)outOfBoundsDirection);

        //Update SmartListItem & data.
        UpdateEdgeIndexes(outOfBoundsDirection);

        int targetIndex = overTop ? _smartListBottomIndex - _uniqueGameObjectsAtTop.Count : _smartListTopIndex;

        //Set or clear smart list item.
        if (targetIndex >= 0 && targetIndex < _contentListLenght)
            OnNewDataRequested?.Invoke(targetIndex);
        else
            smartListItem.SetVisibility(false);
    }

    private void UpdateEdgeIndexes(VerticalDirectionType outOfBoundsDirection)
    {
        _smartListTopIndex += (int)outOfBoundsDirection * -1;
        _smartListBottomIndex += (int)outOfBoundsDirection * -1;
    }
    #endregion

    #region Helper Functions

    private static float HalfHeight(RectTransform rectTransform) { return rectTransform.rect.height * 0.5f; }

    private static void SetAnchoredPosition(RectTransform rectTransform, float sizeWithPadding, int number = 1)
    {
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, sizeWithPadding * number);
    }

    private VerticalDirectionType OutOfBoundsVerticalCheck(RectTransform rectTransform)
    {
        float smartItemBorderTop = rectTransform.localPosition.y;
        float smartItemBorderBottom = rectTransform.localPosition.y - rectTransform.rect.height;

        bool overTop = _viewportTopAnchoredBorder < smartItemBorderBottom && _velocity > 0;
        bool overBottom = _viewportBottomAnchoredBorder > smartItemBorderTop && _velocity < 0;
        bool outOfBounds = (overTop || overBottom);

        if (!outOfBounds) return VerticalDirectionType.Neutral;

        return (_viewportTopAnchoredBorder < smartItemBorderBottom ? VerticalDirectionType.Down : VerticalDirectionType.Up);
    }
    #endregion
}
