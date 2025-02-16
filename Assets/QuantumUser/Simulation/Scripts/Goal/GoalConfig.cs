using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public enum GoalType
    {
        TopGoal = 0,
        BottomGoal = 1
    }
    public class GoalConfig : AssetObject
    {
        [Header("Goal Tags")]
        [Tooltip("Select the goal type")]
        public GoalType goal;
    }
}
