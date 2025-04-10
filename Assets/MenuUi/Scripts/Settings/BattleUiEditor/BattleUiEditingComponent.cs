using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Altzone.Scripts.BattleUiShared;

namespace MenuUi.Scripts.Settings.BattleUiEditor
{
    /// <summary>
    /// Handles the functionality for BattleUiEditingComponent prefab.
    /// </summary>
    public class BattleUiEditingComponent: MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Button _scaleHandleButton;
        [SerializeField] private Button _flipButton;
        [SerializeField] private Button _changeOrientationButton;

        public void SetInfo(BattleUiMovableElement movableElement, Transform uiElementHolder)
        {
            _flipButton.gameObject.SetActive(false);
            _changeOrientationButton.gameObject.SetActive(false);

            _movableElement = movableElement;
            _uiElementHolder = uiElementHolder.GetComponent<RectTransform>();

            SetControlButtonSize(_scaleHandleButton, 0.05f);
        }

        public void SetInfo(BattleUiMultiOrientationElement multiOrientationElement, Transform uiElementHolder)
        {
            _flipButton.gameObject.SetActive(true);
            _changeOrientationButton.gameObject.SetActive(true);

            _multiOrientationElement = multiOrientationElement;
            _movableElement = multiOrientationElement;
            _uiElementHolder = uiElementHolder.GetComponent<RectTransform>();

            SetControlButtonSize(_scaleHandleButton, 0.05f);
            SetControlButtonSize(_flipButton, 0.1f);
            SetControlButtonSize(_changeOrientationButton, 0.1f);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Button selectedButton = eventData.selectedObject.GetComponent<Button>();

            if (selectedButton == _flipButton || selectedButton == _changeOrientationButton)
            {
                _currentAction = ActionType.None;
            }
            else if (selectedButton == _scaleHandleButton)
            {
                _currentAction = ActionType.Scale;
            }
            else
            {
                _currentAction = ActionType.Move;
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
                    //eventData.delta
                    break;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            CalculateAndSetAnchors();
        }

        enum ActionType
        {
            None,
            Move,
            Scale
        }

        private RectTransform _uiElementHolder;
        private BattleUiMovableElement _movableElement;
        private BattleUiMultiOrientationElement _multiOrientationElement;
        private ActionType _currentAction;

        private void CalculateAndSetAnchors()
        {
            float holderWidth = _uiElementHolder.rect.width;
            float holderHeight = _uiElementHolder.rect.height;

            RectTransform _movableElementRect = _movableElement.GetComponent<RectTransform>();
            float xPos = _movableElementRect.localPosition.x;
            float yPos = _movableElementRect.localPosition.y;

            float ownWidth = _movableElementRect.rect.width;
            float ownHeight = _movableElementRect.rect.height;

            float anchorXMin = (xPos - ownWidth / 2.0f) / holderWidth + 0.5f;
            float anchorXMax = (xPos + ownWidth / 2.0f) / holderWidth + 0.5f;

            float anchorYMin = (yPos - ownHeight / 2.0f) / holderHeight + 0.5f;
            float anchorYMax = (yPos + ownHeight / 2.0f) / holderHeight + 0.5f;

            Vector2 anchorMin = new(anchorXMin, anchorYMin);
            Vector2 anchorMax = new(anchorXMax, anchorYMax);

            _movableElementRect.anchorMin = anchorMin;
            _movableElementRect.anchorMax = anchorMax;

            _movableElementRect.offsetMin = Vector2.zero;
            _movableElementRect.offsetMax = Vector2.zero;
        }

        // Is used to calculate appropiate size for the control buttons, since if they were anchored they would grow with the element when it's scaled
        private void SetControlButtonSize(Button button, float sizeRatio)
        {
            float buttonWidth = _uiElementHolder.rect.width * sizeRatio;
            Vector2 buttonSize = new Vector2(buttonWidth, buttonWidth);
            button.GetComponent<RectTransform>().sizeDelta = buttonSize;
        }
    }
}
