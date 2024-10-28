using Battle1.Scripts.Battle.Game;
using UnityEngine;

namespace Battle1.Scripts.Battle.Players.PlayerClasses
{
    internal class PlayerClassNone : MonoBehaviour, IPlayerClass
    {
        public IReadOnlyBattlePlayer BattlePlayer => _battlePlayer;

        public bool BounceOnBallShieldCollision => true;

        public void InitInstance(IReadOnlyBattlePlayer battlePlayer)
        {
            _battlePlayer = battlePlayer;

            // debug
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
        }

        public void OnBallShieldCollision()
        {
            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "OnBallShieldCollision called", _syncedFixedUpdateClock.UpdateCount));
        }

        public void OnBallShieldBounce()
        {
            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "OnBallShieldBounce called", _syncedFixedUpdateClock.UpdateCount));
        }

        private IReadOnlyBattlePlayer _battlePlayer;

        // Debug
        private const string DEBUG_LOG_NAME = "[BATTLE] [PLAYER CLASS NONE] ";
        private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;
        private SyncedFixedUpdateClock _syncedFixedUpdateClock; // only needed for logging time
    }
}
