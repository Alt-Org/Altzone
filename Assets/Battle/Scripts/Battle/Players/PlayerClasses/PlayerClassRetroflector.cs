using System;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    internal class PlayerClassRetroflector : MonoBehaviour, IPlayerClass
    {
        // Serialized fields
        [SerializeField] private int _maxReflections;
        [SerializeField] private GameObject _shield;
        [SerializeField] private GameObject[] _shieldShapes; // Different shield shapes
        [SerializeField] private bool _allowLoveProjectiles = false; // Control for love projectiles

        public bool BounceOnBallShieldCollision => !_allowLoveProjectiles;

        // Private fields
        private int _reflectionCount;
        private int _currentShieldShapeIndex = 0;

        // Debug
        private const string DEBUG_LOG_NAME = "[BATTLE] [PLAYER CLASS RETROFLECTION] ";
        private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;
        private SyncedFixedUpdateClock _syncedFixedUpdateClock; // only needed for logging time

        // Public methods

        public void OnBallShieldCollision()
        {
            if (!_allowLoveProjectiles)
            {
                // Collision logic if needed
            }
        }

        public void OnBallShieldBounce()
        {
            TrackShieldReflections();
        }

        // Private methods
        private void Start()
        {
            // Initialize the clock component
            _syncedFixedUpdateClock = gameObject.AddComponent<SyncedFixedUpdateClock>();
            InitializeShield();
        }

        private void InitializeShield()
        {
            // Initialize the shield with the first shape
            if (_shieldShapes.Length > 0)
            {
                _shield = Instantiate(_shieldShapes[_currentShieldShapeIndex], transform);
                _shield.SetActive(true);
            }
        }

        private void TrackShieldReflections()
        {
            _reflectionCount++;
            
            // Check if the number of reflections has reached the maximum
            if (_reflectionCount >= _maxReflections)
            {
                ChangeShieldShape();
            }

            // Debug log to track reflections
            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Shield reflection count: " + _reflectionCount, _syncedFixedUpdateClock.UpdateCount));
        }

        private void ChangeShieldShape()
        {
            _currentShieldShapeIndex++;
            if (_currentShieldShapeIndex < _shieldShapes.Length)
            {
                // Deactivate current shield and activate the next one
                Destroy(_shield);
                _shield = Instantiate(_shieldShapes[_currentShieldShapeIndex], transform);
                
                _reflectionCount = 0; // Reset the reflection count for the new shape

                Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Shield shape changed", _syncedFixedUpdateClock.UpdateCount));
            }
        }
    }
}




