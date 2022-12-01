using UnityConstants;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    /// <summary>
    /// Handles collisions with shield's edge colliders.
    /// </summary>
    /// <remarks>
    /// Edgepoints must be set so that Vector2.Perpendicular points outwards from the shield
    /// Use right hand rule of thumb: index finger forms the edge vector, starting point is the base of the finger, ending at the tip.
    /// The thumb becomes Vector2.Perpendicular
    /// </remarks>
    [RequireComponent(typeof(EdgeCollider2D))]
    public class ShieldEdgeCollider : MonoBehaviour
    {
        private EdgeCollider2D _edgeCollider;
        private const float _bounceForce = 10f;

        private void Awake()
        {
            _edgeCollider = GetComponent<EdgeCollider2D>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var otherGameObject = other.gameObject;
            if (otherGameObject.CompareTag(Tags.Ball))
            {
                var endPoint1 = transform.TransformPoint(_edgeCollider.points[0]);
                var endPoint2 = transform.TransformPoint(_edgeCollider.points[1]);
                var edgeVector = endPoint2 - endPoint1;
                var bounceDirection = Vector2.Perpendicular(edgeVector).normalized;
                var contact = _edgeCollider.ClosestPoint(other.transform.position);
                var otherPos = new Vector2(otherGameObject.transform.position.x, otherGameObject.transform.position.y);
                var dif = (otherPos - contact) - bounceDirection;
                var add = (otherPos - contact) + bounceDirection;
                if (dif.magnitude > add.magnitude)
                {
                    return;
                }
                var rb = otherGameObject.GetComponentInParent<Rigidbody2D>();
                rb.velocity = bounceDirection * _bounceForce;
                UnityEngine.Debug.DrawRay(contact, bounceDirection * 100, Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f), 10f);
            }
        }
    }
}
