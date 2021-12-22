using UnityEngine;

namespace Altzone.Scripts.Config.ScriptableObjects
{
    /// <summary>
    /// Editable persistent settings for the game.
    /// </summary>
    /// <remarks>
    /// Create these in <c>Resources</c> folder with name "PersistentGameSettings" so they can be loaded when needed first time.
    /// </remarks>
    [CreateAssetMenu(menuName = "ALT-Zone/PersistentGameSettings", fileName = "PersistentGameSettings")]
    public class PersistentGameSettings : ScriptableObject
    {
        [Header("Game Features")] public GameFeatures _features;
        [Header("Game Constraints")] public GameConstraints _constraints;
        [Header("Game Variables")] public GameVariables _variables;
        [Header("Game Prefabs")] public GamePrefabs _prefabs;
    }
}