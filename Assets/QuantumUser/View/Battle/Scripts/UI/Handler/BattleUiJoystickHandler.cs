/// @file BattleUiJoystickHandler.cs
/// <summary>
/// Contains @cref{Battle.View.UI,BattleUiJoystickHandler} class which handles the on-screen joysticks.
/// </summary>
///
/// This script:<br/>
/// Handles %Battle Ui joystick functionality.

// Unity usings
using UnityEngine;

// Altzone usings
using Altzone.Scripts.BattleUiShared;

using BattleUiElementType = SettingsCarrier.BattleUiElementType;

namespace Battle.View.UI
{
    /// <summary>
    /// <span class="brief-h">Joystick @uihandlerlink (<a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>).</span><br/>
    /// Handles %Battle Ui joystick functionality.
    /// </summary>
    public class BattleUiJoystickHandler : MonoBehaviour
    {
        /// @anchor BattleUiJoystickHandler-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <summary>[SerializeField] Reference to BattleUiController.</summary>
        /// @ref BattleUiJoystickHandler-SerializeFields
        [SerializeField] private BattleUiController _uiController;

        /// <summary>[SerializeField] Reference to the movement joystick's <see cref="BattleUiMovableJoystickElement"/> component.</summary>
        /// @ref BattleUiJoystickHandler-SerializeFields
        [SerializeField] private BattleUiMovableJoystickElement _moveJoystickMovableElement;

        /// <summary>[SerializeField] Reference to the rotation joystick's <see cref="BattleUiMovableJoystickElement"/> component.</summary>
        /// @ref BattleUiJoystickHandler-SerializeFields
        [SerializeField] private BattleUiMovableJoystickElement _rotateJoystickMovableElement;

        /// <summary>[SerializeField] Reference to the special joystick's <see cref="BattleUiMovableJoystickElement"/> component.</summary>
        /// @ref BattleUiJoystickHandler-SerializeFields
        [SerializeField] private BattleUiMovableJoystickElement _specialJoystickMovableElement;

        /// <summary>[SerializeField] Reference to the movement joystick's <see cref="BattleUiJoystickComponent"/> component.</summary>
        /// @ref BattleUiJoystickHandler-SerializeFields
        [SerializeField] private BattleUiJoystickComponent _moveJoystickComponent;

        /// <summary>[SerializeField] Reference to the rotation joystick's <see cref="BattleUiJoystickComponent"/> component.</summary>
        /// @ref BattleUiJoystickHandler-SerializeFields
        [SerializeField] private BattleUiJoystickComponent _rotateJoystickComponent;

        /// <summary>[SerializeField] Reference to the special joystick's <see cref="BattleUiJoystickComponent"/> component.</summary>
        /// @ref BattleUiJoystickHandler-SerializeFields
        [SerializeField] private BattleUiJoystickComponent _specialJoystickComponent;

        /// @}

        /// <summary>Public getter indicating whether the %UI element is currently visible</summary>
        public bool IsVisible => _moveJoystickMovableElement.gameObject.activeSelf;

        /// <summary>Public getter for #_moveJoystickMovableElement.</summary>
        /// <value>Reference to the %UI element's <see cref="BattleUiMovableElement"/> component.</value>
        public BattleUiMovableElement MoveJoystickMovableElement => _moveJoystickMovableElement;

        /// <summary>Public getter for #_rotateJoystickMovableElement.</summary>
        /// <value>Reference to the %UI element's <see cref="BattleUiMovableElement"/> component.</value>
        public BattleUiMovableElement RotateJoystickMovableElement => _rotateJoystickMovableElement;

        /// <summary>Public getter for #_specialJoystickMovableElement.</summary>
        /// <value>Reference to the %UI element's <see cref="BattleUiMovableElement"/> component.</value>
        public BattleUiMovableElement SpecialJoystickMovableElement => _specialJoystickMovableElement;

        /// <summary>
        /// Sets the %UI element visibility.
        /// </summary>
        ///
        /// <param name="show">If the %UI element should be visible or not.</param>
        /// <param name="uiElementType">The UI element's BattleUiElementType to differentiate between movement, rotation and special joysticks. If None set visibility for movement and rotation joysticks.</param>
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
                case BattleUiElementType.SpecialJoystick:
                    _specialJoystickMovableElement.gameObject.SetActive(show);
                    break;
            }
        }

        /// <summary>
        /// Sets the joysticks locked/unlocked.
        /// </summary>
        ///
        /// <param name="locked">If the joysticks should be locked or not.</param>
        public void SetLocked(bool locked)
        {
            _moveJoystickMovableElement.SetLocked(locked);
            _rotateJoystickMovableElement.SetLocked(locked);
            _specialJoystickMovableElement.SetLocked(locked);
        }

        /// <summary>
        /// Sets BattleUiMovableElementData to the joystick which matches the BattleUiElementType.
        /// </summary>
        ///
        /// <param name="uiElementType">The UI element's BattleUiElementType to differentiate between movement, rotation and special joysticks.</param>
        /// <param name="data">The BattleUiMovableElementData to apply to the joystick.</param>
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
                case BattleUiElementType.SpecialJoystick:
                    movableElement = _specialJoystickMovableElement;
                    break;
                default:
                    return;
            }

            // Setting BattleUiMovableElementData to movable element
            if (data != null) movableElement.SetData(data);
        }

        /// <summary>
        /// Private <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/MonoBehaviour.Awake.html">Awake@u-exlink</a> method
        /// which initially initializes the joysticks.
        /// </summary>
        private void Awake()
        {
            // Connecting listeners for the joystick input
            if (_moveJoystickComponent != null) _moveJoystickComponent.OnJoystickInput += _uiController.GameViewController.UiInputOnJoystickMovement;

            if (_rotateJoystickComponent != null)
            {
                _rotateJoystickComponent.OnJoystickXAxisInput += _uiController.GameViewController.UiInputOnJoystickRotation;
                _rotateJoystickComponent.LockYAxis = true;
            }

            if (_specialJoystickComponent != null)
            {
                _specialJoystickComponent.OnJoystickInput += _uiController.GameViewController.UiInputOnJoystickSpecial;
            }
        }

        /// <summary>
        /// Private <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/MonoBehaviour.OnDestroy.html">OnDestroy@u-exlink</a> method
        /// which removes the listeners from joysticks.
        /// </summary>
        private void OnDestroy()
        {
            // Removing listeners for the joystick input
            if (_moveJoystickComponent != null) _moveJoystickComponent.OnJoystickInput -= _uiController.GameViewController.UiInputOnJoystickMovement;
            if (_rotateJoystickComponent != null) _rotateJoystickComponent.OnJoystickXAxisInput -= _uiController.GameViewController.UiInputOnJoystickRotation;
            if (_specialJoystickComponent != null) _specialJoystickComponent.OnJoystickInput -= _uiController.GameViewController.UiInputOnJoystickSpecial;
        }
    }
}
