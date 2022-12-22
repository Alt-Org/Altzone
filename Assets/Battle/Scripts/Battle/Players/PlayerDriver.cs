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
        private PlayerDriverState _state;

        protected static IPlayerDriverState GetPlayerDriverState(PlayerDriver playerDriver)
        {
            if (playerDriver._state == null)
            {
                playerDriver._state = playerDriver.gameObject.AddComponent<PlayerDriverState>();
            }
            return playerDriver._state;
        }
    }
}
