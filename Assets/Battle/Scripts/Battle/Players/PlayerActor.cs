using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    internal class PlayerActor : MonoBehaviour, IPlayerActor
    {
        private Transform _transform;
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

        private void Update()
        {
            var maxDistanceDelta = _movementSpeed * _playerMoveSpeedMultiplier * Time.deltaTime;
            _tempPosition = Vector2.MoveTowards(_transform.position, _targetPosition, maxDistanceDelta);
            _transform.position = _tempPosition;
        }
    }
}

