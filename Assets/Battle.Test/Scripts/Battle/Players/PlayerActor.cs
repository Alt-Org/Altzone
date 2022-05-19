using UnityEngine;

namespace Battle.Test.Scripts.Battle.Players
{
    /// <summary>
    /// <c>PlayerActor</c> for local and remote instances.
    /// </summary>
    /// <remarks>
    /// This class manages local visual representation of player actor.
    /// </remarks>
    internal class PlayerActor : MonoBehaviour
    {
        [SerializeField] private PlayerDriver _playerDriver;

        private void Awake()
        {
            enabled = false;
        }

        public void SetPlayerDriver(PlayerDriver playerDriver)
        {
            _playerDriver = playerDriver;
            enabled = true;
        }

        private void OnEnable()
        {
            var player = _playerDriver.Player;
            Debug.Log($"{player.GetDebugLabel()}");
        }
    }
}