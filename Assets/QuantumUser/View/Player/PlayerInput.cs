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

            Input i = new()
            {
                MouseClick = mouseClick,
                MousePosition = mouseClick ? Camera.main.ScreenToWorldPoint(ClickStateHandler.GetClickPosition()).ToFPVector3() : FPVector3.Zero,
            };

            callback.SetInput(i, DeterministicInputFlags.Repeatable);
        }
    }
}
