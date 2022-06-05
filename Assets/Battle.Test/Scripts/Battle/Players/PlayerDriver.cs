using UnityEngine;

namespace Battle.Test.Scripts.Battle.Players
{
    /// <summary>
    /// Marker class for <c>PlayerDriver</c> implementations.
    /// </summary>
    internal class PlayerDriver : MonoBehaviour
    {
        public override string ToString()
        {
            return $"{name}";
        }
    }
}