using Photon.Deterministic;
using UnityEngine;

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
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);

            Input i = new()
            {
                MouseClick = UnityEngine.Input.GetMouseButton(0),
                MousePosition = mousePos.ToFPVector3()
            };

            callback.SetInput(i, DeterministicInputFlags.Repeatable);
        }
    }
}
