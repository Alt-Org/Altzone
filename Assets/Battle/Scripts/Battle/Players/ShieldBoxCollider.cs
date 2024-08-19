using UnityConstants;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    internal class ShieldBoxCollider : MonoBehaviour
    {
        // Serialized Fields
        [SerializeField] private float _bounceAngle;

        // Public Properties
        public Transform ShieldTransform => _transform;
        public float BounceAngle => _bounceAngle;
        public float ImpactForce => _playerActor.ImpactForce;
        public bool BounceOnBallShieldCollision => _playerActor.BounceOnBallShieldCollision;

        // Public Methods
        public void OnBallShieldCollision() => _playerActor.OnBallShieldCollision();
        public void OnBallShieldBounce() => _playerActor.OnBallShieldBounce();

        // Private Fields
        private PlayerActor _playerActor;
        private ShieldManager _shieldManager;
        private Transform _transform;

        // Debug
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0052 // Remove unread private members
        private const string DEBUG_LOG_BALL_COLLISION = "[{0:000000}] [BATTLE] [SHIELD BOX COLLIDER] Ball collision: ";
        private SyncedFixedUpdateClock _syncedFixedUpdateClock; // only needed for logging time
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore IDE0052 // Remove unread private members

        #region Private Methods

        private void Awake()
        {
            _transform = GetComponent<Transform>();
            _playerActor = transform.root.GetComponent<PlayerActor>();
            _shieldManager = _transform.parent.parent.GetComponentInParent<ShieldManager>();

            // debug
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
        }

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
