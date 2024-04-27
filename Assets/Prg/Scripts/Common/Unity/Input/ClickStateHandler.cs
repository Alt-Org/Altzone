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

    public static class ClickStateHandler
    {
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
    }
}
