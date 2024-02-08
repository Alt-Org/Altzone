using System.Collections;
using Altzone.Scripts.Config;
using Battle.Scripts.Battle;
using Battle.Scripts.Battle.Game;
using Battle.Scripts.Battle.Players;
using UnityConstants;
using UnityEngine;

namespace Battle.Scripts.Test
{
    public class ShieldBoxColliderTest : MonoBehaviour
    {
        // Serialized Fields
        [SerializeField] private float _bounceAngle;

        private GridManager _gridManager;
        private PlayerActor _playerActor;
        private Transform _transform;
        private Collider2D _collider;
        private float _attackMultiplier;

        // Debug
        private const string DEBUG_LOG_BALL_COLLISION = "[{0:000000}] [BATTLE] [SHIELD BOX COLLIDER] Ball collision: ";
        private SyncedFixedUpdateClockTest _syncedFixedUpdateClock; // only needed for logging time

        public Transform ShieldTransform => _transform;


        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _transform = GetComponent<Transform>();
            _attackMultiplier = GameConfig.Get().Variables._playerAttackMultiplier;
            _playerActor = transform.root.GetComponent<PlayerActor>();
            _gridManager = Context.GetGridManager;

            // debug
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
        }

        private IEnumerator OnTriggerEnter2D(Collider2D collider)
        {
            var otherGameObject = collider.gameObject;
            if (otherGameObject.CompareTag(Tags.Ball))
            {
                var rb = otherGameObject.GetComponentInParent<Rigidbody2D>();

                // Tämä tarkistaa, meneekö ammus oikeaan suuntaan
                var incomingDirection = (rb.position - (Vector2)_transform.position).normalized;
                var shieldDirection = _transform.up; // Kilven suunta
                var isCorrectDirection = Vector2.Dot(incomingDirection, shieldDirection) < 0; 

                // Tämä kohta laskee uuden nopeuden ja suunnan
                var gridPos = _gridManager.WorldPointToGridPosition(rb.position);
                rb.position = _gridManager.GridPositionToWorldPoint(gridPos);
                var angle = _transform.rotation.eulerAngles.z + _bounceAngle;
                var rotation = Quaternion.Euler(0, 0, angle);
                var originalVelocityNormalized = rb.velocity.normalized; 
                var newSpeed = rb.velocity.magnitude + _attackMultiplier; 
                var newVelocity = originalVelocityNormalized * newSpeed; 

                
                // Päätetään nopeuttaako kilpi ammusta
                var isAccelerated = newVelocity.magnitude > rb.velocity.magnitude;

                Debug.Log(string.Format(DEBUG_LOG_BALL_COLLISION + "shield angle {1}, correct direction: {2}, accelerated: {3}", _syncedFixedUpdateClock.UpdateCount, angle, isCorrectDirection, isAccelerated));
                rb.velocity = newVelocity;
                if (_playerActor != null)
                {
                    _playerActor.ShieldHit(1);
                }
                _collider.enabled = false;
                yield return new WaitForSeconds(.1f);
                _collider.enabled = true;
            }
        }
    }
}



