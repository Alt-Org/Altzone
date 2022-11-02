using System.Collections;
using UnityEngine;

namespace Battle0.Scripts.Test
{
    internal class TrailRendererTest : MonoBehaviour
    {
        private static MonoBehaviour _context;
        
        public float _speed = 5f;
        public float _teleportDistance = 3f;
        public Vector2 _startDirection;
        public Vector2 _startPosition;

        public TrailRenderer _trailRenderer;
        public Rigidbody2D _rigidbody;

        private void OnEnable()
        {
            _context = this;
            SetBallPosition(_rigidbody, _trailRenderer, _startPosition);
            SetBallSpeed(_rigidbody, _speed, _startDirection);
            StartCoroutine(RandomTeleport(_rigidbody, _trailRenderer, _teleportDistance));
        }

        private static IEnumerator RandomTeleport(Rigidbody2D rigidbody, TrailRenderer trailRenderer, float teleportDistance)
        {
            var delay = new WaitForSeconds(0.5f);
            for (;;)
            {
                var position = rigidbody.position;
                if (position.y < 1 && position.y > -1)
                {
                    var velocity = rigidbody.velocity;
                    if (velocity.y > 0)
                    {
                        position.y += teleportDistance;
                    }
                    else
                    {
                        position.y -= teleportDistance;
                    }
                    SetBallPosition(rigidbody, trailRenderer, position);
                }
                yield return delay;
            }
        }

        private static void SetBallPosition(Rigidbody2D rigidbody, TrailRenderer trailRenderer, Vector2 position)
        {
            Debug.Log($"{rigidbody.position} <- {position} trail line count #{trailRenderer.positionCount}");
            trailRenderer.ResetTrailRendererAfterTeleport(_context);
            rigidbody.position = position;
            Debug.Log($"{rigidbody.transform.position} transform.position");
        }

        private static void SetBallSpeed(Rigidbody2D rigidbody, float speed, Vector2 direction)
        {
            rigidbody.velocity = direction.normalized * speed;
        }
    }
}