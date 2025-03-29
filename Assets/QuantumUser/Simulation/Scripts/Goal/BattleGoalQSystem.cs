using UnityEngine;
using UnityEngine.Scripting;

using Quantum;

namespace Battle.QSimulation.Goal
{
    [Preserve]
    public unsafe class BattleGoalQSystem : SystemSignalsOnly, ISignalOnTriggerBottomGoal, ISignalOnTriggerTopGoal
    {

        public void OnTriggerBottomGoal(Frame f)
        {
            // Implementation for ISignalOnTriggerTopGoal
            Debug.Log("Bottom goal triggered");

            GameStateChange(f, "BottomGoal");

        }

        public void OnTriggerTopGoal(Frame f)
        {
            // Implementation for ISignalOnTriggerBottomGoal
            Debug.Log("Top goal triggered");

            GameStateChange(f, "TopGoal");
        }

        private void GameStateChange(Frame f, string goalType)
        {
            // Retrieve the GameSession singleton entity reference
            if (!f.TryGetSingletonEntityRef<BattleGameSessionQSingleton>(out EntityRef entity))
            {
                Debug.LogWarning("GameSession singleton not found.");
                return;
            }

            // Retrieve the pointer to the GameSession singleton
            if (!f.Unsafe.TryGetPointer(entity, out BattleGameSessionQSingleton* gameSession))
            {
                Debug.LogWarning("GameSession pointer not found.");
                return;
            }

            // Update the GameSession based on the triggered goal
            switch (goalType)
            {
                case "BottomGoal":
                    gameSession->State = BattleGameState.GameOver;
                    Debug.Log("GameSession state updated to GameOver after BottomGoal.");
                    break;

                case "TopGoal":
                    gameSession->State = BattleGameState.GameOver;
                    Debug.Log("GameSession state updated to Playing after TopGoal.");
                    break;
            }
        }

    }
}
