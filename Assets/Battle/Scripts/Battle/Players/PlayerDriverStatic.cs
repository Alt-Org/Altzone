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

        void IPlayerDriver.MoveTo(Vector2 targetPosition)
        {
            var gridPos = _gridManager.WorldPointToGridPosition(targetPosition);
            targetPosition = _gridManager.GridPositionToWorldPoint(gridPos);
            _playerActor.MoveTo(targetPosition);
        }

    }
}
