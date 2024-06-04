using System;
using UnityEngine;




namespace Battle.Scripts.Battle.Players
{
    internal class PlayerClassConfluence : MonoBehaviour, IPlayerClass
    {


        // Variables

        [SerializeField] GameObject _shieldGumball;
        [SerializeField] GameObject _shieldPopped;

        private int _shieldTurn = 0;








        [Obsolete("SpecialAbilityOverridesBallBounce is deprecated, please use return value of OnBallShieldCollision instead.")]
        public bool SpecialAbilityOverridesBallBounce => false;

        public bool OnBallShieldCollision()
        { return true; }



        public void OnBallShieldBounce()
        {
            if (_shieldTurn == 0)
            {
                _shieldTurn++;
                ShieldFlipper();
                Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "_shieldTurn set to 1", _syncedFixedUpdateClock.UpdateCount));
            }
            else
            {
                _shieldTurn = 0;
                ShieldFlipper();
                Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "_shieldTurn set to 0", _syncedFixedUpdateClock.UpdateCount));
            }


        }


        [Obsolete("ActivateSpecialAbility is deprecated, please use OnBallShieldCollision and/or OnBallShieldBounce instead.")]
        public void ActivateSpecialAbility()
        {
            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Special ability activated", _syncedFixedUpdateClock.UpdateCount));
        }

        // Debug
        private const string DEBUG_LOG_NAME = "[BATTLE] [PLAYER CLASS CONFLUENCE] ";
        private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;
        private SyncedFixedUpdateClock _syncedFixedUpdateClock; // only needed for logging time

        // debug
        private void Start()
        {
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
        }


        private void ShieldFlipper() {

            if (_shieldTurn == 0)
            {
                _shieldGumball.SetActive(true);
                Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Shield is set to _shieldGumball", _syncedFixedUpdateClock.UpdateCount));
            }
            else
            {
                _shieldPopped.SetActive(true);
                Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Shield is set to _shieldPopped", _syncedFixedUpdateClock.UpdateCount));
            }
        }
    }
}
