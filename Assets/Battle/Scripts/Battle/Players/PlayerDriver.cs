using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
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
