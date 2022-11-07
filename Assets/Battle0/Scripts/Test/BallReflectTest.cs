using UnityEngine;

namespace Battle0.Scripts.Test
{
    public class BallReflectTest : MonoBehaviour
    {
        [Header("Ball movement")] public Vector2 _initialVelocity;

        private Rigidbody2D _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            Debug.Log($"START {name} velocity {_initialVelocity}");
            _rigidbody.velocity = _initialVelocity;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            var otherGameObject = other.gameObject;
            var layer = otherGameObject.layer;
            Debug.Log($"COLLIDE {other.name} layer {layer} {otherGameObject.tag}");
            BounceFrom(other);
        }

        private void BounceFrom(Collider2D other)
        {
            var position = _rigidbody.position;
            var closestPoint = other.ClosestPoint(position);
            var direction = closestPoint - position;
            Reflect(direction.normalized);
        }

        private void Reflect(Vector2 collisionNormal)
        {
            var currentVelocity = _rigidbody.velocity;
            var speed = currentVelocity.magnitude;
            var direction = Vector2.Reflect(currentVelocity.normalized, collisionNormal);
            _rigidbody.velocity = direction * Mathf.Max(speed, 0);
        }
    }
}