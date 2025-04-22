using System;
using System.Collections;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Altzone.Scripts.BattleUiShared;
using OrientationType = Altzone.Scripts.BattleUiShared.BattleUiMultiOrientationElement.OrientationType;

namespace MenuUi.Scripts.Settings.BattleUiEditor
{
    /// <summary>
    /// Handles the functionality for BattleUiEditingComponent prefab.
    /// </summary>
    public class BattleUiEditingComponent : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Scale handles")]
        [SerializeField] private Button _scaleHandleTopLeft;
        [SerializeField] private Button _scaleHandleTopRight;
        [SerializeField] private Button _scaleHandleBottomRight;
        [SerializeField] private Button _scaleHandleBottomLeft;

        [Header("Change orientation buttons")]
        [SerializeField] private Button _changeOrientationButtonTop;
        [SerializeField] private Button _changeOrientationButtonBottom;

        [Header("Flip horizontally buttons")]
        [SerializeField] private Button _flipHorizontallyButtonTop;
        [SerializeField] private Button _flipHorizontallyButtonBottom;

        [Header("Flip vertically buttons")]
        [SerializeField] private Button _flipVerticallyButtonRight;
        [SerializeField] private Button _flipVerticallyButtonLeft;

        [Header("Drag delay display")]
        [SerializeField] private GameObject _dragDelayDisplay;
        [SerializeField] private Image _dragDelayFill;

        public Action OnUiElementEdited;

        public delegate void UiElementSelectedHandler(BattleUiEditingComponent self);
        public UiElementSelectedHandler OnUiElementSelected;

        public void SetInfo(BattleUiMovableElement movableElement, Transform uiElementHolder)
        {
            _movableElement = movableElement;
            _uiElementHolder = uiElementHolder.GetComponent<RectTransform>();

            SetControlButtonSizes();
            ShowControls(false);

            _movableElementAspectRatio = movableElement.RectTransformComponent.rect.width / movableElement.RectTransformComponent.rect.height;

            UpdateData();
        }

        public void SetInfo(BattleUiMultiOrientationElement multiOrientationElement, Transform uiElementHolder)
        {
            _multiOrientationElement = multiOrientationElement;
            _movableElement = multiOrientationElement;
            _uiElementHolder = uiElementHolder.GetComponent<RectTransform>();

            SetControlButtonSizes();
            ShowControls(false);

            UpdateData();
        }

        public void ShowControls(bool show)
        {
            CheckControlButtonsVisibility();
            _currentScaleHandle.gameObject.SetActive(show);

            if (_multiOrientationElement == null) show = false;
            _currentFlipHorizontallyButton.gameObject.SetActive(show);
            _currentFlipVerticallyButton.gameObject.SetActive(show);
            _currentChangeOrientationButton.gameObject.SetActive(show);
        }

        public void UpdateData()
        {
            if (_multiOrientationElement == null) _data = _movableElement.GetData();
            else _data = _multiOrientationElement.GetData();
        }

