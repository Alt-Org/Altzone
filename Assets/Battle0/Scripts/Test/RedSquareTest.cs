using UnityEngine;

namespace Battle0.Scripts.Test
{
    public class RedSquareTest : MonoBehaviour
    {
        [Header("Ball movement")] public float _zAngleRotation;

        private Transform _transform;

        private void Awake()
        {
            _transform = GetComponent<Transform>();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            _transform.Rotate(0, 0, _zAngleRotation);
        }
    }
}