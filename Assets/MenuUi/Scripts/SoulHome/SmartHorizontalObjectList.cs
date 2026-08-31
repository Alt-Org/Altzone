using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Altzone.Scripts.Audio;

/// <summary>
/// Recycles a limited amount of gameobjects by moving and repurposing out of bounds gameobjects for the other end that is coming out of the invisible area.
/// </summary>
public class SmartHorizontalObjectList : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private float _anchoredVelocityLimit = 100f;
    [SerializeField] private float _worldVelocityLimit = 100f;
    
    [SerializeField] private float _slowdownTime = 1.5f;
    [Space]
    [SerializeField] private RectTransform _viewport;
    [SerializeField] private RectTransform _content;
    [Space]
    [SerializeField] private List<RectTransform> _uniqueGameObjectsAtLeft = new();
    [SerializeField] private GameObject _contentPrefab;
    private List<RectTransform> _uniqueGameObjectsAtRight = new();
    [Space]
    [SerializeField] private float _smartItemLeftStrenghtMultiplier = 0f;
    [SerializeField] private float _smartItemRightStrenghtMultiplier = 2f;
    [Tooltip("Use to prevent pop in of the items.")]
    [SerializeField] private int _extraSmartListItems = 1;
    [SerializeField] private float _ignoreRemainingVelocityTriggerTime = 0.1f;
    [Space]
    [SerializeField] private int _horizontalPadding = 10;

    private readonly List<SmartListItem> _smartListItems = new();

    private int _contentListLenght = -1;
    private int _smartListRightIndex = -1;
    private int _smartListLeftIndex = -1; //-1 and below are for _uniqueGameObjectsAtLeft.

    private float _viewportRightAnchoredBorder = 0f;
    private float _viewportLeftAnchoredBorder = 0f;
    private float _viewportRightWorldBorder = 0f;
    private float _viewportLeftWorldBorder = 0f;

    private float _velocity = 0f;
    private float _averageVelocity = 0f;
    private const float _averageVelocityNormalizationTime = 3f;
    private const float _averageVelocityUpdateTreshold = 0.1f;
    private float _scrollDiffCompensation = 0f;
    private Coroutine _velocityCoroutine;
    private Vector2 _previousUpdatePosition;
    private Vector2 _pointerStartPosition;
    private Vector2 _contentStartAnchoredPosition;
    private Vector2 _contentStartWorldPosition;
    private float _timeFromLastVelocityUpdate = 0f;

    private int _amountToFillContentList = 0;
    private float _smartListItemLocalWidthWithPadding = 0f;
    private float _rightItemWorldWidthWithPadding = 0f;

    private HorizontalDirectionType _outOfBoundDirection = HorizontalDirectionType.Neutral;

    private RectTransform _locationHelper;

    private bool _buildOnEnable = false;

    public delegate void NewDataRequested(int targetIndex);
    public event NewDataRequested OnNewDataRequested; //Used to tell the host that new content data is needed.

    public delegate void LateDataRequest();
    public event LateDataRequest OnLateDataRequest; //Used to get data for the list when this list is enabled.

    //public List<GameObject> children { get => _content.gameObject.GetChildren();} // ---------------------------------------------------

    public List<GameObject> children // --------------------------------------
    {
        get
        {
            List<GameObject> result = new();

            for (int i = 0; i < _content.childCount; i++)
            {
                result.Add(_content.GetChild(i).gameObject);
            }

            return result;
        }
    }

    private enum HorizontalDirectionType
    {
        Neutral = 0,
        Left = -1,
        Right = 1
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
        _previousUpdatePosition = eventData.position;
        _contentStartWorldPosition = _content.position;
        _scrollDiffCompensation = 0f;

        if (_velocityCoroutine == null) return;

        StopCoroutine(_velocityCoroutine);
        _velocityCoroutine = null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        _velocity = eventData.position.x - _previousUpdatePosition.x;

        if (Mathf.Abs(_velocity) > _anchoredVelocityLimit)
            _velocity = _anchoredVelocityLimit * Mathf.Sign(_velocity);

        ScrollHandling(eventData.position.x - _pointerStartPosition.x + _scrollDiffCompensation);
        _previousUpdatePosition = eventData.position;

        if (_outOfBoundDirection != HorizontalDirectionType.Neutral)
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
        Debug.Log("setup in smarthorizontal -------------------------------------------------------------------------------------------------------------------------------------");
        //if (!isActiveAndEnabled || _buildOnEnable)
        //{
        if (!isActiveAndEnabled)
        {
            Debug.Log("(!isActiveAndEnabled) in smarthorizontal -------------------------------------------------------------------------------------------------------------------------------------");
        }
        if (_buildOnEnable)
        {
            Debug.Log("(_buildOnEnable) in smarthorizontal -------------------------------------------------------------------------------------------------------------------------------------");
            _buildOnEnable = true;
            return;
        }
        //}

        if (!_locationHelper) CreateLocationHelper();

        _contentListLenght = data.Count;
        Debug.Log("_contentListLenght in smarthorizontal -------------------------------------------------------------------------------------------------------------------------------------" + _contentListLenght);

        _viewportLeftAnchoredBorder = -HalfWidth(_viewport);
        _viewportRightAnchoredBorder = HalfWidth(_viewport);

        _locationHelper.anchoredPosition = new Vector2(_viewportLeftAnchoredBorder, 0f);
        _viewportLeftWorldBorder = _locationHelper.position.x;

        _locationHelper.anchoredPosition = new Vector2(_viewportRightAnchoredBorder, 0f);
        _viewportRightWorldBorder = _locationHelper.position.x;

        _smartListLeftIndex = -_uniqueGameObjectsAtLeft.Count;
        _smartListRightIndex = _smartListItems.Count - _extraSmartListItems - _uniqueGameObjectsAtLeft.Count;

        if (_smartListItems.Count != _contentListLenght) CreatePool();

        UpdateContents<T>(data);

        RectTransform rectTransform = GetRightItemRectTransform();

        if (rectTransform)
        {
            _locationHelper.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x + rectTransform.rect.width + _horizontalPadding, 0f);
            float worldBorderRightPosition = _locationHelper.position.x;

            _locationHelper.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 0f);
            float worldBorderLeftPosition = _locationHelper.position.x;

            _rightItemWorldWidthWithPadding = worldBorderRightPosition - worldBorderLeftPosition;
        }
    }

    public void UpdateContent<T>(int index, T data)
    {
        Debug.Log("update data in amrthorizontal -------------------------------------------------------------------------------------------------------------------------------------");
        if (index < _smartListLeftIndex || index > _smartListRightIndex) return;

        int smartIndex = index % _smartListItems.Count;

        _smartListItems[smartIndex].SetVisibility(true);
        _smartListItems[smartIndex].SetData<T>(data);
    }

    private void UpdateContents<T>(List<T> data)
    {
        int smartPositionIndexesUsed = 0;

        //Set positions.
        for (int mainIndex = -_uniqueGameObjectsAtLeft.Count; mainIndex < _contentListLenght + _uniqueGameObjectsAtRight.Count; mainIndex++)
        {
            int rightIndex = mainIndex - _contentListLenght;

            if (mainIndex < 0) //Set left unique items.
            {
                int leftIndex = mainIndex + _uniqueGameObjectsAtLeft.Count;

                if (mainIndex < _smartListLeftIndex || mainIndex > _smartListRightIndex)
                {
                    _uniqueGameObjectsAtLeft[leftIndex].gameObject.SetActive(false);
                    continue;
                }

                float widthWithPadding = _uniqueGameObjectsAtLeft[leftIndex].rect.width + _horizontalPadding;

                SetAnchoredPosition(_uniqueGameObjectsAtLeft[leftIndex], widthWithPadding, leftIndex);
                smartPositionIndexesUsed++;
            }
            else if (rightIndex < _uniqueGameObjectsAtRight.Count && mainIndex >= _contentListLenght) //Set right unique items.
            {
                if (mainIndex < _smartListLeftIndex || mainIndex > _smartListRightIndex)
                {
                    _uniqueGameObjectsAtRight[rightIndex].gameObject.SetActive(false);
                    continue;
                }

                int positionIndex = mainIndex + _uniqueGameObjectsAtLeft.Count;
                float widthWithPadding = _uniqueGameObjectsAtRight[rightIndex].rect.width + _horizontalPadding;

                SetAnchoredPosition(_uniqueGameObjectsAtRight[rightIndex], widthWithPadding, positionIndex);
            }
        }

        //Set all generated smart items.
        for (int i = 0; i < _smartListItems.Count; i++)
        {
            int smartIndex = i % _smartListItems.Count;
            int positionIndex = (i + _uniqueGameObjectsAtLeft.Count) % _smartListItems.Count;

            SetAnchoredPosition(_smartListItems[smartIndex].SelfRectTransform, _smartListItemLocalWidthWithPadding, positionIndex);

            if (i < _smartListLeftIndex || i > _smartListRightIndex || smartPositionIndexesUsed >= _smartListItems.Count || i >= data.Count)
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
        //Create first SmartListItem to calculate how many of it can fit inside based on the width + padding.
        if (_smartListItems.Count == 0)
        {
            SmartListItem firstSmartListItem = Instantiate(_contentPrefab, _content).GetComponent<SmartListItem>();

            if (!firstSmartListItem.SelfRectTransform) firstSmartListItem.SetSelfRectTransform(); // null reference -----------------------

            _smartListItemLocalWidthWithPadding = firstSmartListItem.SelfRectTransform.rect.width + _horizontalPadding;
            firstSmartListItem.SelfRectTransform.sizeDelta = new Vector2(
                firstSmartListItem.SelfRectTransform.sizeDelta.x, _content.rect.height);

            firstSmartListItem.ClearData();
            _smartListItems.Add(firstSmartListItem);
        }
        else
        {
            _smartListItemLocalWidthWithPadding = _smartListItems[0].SelfRectTransform.rect.width + _horizontalPadding;
        }

        _amountToFillContentList = Mathf.CeilToInt(_content.rect.width / _smartListItemLocalWidthWithPadding) + _extraSmartListItems;

        if (_amountToFillContentList <= _smartListItems.Count) return;

        _amountToFillContentList -= _smartListItems.Count;

        //Create the rest of the SmartListItem's.
        for (int i = 0; i < _amountToFillContentList; i++)
        {
            SmartListItem smartListItem = Instantiate(_contentPrefab, _content).GetComponent<SmartListItem>();

            if (!smartListItem.SelfRectTransform) smartListItem.SetSelfRectTransform();

            smartListItem.SelfRectTransform.sizeDelta = new Vector2(
                smartListItem.SelfRectTransform.sizeDelta.x, _content.rect.height);

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

        _velocity = 0f;
    }

    private void ScrollHandling(float distance) //TODO: Add elasticity.
    {
        if (_smartListItems.Count > _contentListLenght) return; //Nothing or not enough content to scroll.

        bool movementCheck = ((_outOfBoundDirection == HorizontalDirectionType.Left && _velocity > 0) ||
                              (_outOfBoundDirection == HorizontalDirectionType.Right && _velocity < 0));

        if (!movementCheck) _content.position = new Vector2(_contentStartWorldPosition.x + distance, _content.position.y);

        float anchoredDistance = _content.anchoredPosition.x - _contentStartAnchoredPosition.x;

        if (!movementCheck) _outOfBoundDirection = ScrollLimiter(distance, anchoredDistance);

        //Out of bounds checks.
        //Unique left objects
        if (_smartListLeftIndex <= 0)
            for (int i = _uniqueGameObjectsAtLeft.Count - 1; i >= 0; i--)
            {
                if (i < (_uniqueGameObjectsAtLeft.Count + _smartListLeftIndex))
                {
                    _uniqueGameObjectsAtLeft[i].gameObject.SetActive(false);
                    continue;
                }

                CheckUniqueItemVisibility(_uniqueGameObjectsAtLeft, i, HorizontalDirectionType.Left);
            }

        //Smart list items
        foreach (SmartListItem smartListItem in _smartListItems) CheckSmartItemVisibility(smartListItem, anchoredDistance);
    }

    /// <summary>
    /// Sets content to the edge position that was crossed over.
    /// </summary>
    /// <returns>HorizontalDirectionType in which there was no more content to be displayed.</returns>
    private HorizontalDirectionType ScrollLimiter(float worldDistance, float anchoredDistance)
    {
        if ((_smartListLeftIndex * -1 - _uniqueGameObjectsAtLeft.Count <= 0 && _uniqueGameObjectsAtLeft.Count != 0 ||
             _uniqueGameObjectsAtLeft.Count == 0 && _smartListLeftIndex < 0) && _velocity > 0) //Over left check.
        {
            float leftItemLeftEdge = GetLeftItemEdgeLocalPositionX() + anchoredDistance;

            if (_viewportLeftAnchoredBorder > leftItemLeftEdge) return HorizontalDirectionType.Neutral;

            float correction = (_viewportLeftWorldBorder - GetLeftItemWorldPositionX());

            _content.position = new Vector2(_contentStartWorldPosition.x + worldDistance + correction, _content.position.y);

            return HorizontalDirectionType.Left;
        }

        if ((_smartListRightIndex >= _contentListLenght + _uniqueGameObjectsAtRight.Count && _uniqueGameObjectsAtRight.Count != 0 ||
             _uniqueGameObjectsAtRight.Count == 0 && _smartListRightIndex >= _contentListLenght) && _velocity < 0) //Over right check.
        {
            float rightItemRightEdge = GetRightItemEdgeLocalPositionX() + anchoredDistance;

            if (_viewportRightAnchoredBorder < rightItemRightEdge) return HorizontalDirectionType.Neutral;

            float correction = GetRightItemWorldPositionX() - _viewportRightWorldBorder +
                               _rightItemWorldWidthWithPadding - _horizontalPadding;

            _content.position = new Vector2(_contentStartWorldPosition.x + worldDistance - correction, _content.position.y);

            return HorizontalDirectionType.Right;
        }

        return HorizontalDirectionType.Neutral;
    }

    private RectTransform GetRightItemRectTransform()
    {
        if (_uniqueGameObjectsAtRight.Count != 0) return _uniqueGameObjectsAtRight[^1];

        if (_smartListItems.Count != 0 && _contentListLenght > 0)
        {
            int smartIndex = (_contentListLenght - 1) % _smartListItems.Count;
            return _smartListItems[smartIndex].SelfRectTransform;
        }

        if (_uniqueGameObjectsAtLeft.Count != 0) return _uniqueGameObjectsAtLeft[^1];

        return null;
    }

    private float GetLeftItemEdgeLocalPositionX()
    {
        if (_uniqueGameObjectsAtLeft.Count != 0)
            return _uniqueGameObjectsAtLeft[0].localPosition.x - HalfWidth(_uniqueGameObjectsAtLeft[0]) * _smartItemLeftStrenghtMultiplier;

        if (_smartListItems.Count != 0)
            return _smartListItems[0].SelfRectTransform.localPosition.x - HalfWidth(_smartListItems[0].SelfRectTransform) * _smartItemLeftStrenghtMultiplier;

        if (_uniqueGameObjectsAtRight.Count != 0)
            return _uniqueGameObjectsAtRight[0].localPosition.x - HalfWidth(_uniqueGameObjectsAtRight[0]) * _smartItemLeftStrenghtMultiplier;

        return 0f;
    }

    private float GetLeftItemWorldPositionX()
    {
        if (_uniqueGameObjectsAtLeft.Count != 0) return _uniqueGameObjectsAtLeft[0].position.x;

        if (_smartListItems.Count != 0) return _smartListItems[0].SelfRectTransform.position.x;

        if (_uniqueGameObjectsAtRight.Count != 0) return _uniqueGameObjectsAtRight[0].position.x;

        return 0f;
    }

    private float GetRightItemEdgeLocalPositionX()
    {
        RectTransform rectTransform = GetRightItemRectTransform();
        if (rectTransform) return rectTransform.localPosition.x + rectTransform.rect.width;

        return 0f;
    }

    private float GetRightItemWorldPositionX()
    {
        RectTransform rectTransform = GetRightItemRectTransform();
        if (rectTransform) return rectTransform.position.x;

        return 0f;
    }

    private void CheckUniqueItemVisibility(List<RectTransform> rectTransforms, int index, HorizontalDirectionType listDirection)
    {
        HorizontalDirectionType outOfBoundsDirection = OutOfBoundsHorizontalCheck(rectTransforms[index]);

        if (outOfBoundsDirection == HorizontalDirectionType.Neutral)
        {
            if (!rectTransforms[index].gameObject.activeSelf)
            {
                float adjacentItem = GetAdjacentItemHorizontalLocation(listDirection);

                rectTransforms[index].anchoredPosition = new Vector2(
                    adjacentItem + (_smartListItemLocalWidthWithPadding * _smartListItems.Count) * (int)outOfBoundsDirection,
                    rectTransforms[index].anchoredPosition.y);
            }

            rectTransforms[index].gameObject.SetActive(true);
            return;
        }

        if (rectTransforms[index].gameObject.activeSelf) UpdateEdgeIndexes(outOfBoundsDirection);

        rectTransforms[index].gameObject.SetActive(false);
    }

    private float GetAdjacentItemHorizontalLocation(HorizontalDirectionType selfDirection)
    {
        if (selfDirection == HorizontalDirectionType.Left)
        {
            if (_smartListItems.Count != 0 && _smartListLeftIndex == 0)
                return _smartListItems[0].SelfRectTransform.anchoredPosition.x;

            if (_smartListLeftIndex >= 0 && _uniqueGameObjectsAtRight.Count != 0)
                return _uniqueGameObjectsAtRight[0].anchoredPosition.x;

            return _uniqueGameObjectsAtLeft[_smartListLeftIndex + _uniqueGameObjectsAtLeft.Count].anchoredPosition.x;
        }

        //Right
        if (_smartListItems.Count != 0 && _smartListRightIndex == _contentListLenght - 1)
            return _smartListItems[^1].SelfRectTransform.anchoredPosition.x;

        if (_smartListRightIndex == _contentListLenght - 1 && _uniqueGameObjectsAtLeft.Count != 0)
            return _uniqueGameObjectsAtLeft[^1].anchoredPosition.x;

        return _uniqueGameObjectsAtRight[_smartListRightIndex].anchoredPosition.x;
    }

    private float GetCurrentEdgeItemWidth(HorizontalDirectionType scrollDirection)
    {
        if (scrollDirection == HorizontalDirectionType.Left)
        {
            if (_smartListItems.Count != 0 && _smartListRightIndex < _smartListItems.Count)
                return _smartListItems[_smartListRightIndex].SelfRectTransform.rect.width;

            if (_uniqueGameObjectsAtRight.Count != 0 && _smartListRightIndex >= _contentListLenght)
                return _uniqueGameObjectsAtRight[_smartListRightIndex - _contentListLenght].rect.width;

            return 0f;
        }

        if (_smartListItems.Count != 0 && _smartListLeftIndex >= 0 && _smartListLeftIndex < _smartListItems.Count)
            return _smartListItems[_smartListLeftIndex].SelfRectTransform.rect.width;

        if (_uniqueGameObjectsAtLeft.Count != 0 && _smartListLeftIndex < 0)
            return _uniqueGameObjectsAtLeft[_smartListLeftIndex + _uniqueGameObjectsAtLeft.Count].rect.width;

        return 0f;
    }

    private void CheckSmartItemVisibility(SmartListItem smartListItem, float anchoredDistance)
    {
        RectTransform rectTransform = smartListItem.SelfRectTransform;

        //Out of bounds calculations.
        float smartItemBorderLeft = rectTransform.localPosition.x + anchoredDistance;
        float smartItemBorderRight = rectTransform.localPosition.x + rectTransform.rect.width + anchoredDistance;

        if (Mathf.Abs(_averageVelocity - _velocity) > _averageVelocityUpdateTreshold)
            _averageVelocity = Mathf.Lerp(_velocity, _averageVelocity, Time.deltaTime / _averageVelocityNormalizationTime);

        bool overLeft = _viewportLeftAnchoredBorder > smartItemBorderRight && _averageVelocity < 0f;
        bool overRight = _viewportRightAnchoredBorder < smartItemBorderLeft && _averageVelocity > 0f;
        bool outOfBounds = (overLeft || overRight);
        bool outOfRange = ((overLeft && _smartListRightIndex >= _contentListLenght) ||
                           (overRight && _smartListLeftIndex < 0));

        if (!outOfBounds || outOfRange) return;

        //Move item to opposite end.
        HorizontalDirectionType outOfBoundsDirection =
            (overLeft ? HorizontalDirectionType.Right : HorizontalDirectionType.Left);

        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x +
            (_smartListItemLocalWidthWithPadding * _smartListItems.Count) * (int)outOfBoundsDirection,
            rectTransform.anchoredPosition.y);

        //Update SmartListItem & data.
        UpdateEdgeIndexes(outOfBoundsDirection);

        int targetIndex = overLeft ? _smartListRightIndex : _smartListLeftIndex;

        //Set or clear smart list item.
        if (targetIndex >= 0 && targetIndex < _contentListLenght)
            OnNewDataRequested?.Invoke(targetIndex);
        else
            smartListItem.ClearData();
    }

    private void UpdateEdgeIndexes(HorizontalDirectionType outOfBoundsDirection)
    {
        _smartListLeftIndex += (int)outOfBoundsDirection;
        _smartListRightIndex += (int)outOfBoundsDirection;
    }
    #endregion

    #region Helper Functions

    private static float HalfWidth(RectTransform rectTransform) { return rectTransform.rect.width * 0.5f; }

    private static void SetAnchoredPosition(RectTransform rectTransform, float sizeWithPadding, int number = 1)
    {
        rectTransform.anchoredPosition = new Vector2(sizeWithPadding * number, rectTransform.anchoredPosition.y);
    }

    private HorizontalDirectionType OutOfBoundsHorizontalCheck(RectTransform rectTransform)
    {
        float smartItemBorderLeft = rectTransform.localPosition.x;
        float smartItemBorderRight = rectTransform.localPosition.x + rectTransform.rect.width;

        bool overLeft = _viewportLeftAnchoredBorder > smartItemBorderRight && _velocity < 0;
        bool overRight = _viewportRightAnchoredBorder < smartItemBorderLeft && _velocity > 0;
        bool outOfBounds = (overLeft || overRight);

        if (!outOfBounds) return HorizontalDirectionType.Neutral;

        return (overLeft ? HorizontalDirectionType.Right : HorizontalDirectionType.Left);
    }
    #endregion
}
