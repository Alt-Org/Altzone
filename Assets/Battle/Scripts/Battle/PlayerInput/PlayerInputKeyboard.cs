using Battle.Scripts.Battle.interfaces;
using UnityEngine;

namespace Battle.Scripts.Battle.PlayerInput
{
    /// <summary>
    /// Polls joystick "Horizontal" and "Vertical" movements and forwards them to <c>IMovablePlayer</c> for processing.
    /// </summary>
    internal class PlayerInputKeyboard : MonoBehaviour
    {
        // Current gameplay area for one team is about 10.0 x 8.5
        private const float UnReachableDistance = 20;

        [Header("Live Data"), SerializeField] private Transform _transform;
        [SerializeField] private Vector2 _joystickPosition;
        [SerializeField] private Vector2 _targetPosition;
        [SerializeField] private bool _isMoving;

        private IMovablePlayer _playerMovement;
        private Vector3 _curPosition;

        public IMovablePlayer PlayerMovement
        {
            get => _playerMovement;
            set
            {
                _playerMovement = value;
                _transform = _playerMovement.Transform;
                enabled = true;
            }
        }

        private void Awake()
        {
            // We need PlayerMovement before we can do work.
            enabled = false;
        }

        private void Update()
        {
            _joystickPosition.x = Input.GetAxis("Horizontal");
            _joystickPosition.y = Input.GetAxis("Vertical");
            if (_joystickPosition.x == 0 && _joystickPosition.y == 0)
            {
                if (!_isMoving)
                {
                    return;
                }
                // Stop moving, otherwise player would continue to move until it reached "unReachableDistance" we set below
                _isMoving = false;
                _curPosition = _transform.position;
                _targetPosition.x = _curPosition.x;
                _targetPosition.y = _curPosition.y;
            }
            else
            {
                // No need to normalize anything as we just want a point that is so far away that it is unreachable for the player.
                _isMoving = true;
                _curPosition = _transform.position;
                _targetPosition.x = _curPosition.x + _joystickPosition.x * UnReachableDistance;
                _targetPosition.y = _curPosition.y + _joystickPosition.y * UnReachableDistance;
            }
            _playerMovement.MoveTo(_targetPosition);
        }
    }
}