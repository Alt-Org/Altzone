using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private float _bounceAngle;

        private GridManager _gridManager;
        private IPlayerActor _playerActor;
        private Transform _transform;
        private Collider2D _collider;
        private float _attackMultiplier;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _transform = GetComponent<Transform>();
            _attackMultiplier = GameConfig.Get().Variables._playerAttackMultiplier;
            _playerActor = transform.root.GetComponent<PlayerActor>();
            _gridManager = Context.GetGridManager;
        }

        private IEnumerator OnTriggerEnter2D(Collider2D collider)
        {
            var otherGameObject = collider.gameObject;
            if (otherGameObject.CompareTag(Tags.Ball))
            {
                var rb = otherGameObject.GetComponentInParent<Rigidbody2D>();
                var gridPos = _gridManager.WorldPointToGridPosition(rb.position);
                rb.transform.position = _gridManager.GridPositionToWorldPoint(gridPos);
                var angle = _transform.rotation.eulerAngles.z + _bounceAngle;
                Debug.Log($"shield angle {angle}");
                var rotation = Quaternion.Euler(0, 0, angle);
                Debug.Log($"rot: {rotation}");
                rb.velocity = rotation * Vector2.up * _attackMultiplier;
                Debug.Log($"vel: {rb.velocity}");
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
