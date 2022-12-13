using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    public class PlayerDriverStatic : MonoBehaviour, IPlayerDriver
    {
        [SerializeField] private GameObject _playerPrefab;
        private IPlayerActor _playerActor;
        private IGridManager _gridManager;

        private void Awake()
        {
            _gridManager = Context.GetGridManager();
            _playerActor = Instantiate(_playerPrefab).GetComponent<PlayerActor>();
        }

        private void OnEnable()
        {
            var playerInputHandler = Context.GetPlayerInputHandler();
            playerInputHandler.SetPlayerDriver(this);
        }

        void IPlayerDriver.MoveTo(Vector2 targetPosition)
        {
            var gridPos = _gridManager.WorldPointToGridPosition(targetPosition);
            targetPosition = _gridManager.GridPositionToWorldPoint(gridPos);
            _playerActor.MoveTo(targetPosition);
        }
        void IPlayerDriver.MoveTo(GridPos gridPos)
        {
            var targetPosition = _gridManager.GridPositionToWorldPoint(gridPos);
            _playerActor.MoveTo(targetPosition);
        }
    }
}
