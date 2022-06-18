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
        [SerializeField] private PlayerDriverState _state;

        protected static void ConnectDistanceMeter(IPlayerDriver playerDriver, PlayerDistanceMeter distanceMeter)
        {
            if (distanceMeter == null)
            {
                return;
            }
            distanceMeter.SetPlayerDriver(playerDriver);
        }

        protected static void DisconnectDistanceMeter(IPlayerDriver playerDriver, PlayerDistanceMeter distanceMeter)
        {
            if (distanceMeter == null)
            {
                return;
            }
            distanceMeter.SetPlayerDriver(null);
        }

        protected static IPlayerDriverState GetPlayerDriverState(PlayerDriver playerDriver)
        {
            if (playerDriver._state == null)
            {
                playerDriver._state = playerDriver.gameObject.AddComponent<PlayerDriverState>();
            }
            return playerDriver._state;
        }

        public override string ToString()
        {
            return $"{name} {_state}";
        }
    }
}