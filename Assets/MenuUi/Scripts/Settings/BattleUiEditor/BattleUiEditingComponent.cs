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

        public void SetInfo(BattleUiMovableElement movableElement)
        {
            _flipButton.gameObject.SetActive(false);
            _changeOrientationButton.gameObject.SetActive(false);

            _movableElement = movableElement;
        }

        public void SetInfo(BattleUiMultiOrientationElement multiOrientationElement)
        {
            _flipButton.gameObject.SetActive(true);
            _changeOrientationButton.gameObject.SetActive(true);

            _multiOrientationElement = multiOrientationElement;
            _movableElement = multiOrientationElement;
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

        }

        enum ActionType
        {
            None,
            Move,
            Scale
        }

        private BattleUiMovableElement _movableElement;
        private BattleUiMultiOrientationElement _multiOrientationElement;
        private ActionType _currentAction;
    }
}
