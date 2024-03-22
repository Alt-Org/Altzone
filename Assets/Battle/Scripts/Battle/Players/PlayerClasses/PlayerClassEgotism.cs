using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Battle.Scripts.Battle.Players
{
    internal class PlayerClassEgotism : MonoBehaviour, IPlayerClass
    {
        [Obsolete("SpecialAbilityOverridesBallBounce is deprecated, please use return value of OnBallShieldCollision instead.")]
        public bool SpecialAbilityOverridesBallBounce => false;

        public void ClassInit(PlayerActor actor, ShieldPoseManager shieldManager, TeamsAreReadyForGameplay teamsAreReadyForGameplayMsgData)
        { }

        public bool OnBallShieldCollision()
        { return true; }

        public void OnBallShieldBounce()
        { }

        [Obsolete("ActivateSpecialAbility is deprecated, please use OnBallShieldCollision and/or OnBallShieldBounce instead.")]
        public void ActivateSpecialAbility()
        {

            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Special ability activated", _syncedFixedUpdateClock.UpdateCount));
        }

        // Debug
        private const string DEBUG_LOG_NAME = "[BATTLE] [PLAYER CLASS EGOTISM] ";
        private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;
        private SyncedFixedUpdateClock _syncedFixedUpdateClock; // only needed for logging time

        // debug
        private void Start()
        {
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
        }
    }
}
