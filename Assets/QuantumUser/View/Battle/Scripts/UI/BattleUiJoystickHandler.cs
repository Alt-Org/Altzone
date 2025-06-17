using UnityEngine;

using Altzone.Scripts.BattleUiShared;

using BattleUiElementType = SettingsCarrier.BattleUiElementType;

namespace Battle.View.UI
{
    /// <summary>
    /// Handles Battle Ui joystick functionality.
    /// </summary>
    public class BattleUiJoystickHandler : MonoBehaviour
    {
        [SerializeField] private BattleUiController _uiController;
        [SerializeField] private BattleUiMovableElement _moveJoystickMovableElement;
        [SerializeField] private BattleUiMovableElement _rotateJoystickMovableElement;
        [SerializeField] private BattleUiJoystickComponent _moveJoystickComponent;
        [SerializeField] private BattleUiJoystickComponent _rotateJoystickComponent;

        public bool IsVisible => _moveJoystickMovableElement.gameObject.activeSelf;
        public BattleUiMovableElement MoveJoystickMovableElement => _moveJoystickMovableElement;
        public BattleUiMovableElement RotateJoystickMovableElement => _rotateJoystickMovableElement;

        public void SetShow(bool show, BattleUiElementType uiElementType = BattleUiElementType.None)
        {
            switch (uiElementType)
            {
                case BattleUiElementType.None:
                    _moveJoystickMovableElement.gameObject.SetActive(show);
                    _rotateJoystickMovableElement.gameObject.SetActive(show);
                    break;
                case BattleUiElementType.MoveJoystick:
                    _moveJoystickMovableElement.gameObject.SetActive(show);
                    break;
                case BattleUiElementType.RotateJoystick:
                    _rotateJoystickMovableElement.gameObject.SetActive(show);
                    break;
            }
        }

        public void SetInfo(BattleUiElementType uiElementType, BattleUiMovableElementData data = null)
        {
            // Selecting correct movable element
            BattleUiMovableElement movableElement;
            switch (uiElementType)
            {
                case BattleUiElementType.MoveJoystick:
                    movableElement = _moveJoystickMovableElement;
                    break;
                case BattleUiElementType.RotateJoystick:
                    movableElement = _rotateJoystickMovableElement;
                    break;
                default:
                    return;
            }

            // Setting BattleUiMovableElementData to movable element
            if (data != null) movableElement.SetData(data);

            // Setting correct icon to the joystick
            BattleUiJoystickIconSetter iconSetter = movableElement.GetComponent<BattleUiJoystickIconSetter>();
            if (iconSetter != null) iconSetter.SetIcon(uiElementType);
        }

        private void Awake()
        {
            // Connecting listeners for the joystick input
            if (_moveJoystickComponent != null) _moveJoystickComponent.OnJoystickInput += _uiController.GameViewController.UiInputOnMovementJoystick;
            if (_rotateJoystickComponent != null)
            {
                _rotateJoystickComponent.OnJoystickXAxisInput += _uiController.GameViewController.UiInputOnRotationJoystick;
                _rotateJoystickComponent.LockYAxis = true;
            }
        }

        private void OnDestroy()
        {
            // Removing listeners for the joystick input
            if (_moveJoystickComponent != null) _moveJoystickComponent.OnJoystickInput -= _uiController.GameViewController.UiInputOnMovementJoystick;
            if (_rotateJoystickComponent != null) _rotateJoystickComponent.OnJoystickXAxisInput -= _uiController.GameViewController.UiInputOnRotationJoystick;
        }
    }
}