        public void ToggleGrid(bool toggle)
        {
            _isGridToggled = toggle;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPointerDown = true;
            OnUiElementSelected?.Invoke(this);

            if (_dragTimerHolder == null)
            {
                _dragTimerHolder = StartCoroutine(StartDragTimer(() =>
                {
                    _dragTimerHolder = null;
                    if (!_isPointerDown) return;
                    _currentAction = ActionType.Move;
                }));
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPointerDown = false;
            if (_dragTimerHolder != null)
            {
                StopCoroutine(_dragTimerHolder);
                _dragTimerHolder = null;

                if (_dragDelayDisplay.activeSelf)
                {
                    ShowControls(false);
                    _dragDelayDisplay.SetActive(false);
                }
                else
                {
                    ShowControls(!_currentScaleHandle.gameObject.activeSelf);
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Button selectedButton = eventData.selectedObject != null ? eventData.selectedObject.GetComponent<Button>() : null;

            if (selectedButton == _currentFlipHorizontallyButton || selectedButton == _currentFlipVerticallyButton || selectedButton == _currentChangeOrientationButton)
            {
                _currentAction = ActionType.None;
            }
            else if (selectedButton == _currentScaleHandle)
            {
                _currentAction = ActionType.Scale;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            switch (_currentAction)
            {
                case ActionType.Move:
                    Vector2 newPos = Vector2.zero;

                    if (_isGridToggled) // Snapping to grid while moving
                    {
                        newPos.x = Mathf.Round(eventData.position.x / BattleUiEditor.GridCellWidth) * BattleUiEditor.GridCellWidth;
                        newPos.y = Mathf.Round(eventData.position.y / BattleUiEditor.GridCellHeight) * BattleUiEditor.GridCellHeight;
                    }
                    else // Free movement
                    {
                        newPos = eventData.position;
                    }

                    // Clamping position to be inside the editor
                    newPos.x = Mathf.Clamp(newPos.x, _minPosX, _maxPosX);
                    newPos.y = Mathf.Clamp(newPos.y, _minPosY, _maxPosY);

                    _movableElement.transform.position = newPos;
                    break;

                case ActionType.Scale:
                    // Scaling while keeping aspect ratio, we have to invert scaling for the left side scale handles
                    float sizeIncreaseX;
                    if (_currentScaleHandleIdx == (int)CornerType.TopLeft || _currentScaleHandleIdx == (int)CornerType.BottomLeft)
                    {
                        sizeIncreaseX = -((eventData.position.x - eventData.pressPosition.x) * 2);
                    }
                    else
                    {
                        sizeIncreaseX = (eventData.position.x - eventData.pressPosition.x) * 2;
                    }
                         
                    float sizeIncreaseY = sizeIncreaseX / (_movableElement.RectTransformComponent.rect.width / _movableElement.RectTransformComponent.rect.height);
                    _movableElement.RectTransformComponent.sizeDelta = new Vector2(sizeIncreaseX, sizeIncreaseY);

                    // Preventing being scaled too small or too big
                    if (_multiOrientationElement == null || _multiOrientationElement.IsHorizontal)
                    {
                        float clampedWidth = Mathf.Clamp(_movableElement.RectTransformComponent.rect.width, _minWidth, _maxWidth);

                        // Snapping scaling to grid
                        if (_isGridToggled) clampedWidth = Mathf.Round(clampedWidth / (BattleUiEditor.GridCellWidth * 2)) * (BattleUiEditor.GridCellWidth * 2);

                        float aspectRatio = _multiOrientationElement == null ? _movableElementAspectRatio : _multiOrientationElement.HorizontalAspectRatio;

                        _movableElement.RectTransformComponent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, clampedWidth);
                        _movableElement.RectTransformComponent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, clampedWidth / aspectRatio);
                    }
                    else if (!_multiOrientationElement.IsHorizontal)
                    {
                        float clampedHeight = Mathf.Clamp(_movableElement.RectTransformComponent.rect.height, _minHeight, _maxHeight);

                        // Snapping scaling to grid
                        if (_isGridToggled) clampedHeight = Mathf.Round(clampedHeight / (BattleUiEditor.GridCellHeight * 2)) * (BattleUiEditor.GridCellHeight * 2);

                        _movableElement.RectTransformComponent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, clampedHeight * _multiOrientationElement.VerticalAspectRatio);
                        _movableElement.RectTransformComponent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, clampedHeight);
                    }

                    // Preventing Ui element from going out of editor bounds while scaling
                    float clampedPosX = Mathf.Clamp(_movableElement.RectTransformComponent.position.x, _minPosX, _maxPosX);
                    float clampedPosY = Mathf.Clamp(_movableElement.RectTransformComponent.position.y, _minPosY, _maxPosY);

                    if (clampedPosX != _movableElement.RectTransformComponent.position.x || clampedPosY != _movableElement.RectTransformComponent.position.y)
                    {
                        _movableElement.RectTransformComponent.position = new Vector2(clampedPosX, clampedPosY);
                    }
                    break;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_currentAction == ActionType.Move || _currentAction == ActionType.Scale) CalculateAndSetAnchors();
            if (_currentAction == ActionType.Scale) CheckControlButtonsVisibility();
            _currentAction = ActionType.None;
        }

        enum ActionType
        {
            None,
            Move,
            Scale
        }

        enum CornerType // Helper enum to access button world corners and scale handles array more readably.
        {
            BottomLeft = 0,
            TopLeft = 1,
            TopRight = 2,
            BottomRight = 3,
        }

        enum ControlButtonVertical
        {
            Top = 0,
            Bottom = 1,
        }

        enum ControlButtonHorizontal
        {
            Left = 0,
            Right = 1,
        }

        private const float ScaleHandleSizeRatio = 0.05f;
        private const float ControlButtonSizeRatio = 0.1f;

        private const int DefaultScaleHandleIdx = (int)CornerType.BottomRight;
        private const int DefaultChangeOrientationButtonIdx = (int)ControlButtonVertical.Top;
        private const int DefaultFlipHorizontallyButtonIdx = (int)ControlButtonVertical.Top;
        private const int DefaultFlipVerticallyButtonIdx = (int)ControlButtonHorizontal.Right;

