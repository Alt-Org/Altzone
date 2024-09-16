using System;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    internal class PlayerClassRetroflector : MonoBehaviour, IPlayerClass
    {
        // Serialized fields
        [SerializeField] private int _maxReflections;
        [SerializeField] private ShieldManager _shieldManager;
        [SerializeField] private GameObject[] _shieldShapes; // Different shield shapes
        [SerializeField] private bool _allowLoveProjectiles = false; // Control for love projectiles

        public IReadOnlyBattlePlayer BattlePlayer => _battlePlayer;

        public bool BounceOnBallShieldCollision => !_allowLoveProjectiles;

        // Public methods

        public void InitInstance(IReadOnlyBattlePlayer battlePlayer)
        {
            _battlePlayer = battlePlayer;
        }

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

        // Private fields
        private IReadOnlyBattlePlayer _battlePlayer;
        private int _reflectionCount;
        private int _currentShieldShapeIndex = 0;

        // Debug
        private const string DEBUG_LOG_NAME = "[BATTLE] [PLAYER CLASS RETROFLECTION] ";
        private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;
        private SyncedFixedUpdateClock _syncedFixedUpdateClock; // only needed for logging time

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
                _shieldManager.SetShield(shieldGameObject: _shieldShapes[_currentShieldShapeIndex]);
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


                _reflectionCount = 0; // Reset the reflection count for the new shape

                Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Shield shape changed", _syncedFixedUpdateClock.UpdateCount));
            }
        }
    }




