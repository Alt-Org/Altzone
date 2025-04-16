using System;
using System.Collections;

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
        [SerializeField] private Button _scaleHandleButton;
        [SerializeField] private Button _flipHorizontallyButton;
        [SerializeField] private Button _flipVerticallyButton;
        [SerializeField] private Button _changeOrientationButton;
        [SerializeField] private GameObject _dragDelayDisplay;
        [SerializeField] private Image _dragDelayFill;

        public void SetInfo(BattleUiMovableElement movableElement, Transform uiElementHolder)
        {
            _movableElement = movableElement;
            _uiElementHolder = uiElementHolder.GetComponent<RectTransform>();

            SetControlButtonSize(_scaleHandleButton, 0.05f);

            _data = movableElement.GetData();
            _movableElementAspectRatio = movableElement.RectTransformComponent.rect.width / movableElement.RectTransformComponent.rect.height;
        }

        public void SetInfo(BattleUiMultiOrientationElement multiOrientationElement, Transform uiElementHolder)
        {
            _multiOrientationElement = multiOrientationElement;
            _movableElement = multiOrientationElement;
            _uiElementHolder = uiElementHolder.GetComponent<RectTransform>();

            SetControlButtonSize(_scaleHandleButton, 0.05f);
            SetControlButtonSize(_flipHorizontallyButton, 0.1f);
            SetControlButtonSize(_flipVerticallyButton, 0.1f);
            SetControlButtonSize(_changeOrientationButton, 0.1f);

            _flipHorizontallyButton.onClick.AddListener(FlipHorizontally);
            _flipVerticallyButton.onClick.AddListener(FlipVertically);
            _changeOrientationButton.onClick.AddListener(ChangeOrientation);

            _data = multiOrientationElement.GetData();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPointerDown = true;

            if (_dragTimerHolder == null) _dragTimerHolder = StartCoroutine(StartDragTimer(() =>
            {
                _dragTimerHolder = null;
                if (!_isPointerDown) return;
                _currentAction = ActionType.Move;
            }));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPointerDown = false;
            if (_dragTimerHolder != null)
            {
                StopCoroutine(_dragTimerHolder);
                _dragTimerHolder = null;
                _dragDelayDisplay.SetActive(false);
                ShowControls(!_scaleHandleButton.gameObject.activeSelf);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Button selectedButton = eventData.selectedObject != null ? eventData.selectedObject.GetComponent<Button>() : null;

            if (selectedButton == _flipHorizontallyButton || selectedButton == _flipVerticallyButton || selectedButton == _changeOrientationButton)
            {
                _currentAction = ActionType.None;
            }
            else if (selectedButton == _scaleHandleButton)
            {
                _currentAction = ActionType.Scale;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            switch (_currentAction)
            {
                case ActionType.Move:
                    _movableElement.transform.position = eventData.position;
                    break;
                case ActionType.Scale:
                    // Scaling while keeping aspect ratio
                    float sizeIncreaseX = eventData.delta.x;
                    float sizeIncreaseY = sizeIncreaseX / (_movableElement.RectTransformComponent.rect.width / _movableElement.RectTransformComponent.rect.height);
                    _movableElement.RectTransformComponent.sizeDelta += new Vector2(sizeIncreaseX, sizeIncreaseY);

                    // Preventing out of bounds scaling or being scaled too small
                    if (_multiOrientationElement == null || _multiOrientationElement.IsHorizontal)
                    {
                        float clampedWidth = Mathf.Clamp(_movableElement.RectTransformComponent.rect.width, _minWidth, _maxWidth);
                        float aspectRatio = _multiOrientationElement == null ? _movableElementAspectRatio : _multiOrientationElement.HorizontalAspectRatio;

                        _movableElement.RectTransformComponent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, clampedWidth);
                        _movableElement.RectTransformComponent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, clampedWidth / aspectRatio);
                    }
                    else if (!_multiOrientationElement.IsHorizontal)
                    {
                        float clampedHeight = Mathf.Clamp(_movableElement.RectTransformComponent.rect.height, _minHeight, _maxHeight);

                        _movableElement.RectTransformComponent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, clampedHeight * _multiOrientationElement.VerticalAspectRatio);
                        _movableElement.RectTransformComponent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, clampedHeight);
                    }
                    break;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            CalculateAndSetAnchors();
            CheckControlButtonsVisibility();
            _currentAction = ActionType.None;
        }

        enum ActionType
        {
            None,
            Move,
            Scale
        }

        enum CornerType // Helper enum to access button world corners more readably.
        {
            BottomLeft = 0,
            TopLeft = 1,
            TopRight = 2,
            BottomRight = 3,
        }

        private RectTransform _uiElementHolder;
        private float _maxWidth => _uiElementHolder.rect.width / 1.5f;
        private float _maxHeight => _uiElementHolder.rect.height / 2;
        private float _minWidth => _uiElementHolder.rect.width / 7.5f;
        private float _minHeight => _uiElementHolder.rect.height / 10;

        private BattleUiMovableElement _movableElement;
        private BattleUiMultiOrientationElement _multiOrientationElement;
        private BattleUiMovableElementData _data;

        private float _movableElementAspectRatio;

        private bool _isPointerDown = false;
        private Coroutine _dragTimerHolder = null;
        private ActionType _currentAction;

        private void OnEnable()
        {
            ShowControls(false);
        }

        private void OnDestroy()
        {
            _flipHorizontallyButton.onClick.RemoveAllListeners();
            _flipVerticallyButton.onClick.RemoveAllListeners();
            _changeOrientationButton.onClick.RemoveAllListeners();
        }

        private IEnumerator StartDragTimer(Action callback)
        {
            _dragDelayFill.fillAmount = 0;
            _dragDelayDisplay.SetActive(true);

            float timePassed = 0f;
            while (timePassed < 0.5f)
            {
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
        }

        private void FlipHorizontally()
        {
            _data.IsFlippedHorizontally = !_data.IsFlippedHorizontally;
            _multiOrientationElement.SetData(_data);
        }

        private void FlipVertically()
        {
            _data.IsFlippedVertically = !_data.IsFlippedVertically;
            _multiOrientationElement.SetData(_data);
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

        private void ShowControls(bool show)
        {
            _scaleHandleButton.gameObject.SetActive(show);

            if (_multiOrientationElement == null) show = false;
            _flipHorizontallyButton.gameObject.SetActive(show);
            _flipVerticallyButton.gameObject.SetActive(show);
            _changeOrientationButton.gameObject.SetActive(show);
        }

        // Method used to calculate appropiate size for the control buttons, since if they were anchored they would grow with the element when it's scaled
        private void SetControlButtonSize(Button button, float sizeRatio)
        {
            float buttonWidth = _uiElementHolder.rect.width * sizeRatio;
            Vector2 buttonSize = new Vector2(buttonWidth, buttonWidth);
            button.GetComponent<RectTransform>().sizeDelta = buttonSize;
        }

        private void CheckControlButtonsVisibility()
        {
            Button[] buttons = new Button[] { _scaleHandleButton, _changeOrientationButton, _flipHorizontallyButton, _flipVerticallyButton };

            foreach (Button button in buttons)
            {
                RectTransform buttonRectTransform = button.GetComponent<RectTransform>();

                // Setting button to default position
                buttonRectTransform.anchoredPosition = Vector2.zero;

                // Getting button corners
                Vector3[] buttonCorners = new Vector3[4];
                buttonRectTransform.GetWorldCorners(buttonCorners);

                // Checking that the button is entirely inside ui element holder
                bool isButtonInside = true;
                for (int i = 0; i < buttonCorners.Length; i++)
                {
                    Vector3 localSpacePoint = _uiElementHolder.InverseTransformPoint(buttonCorners[i]);
                    if (!_uiElementHolder.rect.Contains(localSpacePoint))
                    {
                        isButtonInside = false;
                        break;
                    }
                }

                // If not updating the position
                if (!isButtonInside)
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

                    // Checking bottom side
                    float bottom = buttonCorners[(int)CornerType.BottomLeft].y;
                    if (bottom < 0)
                    {
                        newPosition.y += Mathf.Abs(bottom);
                    }

                    // Checking top side
                    float buttonTop = buttonCorners[(int)CornerType.TopLeft].y;
                    float holderTop = holderCorners[(int)CornerType.TopLeft].y;
                    if (buttonTop > holderTop)
                    {
                        newPosition.y -= buttonTop - holderTop;
                    }

                    buttonRectTransform.anchoredPosition = newPosition;
                }
            }
        }
    }
}
