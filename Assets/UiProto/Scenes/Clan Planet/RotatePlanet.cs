using UnityEngine;

namespace Scenes.Clan_Planet
{
    public class RotatePlanet : MonoBehaviour
    {
        public float rotationSpeed;
        public float dragSpeed;
        public float dragGuardDelay;

        private float dragGuardTime;

        private void Update()
        {
            if (dragGuardTime > Time.time)
            {
                return;
            }
            var angle = rotationSpeed * Time.deltaTime;
            transform.Rotate(angle, angle, 0f);
        }

        public void onUnityEvent_onMouseDown(Component target) // Editor callback for: UnityEvent<Component>
        {
        }

        public void onUnityEvent_onMouseDrag() // Editor callback for: UnityEvent
        {
            dragGuardTime = Time.time + dragGuardDelay;

            var speed = dragSpeed * Mathf.Deg2Rad;
            var angleX = Input.GetAxis("Mouse X") * speed;
            var angleY = Input.GetAxis("Mouse Y") * speed;

            transform.Rotate(Vector3.up, -angleX);
            transform.Rotate(Vector3.right, angleY);
        }
    }
}