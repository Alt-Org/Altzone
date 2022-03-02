using UnityEngine;

namespace Battle.Scripts.Battle.Room
{
    /// <summary>
    /// Automatic marker to identify <c>GameObject</c> with unique id.
    /// </summary>
    internal class IdMarker : MonoBehaviour
    {
        private static int _idCounter;

        [SerializeField] private int _id;

        public int Id => _id;

        private void Awake()
        {
            _id = ++_idCounter;
        }
    }
}