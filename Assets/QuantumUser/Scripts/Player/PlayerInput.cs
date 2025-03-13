using Photon.Deterministic;
using UnityEngine;

using Prg.Scripts.Common;

using QuantumUser.Scripts;

namespace Quantum
{
    public class PlayerInput : MonoBehaviour
    {
        private bool _mouseDownPrevious;

        private void OnEnable()
        {
            QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
        }

        private void PollInput(CallbackPollInput callback)
        {
            bool mouseDown = ClickStateHandler.GetClickState() is ClickState.Start or ClickState.Hold or ClickState.Move;
            bool twoFingers = ClickStateHandler.GetClickType() is ClickType.TwoFingerOrScroll;
            bool mouseClick = !twoFingers && mouseDown && !_mouseDownPrevious;
            _mouseDownPrevious = mouseDown;

            Input i = new()
            {
                MouseClick = mouseClick,
                MousePosition = mouseClick ? BattleCamera.Camera.ScreenToWorldPoint(ClickStateHandler.GetClickPosition()).ToFPVector3() : FPVector3.Zero,
                //RotateMotion = twoFingers,
                RotationDirection = twoFingers ? FP.FromFloat_UNSAFE(ClickStateHandler.GetRotationDirection()) : 0,
            };

            callback.SetInput(i, DeterministicInputFlags.Repeatable);
        }
    }
}
