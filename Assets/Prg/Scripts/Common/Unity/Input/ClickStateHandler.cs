using UnityEngine;
using UnityEngine.InputSystem;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Prg.Scripts.Common
{
    public enum ClickState
    {
        Start,
        Hold,
        Move,
        End,
        None
    }

    public enum ClickType
    {
        Click,
        Pinch,
        None
    }

    public enum ClickInputDevice
    {
        Touch,
        Mouse,
        None
    }

    /// <summary>
    /// Wrapper for getting the current state of clicking/touching regardless whether you are using touchscreen or a mouse.
    /// </summary>
    public static class ClickStateHandler
    {
        private static Vector2 s_rotationStartVector = Vector2.zero;

        /// <summary>
        /// <para>Returns a <c>ClickState</c> enum according to the either the current <c>Touch</c> phase or the current <c>Mouse</c> clickstate.</para>
        ///
        /// <para>If you're starting to touch or press down the mouse button on this frame, returns ClickState.Start.<br/>
        /// If you're end the touch or release the mouse button on this frame, returns ClickState.End.<br/>
        /// If you're touching the screen or holding down mouse button on this frame, that was already going on before, and the position of the touch/click is same as previous frame, returns ClickState.Hold.<br/>
        /// If you're touching the screen or holding down mouse button on this frame, that was already going on before, and the position of the touch/click is changed, returns ClickState.Move.<br/>
        /// If somehow none of these apply, the returns <c>ClickState.None.</c></para>
        /// </summary>
        /// <returns> ClickState </returns>
        public static ClickState GetClickState(ClickInputDevice inputDevice = ClickInputDevice.None)
        {
            // Mouse
            if (Mouse.current != null && inputDevice is not ClickInputDevice.Touch)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                    return ClickState.Start;

                if (Mouse.current.leftButton.wasReleasedThisFrame)
                    return ClickState.End;

                if (Mouse.current.leftButton.isPressed &&
                    !Mouse.current.position.ReadValue().Equals(Mouse.current.position.ReadValueFromPreviousFrame()))
                    return ClickState.Move;

                if (Mouse.current.leftButton.isPressed &&
                    Mouse.current.position.ReadValue().Equals(Mouse.current.position.ReadValueFromPreviousFrame()))
                    return ClickState.Hold;
            }

            // Touch
            if (Touch.activeFingers.Count > 0 && inputDevice is not ClickInputDevice.Mouse)
            {
                Touch touch = Touch.activeTouches[0];

                if (touch.phase is UnityEngine.InputSystem.TouchPhase.Began)
                    return ClickState.Start;

                if (touch.phase is UnityEngine.InputSystem.TouchPhase.Ended or UnityEngine.InputSystem.TouchPhase.Canceled)
                    return ClickState.End;

                if (touch.phase is UnityEngine.InputSystem.TouchPhase.Moved)
                    return ClickState.Move;

                if (touch.phase is UnityEngine.InputSystem.TouchPhase.Stationary)
                    return ClickState.Hold;
            }

            return ClickState.None;
        }

        public static ClickType GetClickType(ClickInputDevice inputDevice = ClickInputDevice.None)
        {
            if (inputDevice is not ClickInputDevice.Mouse)
            {
                if (Touch.activeTouches.Count == 1) return ClickType.Click;
                if (Touch.activeTouches.Count == 2) return ClickType.Pinch;
            }

            if (Mouse.current != null && inputDevice is not ClickInputDevice.Touch)
            {
                if (Mouse.current.scroll.ReadValue() == Vector2.zero) return ClickType.Click;
                else return ClickType.Pinch;
            }

            return ClickType.None;
        }

        public static Vector2 GetClickPosition(ClickInputDevice inputDevice = ClickInputDevice.None)
        {
            if (GetClickState() is not ClickState.None)
            {
                if (Touch.activeFingers.Count >= 1 && inputDevice is not ClickInputDevice.Mouse)
                    return Touch.activeTouches[0].screenPosition;

                if (Mouse.current != null && inputDevice is not ClickInputDevice.Touch)
                    return Mouse.current.position.ReadValue();
            }

            return Vector2.negativeInfinity;
        }

        public static float GetPinchDistance(ClickInputDevice inputDevice = ClickInputDevice.None)
        {
            if (Touch.activeTouches.Count >= 2 && inputDevice is not ClickInputDevice.Mouse)
            {
                Vector2 touch1 = Touch.activeFingers[0].screenPosition;
                Vector2 touch2 = Touch.activeFingers[1].screenPosition;

                return Vector2.Distance(touch1, touch2);
            }

            if (Mouse.current != null && inputDevice is not ClickInputDevice.Touch)
            {
                return Mouse.current.scroll.ReadValue().y;
            }

            return -1;
        }

        public static float GetRotationDirection(ClickInputDevice inputDevice = ClickInputDevice.None)
        {

            if (Touch.activeTouches.Count >= 2 && (inputDevice is ClickInputDevice.Touch or ClickInputDevice.None))
            {
                Touch touch1 = Touch.activeTouches[0];
                Touch touch2 = Touch.activeTouches[1];
                Vector2 touch1Position = Touch.activeTouches[0].screenPosition;
                Vector2 touch2Position = Touch.activeTouches[1].screenPosition;

                if (touch1.began && touch2.began)
                {
                    s_rotationStartVector = touch2Position - touch1Position;
                    return 0;
                }

                if (touch1.isInProgress && touch2.isInProgress)
                {
                    Vector2 currentVector = touch2Position - touch1Position;
                    float crossProduct = s_rotationStartVector.x * currentVector.y - s_rotationStartVector.y * currentVector.x;
                    s_rotationStartVector = currentVector;

                    return crossProduct;
                }

                if (touch1.ended || touch2.ended)
                {
                    s_rotationStartVector = Vector2.zero;
                    return 0;
                }

            }

            if (Mouse.current != null && inputDevice is not ClickInputDevice.Touch)
            {
                return Mouse.current.scroll.ReadValue().y;
            }

            return 0;
        }
    }
}
