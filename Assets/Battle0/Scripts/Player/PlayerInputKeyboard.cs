using Battle0.Scripts.interfaces;
using UnityEngine;

namespace Battle0.Scripts.Player
{
    /// <summary>
    /// Polls joystick "Horizontal" and "Vertical" movements and forwards them to <c>IMovablePlayer</c> for processing.
    /// </summary>
    public class PlayerInputKeyboard : MonoBehaviour
    {
        private const float unReachableDistance = 10;

        [Header("Live Data"), SerializeField] private Camera _camera;
        [SerializeField] protected Transform _transform;
        [SerializeField] private float playerPositionZ;
        [SerializeField] private Vector2 joystickPosition;
        [SerializeField] private Vector3 targetPosition;
        [SerializeField] private bool isMoving;

        private IMovablePlayer playerMovement;

        public IMovablePlayer PlayerMovement
        {
            get => playerMovement;
            set
            {
                playerMovement = value;
                _transform = transform;
                playerPositionZ = _transform.position.z;
                targetPosition.z = playerPositionZ;
            }
        }

        private void Update()
        {
            joystickPosition.x = Input.GetAxis("Horizontal");
            joystickPosition.y = Input.GetAxis("Vertical");
            if (joystickPosition.x == 0 && joystickPosition.y == 0)
            {
                if (!isMoving)
                {
                    return;
                }
                // Stop moving, otherwise player would continue to move until it reached "unReachableDistance" we set below
                isMoving = false;
                targetPosition = _transform.position;
            }
            else
            {
                joystickPosition.Normalize();
                targetPosition = _transform.position;
                targetPosition.x += joystickPosition.x * unReachableDistance;
                targetPosition.y += joystickPosition.y * unReachableDistance;
                isMoving = true;
            }
            playerMovement.moveTo(targetPosition);
        }
    }
}