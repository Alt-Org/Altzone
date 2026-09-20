using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Altzone.Scripts.Audio;

/// <summary>
/// Recycles a limited amount of gameobjects by moving and repurposing out of bounds gameobjects for the other end that is coming out of the invisible area.
/// </summary>
public class SmartHorizontalObjectList : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
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
    [SerializeField] private float _smartItemLeftStrengthMultiplier = 0f;
    [SerializeField] private float _smartItemRightStrengthMultiplier = 2f;
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
    private readonly Vector3[] _itemWorldCorners = new Vector3[4];

    private bool _buildOnEnable = false;

    public delegate void NewDataRequested(int targetIndex);
    public event NewDataRequested OnNewDataRequested; //Used to tell the host that new content data is needed.

    public delegate void LateDataRequest();
    public event LateDataRequest OnLateDataRequest; //Used to get data for the list when this list is enabled.

    private enum HorizontalDirectionType
    {
        Neutral = 0,
        Left = -1,
        Right = 1
    }

    public enum DragIntent { Pending, Scroll, Furniture }
    private Vector2 _gestureStartPosition;
    private DragIntent _dragIntent;
    private int? _dragPointerId;
    private bool _scrollEnabled = true;

    public bool scroll_enabled
    {
        get => _scrollEnabled;
        set
        {
            _scrollEnabled = value;
            if (!value) StopMovement();
        }
    }

    // Shared by UI drag events and the controller's physics picking path.
    public void BeginGesture(Vector2 position)
    {
        StopMovement();
        _gestureStartPosition = position;
        _dragIntent = DragIntent.Pending;
    }

    public DragIntent ResolveGesture(Vector2 position)
    {
        if (_dragIntent != DragIntent.Pending) return _dragIntent;
        Vector2 delta = position - _gestureStartPosition;
        float threshold = Mathf.Max(1, EventSystem.current != null ? EventSystem.current.pixelDragThreshold : 10);
        if (delta.sqrMagnitude < threshold * threshold) return _dragIntent;
        _dragIntent = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y) ? DragIntent.Scroll : DragIntent.Furniture;
        return _dragIntent;
    }

    public void StopMovement()
    {
        if (_velocityCoroutine != null) StopCoroutine(_velocityCoroutine);
        _velocityCoroutine = null;
        _velocity = 0f;
        _averageVelocity = 0f;
        _timeFromLastVelocityUpdate = float.NegativeInfinity;
    }

    public void CancelGesture()
    {
        StopMovement();
        _dragPointerId = null;
        // Ignore remaining drag events until the next press initializes a gesture.
        _dragIntent = DragIntent.Furniture;
    }

    private void OnDisable()
    {
        CancelGesture();
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || _dragPointerId.HasValue) return;
        BeginGesture(eventData.pressPosition);
    }

    private void Awake()
    {
        if (_smartListItems.Count == 0)
        {
            CreatePool();
        } 
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
        if (!scroll_enabled || eventData.button != PointerEventData.InputButton.Left || _dragPointerId.HasValue) return;
        _dragPointerId = eventData.pointerId;
        _pointerStartPosition = eventData.pressPosition;
        _previousUpdatePosition = eventData.pressPosition;
        _contentStartWorldPosition = _content.position;
        _scrollDiffCompensation = 0f;

        if (_velocityCoroutine == null) return;

        StopCoroutine(_velocityCoroutine);
        _velocityCoroutine = null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!scroll_enabled || _dragPointerId != eventData.pointerId ||
            ResolveGesture(eventData.position) != DragIntent.Scroll) return;
        
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
        if (_dragPointerId != eventData.pointerId) return;
        _dragPointerId = null;
        _pointerStartPosition = eventData.position;
        _contentStartWorldPosition = _content.position;
        _scrollDiffCompensation = 0f;

        if (scroll_enabled && _dragIntent == DragIntent.Scroll &&
            _ignoreRemainingVelocityTriggerTime > (Time.time - _timeFromLastVelocityUpdate))
            _velocityCoroutine = StartCoroutine(HandleVelocity());
    }

    #region Data
    public void Setup<T>(List<T> data)
    {
        StopMovement();
        _outOfBoundDirection = HorizontalDirectionType.Neutral;
        _scrollDiffCompensation = 0f;
        if (_smartListItems.Count == 0)
        {
            CreatePool(); // Awake() is not called yet (where this should be called), causing _smartListItems to be disabled --------------------
        } 
        
        if (!isActiveAndEnabled)
        {
            // When furniture tray loaded, smart list always disabled
        }
        if (_buildOnEnable)
        {
            _buildOnEnable = true;
            return;
        }

        if (data.Count == 0) // Hide smart list slots if list empty
        {
            _contentListLenght = 0;
            for (int i = 0; i < _smartListItems.Count; i++)
            {
                _smartListItems[i].SetVisibility(false);
            }
            Clear(); 
            return; 
        }

        if (!_locationHelper) CreateLocationHelper();

        _contentListLenght = data.Count;

        _viewportLeftAnchoredBorder = _viewport.rect.xMin;
        _viewportRightAnchoredBorder = _viewport.rect.xMax;

        _viewportLeftWorldBorder = _viewport.TransformPoint(new Vector3(_viewportLeftAnchoredBorder, 0f, 0f)).x;

        _viewportRightWorldBorder = _viewport.TransformPoint(new Vector3(_viewportRightAnchoredBorder, 0f, 0f)).x;

        if (_smartListItems.Count != _contentListLenght) CreatePool();

        _smartListLeftIndex = -_uniqueGameObjectsAtLeft.Count;
        _smartListRightIndex = _smartListItems.Count - 1 - _uniqueGameObjectsAtLeft.Count;

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
        if (index < 0 || index >= _contentListLenght || _smartListItems.Count == 0 ||
            index < _smartListLeftIndex || index > _smartListRightIndex) return;

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
            else
            {
                _smartListItems[smartIndex].SetVisibility(true);
            }
            _smartListItems[smartIndex].SetData<T>(data[i]);
            smartPositionIndexesUsed++;
        }
    }

    public void Clear() { foreach (var smartListItem in _smartListItems) smartListItem.ClearData(); }

    private void CreatePool()
    {
        RectTransform prefabRect = _contentPrefab.GetComponent<RectTransform>();
        float _size_ratio = prefabRect.sizeDelta.x / prefabRect.sizeDelta.y;

        //Create first SmartListItem to calculate how many of it can fit inside based on the width + padding.
        if (_smartListItems.Count == 0)
        {
            SmartListItem firstSmartListItem = Instantiate(_contentPrefab, _content).GetComponent<SmartListItem>();

            if (!firstSmartListItem.SelfRectTransform) firstSmartListItem.SetSelfRectTransform();

            _size_ratio = firstSmartListItem.SelfRectTransform.sizeDelta.x / firstSmartListItem.SelfRectTransform.sizeDelta.y; // ----------

            // Set the size of the first smart list item
            firstSmartListItem.SelfRectTransform.sizeDelta = new Vector2( // ------------------------------
                _content.rect.height * _size_ratio, _content.rect.height); // ------------------------------

            _smartListItemLocalWidthWithPadding = firstSmartListItem.SelfRectTransform.rect.width + _horizontalPadding;

            firstSmartListItem.ClearData();
            _smartListItems.Add(firstSmartListItem);
        }
        else
        {
            // Set the padding of all smart list items based on the first one
            _smartListItemLocalWidthWithPadding = _smartListItems[0].SelfRectTransform.rect.width + _horizontalPadding;
        }

        // One buffer slot is needed while slots at both viewport edges are partially visible.
        Vector3 viewportLeft = _content.InverseTransformPoint(_viewport.TransformPoint(new Vector3(_viewport.rect.xMin, 0f, 0f)));
        Vector3 viewportRight = _content.InverseTransformPoint(_viewport.TransformPoint(new Vector3(_viewport.rect.xMax, 0f, 0f)));
        float visibleWidth = Mathf.Abs(viewportRight.x - viewportLeft.x);
        _amountToFillContentList = Mathf.CeilToInt(visibleWidth / _smartListItemLocalWidthWithPadding) + Mathf.Max(1, _extraSmartListItems);

        if (_amountToFillContentList <= _smartListItems.Count) return;

        _amountToFillContentList -= _smartListItems.Count;

        //Create the rest of the SmartListItem's.
        for (int i = 0; i < _amountToFillContentList; i++)
        {
            SmartListItem smartListItem = Instantiate(_contentPrefab, _content).GetComponent<SmartListItem>();

            if (!smartListItem.SelfRectTransform) smartListItem.SetSelfRectTransform();

            // Set the size of all the other slots
            smartListItem.SelfRectTransform.sizeDelta = new Vector2( // ------------------------------
                _content.rect.height * _size_ratio, _content.rect.height); // ------------------------------

            smartListItem.ClearData(); // does nothing?? what is it even clearing in a fresh prefab? (OK)
            _smartListItems.Add(smartListItem);
        }
    }
    #endregion

    #region Movement
    private IEnumerator HandleVelocity()
    {
        float totalDistance = 0f;
        float timer = 0f;

        while (scroll_enabled && timer < _slowdownTime)
        {
            totalDistance += Mathf.Lerp(_velocity, 0f, timer / _slowdownTime);

            ScrollHandling(totalDistance);
            yield return null;
            timer += Time.deltaTime;
        }

        _velocity = 0f;
        _velocityCoroutine = null;
    }

    private void ScrollHandling(float distance) //TODO: Add elasticity.
    {
        if (_contentListLenght <= 0) return;
        float contentWidth = _contentListLenght * _smartListItemLocalWidthWithPadding - _horizontalPadding;
        foreach (RectTransform item in _uniqueGameObjectsAtLeft) contentWidth += item.rect.width + _horizontalPadding;
        foreach (RectTransform item in _uniqueGameObjectsAtRight) contentWidth += item.rect.width + _horizontalPadding;
        if (contentWidth <= _viewport.rect.width) return;

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
        foreach (SmartListItem smartListItem in _smartListItems) CheckSmartItemVisibility(smartListItem);
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
            RectTransform leftItem = _uniqueGameObjectsAtLeft.Count > 0
                ? _uniqueGameObjectsAtLeft[0] : _smartListItems[0].SelfRectTransform;
            GetViewportEdges(leftItem, out float leftItemLeftEdge, out _);

            if (_viewportLeftAnchoredBorder > leftItemLeftEdge) return HorizontalDirectionType.Neutral;

            _content.position += _viewport.TransformVector(new Vector3(_viewportLeftAnchoredBorder - leftItemLeftEdge, 0f, 0f));

            return HorizontalDirectionType.Left;
        }

        if ((_smartListRightIndex >= _contentListLenght + _uniqueGameObjectsAtRight.Count && _uniqueGameObjectsAtRight.Count != 0 ||
             _uniqueGameObjectsAtRight.Count == 0 && _smartListRightIndex >= _contentListLenght) && _velocity < 0) //Over right check.
        {
            GetViewportEdges(GetRightItemRectTransform(), out _, out float rightItemRightEdge);

            if (_viewportRightAnchoredBorder < rightItemRightEdge) return HorizontalDirectionType.Neutral;

            _content.position += _viewport.TransformVector(new Vector3(_viewportRightAnchoredBorder - rightItemRightEdge, 0f, 0f));

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
            return _uniqueGameObjectsAtLeft[0].localPosition.x - HalfWidth(_uniqueGameObjectsAtLeft[0]) * _smartItemLeftStrengthMultiplier;

        if (_smartListItems.Count != 0)
            return _smartListItems[0].SelfRectTransform.localPosition.x - HalfWidth(_smartListItems[0].SelfRectTransform) * _smartItemLeftStrengthMultiplier;

        if (_uniqueGameObjectsAtRight.Count != 0)
            return _uniqueGameObjectsAtRight[0].localPosition.x - HalfWidth(_uniqueGameObjectsAtRight[0]) * _smartItemLeftStrengthMultiplier;

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

    private void CheckSmartItemVisibility(SmartListItem smartListItem)
    {
        RectTransform rectTransform = smartListItem.SelfRectTransform;

        //Out of bounds calculations.
        GetViewportEdges(rectTransform, out float smartItemBorderLeft, out float smartItemBorderRight);

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

    // Compare actual rectangle edges in viewport space, including anchors, pivots and scale.
    private void GetViewportEdges(RectTransform item, out float left, out float right)
    {
        item.GetWorldCorners(_itemWorldCorners);
        left = float.PositiveInfinity;
        right = float.NegativeInfinity;
        foreach (Vector3 corner in _itemWorldCorners)
        {
            float x = _viewport.InverseTransformPoint(corner).x;
            left = Mathf.Min(left, x);
            right = Mathf.Max(right, x);
        }
    }

    private static void SetAnchoredPosition(RectTransform rectTransform, float sizeWithPadding, int number = 1)
    {
        rectTransform.anchoredPosition = new Vector2(sizeWithPadding * number, rectTransform.anchoredPosition.y);
    }

    private HorizontalDirectionType OutOfBoundsHorizontalCheck(RectTransform rectTransform)
    {
        GetViewportEdges(rectTransform, out float smartItemBorderLeft, out float smartItemBorderRight);

        bool overLeft = _viewportLeftAnchoredBorder > smartItemBorderRight && _velocity < 0;
        bool overRight = _viewportRightAnchoredBorder < smartItemBorderLeft && _velocity > 0;
        bool outOfBounds = (overLeft || overRight);

        if (!outOfBounds) return HorizontalDirectionType.Neutral;

        return (overLeft ? HorizontalDirectionType.Right : HorizontalDirectionType.Left);
    }
    #endregion
}
