using UnityEngine;

namespace Battle.Scripts.Battle.Room
{
    /// <summary>
    /// Helper for game background to find it and change it if required.
    /// </summary>
    public class GameBackground : MonoBehaviour
    {
        [SerializeField] private GameObject _gameBackground;

        public GameObject Background => _gameBackground;

        public bool IsRotated => _gameBackground.transform.rotation.z != 0f;
    }
}