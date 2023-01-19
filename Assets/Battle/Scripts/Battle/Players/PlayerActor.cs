using System.Collections;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    /// <summary>
    /// <c>PlayerActor</c> for local and remote instances.
    /// </summary>
    internal class PlayerActor : PlayerActorBase, IPlayerActor
    {
        [SerializeField] private Transform _geometryRoot;
        [SerializeField] private float _movementSpeed;
        [SerializeField] private float _playerMoveSpeedMultiplier;

        private Transform _transform;
        private Vector2 _targetPosition;
        private Vector2 _tempPosition;
        private bool _hasTarget;

        private void Awake()
        {
            _transform = GetComponent<Transform>();
            _targetPosition = transform.position;
        }

        private IEnumerator MoveCoroutine(Vector2 targetPosition)
        {
            _hasTarget = true;
            while (_hasTarget)
            {
                yield return null;
                var maxDistanceDelta = _movementSpeed * _playerMoveSpeedMultiplier * Time.deltaTime;
                _tempPosition = Vector2.MoveTowards(_transform.position, _targetPosition, maxDistanceDelta);
                _transform.position = _tempPosition;
                _hasTarget = !(Mathf.Approximately(_tempPosition.x, _targetPosition.x) && Mathf.Approximately(_tempPosition.y, _targetPosition.y));
            }
        }

        #region IPlayerActor

        bool IPlayerActor.IsBusy => _hasTarget;

        void IPlayerActor.MoveTo(Vector2 targetPosition)
        {
            _targetPosition = targetPosition;
            StartCoroutine(MoveCoroutine(targetPosition));
        }

        void IPlayerActor.Rotate(float angle)
        {
            _geometryRoot.Rotate(0, 0, angle);
        }

        #endregion
    }
}
