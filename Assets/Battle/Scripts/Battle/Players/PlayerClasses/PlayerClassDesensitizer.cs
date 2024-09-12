using System;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    internal class PlayerClassDesensitizer : MonoBehaviour, IPlayerClass
    {
        // Serialized fields
        [SerializeField] private int _maxCollisions;
        [SerializeField] GameObject _shield;

        public IReadOnlyBattlePlayer BattlePlayer => _battlePlayer;

        public bool BounceOnBallShieldCollision => true;

        public void InitInstance(IReadOnlyBattlePlayer battlePlayer)
        {
            _battlePlayer = battlePlayer;
        }

        public void OnBallShieldCollision()
        {}

        public void OnBallShieldBounce()
        {
            TrackShieldCollisions();
        }

        private IReadOnlyBattlePlayer _battlePlayer;

        // Shield collision tracking variables
        private int _collisionCount;

        // Debug
        private const string DEBUG_LOG_NAME = "[BATTLE] [PLAYER CLASS DESENSITIZER] ";
        private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;
        private SyncedFixedUpdateClock _syncedFixedUpdateClock; // only needed for logging time

        // debug
        private void Start()
        {
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
        }

        private void TrackShieldCollisions()
        {
            _collisionCount++;

            // Check if the number of collision has reached maximum
            if (_collisionCount >= _maxCollisions)
            {
                // Deactivate the shield
                _shield.SetActive(false);
            }
        }
    }
}
