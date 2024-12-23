using Photon.Deterministic;
using UnityEngine;

using Prg.Scripts.Common;

namespace Quantum
{
    public class PlayerInput : MonoBehaviour
    {
        private void OnEnable()
        {
            QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
        }

        public void PollInput(CallbackPollInput callback)
        {
            bool mouseClick = ClickStateHandler.GetClickState() is ClickState.Start or ClickState.Hold or ClickState.Move;
            bool twoFingers = ClickStateHandler.GetClickType() is ClickType.Pinch;

            Input i = new()
            {
                MouseClick = mouseClick && !twoFingers,
                MousePosition = mouseClick ? Camera.main.ScreenToWorldPoint(ClickStateHandler.GetClickPosition()).ToFPVector3() : FPVector3.Zero,
                RotateMotion = twoFingers,
                RotationDirection = twoFingers ? FP.FromFloat_UNSAFE(ClickStateHandler.GetRotationDirection()) : 0,
            };

            callback.SetInput(i, DeterministicInputFlags.Repeatable);
        }
    }
}
