using System;
using Photon.Pun;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    internal class PlayerClassRetroflector : MonoBehaviour, IPlayerClass
    {
        // Serialized fields
        [SerializeField] private ShieldManager _shieldManager;

        [SerializeField] private bool _allowLoveProjectiles = false; // Control for love projectiles

        [Header("Shield functions")]
        [SerializeField] private int _maxReflections;
        [SerializeField] private GameObject[] _shieldOptions;

        public IReadOnlyBattlePlayer BattlePlayer => _battlePlayer;

        public bool BounceOnBallShieldCollision => !_allowLoveProjectiles;

        // Public methods

        public void InitInstance(IReadOnlyBattlePlayer battlePlayer)
        {
            _battlePlayer = battlePlayer;
            _battleDebugLogger = new BattleDebugLogger(this);
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
            _shieldManager = BattlePlayer.PlayerShieldManager;
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
            _syncedFixedUpdateClock.ExecuteOnUpdate(_syncedFixedUpdateClock.UpdateCount + 5, -10, () =>
            {
                _reflectionCount++;
                _battleDebugLogger.LogInfo("Shield reflection count: " + _reflectionCount);
                TrackShieldReflections();
            });
        }

        // Private fields
        private IReadOnlyBattlePlayer _battlePlayer;
        private BattleDebugLogger _battleDebugLogger;
        private int _currentShieldShapeIndex = -1;
        private int _reflectionCount = 0;



        // Debug
        private const string DEBUG_LOG_NAME = "[BATTLE] [PLAYER CLASS RETROFLECTION] ";
        private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;
        private SyncedFixedUpdateClock _syncedFixedUpdateClock; // only needed for logging time

        // Private methods
        private void Start()
        {
            // Initialize the clock component
            /*            InitializeShield();*/
        }

/*        private void InitializeShield()
        {
            // Initialize the shield with the first shape
            if (_shieldOptions.Length > 0)
            {
                _shieldManager.SetShield(shieldGameObject: _shieldOptions[_currentShieldShapeIndex]);
            }
        }*/

        private void TrackShieldReflections()
        {
            // Check if the number of reflections has reached the maximum

            if (_reflectionCount >= _maxReflections)
            {
                    _currentShieldShapeIndex++;
                    _battleDebugLogger.LogInfo("Shield index set to " + _currentShieldShapeIndex);
                    ChangeShieldShape(_currentShieldShapeIndex);
                    // Debug log to track reflections
            }   
        }

        private void ChangeShieldShape(int _currentShieldShapeIndex)
        {
            _battleDebugLogger.LogInfo("ShieldOptions 1 " + _shieldOptions);

            _syncedFixedUpdateClock.ExecuteOnUpdate(_syncedFixedUpdateClock.UpdateCount + 5, -10, () =>
            {
                if (_currentShieldShapeIndex > _shieldOptions.Length)
                {
                    return;
                }

                if (_currentShieldShapeIndex < _shieldOptions.Length)
                {
                    GameObject _shieldPick = _shieldOptions[_currentShieldShapeIndex];

                    _reflectionCount = 0; // Reset the reflection count for the new shape

                    _battleDebugLogger.LogInfo("Change shield to index " + _currentShieldShapeIndex);
                    _shieldManager.SetShield(_shieldPick);
                    _battleDebugLogger.LogInfo("Shield is set to choice " + _shieldPick);
                    _battleDebugLogger.LogInfo("Shield shape changed to " + _shieldOptions[_currentShieldShapeIndex]);
                };
            });
        }
    }
}




