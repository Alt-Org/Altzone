using System;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    internal class PlayerClassDesensitization : MonoBehaviour, IPlayerClass
    {
        // Serialized fields
        [SerializeField] private int _maxCollisions;
        [SerializeField] GameObject _shield;

        [Obsolete("SpecialAbilityOverridesBallBounce is deprecated, please use return value of OnBallShieldCollision instead.")]
        public bool SpecialAbilityOverridesBallBounce => false;

        public bool OnBallShieldCollision()
        { return true; }

        public void OnBallShieldBounce()
        {
            TrackShieldCollisions();
        }

        [Obsolete("ActivateSpecialAbility is deprecated, please use OnBallShieldCollision and/or OnBallShieldBounce instead.")]
        public void ActivateSpecialAbility()
        {
            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Special ability activated", _syncedFixedUpdateClock.UpdateCount));
        }

        // Shield collision tracking variables
        private int collisionCount;

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
            collisionCount++;

            // Check if the number of collision has reached maximum
            if (collisionCount >= _maxCollisions)
            {
                // Deactivate the shield
                _shield.SetActive(false);
            }
        }
    }
}
