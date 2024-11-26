using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum.QuantumUser.Simulation.Goal
{
    [Preserve]
    public unsafe class GoalSystem : SystemSignalsOnly, ISignalOnTriggerBottomGoal, ISignalOnTriggerTopGoal
    {

        public void OnTriggerBottomGoal(Frame f)
        {
            // Implementation for ISignalOnTriggerTopGoal
            Debug.Log("Top goal triggered");


        }

        public void OnTriggerTopGoal(Frame f)
        {
            // Implementation for ISignalOnTriggerBottomGoal
            Debug.Log("Bottom goal triggered");
        }

    }
}
