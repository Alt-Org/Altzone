using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    /// <summary>
    /// Marker class for <c>PlayerDriver</c> implementations.
    /// </summary>
    /// <remarks>
    /// And this is good place for common code used by all <c>IPlayerDriver</c> implementations.
    /// </remarks>
    internal class PlayerDriver : MonoBehaviour
    {
        [SerializeField] private PlayerDriverState _stateInstance;

        protected static void ConnectDistanceMeter(IPlayerDriver playerDriver, Transform playerActorTransform)
        {
            var distanceMeter = FindObjectOfType<PlayerDistanceMeter>();
            if (distanceMeter == null)
            {
                return;
            }
            distanceMeter.SetPlayerDriver(playerDriver, playerActorTransform);
        }

        protected static IPlayerDriverState GetPlayerDriverState(PlayerDriver playerDriver)
        {
            if (playerDriver._stateInstance == null)
            {
                playerDriver._stateInstance = playerDriver.gameObject.AddComponent<PlayerDriverState>();
            }
            return playerDriver._stateInstance;
        }

        public override string ToString()
        {
            return $"{name} {_stateInstance}";
        }
    }
}