using Battle1.Scripts.Battle.Game;
using UnityConstants;
using UnityEngine;

namespace Battle1.Scripts.Battle.Players
{
    internal class ShieldBoxCollider : MonoBehaviour
    {
        // Serialized Fields
        [SerializeField] private float _bounceAngle;

        // Public Properties
        public ShieldManager shieldManager => _shieldManager;
        public float BounceAngle => _bounceAngle;


        // Public Methods
        public void InitInstance(ShieldManager shieldManager, IReadOnlyBattlePlayer battlePlayer)
        {
            _shieldManager = shieldManager;
            _playerActor = battlePlayer.PlayerActor;

            // debug
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
        }


        // Private Fields
        private PlayerActor _playerActor;
        private ShieldManager _shieldManager;

        // Debug
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0052 // Remove unread private members
        private const string DEBUG_LOG_BALL_COLLISION = "[{0:000000}] [BATTLE] [SHIELD BOX COLLIDER] Ball collision: ";
        private SyncedFixedUpdateClock _syncedFixedUpdateClock; // only needed for logging time
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore IDE0052 // Remove unread private members

        #region Private Methods

        private void OnCollisionEnter2D(Collision2D collision)
        {
            GameObject otherGameObject = collision.gameObject;
            if (otherGameObject.CompareTag(Tags.Ball))
            {
                /*
                if (_playerActor != null)
                {
                    _playerActor.ShieldHit(1);
                }
                */
                _shieldManager.OnShieldBoxCollision();
            }
        }

        #endregion Private Methods
    }
}
