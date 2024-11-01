using Photon.Deterministic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.Scripting;

namespace Quantum.Battle
{
    public class PlayerInput : MonoBehaviour
    {
        private void OnEnable()
        {
            QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
        }

        public void PollInput(CallbackPollInput callback)
        {
            Quantum.Input i = new Quantum.Input();


            i.MouseClick = UnityEngine.Input.GetMouseButton(0);

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
            i.MousePosition = mousePos.ToFPVector3();

            

            callback.SetInput(i, DeterministicInputFlags.Repeatable);
        }
    }
}