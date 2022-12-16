using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    internal class PlayerActor : MonoBehaviour, IPlayerActor
    {
        private Transform _transform;
        [Header("Settings"), SerializeField] private Transform _geometryRoot;

        [SerializeField] private float _movementSpeed;
        [SerializeField] private float _playerMoveSpeedMultiplier;
        private Vector2 _targetPosition;
        private Vector2 _tempPosition;

        private void Awake()
        {
            _transform = GetComponent<Transform>();
        }

        void IPlayerActor.MoveTo(Vector2 targetPosition)
        {
            _targetPosition = targetPosition;
        }
        void IPlayerActor.Rotate(float angle)
        {
            _geometryRoot.Rotate(0, 0, angle);
        }

        private void Update()
        {
            var maxDistanceDelta = _movementSpeed * _playerMoveSpeedMultiplier * Time.deltaTime;
            _tempPosition = Vector2.MoveTowards(_transform.position, _targetPosition, maxDistanceDelta);
            _transform.position = _tempPosition;
        }
    }
}

