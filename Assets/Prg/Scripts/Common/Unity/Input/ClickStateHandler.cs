using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine;
using Input = UnityEngine.Input;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using UnityEngine.UIElements;

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
        public static ClickState GetClickState()
        {
            // Mouse
            if (AppPlatform.IsDesktop && !AppPlatform.IsSimulator)
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
            {
                Touch touch = new();
                if (Touch.activeFingers.Count > 0) touch = Touch.activeTouches[0];

                if (Touch.activeFingers.Count > 0 &&
                    touch.phase is UnityEngine.InputSystem.TouchPhase.Began)
                    return ClickState.Start;

                if (Touch.activeFingers.Count > 0 &&
                    (touch.phase is UnityEngine.InputSystem.TouchPhase.Ended or UnityEngine.InputSystem.TouchPhase.Canceled))
                    return ClickState.End;

                if (Touch.activeFingers.Count > 0 &&
                    touch.phase is UnityEngine.InputSystem.TouchPhase.Moved)
                    return ClickState.Move;

                if (Touch.activeFingers.Count > 0 &&
                    touch.phase is UnityEngine.InputSystem.TouchPhase.Stationary)
                    return ClickState.Hold;
            }

            return ClickState.None;
        }

        public static ClickType GetClickType(ClickInputDevice inputDevice = ClickInputDevice.None)
        {
            if (inputDevice is ClickInputDevice.Touch or ClickInputDevice.None)
            {
                if (Touch.activeTouches.Count == 1) return ClickType.Click;
                if (Touch.activeTouches.Count == 2) return ClickType.Pinch;
            }

            if (Mouse.current != null && (inputDevice is ClickInputDevice.Mouse or ClickInputDevice.None))
            {
                if (Mouse.current.scroll.ReadValue() != Vector2.zero) return ClickType.Click;
                else return ClickType.Pinch;
            }

            return ClickType.None;
        }

        public static Vector2 GetClickPosition(ClickInputDevice inputDevice = ClickInputDevice.None)
        {
            if (GetClickState() is not ClickState.None)
            {
                if (Touch.activeFingers.Count >= 1 && (inputDevice is ClickInputDevice.Touch or ClickInputDevice.None))
                    return Touch.activeTouches[0].screenPosition;

                if (Mouse.current != null && (inputDevice is ClickInputDevice.Mouse or ClickInputDevice.None))
                    return Mouse.current.position.ReadValue();
            }

            return Vector2.negativeInfinity;
        }

        public static float GetPinchDistance(ClickInputDevice inputDevice = ClickInputDevice.None)
        {
            if (Touch.activeTouches.Count >= 2 && (inputDevice is ClickInputDevice.Touch or ClickInputDevice.None))
            {
                Vector2 touch1 = Touch.activeFingers[0].screenPosition;
                Vector2 touch2 = Touch.activeFingers[1].screenPosition;

                return Vector2.Distance(touch1, touch2);
            }

            if (Mouse.current != null && (inputDevice is ClickInputDevice.Mouse or ClickInputDevice.None))
            {
                return Mouse.current.scroll.ReadValue().y;
            }

            return -1;
        }

        public static float GetRotationDirection(ClickInputDevice inputDevice = ClickInputDevice.None)
        {
            /*
            if (Touch.activeTouches.Count >= 2 && (inputDevice is ClickInputDevice.Touch or ClickInputDevice.None))
            {
            }
            */

            if (Mouse.current != null && (inputDevice is ClickInputDevice.Mouse or ClickInputDevice.None))
            {
                Debug.LogFormat("[PlayerRotating] Player is rotating scrollwheel");
                return Mouse.current.scroll.ReadValue().y;
            }

            Debug.LogFormat("[PlayerRotating] Rotation direction not working");
            return 0;
        }
    }
}
