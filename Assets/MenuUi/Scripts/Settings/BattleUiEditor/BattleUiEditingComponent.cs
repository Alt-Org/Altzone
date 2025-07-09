using System;
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

        [Header("Handle size sliders")]
        [SerializeField] private RectTransform _handleSizeRectTransformTop;
        [SerializeField] private Slider _handleSizeSliderTop;
        [SerializeField] private RectTransform _handleSizeRectTransformBottom;
        [SerializeField] private Slider _handleSizeSliderBottom;

        public delegate void UiElementSelectedHandler(BattleUiEditingComponent self);
        public UiElementSelectedHandler OnUiElementSelected;

        public delegate void GridSnapHandler(int gridColumnIndex, int gridRowIndex);
        public GridSnapHandler OnGridSnap;

        public Action OnUiElementEdited;

        public void SetInfo(BattleUiMovableElement movableElement)
        {
            _movableElement = movableElement;
            UpdateData();

            InitializeControlButtons();
            SetControlButtonSizes();
            ShowControls(false);

            // Hiding handle size rect transforms as a special case because they have aspect ratio fitter which needs to be active to work
            // And show controls only hides the current controls which the ui element displays
            _handleSizeRectTransformTop.gameObject.SetActive(false);
            _handleSizeRectTransformBottom.gameObject.SetActive(false);

            _movableElementAspectRatio = movableElement.RectTransformComponent.rect.width / movableElement.RectTransformComponent.rect.height;
        }

        public void SetInfo(BattleUiMovableJoystickElement movableJoystickElement)
        {
            _movableJoystickElement = movableJoystickElement;
            SetInfo((BattleUiMovableElement)movableJoystickElement);
        }

        public void SetInfo(BattleUiMultiOrientationElement multiOrientationElement)
        {
            _multiOrientationElement = multiOrientationElement;
            SetInfo((BattleUiMovableElement)multiOrientationElement);
        }

        public void ShowControls(bool show)
        {
            if (show) CheckControlButtonsVisibility();
            _currentScaleHandle.gameObject.SetActive(show);

            if (_movableJoystickElement != null)
            {
                _handleSizeSliders[_currentHandleSizeIdx].SetValueWithoutNotify(_data.HandleSize == 0 ? BattleUiMovableJoystickElement.HandleSizeDefault : _data.HandleSize);
                _currentHandleSizeRectTransform.gameObject.SetActive(show);
            }

            if (_multiOrientationElement == null) return;
            _currentFlipHorizontallyButton.gameObject.SetActive(show);
            _currentFlipVerticallyButton.gameObject.SetActive(show);
            _currentChangeOrientationButton.gameObject.SetActive(show);
        }

        public void UpdateData()
        {
            if (_multiOrientationElement != null) _data = _multiOrientationElement.GetData();
            else if (_movableJoystickElement != null) _data = _movableJoystickElement.GetData();
            else _data = _movableElement.GetData();
        }

        public void ToggleGrid(bool toggle)
        {
            _isGridAlignToggled = toggle;
        }

        public void ToggleIncrementScaling(bool toggle)
        {
            _isIncrementalScalingToggled = toggle;
        }

        public void UpdateTransparency(int newTransparency)
        {
            _data.Transparency = newTransparency;
            _movableElement.SetData(_data);
        }

        public int GetCurrentTransparency()
        {
            return _data.Transparency;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnUiElementSelected?.Invoke(this);
            _movableElement.transform.SetAsLastSibling();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_currentAction != ActionType.Scale) ShowControls(!_currentScaleHandle.gameObject.activeSelf);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Button selectedButton = eventData.selectedObject != null ? eventData.selectedObject.GetComponent<Button>() : null;

            if (_multiOrientationElement != null && (selectedButton == _currentFlipHorizontallyButton || selectedButton == _currentFlipVerticallyButton || selectedButton == _currentChangeOrientationButton))
            {
                _currentAction = ActionType.None;
            }
            else if (selectedButton == _currentScaleHandle)
            {
                _currentAction = ActionType.Scale;
                if (_movableJoystickElement != null) _joystickElementSizeDelta = _movableJoystickElement.RectTransformComponent.sizeDelta;
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
                        _dragPos = _movableElement.RectTransformComponent.position;
                    }
                    break;

                case ActionType.Move:
                    _dragPos += eventData.delta;
                    Vector3 newPos = _dragPos;
                    //RectTransformUtility.ScreenPointToWorldPointInRectangle(BattleUiEditor.EditorRectTransform, eventData.position, null, out newPos);

                    if (_isGridAlignToggled) // Snapping to grid while moving
                    {
                        (int gridColumnIndex, int gridRowIndex) = GridController.GetGridLinesIndex(newPos);
                        newPos = GridController.GetGridSnapPosition(gridColumnIndex, gridRowIndex);

                        OnGridSnap?.Invoke(gridColumnIndex, gridRowIndex);
                    }

                    // Clamping position to be inside the editor and setting it with the method
                    ClampAndSetPosition(newPos);
                    break;

                case ActionType.Scale:
                    Vector2 sizeIncrease = Vector2.zero;

                    // If vertical multiorientation element we scale in y axis
                    if (_multiOrientationElement != null && !_multiOrientationElement.IsHorizontal)
                    {
                        sizeIncrease.y = -((eventData.position.y - eventData.pressPosition.y) * 2);

                        // We have to invert scaling for the top side scale handles when scaling vertically
                        if (_currentScaleHandleIdx == (int)CornerType.TopLeft || _currentScaleHandleIdx == (int)CornerType.TopRight)
                        {
                            sizeIncrease.y = -sizeIncrease.y;
                        }
                    }
                    else // If horizontal ui element we scale in x axis
                    {
                        sizeIncrease.x = (eventData.position.x - eventData.pressPosition.x) * 2;

                        // We have to invert scaling for the left side scale handles when scaling horizontally
                        if (_currentScaleHandleIdx == (int)CornerType.TopLeft || _currentScaleHandleIdx == (int)CornerType.BottomLeft)
                        {
                            sizeIncrease.x = -sizeIncrease.x;
                        }
                    }

                    // Getting the other value from aspect ratio
                    if (sizeIncrease.x == 0f)
                    {
                        sizeIncrease.x = sizeIncrease.y * _aspectRatio;
                    }
                    else if (sizeIncrease.y == 0f)
                    {
                        // Exception for setting height to rotate joystick since its height should stay the same
                        if (_data.UiElementType != SettingsCarrier.BattleUiElementType.RotateJoystick) sizeIncrease.y = sizeIncrease.x / _aspectRatio;
                    }

                    // Setting sizeDelta from the size increase
                    _movableElement.RectTransformComponent.sizeDelta = _movableJoystickElement == null ? sizeIncrease : sizeIncrease + _joystickElementSizeDelta;

                    // Preventing being scaled too small or too big
                    ClampSize();

                    // Preventing Ui element from going out of editor bounds while scaling
                    ClampAndSetPosition(_movableElement.RectTransformComponent.position);
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

        private const float ScaleHandleSizeRatio = 0.075f;
        private const float ControlButtonSizeRatio = 0.1f;

        private const int DefaultScaleHandleIdx = (int)CornerType.BottomRight;
        private const int DefaultChangeOrientationButtonIdx = (int)ControlButtonVertical.Top;
        private const int DefaultFlipHorizontallyButtonIdx = (int)ControlButtonVertical.Top;
        private const int DefaultFlipVerticallyButtonIdx = (int)ControlButtonHorizontal.Right;
        private const int DefaultHandleSizeRectTransformIdx = (int)ControlButtonVertical.Top;

        private const int ScalingIncrementAmount = 5;
        private const int MoveActionTreshold = 20;

        private Button[] _scaleHandles;
        private Button[] _changeOrientationButtons;
        private Button[] _flipHorizontallyButtons;
        private Button[] _flipVerticallyButtons;
        private Slider[] _handleSizeSliders;
        private RectTransform[] _handleSizeRectTransforms;

        private int _currentScaleHandleIdx = DefaultScaleHandleIdx;
        private int _currentChangeOrientationButtonIdx = DefaultChangeOrientationButtonIdx;
        private int _currentFlipHorizontallyButtonIdx = DefaultFlipHorizontallyButtonIdx;
        private int _currentFlipVerticallyButtonIdx = DefaultFlipVerticallyButtonIdx;
        private int _currentHandleSizeIdx = DefaultHandleSizeRectTransformIdx;

        private Button _currentScaleHandle => _scaleHandles[_currentScaleHandleIdx];
        private Button _currentChangeOrientationButton => _changeOrientationButtons[_currentChangeOrientationButtonIdx];
        private Button _currentFlipHorizontallyButton => _flipHorizontallyButtons[_currentFlipHorizontallyButtonIdx];
        private Button _currentFlipVerticallyButton => _flipVerticallyButtons[_currentFlipVerticallyButtonIdx];
        private RectTransform _currentHandleSizeRectTransform => _handleSizeRectTransforms[_currentHandleSizeIdx];

        private float _maxWidth => BattleUiEditor.EditorRect.width / 2;
        private float _maxHeight => BattleUiEditor.EditorRect.height / 3;
        private float _minWidth => _movableJoystickElement == null ? BattleUiEditor.EditorRect.width / 6 : _data.HandleSize;
        private float _minHeight => BattleUiEditor.EditorRect.height / 10;

        private Vector2 _maxPos
        {
            get
            {
                Vector3[] editorCorners = GetEditorCorners();
                Vector2 worldSpaceSize = GetUiElementSizeInWorldSpace();

                return new Vector2(
                editorCorners[(int)CornerType.TopRight].x - worldSpaceSize.x * 0.5f,
                editorCorners[(int)CornerType.TopRight].y - worldSpaceSize.y * 0.5f);
            }
        }
        private Vector2 _minPos
        {
            get
            {
                Vector3[] editorCorners = GetEditorCorners();
                Vector2 worldSpaceSize = GetUiElementSizeInWorldSpace();

                return new Vector2(
                editorCorners[(int)CornerType.BottomLeft].x + worldSpaceSize.x * 0.5f,
                editorCorners[(int)CornerType.BottomLeft].y + worldSpaceSize.y * 0.5f);
            }
        }

        private float _aspectRatio => _multiOrientationElement == null ? _movableElementAspectRatio : _multiOrientationElement.HorizontalAspectRatio;

        private BattleUiMovableElement _movableElement;
        private BattleUiMovableJoystickElement _movableJoystickElement;
        private BattleUiMultiOrientationElement _multiOrientationElement;
        private BattleUiMovableElementData _data;

        private float _movableElementAspectRatio;

        private bool _isGridAlignToggled = false;
        private bool _isIncrementalScalingToggled = false;

        private ActionType _currentAction;
        private Vector2 _dragPos;
        private Vector2 _joystickElementSizeDelta;

        private void OnDisable()
        {
            ShowControls(false);
        }

        private void OnDestroy()
        {
            if (_multiOrientationElement != null)
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
                
            if (_movableJoystickElement != null)
            {
                foreach (Slider slider in _handleSizeSliders)
                {
                    slider.onValueChanged.RemoveAllListeners();
                }
            }
        }

        private void InitializeControlButtons()
        {
            // Arranging scale handles to array so that they can be accessed better with the enums
            _scaleHandles = new[] { _scaleHandleBottomLeft, _scaleHandleTopLeft, _scaleHandleTopRight, _scaleHandleBottomRight };

            if (_multiOrientationElement != null)
            {
                // Arranging control buttons to array so that they can be accessed better with the enums
                _changeOrientationButtons = new[] { _changeOrientationButtonTop, _changeOrientationButtonBottom };
                _flipHorizontallyButtons = new[] { _flipHorizontallyButtonTop, _flipHorizontallyButtonBottom };
                _flipVerticallyButtons = new[] { _flipVerticallyButtonLeft, _flipVerticallyButtonRight };

                // Adding listeners to control buttons
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

            if (_movableJoystickElement != null)
            {
                // Arranging handle size Sliders and RectTransforms to array so that they can be accessed better with the enums
                _handleSizeRectTransforms = new[] { _handleSizeRectTransformTop, _handleSizeRectTransformBottom };
                _handleSizeSliders = new[] { _handleSizeSliderTop, _handleSizeSliderBottom };

                // Initializing sliders and adding listeners
                foreach (Slider slider in _handleSizeSliders)
                {
                    slider.maxValue = BattleUiMovableJoystickElement.HandleSizeMax;
                    slider.minValue = BattleUiMovableJoystickElement.HandleSizeMin;
                    slider.onValueChanged.AddListener(ChangeHandleSize);
                }
            }
        }

        private void ClampAndSetPosition(Vector2 newPos)
        {
            Vector2 clampedPos = new(
                Mathf.Clamp(newPos.x, _minPos.x, _maxPos.x),
                Mathf.Clamp(newPos.y, _minPos.y, _maxPos.y)
            );

            if ((Vector2)_movableElement.RectTransformComponent.position != clampedPos) _movableElement.RectTransformComponent.position = clampedPos;
        }

        private void ClampSize(bool useIncrementalScalingIfToggled = true)
        {
            Vector2 clampedSize = Vector2.zero;
            if (_multiOrientationElement == null || _multiOrientationElement.IsHorizontal) // For horizontal elements
            {
                clampedSize.x = Mathf.Clamp(_movableElement.RectTransformComponent.rect.width, _minWidth, _maxWidth);

                // Incremental scaling
                if (_isIncrementalScalingToggled && useIncrementalScalingIfToggled)
                {
                    float increment = (_maxWidth - _minWidth) / ScalingIncrementAmount;
                    clampedSize.x = Mathf.Clamp(Mathf.Round(clampedSize.x / increment) * increment, _minWidth, _maxWidth);
                }

                clampedSize.y = clampedSize.x / _aspectRatio;
            }
            else if (!_multiOrientationElement.IsHorizontal) // For vertical multiorientation elements
            {
                clampedSize.y = Mathf.Clamp(_movableElement.RectTransformComponent.rect.height, _minHeight, _maxHeight);

                // Incremental scaling
                if (_isIncrementalScalingToggled && useIncrementalScalingIfToggled)
                {
                    float increment = (_maxHeight - _minHeight) / ScalingIncrementAmount;
                    clampedSize.y = Mathf.Clamp(Mathf.Round(clampedSize.y / increment) * increment, _minWidth, _maxWidth);
                }

                clampedSize.x = clampedSize.y * _multiOrientationElement.VerticalAspectRatio;
            }

            _movableElement.RectTransformComponent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, clampedSize.x);
            if (_data.UiElementType != SettingsCarrier.BattleUiElementType.RotateJoystick) _movableElement.RectTransformComponent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, clampedSize.y);
        }

        private void CalculateAndSetAnchors(Vector2? newSize = null)
        {
            Vector2 size = newSize == null ? _movableElement.RectTransformComponent.rect.size : newSize.Value;

            // Calculating anchors
            (Vector2 anchorMin, Vector2 anchorMax) = BattleUiEditor.CalculateAnchors(size, _movableElement.RectTransformComponent.localPosition, 0.5f, true);

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

        private void ChangeHandleSize(float value)
        {
            _data.HandleSize = (int)value;
            _movableJoystickElement.SetData(_data);

            // Clamping Ui element size and position because changing handle size makes the ui element grow vertically,
            // and horizontally it should be atleast the width of the handle
            ClampSize(false);
            ClampAndSetPosition(_movableElement.RectTransformComponent.position);

            _movableElementAspectRatio = _movableJoystickElement.RectTransformComponent.rect.width / _movableJoystickElement.RectTransformComponent.rect.height;

            CheckControlButtonsVisibility();
            OnUiElementEdited?.Invoke();
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

            // Setting handle size slider size if the ui element is a joystick
            if (_movableJoystickElement != null)
            {
                foreach (RectTransform rectTransform in _handleSizeRectTransforms)
                {
                    // Aspect ratio fitter will adjust the width to be correct for the handle size sliders
                    rectTransform.sizeDelta = buttonSize;
                }
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
            CheckNewGameObjectIsActive(oldScaleHandle.gameObject, _currentScaleHandle.gameObject);

            if (_movableJoystickElement != null)
            {
                // Adding offset to the rect transforms to ensure they are not overlapping with the sides
                foreach (RectTransform rectTransform in _handleSizeRectTransforms)
                {
                    AddXOffsetIfOutsideBounds(rectTransform);
                }

                RectTransform oldHandleSizeRectTransform = _currentHandleSizeRectTransform;

                // Getting world corners for default handle size control
                Vector3[] defaultHandleSizeCorners = new Vector3[4];
                _handleSizeRectTransforms[DefaultHandleSizeRectTransformIdx].GetWorldCorners(defaultHandleSizeCorners);

                // Checking if the default handle size slider is inside the editor
                if (IsButtonInsideEditor(defaultHandleSizeCorners))
                {
                    _currentHandleSizeIdx = DefaultHandleSizeRectTransformIdx;
                }
                else // Setting the other handle size slider as current
                {
                    _currentHandleSizeIdx = DefaultHandleSizeRectTransformIdx == ControlButtonVertical.Top ? (int)ControlButtonVertical.Bottom : (int)ControlButtonVertical.Top;
                }

                // Checking that the new visible handle size slider gameobject is active
                CheckNewGameObjectIsActive(oldHandleSizeRectTransform.gameObject, _currentHandleSizeRectTransform.gameObject);

                // Getting scale handle rect to check if it's overlapping with the handle size slider rect
                RectTransform scaleRectTransform = _currentScaleHandle.GetComponent<RectTransform>();
                Rect scaleRect = scaleRectTransform.rect;

                // Checking if the scale handle overlaps handle size slider rect
                if (scaleRect.Overlaps(_currentHandleSizeRectTransform.rect))
                {
                    Vector3[] handleSizeCorners = new Vector3[4];
                    _currentHandleSizeRectTransform.GetWorldCorners(handleSizeCorners);

                    // Changing scale handle world position to one of the handle size rect transform's corners
                    switch ((CornerType)_currentScaleHandleIdx)
                    {
                        case CornerType.BottomLeft:
                            scaleRectTransform.position = handleSizeCorners[(int)CornerType.TopLeft];
                            break;
                        case CornerType.TopLeft:
                            scaleRectTransform.position = handleSizeCorners[(int)CornerType.BottomLeft];
                            break;
                        case CornerType.BottomRight:
                            scaleRectTransform.position = handleSizeCorners[(int)CornerType.TopRight];
                            break;
                        case CornerType.TopRight:
                            scaleRectTransform.position = handleSizeCorners[(int)CornerType.BottomRight];
                            break;
                    }
                }
            }

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
                _currentFlipVerticallyButtonIdx = DefaultFlipVerticallyButtonIdx == (int)ControlButtonHorizontal.Right ? (int)ControlButtonHorizontal.Left : (int)ControlButtonHorizontal.Right;
            }

            // Checking if we should change the visible button
            CheckNewGameObjectIsActive(oldFlipVerticallyButton.gameObject, _currentFlipVerticallyButton.gameObject);

            // Positioning top buttons, placing them into one array first
            Button[] topButtons = _flipHorizontallyButtons.Concat(_changeOrientationButtons).ToArray();

            // Adding left or right side offset to the buttons if they go outside the ui element holder
            foreach (Button button in topButtons)
            {
                RectTransform buttonRectTransform = button.GetComponent<RectTransform>();
                AddXOffsetIfOutsideBounds(buttonRectTransform);
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
            CheckNewGameObjectIsActive(oldFlipHorizontallyButton.gameObject, _currentFlipHorizontallyButton.gameObject);
            CheckNewGameObjectIsActive(oldChangeOrientationButton.gameObject, _currentChangeOrientationButton.gameObject);

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

        private void CheckNewGameObjectIsActive(GameObject oldControl, GameObject newControl)
        {
            if (oldControl != newControl)
            {
                if (oldControl.activeSelf)
                {
                    oldControl.SetActive(false);
                    newControl.SetActive(true);
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

        private Vector3[] GetEditorCorners()
        {
            Vector3[] corners = new Vector3[4];
            BattleUiEditor.EditorRectTransform.GetWorldCorners(corners);
            return corners;
        }

        private void AddXOffsetIfOutsideBounds(RectTransform rectTransform)
        {
            // Setting rect transform to default position
            rectTransform.anchoredPosition = Vector2.zero;

            // Getting world corners
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            // If rect transform corners is not inside the editor adding offset
            if (!IsButtonInsideEditor(corners))
            {
                Vector3[] holderCorners = GetEditorCorners();
                
                Vector2 newPosition = Vector2.zero;

                // Checking left side and adding offset
                float leftSide = corners[(int)CornerType.BottomLeft].x;
                float holderLeft = holderCorners[(int)CornerType.BottomLeft].x;
                if (leftSide < holderLeft)
                {
                    newPosition.x += holderLeft - leftSide;
                }

                // Checking right side and adding offset
                float rightSide = corners[(int)CornerType.BottomRight].x;
                float holderRight = holderCorners[(int)CornerType.BottomRight].x;
                if (rightSide > holderRight)
                {
                    newPosition.x -= rightSide - holderRight;
                }

                // Setting the new offset position
                rectTransform.anchoredPosition = newPosition;
            }
        }

        private Vector2 GetUiElementSizeInWorldSpace()
        {
            Vector3[] uiElementCorners = new Vector3[4];
            _movableElement.RectTransformComponent.GetWorldCorners(uiElementCorners);

            return new(uiElementCorners[(int)CornerType.TopRight].x - uiElementCorners[(int)CornerType.TopLeft].x,
                       uiElementCorners[(int)CornerType.TopRight].y - uiElementCorners[(int)CornerType.BottomRight].y);
        }
    }
}
