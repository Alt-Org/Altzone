using Input = Quantum.Input;
using Quantum;
using UnityEngine;
using Photon.Deterministic;

public class RaidInput : MonoBehaviour
{
    private void OnEnable()
    {
        Debug.Log("Registering RaidInput callback");
        QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
    }

    private void PollInput(CallbackPollInput callBack)
    {
        Debug.Log("Polling RaidInput");
        Input i = new()
        {
            RaidClickPosition = new FPVector2(100, 200)
            
        };
        callBack.SetInput(i, DeterministicInputFlags.Repeatable);
    }
}
