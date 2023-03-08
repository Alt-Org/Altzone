using System.Collections;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using UnityConstants;
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

        private float _playerMoveSpeedMultiplier;
        private Transform _transform;
        private Vector3 _tempPosition;
        private bool _hasTarget;

        private void Awake()
        {
            _transform = GetComponent<Transform>();
            var variables = GameConfig.Get().Variables;
            _playerMoveSpeedMultiplier = variables._playerMoveSpeedMultiplier;
        }

        private IEnumerator MoveCoroutine(Vector2 position)
        {
            Vector3 targetPosition = position;
            _hasTarget = true;
            while (_hasTarget)
            {
                yield return null;
                var maxDistanceDelta = _movementSpeed * _playerMoveSpeedMultiplier * Time.deltaTime;
                _tempPosition = Vector3.MoveTowards(_transform.position, targetPosition, maxDistanceDelta);
                _transform.position = _tempPosition;
                _hasTarget = !(Mathf.Approximately(_tempPosition.x, targetPosition.x) && Mathf.Approximately(_tempPosition.y, targetPosition.y));
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

        public static IPlayerActor InstantiatePrefabFor(int playerPos, PlayerActorBase playerPrefab, string gameObjectName)
        {
            var instantiationGridPosition = Context.GetBattlePlayArea.GetPlayerStartPosition(playerPos);
            var instantiationPosition = Context.GetGridManager.GridPositionToWorldPoint(instantiationGridPosition);
            var playerActorBase = Instantiate(playerPrefab, instantiationPosition, Quaternion.identity);
            if (playerActorBase != null)
            {
                playerActorBase.name = playerActorBase.name.Replace("Clone", gameObjectName);
                switch (playerPos)
                {
                    case PhotonBattle.PlayerPosition1:
                        playerActorBase.gameObject.layer = Layers.Player1;
                        break;
                    case PhotonBattle.PlayerPosition2:
                        playerActorBase.gameObject.layer = Layers.Player2;
                        break;
                    case PhotonBattle.PlayerPosition3:
                        playerActorBase.gameObject.layer = Layers.Player3;
                        break;
                    case PhotonBattle.PlayerPosition4:
                        playerActorBase.gameObject.layer = Layers.Player4;
                        break;
                    default:
                        throw new UnityException($"Invalid player position {playerPos}");
                }
            }
            var playerActor = (IPlayerActor)playerActorBase;
            return playerActor;
        }
    }
}
