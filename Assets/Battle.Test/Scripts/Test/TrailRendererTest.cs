using System.Collections;
using UnityEngine;

namespace Battle.Test.Scripts.Test
{
    public class TrailRendererTest : MonoBehaviour
    {
        public float _speed = 5f;
        public float _teleportDistance = 3f;
        public Vector2 _startDirection;
        public Vector2 _startPosition;

        public TrailRenderer _trailRenderer;
        public Rigidbody2D _rigidbody;

        private void OnEnable()
        {
            SetBallPosition(_rigidbody, _startPosition);
            SetBallSpeed(_rigidbody, _speed, _startDirection);
            StartCoroutine(RandomTeleport(_rigidbody, _teleportDistance));
        }

        private static IEnumerator RandomTeleport(Rigidbody2D rigidbody, float teleportDistance)
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
                    Debug.Log($"SetBallPosition {rigidbody.position} <- {position}");
                    SetBallPosition(rigidbody, position);
                }
                yield return delay;
            }
        }

        private static void SetBallPosition(Rigidbody2D rigidbody, Vector2 position)
        {
            rigidbody.position = position;
        }

        private static void SetBallSpeed(Rigidbody2D rigidbody, float speed, Vector2 direction)
        {
            rigidbody.velocity = direction.normalized * speed;
        }
    }
}