        private Button[] _scaleHandles;
        private Button[] _changeOrientationButtons;
        private Button[] _flipHorizontallyButtons;
        private Button[] _flipVerticallyButtons;

        private int _currentScaleHandleIdx = DefaultScaleHandleIdx;
        private int _currentChangeOrientationButtonIdx = DefaultChangeOrientationButtonIdx;
        private int _currentFlipHorizontallyButtonIdx = DefaultFlipHorizontallyButtonIdx;
        private int _currentFlipVerticallyButtonIdx = DefaultFlipVerticallyButtonIdx;

        private Button _currentScaleHandle => _scaleHandles[(int)_currentScaleHandleIdx];
        private Button _currentChangeOrientationButton => _changeOrientationButtons[(int)_currentChangeOrientationButtonIdx];
        private Button _currentFlipHorizontallyButton => _flipHorizontallyButtons[(int)_currentFlipHorizontallyButtonIdx];
        private Button _currentFlipVerticallyButton => _flipVerticallyButtons[(int)_currentFlipVerticallyButtonIdx];

        private float _maxWidth => BattleUiEditor.GridCellWidth * 10;
        private float _maxHeight => BattleUiEditor.GridCellHeight * 16;
        private float _minWidth => BattleUiEditor.GridCellWidth * 2;
        private float _minHeight => BattleUiEditor.GridCellHeight * 4;

        private float _maxPosX => _uiElementHolder.rect.width * (Screen.width/ _uiElementHolder.rect.width) - _movableElement.RectTransformComponent.rect.width / 2;
        private float _maxPosY => _uiElementHolder.rect.height * (Screen.width / _uiElementHolder.rect.width) - _movableElement.RectTransformComponent.rect.height / 2;
        private float _minPosX => _movableElement.RectTransformComponent.rect.width / 2;
        private float _minPosY => _movableElement.RectTransformComponent.rect.height / 2;

        private RectTransform _uiElementHolder;

        private BattleUiMovableElement _movableElement;
        private BattleUiMultiOrientationElement _multiOrientationElement;
        private BattleUiMovableElementData _data;

        private float _movableElementAspectRatio;

        private bool _isPointerDown = false;
        private bool _isGridToggled = false;

        private Coroutine _dragTimerHolder = null;

        private ActionType _currentAction;

        private void OnDisable()
        {
            ShowControls(false);
        }

        private void Awake()
        {
            // Arranging scale handles and control buttons to array so that they can be accessed better with the enums
            _scaleHandles = new[] { _scaleHandleBottomLeft, _scaleHandleTopLeft, _scaleHandleTopRight, _scaleHandleBottomRight };
            _changeOrientationButtons = new[] { _changeOrientationButtonTop, _changeOrientationButtonBottom };
            _flipHorizontallyButtons = new[] { _flipHorizontallyButtonTop, _flipHorizontallyButtonBottom };
            _flipVerticallyButtons = new[] { _flipVerticallyButtonLeft, _flipVerticallyButtonRight };

            foreach (var button in _changeOrientationButtons)
            {
                button.onClick.AddListener(ChangeOrientation);
            }

            foreach (var button in _flipHorizontallyButtons)
            {
                button.onClick.AddListener(FlipHorizontally);
            }

            foreach (var button in _flipVerticallyButtons)
            {
                button.onClick.AddListener(FlipVertically);
            }
        }

        private void OnDestroy()
        {
            foreach (var button in _changeOrientationButtons)
            {
                button.onClick.RemoveAllListeners();
            }

            foreach (var button in _flipHorizontallyButtons)
            {
                button.onClick.RemoveAllListeners();
            }

            foreach (var button in _flipVerticallyButtons)
            {
                button.onClick.RemoveAllListeners();
            }
        }

        private IEnumerator StartDragTimer(Action callback)
        {
            _dragDelayFill.fillAmount = 0;

            float timePassed = 0f;
            while (timePassed < 0.5f)
            {
                if (!_dragDelayDisplay.activeSelf && timePassed >= 0.2f)
                {
                    _dragDelayDisplay.SetActive(true);
                    ShowControls(false);
                }
                _dragDelayFill.fillAmount = timePassed * 2;
                yield return null;
                timePassed += Time.deltaTime;
            }

            _dragDelayDisplay.SetActive(false);
            callback();
        }

