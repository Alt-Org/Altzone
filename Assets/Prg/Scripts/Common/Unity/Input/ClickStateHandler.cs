using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine;
using Input = UnityEngine.Input;
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

    public static class ClickStateHandler
    {
        public static ClickState GetClickState()
        {
            Touch touch = new();
            if (Touch.activeFingers.Count > 0) touch = Touch.activeTouches[0];


            if ((AppPlatform.IsDesktop && !AppPlatform.IsSimulator && Input.GetMouseButtonDown(0)) || (Touch.activeFingers.Count > 0 && touch.phase == UnityEngine.InputSystem.TouchPhase.Began))
            {
                return ClickState.Start;
            }
            else if (((AppPlatform.IsDesktop && !AppPlatform.IsSimulator && Input.GetMouseButtonUp(0)) || (Touch.activeFingers.Count > 0 && (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended || touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled))))
            {
                return ClickState.End;
            }
            else if (((AppPlatform.IsDesktop && !AppPlatform.IsSimulator && Input.GetMouseButton(0) && (!Input.GetAxis("Mouse X").Equals(0) || !Input.GetAxis("Mouse Y").Equals(0))) || (Touch.activeFingers.Count > 0 && touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)))
            {
                return ClickState.Move;
            }
            else if ((AppPlatform.IsDesktop && !AppPlatform.IsSimulator && Input.GetMouseButton(0) && Input.GetAxis("Mouse X").Equals(0) && Input.GetAxis("Mouse Y").Equals(0)) || (Touch.activeFingers.Count > 0 && touch.phase == UnityEngine.InputSystem.TouchPhase.Stationary))
            {
                return ClickState.Hold;
            }
            return ClickState.None;
        }
    }
}
