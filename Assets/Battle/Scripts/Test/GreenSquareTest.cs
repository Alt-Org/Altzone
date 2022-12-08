using UnityEngine;
using UnityEngine.InputSystem;
using Battle.Scripts.Battle;

namespace Battle.Scripts.Test
{
    public class GreenSquareTest : MonoBehaviour
    {
        private Transform _transform;
        private Camera _camera;
        private Vector2 _lookTowards;
        private Vector2 _targetPosition;
        private Vector2 _tempPosition;
        [SerializeField] private float _movementSpeed;

        private void Awake()
        {
            _transform = GetComponent<Transform>();
            _camera = Context.GetBattleCamera.Camera;
        }

        void Update()
        {
            _lookTowards = _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            _transform.up = _lookTowards;
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _targetPosition = _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            }
            var maxWorld = _camera.ViewportToWorldPoint(Vector2.one);
            var minWorld = _camera.ViewportToWorldPoint(Vector2.zero);
            _targetPosition.x = Mathf.Clamp(_targetPosition.x, -5, 5);
            _targetPosition.y = Mathf.Clamp(_targetPosition.y, -9, 9);

            var maxDistanceDelta = _movementSpeed * Time.deltaTime;
            _tempPosition = Vector2.MoveTowards(_transform.position, _targetPosition, maxDistanceDelta);
            _transform.position = _tempPosition;
        }
    }
}
