using UnityConstants;
using UnityEngine;
using Altzone.Scripts.Config;
using System.Collections;


namespace Battle.Scripts.Battle.Players
{
    public class Shield : MonoBehaviour
    {
        private PolygonCollider2D _shieldCollider;
        private float _ballSpeedCompensation;
        private float _attackMultiplier;
        private GameObject _otherGameObject;
        private LayerMask _layerMask;

        private void Awake()
        {
            _shieldCollider = GetComponentInChildren<PolygonCollider2D>();
            _attackMultiplier = GameConfig.Get().Variables._playerAttackMultiplier;
            _ballSpeedCompensation = GameConfig.Get().Variables._ballSpeedCompensation;
            _layerMask = 1 << gameObject.layer;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            _otherGameObject = other.gameObject;
            if (_otherGameObject.CompareTag(Tags.Ball))
            {
                var rb = _otherGameObject.GetComponentInParent<Rigidbody2D>();
                var origin = rb.position - rb.velocity.normalized * _ballSpeedCompensation;
                RaycastHit2D hit = Physics2D.Raycast(origin, rb.velocity, Mathf.Infinity, _layerMask);
                UnityEngine.Debug.DrawRay(origin, rb.velocity * 100, Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f), 5f);
                if (hit)
                {
                    if (hit.collider == _shieldCollider)
                    {
                        var normal = hit.normal;
                        rb.velocity = normal * _attackMultiplier;
                        UnityEngine.Debug.DrawRay(hit.point, normal * 100, Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f), 10f);
                    }
                }
            }
        }
    }
}