        private void CalculateAndSetAnchors(Vector2? newSize = null)
        {
            // Saving values used in calculations to easier to read variables
            float holderWidth = _uiElementHolder.rect.width;
            float holderHeight = _uiElementHolder.rect.height;

            float xPos = _movableElement.RectTransformComponent.localPosition.x;
            float yPos = _movableElement.RectTransformComponent.localPosition.y;

            float ownWidth = newSize == null ? _movableElement.RectTransformComponent.rect.width : newSize.Value.x;
            float ownHeight = newSize == null ? _movableElement.RectTransformComponent.rect.height : newSize.Value.y;

            // Calculating anchors
            float anchorXMin = (xPos - ownWidth / 2.0f) / holderWidth + 0.5f;
            float anchorXMax = (xPos + ownWidth / 2.0f) / holderWidth + 0.5f;

            float anchorYMin = (yPos - ownHeight / 2.0f) / holderHeight + 0.5f;
            float anchorYMax = (yPos + ownHeight / 2.0f) / holderHeight + 0.5f;

            // Checking that the anchors don't go over borders
            if (anchorXMin < 0)
            {
                anchorXMax += Mathf.Abs(anchorXMin);
                anchorXMin = 0;
            }

            if (anchorXMax > 1)
            {
                anchorXMin -= anchorXMax - 1;
                anchorXMax = 1;

                if (anchorXMin < 0)
                {
                    anchorXMin = 0;
                }
            }

            if (anchorYMin < 0)
            {
                anchorYMax += Mathf.Abs(anchorYMin);
                anchorYMin = 0;
            }

            if (anchorYMax > 1)
            {
                anchorYMin -= anchorYMax - 1;
                anchorYMax = 1;

                if (anchorYMin < 0)
                {
                    anchorYMin = 0;
                }
            }

            _data.AnchorMin = new(anchorXMin, anchorYMin);
            _data.AnchorMax = new(anchorXMax, anchorYMax);

            _movableElement.SetData(_data);

            OnUiElementEdited?.Invoke();
        }

        private void FlipHorizontally()
        {
            _data.IsFlippedHorizontally = !_data.IsFlippedHorizontally;
            _multiOrientationElement.SetData(_data);

            OnUiElementEdited?.Invoke();
        }

        private void FlipVertically()
        {
            _data.IsFlippedVertically = !_data.IsFlippedVertically;
            _multiOrientationElement.SetData(_data);

            OnUiElementEdited?.Invoke();
        }

        private void ChangeOrientation()
        {
            _data.Orientation = _multiOrientationElement.IsHorizontal ? OrientationType.Vertical : OrientationType.Horizontal;
            _data.IsFlippedHorizontally = false; // Resetting flip statuses for new orientation
            _data.IsFlippedVertically = false;

            // Calculating new aspect ratio size
            Vector2 newSize = new();
            if (_multiOrientationElement.IsHorizontal) // If element is currently horizontal we are trying to change it to be vertical
            {
                newSize.y = Mathf.Clamp(_multiOrientationElement.RectTransformComponent.rect.width, _minHeight, _maxHeight);
                newSize.x = newSize.y * _multiOrientationElement.VerticalAspectRatio;
            }
            else
            {
                newSize.x = Mathf.Clamp(_multiOrientationElement.RectTransformComponent.rect.height, _minWidth, _maxWidth);
                newSize.y = newSize.x / _multiOrientationElement.HorizontalAspectRatio;
            }

            CalculateAndSetAnchors(newSize);
        }

        // Method used to calculate appropiate size for the control buttons, since if they were anchored they would grow with the element when it's scaled
        private void SetControlButtonSizes()
        {
            float scaleHandleWidth = _uiElementHolder.rect.width * ScaleHandleSizeRatio;
            Vector2 buttonSize = new Vector2(scaleHandleWidth, scaleHandleWidth);

            foreach (Button scaleHandle in _scaleHandles)
            {
                scaleHandle.GetComponent<RectTransform>().sizeDelta = buttonSize;
            }

            if (_multiOrientationElement == null) return; // Returning if not a multiorientation element

            float controlButtonWidth = _uiElementHolder.rect.width * ControlButtonSizeRatio;
            buttonSize = new Vector2(controlButtonWidth, controlButtonWidth);

            foreach (Button button in _changeOrientationButtons)
            {
                button.GetComponent<RectTransform>().sizeDelta = buttonSize;
            }

            foreach (Button button in _flipHorizontallyButtons)
            {
                button.GetComponent<RectTransform>().sizeDelta = buttonSize;
            }

            foreach (Button button in _flipVerticallyButtons)
            {
                button.GetComponent<RectTransform>().sizeDelta = buttonSize;
            }
        }

