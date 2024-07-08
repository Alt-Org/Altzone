using System;
using System.Collections;
using Altzone.Scripts.Config;
using Battle.Scripts.Battle;
using Battle.Scripts.Battle.Game;
using Battle.Scripts.Battle.Players;
using UnityConstants;
using UnityEngine;

namespace Battle.Scripts.Test
{
    public class ShieldBoxCollider : MonoBehaviour
    {
        // Serialized Fields
        [SerializeField] private float _bounceAngle;

        public Transform ShieldTransform => _transform;
        public float BounceAngle => _bounceAngle;
        public float ImpactForce => _playerActor.ImpactForce;
        public float AttackMultiplier => _attackMultiplier;

        public bool OnBallShieldCollision() => _playerActor.OnBallShieldCollision();
        public void OnBallShieldBounce() => _playerActor.OnBallShieldBounce();

        //private GridManager _gridManager;
        private PlayerActor _playerActor;
        private ShieldManager _shieldManager;
        private Transform _transform;
        private float _attackMultiplier;

        // Debug
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0052 // Remove unread private members
        private const string DEBUG_LOG_BALL_COLLISION = "[{0:000000}] [BATTLE] [SHIELD BOX COLLIDER] Ball collision: ";
        private SyncedFixedUpdateClock _syncedFixedUpdateClock; // only needed for logging time
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore IDE0052 // Remove unread private members

        private void Awake()
        {
            _transform = GetComponent<Transform>();
            _attackMultiplier = GameConfig.Get().Variables._playerAttackMultiplier;
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
    }
}
