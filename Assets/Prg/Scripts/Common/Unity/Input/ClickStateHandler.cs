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
            Touch touch = new();
            if (Touch.activeFingers.Count > 0) touch = Touch.activeTouches[0];


            if ((AppPlatform.IsDesktop && !AppPlatform.IsSimulator && Mouse.current.leftButton.wasPressedThisFrame) || (Touch.activeFingers.Count > 0 && touch.phase == UnityEngine.InputSystem.TouchPhase.Began))
            {
                return ClickState.Start;
            }
            else if (((AppPlatform.IsDesktop && !AppPlatform.IsSimulator && Mouse.current.leftButton.wasReleasedThisFrame) || (Touch.activeFingers.Count > 0 && (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended || touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled))))
            {
                return ClickState.End;
            }
            else if (((AppPlatform.IsDesktop && !AppPlatform.IsSimulator && Mouse.current.leftButton.isPressed && !Mouse.current.position.ReadValue().Equals(Mouse.current.position.ReadValueFromPreviousFrame())) || (Touch.activeFingers.Count > 0 && touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)))
            {
                return ClickState.Move;
            }
            else if ((AppPlatform.IsDesktop && !AppPlatform.IsSimulator && Mouse.current.leftButton.isPressed && Mouse.current.position.ReadValue().Equals(Mouse.current.position.ReadValueFromPreviousFrame())) || (Touch.activeFingers.Count > 0 && touch.phase == UnityEngine.InputSystem.TouchPhase.Stationary))
            {
                return ClickState.Hold;
            }
            return ClickState.None;
        }

        public static ClickType GetClickType(ClickInputDevice inputDevice = ClickInputDevice.None)
        {
            if ((Touch.activeTouches.Count == 1 && (inputDevice is ClickInputDevice.Touch || inputDevice is ClickInputDevice.None))
                || (Mouse.current != null && Mouse.current.leftButton.isPressed && Mouse.current.scroll.ReadValue() == Vector2.zero) && (inputDevice is ClickInputDevice.Mouse || inputDevice is ClickInputDevice.None))
                return ClickType.Click;
            else if (Touch.activeTouches.Count == 2 && (inputDevice is ClickInputDevice.Touch || inputDevice is ClickInputDevice.None)
                || (Mouse.current != null && Mouse.current.scroll.ReadValue() != Vector2.zero) && (inputDevice is ClickInputDevice.Mouse || inputDevice is ClickInputDevice.None))
                return ClickType.Pinch;
            else return ClickType.None;
        }

        public static Vector2 GetClickPosition(ClickInputDevice inputDevice = ClickInputDevice.None)
        {
            if (GetClickState() is not ClickState.None)
                if (Touch.activeFingers.Count >= 1 && (inputDevice is ClickInputDevice.Touch || inputDevice is ClickInputDevice.None))
                {
                    Touch touch = Touch.activeTouches[0];
                    return touch.screenPosition;
                }
                else if(Mouse.current != null && (inputDevice is ClickInputDevice.Mouse || inputDevice is ClickInputDevice.None)) return Mouse.current.position.ReadValue();
            return Vector2.negativeInfinity;
        }

        public static float GetPinchDistance(ClickInputDevice inputDevice = ClickInputDevice.None)
        {
            float distance = -1f;
            if (Touch.activeTouches.Count >= 2 && (inputDevice is ClickInputDevice.Touch || inputDevice is ClickInputDevice.None))
            {
                Vector2 touch1 = Touch.activeFingers[0].screenPosition;
                Vector2 touch2 = Touch.activeFingers[1].screenPosition;

                distance = Vector2.Distance(touch1, touch2);
            }
            else if (Mouse.current != null && (inputDevice is ClickInputDevice.Mouse || inputDevice is ClickInputDevice.None))
            {
                distance = Mouse.current.scroll.ReadValue().y;
            }
            return distance;
        }
    }
}
