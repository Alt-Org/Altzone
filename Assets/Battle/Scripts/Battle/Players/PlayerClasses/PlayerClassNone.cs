using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    public class PlayerClassNone : MonoBehaviour, IPlayerClass
    {
        public bool SpecialAbilityOverridesBallBounce => false;

        public void ActivateSpecialAbility()
        {
            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Special ability activated", _syncedFixedUpdateClock.UpdateCount));
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
