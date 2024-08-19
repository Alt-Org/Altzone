using System;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    internal class PlayerClassNone : MonoBehaviour, IPlayerClass
    {
        public bool BounceOnBallShieldCollision => true;

        public void OnBallShieldCollision()
        {
            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "OnBallShieldCollision called", _syncedFixedUpdateClock.UpdateCount));
        }

        public void OnBallShieldBounce()
        {
            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "OnBallShieldBounce called", _syncedFixedUpdateClock.UpdateCount));
        }

        // Debug
        private const string DEBUG_LOG_NAME = "[BATTLE] [PLAYER CLASS NONE] ";
        private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;
        private SyncedFixedUpdateClock _syncedFixedUpdateClock; // only needed for logging time

        // debug
        private void Start()
        {
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
        }
    }
}
