using System;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    internal class PlayerClassNone : MonoBehaviour, IPlayerClass
    {
        [Obsolete("SpecialAbilityOverridesBallBounce is deprecated, please use return value of OnBallShieldCollision instead.")]
        public bool SpecialAbilityOverridesBallBounce => false;

        public void ClassInit(PlayerActor actor, ShieldPoseManager shieldManager, TeamsAreReadyForGameplay teamsAreReadyForGameplayMsgData)
        {}

        public bool OnBallShieldCollision()
        {
            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "OnBallShieldCollision called", _syncedFixedUpdateClock.UpdateCount));
            return true;
        }

        public void OnBallShieldBounce()
        {
            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "OnBallShieldBounce called", _syncedFixedUpdateClock.UpdateCount));
        }

        [Obsolete("ActivateSpecialAbility is deprecated, please use OnBallShieldCollision and/or OnBallShieldBounce instead.")]
        public void ActivateSpecialAbility()
        { }

        // Debug
        private const string DEBUG_LOG_NAME = "[BATTLE] [PLAYER CLASS NONE] ";
        private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;
        private SyncedFixedUpdateClock _syncedFixedUpdateClock; // only needed for logging time

        private ShieldPoseManager _shieldManager;

        // debug
        private void Start()
        {
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
        }
    }
}
