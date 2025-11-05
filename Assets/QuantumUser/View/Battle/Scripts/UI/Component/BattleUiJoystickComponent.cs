/// @file BattleUiJoystickComponent.cs
/// <summary>
/// Contains @cref{Battle.View.UI,BattleUiJoystickComponent} class which handles joystick drag input.
/// </summary>
///
/// This script:<br/>
/// Handles %Battle Ui joystick drag input functionality.
/// Implements IPointerDownHandler, IDragHandler and IPointerUpHandler interfaces.

// Unity usings
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battle.View.UI
{
    /// <summary>
    /// <span class="brief-h">Joystick @uicomponentlink (<a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>).</span><br/>
    /// Handles %Battle Ui joystick drag input functionality using IPointerDownHandler, IDragHandler and IPointerUpHandler interfaces.
    /// </summary>
    ///
    /// This script is meant to be attached to BattleUiJoystick prefab's top level GameObject, since it calculates #_joystickRadius from the same <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObject@u-exlink</a>'s <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RectTransform.html">RectTransform@u-exlink</a> component.
    [RequireComponent(typeof(RectTransform))]
    public class BattleUiJoystickComponent : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        /// @anchor BattleUiJoystickComponent-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <summary>[SerializeField] Reference to joystick handle's <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RectTransform.html">RectTransform@u-exlink</a> component.</summary>
        /// @ref BattleUiJoystickComponent-SerializeFields
        [SerializeField] private RectTransform _handleRectTransform;

        /// @}

        /// <value>Is joystick locked in the Y axis (can only be moved left to right)</value>
        public bool LockYAxis = false;

        /// <summary>
        /// Event delegate for joystick input with both axis.
        /// </summary>
        ///
        /// <param name="input">Joystick input Vector2.</param>
        public delegate void JoystickInputHandler(Vector2 input);

        /// <summary>Event which gets invoked on joystick input with both x and y axis.</summary>
        public event JoystickInputHandler OnJoystickInput;

        /// <summary>
        /// Event delegate for joystick input on x axis.
        /// </summary>
        ///
        /// <param name="input">Joystick input value on x axis.</param>
        public delegate void JoystickXAxisInputHandler(float input);

        /// <summary>Event which gets invoked on joystick input with only x axis.</summary>
        public event JoystickXAxisInputHandler OnJoystickXAxisInput;

        /// <summary>
        /// Handler method required by IPointerDownHandler interface.<br/>
        /// Calculates joystick radius needed for calculating the joystick input and calls #HandleDrag method so that joystick input gets registered even on a single joystick press.
        /// </summary>
        ///
        /// <param name="eventData">The event data.</param>
        public void OnPointerDown(PointerEventData eventData)
        {
            // Calculating joystick radius. If y axis is locked the joystick is rectangular and we take background radius from the width instead of which side is smaller.
            float handleOffset = _handleRectTransform.rect.width * 0.5f;
            float backgroundRadius = (LockYAxis ? _rectTransform.rect.width : Mathf.Min(_rectTransform.rect.width, _rectTransform.rect.height)) * 0.5f;
            _joystickRadius = backgroundRadius - handleOffset;

            HandleDrag(eventData.position);
        }

        /// <summary>
        /// Handler method required by IDragHandler interface.<br/>
        /// Calls #HandleDrag method to calculate joystick input.
        /// </summary>
        ///
        /// <param name="eventData">The event data.</param>
        public void OnDrag(PointerEventData eventData)
        {
            HandleDrag(eventData.position);
        }

        /// <summary>
        /// Handler method required by IPointerUpHandler interface.<br/>
        /// Resets the joystick handle position and invokes either #OnJoystickXAxisInput or #OnJoystickInput with zero to indicate stopping movement.
        /// </summary>
        ///
        /// <param name="eventData">The event data.</param>
        public void OnPointerUp(PointerEventData eventData)
        {
            _handleRectTransform.localPosition = Vector3.zero;

            if (LockYAxis) OnJoystickXAxisInput(0);
            else OnJoystickInput(Vector2.zero);
        }

        /// <value>Joystick radius used in calculations.</value>
        private float _joystickRadius;

        /// <value>Reference to joystick's <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RectTransform.html">RectTransform@u-exlink</a> component which is used in calculating #_joystickRadius.</value>
        private RectTransform _rectTransform;

        /// <summary>
        /// Private <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/MonoBehaviour.Awake.html">Awake@u-exlink</a> method which initializes #_rectTransform.
        /// </summary>
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        /// <summary>
        /// Calculates the drag input vector and invokes either #OnJoystickXAxisInput or #OnJoystickInput depending on #LockYAxis.
        /// </summary>
        ///
        /// <param name="dragPos">The player's PointerEventData position.</param>
        private void HandleDrag(Vector2 dragPos)
        {
            Vector2 joystickPos = _rectTransform.position;
            Vector2 dragVector = Vector2.ClampMagnitude(dragPos - joystickPos, _joystickRadius);

            if (LockYAxis) dragVector.y = 0;

            _handleRectTransform.position = joystickPos + dragVector;

            Vector2 inputVector = dragVector / _joystickRadius;
            if (LockYAxis) OnJoystickXAxisInput(inputVector.x);
            else OnJoystickInput(inputVector);
        }
    }
}
