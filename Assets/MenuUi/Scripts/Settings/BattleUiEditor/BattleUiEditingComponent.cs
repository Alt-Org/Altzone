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

        public delegate void UiElementSelectedHandler(BattleUiEditingComponent self);
        public UiElementSelectedHandler OnUiElementSelected;

        public delegate void GridSnapHandler(int gridColumnIndex, int gridRowIndex);
        public GridSnapHandler OnGridSnap;

        public Action OnUiElementEdited;

        public void SetInfo(BattleUiMovableElement movableElement)
        {
            _movableElement = movableElement;

            SetControlButtonSizes();
            ShowControls(false);

            _movableElementAspectRatio = movableElement.RectTransformComponent.rect.width / movableElement.RectTransformComponent.rect.height;

            UpdateData();
        }

        public void SetInfo(BattleUiMultiOrientationElement multiOrientationElement)
        {
            _multiOrientationElement = multiOrientationElement;
            _movableElement = multiOrientationElement;

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
            _isGridAlignToggled = toggle;
        }

        public void ToggleIncrementScaling(bool toggle)
        {
            _isIncrementalScalingToggled = toggle;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnUiElementSelected?.Invoke(this);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_currentAction != ActionType.Scale) ShowControls(!_currentScaleHandle.gameObject.activeSelf);
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
                case ActionType.None:
                    // Starting moving element if player dragged at least for the treshold amount
                    if ((eventData.position - eventData.pressPosition).magnitude > MoveActionTreshold)
                    {
                        ShowControls(false);
                        _currentAction = ActionType.Move;

                        // Setting offset for moving
                        _moveOffset.x = _movableElement.transform.position.x - eventData.pressPosition.x;
                        _moveOffset.y = _movableElement.transform.position.y - eventData.pressPosition.y;
                    }
                    break;

                case ActionType.Move:
                    Vector2 newPos = eventData.position + _moveOffset;

                    if (_isGridAlignToggled) // Snapping to grid while moving
                    {
                        int gridColumnIndex = GridController.GetGridColumnIndex(newPos.x);
                        int gridRowIndex = GridController.GetGridRowIndex(newPos.y);

                        newPos.x = GridController.GetGridSnapPositionX(gridColumnIndex);
                        newPos.y = GridController.GetGridSnapPositionY(gridRowIndex);

                        OnGridSnap?.Invoke(gridColumnIndex, gridRowIndex);
                    }

                    // Clamping position to be inside the editor
                    newPos.x = Mathf.Clamp(newPos.x, _minPosX, _maxPosX);
                    newPos.y = Mathf.Clamp(newPos.y, _minPosY, _maxPosY);

                    _movableElement.transform.position = newPos;
                    break;

                case ActionType.Scale:
                    float sizeIncreaseX = 0f;
                    float sizeIncreaseY = 0f;

                    // If vertical multiorientation element we scale in y axis
                    if (_multiOrientationElement != null && !_multiOrientationElement.IsHorizontal) 
                    {
                        sizeIncreaseY = -((eventData.position.y - eventData.pressPosition.y) * 2);

                        // We have to invert scaling for the top side scale handles when scaling vertically
                        if (_currentScaleHandleIdx == (int)CornerType.TopLeft || _currentScaleHandleIdx == (int)CornerType.TopRight)
                        {
                            sizeIncreaseY = -sizeIncreaseY;
                        }
                    }
                    else // If horizontal ui element we scale in x axis
                    {
                        sizeIncreaseX = (eventData.position.x - eventData.pressPosition.x) * 2;

                        // We have to invert scaling for the left side scale handles when scaling horizontally
                        if (_currentScaleHandleIdx == (int)CornerType.TopLeft || _currentScaleHandleIdx == (int)CornerType.BottomLeft)
                        {
                            sizeIncreaseX = -sizeIncreaseX;
                        }
                    }

                    // Getting the other value from aspect ratio
                    if (sizeIncreaseX == 0f)
                    {
                        sizeIncreaseX = sizeIncreaseY * _aspectRatio;
                    }
                    else if (sizeIncreaseY == 0f)
                    {
                        sizeIncreaseY = sizeIncreaseX / _aspectRatio;
                    }

                    // Setting sizeDelta from the size increase
                    _movableElement.RectTransformComponent.sizeDelta = new Vector2(sizeIncreaseX, sizeIncreaseY);

                    // Preventing being scaled too small or too big
                    if (_multiOrientationElement == null || _multiOrientationElement.IsHorizontal) // For horizontal elements
                    {
                        float clampedWidth = Mathf.Clamp(_movableElement.RectTransformComponent.rect.width, _minWidth, _maxWidth);

                        // Incremental scaling
                        if (_isIncrementalScalingToggled)
                        {
                            float increment = (_maxWidth - _minWidth) / ScalingIncrementAmount;
                            clampedWidth = Mathf.Round(clampedWidth / increment) * increment;
                        }

                        _movableElement.RectTransformComponent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, clampedWidth);
                        _movableElement.RectTransformComponent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, clampedWidth / _aspectRatio);
                    }
                    else if (!_multiOrientationElement.IsHorizontal) // For vertical elements
                    {
                        float clampedHeight = Mathf.Clamp(_movableElement.RectTransformComponent.rect.height, _minHeight, _maxHeight);

                        // Incremental scaling
                        if (_isIncrementalScalingToggled)
                        {
                            float increment = (_maxHeight - _minHeight) / ScalingIncrementAmount;
                            clampedHeight = Mathf.Round(clampedHeight / increment) * increment;
                        }

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

        private const int ScalingIncrementAmount = 5;
        private const int MoveActionTreshold = 20;

        private Button[] _scaleHandles;
        private Button[] _changeOrientationButtons;
        private Button[] _flipHorizontallyButtons;
        private Button[] _flipVerticallyButtons;

        private int _currentScaleHandleIdx = DefaultScaleHandleIdx;
        private int _currentChangeOrientationButtonIdx = DefaultChangeOrientationButtonIdx;
        private int _currentFlipHorizontallyButtonIdx = DefaultFlipHorizontallyButtonIdx;
        private int _currentFlipVerticallyButtonIdx = DefaultFlipVerticallyButtonIdx;

        private Button _currentScaleHandle => _scaleHandles[_currentScaleHandleIdx];
        private Button _currentChangeOrientationButton => _changeOrientationButtons[_currentChangeOrientationButtonIdx];
        private Button _currentFlipHorizontallyButton => _flipHorizontallyButtons[_currentFlipHorizontallyButtonIdx];
        private Button _currentFlipVerticallyButton => _flipVerticallyButtons[_currentFlipVerticallyButtonIdx];

        private float _maxWidth => BattleUiEditor.EditorRect.width / 2;
        private float _maxHeight => BattleUiEditor.EditorRect.height / 3;
        private float _minWidth => BattleUiEditor.EditorRect.width / 6;
        private float _minHeight => BattleUiEditor.EditorRect.height / 10;

        private float _maxPosX => BattleUiEditor.EditorRect.width * BattleUiEditor.ScreenSpaceRatio - _movableElement.RectTransformComponent.rect.width * BattleUiEditor.ScreenSpaceRatio / 2;
        private float _maxPosY => BattleUiEditor.EditorRect.height * BattleUiEditor.ScreenSpaceRatio - _movableElement.RectTransformComponent.rect.height * BattleUiEditor.ScreenSpaceRatio / 2;
        private float _minPosX => _movableElement.RectTransformComponent.rect.width * BattleUiEditor.ScreenSpaceRatio / 2;
        private float _minPosY => _movableElement.RectTransformComponent.rect.height * BattleUiEditor.ScreenSpaceRatio / 2;
        private float _aspectRatio => _multiOrientationElement == null ? _movableElementAspectRatio : _multiOrientationElement.HorizontalAspectRatio;

        private BattleUiMovableElement _movableElement;
        private BattleUiMultiOrientationElement _multiOrientationElement;
        private BattleUiMovableElementData _data;

        private float _movableElementAspectRatio;

        private bool _isGridAlignToggled = false;
        private bool _isIncrementalScalingToggled = false;

        private ActionType _currentAction;
        private Vector2 _moveOffset;

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

            foreach (Button button in _changeOrientationButtons)
            {
                button.onClick.AddListener(ChangeOrientation);
            }

            foreach (Button button in _flipHorizontallyButtons)
            {
                button.onClick.AddListener(FlipHorizontally);
            }

            foreach (Button button in _flipVerticallyButtons)
            {
                button.onClick.AddListener(FlipVertically);
            }
        }

        private void OnDestroy()
        {
            foreach (Button button in _changeOrientationButtons)
            {
                button.onClick.RemoveAllListeners();
            }

            foreach (Button button in _flipHorizontallyButtons)
            {
                button.onClick.RemoveAllListeners();
            }

            foreach (Button button in _flipVerticallyButtons)
            {
                button.onClick.RemoveAllListeners();
            }
        }

        private void CalculateAndSetAnchors(Vector2? newSize = null)
        {
            Vector2 size = new(
                newSize == null ? _movableElement.RectTransformComponent.rect.width : newSize.Value.x,
                newSize == null ? _movableElement.RectTransformComponent.rect.height : newSize.Value.y
            );

            // Calculating anchors
            (Vector2 anchorMin, Vector2 anchorMax) = BattleUiEditor.CalculateAnchors(size, _movableElement.RectTransformComponent.localPosition, 0.5f);

            _data.AnchorMin = anchorMin;
            _data.AnchorMax = anchorMax;

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
            float scaleHandleWidth = BattleUiEditor.EditorRect.width * ScaleHandleSizeRatio;
            Vector2 buttonSize = new(scaleHandleWidth, scaleHandleWidth);

            foreach (Button scaleHandle in _scaleHandles)
            {
                scaleHandle.GetComponent<RectTransform>().sizeDelta = buttonSize;
            }

            if (_multiOrientationElement == null) return; // Returning if not a multiorientation element

            float controlButtonWidth = BattleUiEditor.EditorRect.width * ControlButtonSizeRatio;
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

            // Using method to check and switch visible scale handle in case it changed
            CheckAndSwitchVisibleControlButton(oldScaleHandle, _currentScaleHandle);

            // Returning if not a multiorientation element since the other control buttons aren't needed for normal elements
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
            CheckAndSwitchVisibleControlButton(oldFlipVerticallyButton, _currentFlipVerticallyButton);

            // Positioning top buttons, placing them into one array first
            Button[] topButtons = _flipHorizontallyButtons.Concat(_changeOrientationButtons).ToArray();

            // Adding left or right side offset to the buttons if they go outside the ui element holder
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
                    BattleUiEditor.EditorRectTransform.GetWorldCorners(holderCorners);

                    Vector2 newPosition = Vector2.zero;

                    // Checking left side and adding offset
                    float left = buttonCorners[(int)CornerType.BottomLeft].x;
                    if (left < 0)
                    {
                        newPosition.x += Mathf.Abs(left);
                    }

                    // Checking right side and adding offset
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

            // Ensuring the top and bottom buttons are next to each other
            for (int i = 0; i < _flipHorizontallyButtons.Length; i++)
            {
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

                // Setting the buttons which are both inside the editor as current
                if (IsButtonInsideEditor(GetButtonCorners(_changeOrientationButtons[i])) && IsButtonInsideEditor(GetButtonCorners(_flipHorizontallyButtons[i])))
                {
                    _currentChangeOrientationButtonIdx = i;
                    _currentFlipHorizontallyButtonIdx = i;
                    break;
                }
            }

            // Checking visibility for the top buttons
            CheckAndSwitchVisibleControlButton(oldFlipHorizontallyButton, _currentFlipHorizontallyButton);
            CheckAndSwitchVisibleControlButton(oldChangeOrientationButton, _currentChangeOrientationButton);

            // Check if the scale handle is overlapping (only if vertical multi orientation element)
            if (_multiOrientationElement.IsHorizontal) return;

            // Getting rects and rect transforms
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

        private void CheckAndSwitchVisibleControlButton(Button oldButton, Button currentButton)
        {
            if (oldButton != currentButton)
            {
                if (oldButton.gameObject.activeSelf)
                {
                    oldButton.gameObject.SetActive(false);
                    currentButton.gameObject.SetActive(true);
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
                Vector3 localSpacePoint = BattleUiEditor.EditorRectTransform.InverseTransformPoint(buttonCorners[i]);
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
            Rect uiRect = BattleUiEditor.EditorRect;
            return point.x >= uiRect.xMin && point.x <= uiRect.xMax && point.y >= uiRect.yMin && point.y <= uiRect.yMax;
        }
    }
}