        private void CheckControlButtonsVisibility()
        {
            // Resetting scale handle offset in case it had any
            _currentScaleHandle.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            Button oldScaleHandle = _currentScaleHandle;

            // Checking first if the default scale handle is free
            Vector3[] defaultScaleHandleCorners = GetButtonCorners(_scaleHandles[DefaultScaleHandleIdx]);
            if (IsButtonInsideEditor(defaultScaleHandleCorners))
            {
                _currentScaleHandleIdx = DefaultScaleHandleIdx;
            }
            else if (IsButtonInsideEditor(GetButtonCorners(_scaleHandles[(int)CornerType.BottomLeft]))) // Checking bottom left 2nd
            {
                _currentScaleHandleIdx = (int)CornerType.BottomLeft;
            }
            else // Else setting the first visible scale handle visible
            {
                for (int i = 0; i < _scaleHandles.Length; i++)
                {
                    if (IsButtonInsideEditor(GetButtonCorners(_scaleHandles[i])))
                    {
                        _currentScaleHandleIdx = i;
                        break;
                    }
                }
            }

            // If the visible scale handle changed we set the new scale handle visible if the old scale handle was visible
            if (oldScaleHandle != _currentScaleHandle)
            {
                if (oldScaleHandle.gameObject.activeSelf)
                {
                    oldScaleHandle.gameObject.SetActive(false);
                    _currentScaleHandle.gameObject.SetActive(true);
                }
            }

            // Returning if not a multiorientation element since the other control buttons are only for those
            if (_multiOrientationElement == null) return;

            Button oldFlipVerticallyButton = _currentFlipVerticallyButton;

            // Checking if default flip vertically button button is inside editor
            Vector3[] defaultFlipVerticallyCorners = GetButtonCorners(_flipVerticallyButtons[DefaultFlipVerticallyButtonIdx]);
            if (IsButtonInsideEditor(defaultFlipVerticallyCorners))
            {
                _currentFlipVerticallyButtonIdx = DefaultFlipVerticallyButtonIdx;
            }
            else // If not setting the other side's button as current
            {
                _currentFlipVerticallyButtonIdx = (int)ControlButtonHorizontal.Left;
            }

            // Checking if we should change the visible button
            if (oldFlipVerticallyButton != _currentFlipVerticallyButton)
            {
                if (oldFlipVerticallyButton.gameObject.activeSelf)
                {
                    oldFlipVerticallyButton.gameObject.SetActive(false);
                    _currentFlipVerticallyButton.gameObject.SetActive(true);
                }
            }

            // Positioning top buttons
            Button[] topButtons = _flipHorizontallyButtons.Concat(_changeOrientationButtons).ToArray();

            foreach (Button button in topButtons)
            {
                RectTransform buttonRectTransform = button.GetComponent<RectTransform>();

                // Setting button to default position
                buttonRectTransform.anchoredPosition = Vector2.zero;

                // Getting button corners
                Vector3[] buttonCorners = new Vector3[4];
                buttonRectTransform.GetWorldCorners(buttonCorners);

                // If button is not inside the editor updating the position
                if (!IsButtonInsideEditor(buttonCorners))
                {
                    Vector3[] holderCorners = new Vector3[4];
                    _uiElementHolder.GetWorldCorners(holderCorners);

                    Vector2 newPosition = Vector2.zero;
                    // Checking left side
                    float left = buttonCorners[(int)CornerType.BottomLeft].x;
                    if (left < 0)
                    {
                        newPosition.x += Mathf.Abs(left);
                    }

                    // Checking right side
                    float buttonRight = buttonCorners[(int)CornerType.BottomRight].x;
                    float holderRight = holderCorners[(int)CornerType.BottomRight].x;
                    if (buttonRight > holderRight)
                    {
                        newPosition.x -= buttonRight - holderRight;
                    }

                    buttonRectTransform.anchoredPosition = newPosition;
                }
            }

            Button oldFlipHorizontallyButton = _currentFlipHorizontallyButton;
            Button oldChangeOrientationButton = _currentChangeOrientationButton;

            for (int i = 0; i < _flipHorizontallyButtons.Length; i++)
            {
                // Ensuring the top and bottom buttons are next to each other
                RectTransform _changeOrientationRectTransform = _changeOrientationButtons[i].GetComponent<RectTransform>();
                RectTransform _flipHorizontallyRectTransform = _flipHorizontallyButtons[i].GetComponent<RectTransform>();

                if (_flipHorizontallyRectTransform == null || _changeOrientationRectTransform == null) continue;

                if (_changeOrientationRectTransform.anchoredPosition != Vector2.zero)
                {
                    _flipHorizontallyRectTransform.anchoredPosition += _changeOrientationRectTransform.anchoredPosition;
                }
                else if (_flipHorizontallyRectTransform.anchoredPosition != Vector2.zero)
                {
                    _changeOrientationRectTransform.anchoredPosition += _flipHorizontallyRectTransform.anchoredPosition;
                }

                // Showing the buttons which are both inside the editor
                if (IsButtonInsideEditor(GetButtonCorners(_changeOrientationButtons[i])) && IsButtonInsideEditor(GetButtonCorners(_flipHorizontallyButtons[i])))
                {
                    _currentChangeOrientationButtonIdx = i;
                    _currentFlipHorizontallyButtonIdx = i;
                    break;
                }
            }

            // Checking visibility for the top buttons
            if (oldFlipHorizontallyButton != _currentFlipHorizontallyButton)
            {
                if (oldFlipHorizontallyButton.gameObject.activeSelf)
                {
                    oldFlipHorizontallyButton.gameObject.SetActive(false);
                    _currentFlipHorizontallyButton.gameObject.SetActive(true);
                }
            }

            if (oldChangeOrientationButton != _currentChangeOrientationButton)
            {
                if (oldChangeOrientationButton.gameObject.activeSelf)
                {
                    oldChangeOrientationButton.gameObject.SetActive(false);
                    _currentChangeOrientationButton.gameObject.SetActive(true);
                }
            }

            // Check if the scale handle is overlapping (only if vertical multi orientation element)
            if (_multiOrientationElement.IsHorizontal) return;

            // Getting rect and rect transforms
            RectTransform scaleHandleRectTransform = _currentScaleHandle.GetComponent<RectTransform>();
            Rect scaleHandleRect = scaleHandleRectTransform.rect;

            Rect flipHorizontallyRect = _currentFlipHorizontallyButton.GetComponent<RectTransform>().rect;
            Vector3[] flipHorizontallyCorners = GetButtonCorners(_currentFlipHorizontallyButton);

            Rect changeOrientationRect = _currentChangeOrientationButton.GetComponent<RectTransform>().rect;
            Vector3[] changeOrientationCorners = GetButtonCorners(_currentChangeOrientationButton);

            // Checking if the scale handle overlaps with flip horizontally or change orientation buttons
            if (scaleHandleRect.Overlaps(flipHorizontallyRect) || scaleHandleRect.Overlaps(changeOrientationRect))
            {
                // Changing scaleHandle world position
                switch ((CornerType)_currentScaleHandleIdx)
                {
                    case CornerType.BottomLeft:
                        scaleHandleRectTransform.position = changeOrientationCorners[(int)CornerType.TopLeft];
                        break;
                    case CornerType.TopLeft:
                        scaleHandleRectTransform.position = changeOrientationCorners[(int)CornerType.BottomLeft];
                        break;
                    case CornerType.BottomRight:
                        scaleHandleRectTransform.position = flipHorizontallyCorners[(int)CornerType.TopRight];
                        break;
                    case CornerType.TopRight:
                        scaleHandleRectTransform.position = flipHorizontallyCorners[(int)CornerType.BottomRight];
                        break;
                }
            }
        }

        private Vector3[] GetButtonCorners(Button button)
        {
            RectTransform rectTransform = button.GetComponent<RectTransform>();
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            return corners;
        }

        private bool IsButtonInsideEditor(Vector3[] buttonCorners)
        {
            bool isButtonInside = true;
            for (int i = 0; i < buttonCorners.Length; i++)
            {
                Vector3 localSpacePoint = _uiElementHolder.InverseTransformPoint(buttonCorners[i]);
                if (!HolderRectContains(localSpacePoint))
                {
                    isButtonInside = false;
                    break;
                }
            }

            return isButtonInside;
        }

        // This method is needed because the default Contains method compares with < and > instead of <= and >= for the max x and y values
        private bool HolderRectContains(Vector3 point) 
        {
            Rect uiRect = _uiElementHolder.rect;
            return point.x >= uiRect.xMin && point.x <= uiRect.xMax && point.y >= uiRect.yMin && point.y <= uiRect.yMax;
        }
    }
}
