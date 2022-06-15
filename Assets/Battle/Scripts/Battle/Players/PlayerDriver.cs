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
        
        protected IPlayerDriverState GetPlayerDriverState()
        {
            if (_stateInstance == null)
            {
                _stateInstance = gameObject.AddComponent<PlayerDriverState>();
            }
            return _stateInstance;
        }

        public override string ToString()
        {
            return $"{name}";
        }
    }
}