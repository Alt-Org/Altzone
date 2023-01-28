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
        private Vector3 _tempPosition;
        private bool _hasTarget;

        private void Awake()
        {
            _transform = GetComponent<Transform>();
        }

        private IEnumerator MoveCoroutine(Vector2 targetPosition)
        {
            var targetPos3d = (Vector3)targetPosition;
            _hasTarget = true;
            while (_hasTarget)
            {
                yield return null;
                var maxDistanceDelta = _movementSpeed * _playerMoveSpeedMultiplier * Time.deltaTime;
                _tempPosition = Vector3.MoveTowards(_transform.position, targetPos3d, maxDistanceDelta);
                _transform.position = _tempPosition;
                _hasTarget = !(Mathf.Approximately(_tempPosition.x, targetPos3d.x) && Mathf.Approximately(_tempPosition.y, targetPos3d.y));
            }
        }

        #region IPlayerActor

        bool IPlayerActor.IsBusy => _hasTarget;

        void IPlayerActor.MoveTo(Vector2 targetPosition)
        {
            StartCoroutine(MoveCoroutine(targetPosition));
        }

        void IPlayerActor.SetRotation(float angle)
        {
            _geometryRoot.eulerAngles = new Vector3(0, 0, angle);
        }

        #endregion
    }
}
