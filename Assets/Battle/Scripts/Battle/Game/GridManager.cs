using System;
using UnityEngine;

namespace Battle.Scripts.Battle.Game
{
    internal class GridManager : MonoBehaviour, IGridManager
    {
        [SerializeField] private Vector2 _testPosition;
        [SerializeField] private GridPos _testGridPos;

        private int _movementGridWidth;
        private int _movementGridHeight;
        private IBattlePlayArea _battlePlayArea;
        private float _arenaWidth;
        private float _arenaHeight;

        private void Awake()
        {
            _battlePlayArea = Context.GetBattlePlayArea();
            _arenaWidth = _battlePlayArea.ArenaWidth;
            _arenaHeight = _battlePlayArea.ArenaHeight;

            _movementGridWidth = _battlePlayArea.MovementGridWidth;
            _movementGridHeight = _battlePlayArea.MovementGridHeight;
        }

        Vector2 IGridManager.GridPositionToWorldPoint(GridPos gridPos)
        {
            var xPosition = gridPos.Col * _arenaWidth / _movementGridWidth + _arenaWidth / _movementGridWidth * 0.5f;
            var yPosition = gridPos.Row * _arenaHeight / _movementGridHeight + _arenaHeight / _movementGridHeight * 0.5f;
            Vector2 worldPosition = new Vector2(xPosition - _arenaWidth / 2, yPosition - _arenaHeight / 2);
            return worldPosition;
        }

        GridPos IGridManager.WorldPointToGridPosition(Vector2 targetPosition)
        {
            var posNew = new Vector2(targetPosition.x + _arenaWidth / 2, targetPosition.y + _arenaHeight / 2);
            var col = Math.Min(_movementGridWidth - 1, (int) (posNew.x / (_arenaWidth / _movementGridWidth)));
            var row = Math.Min(_movementGridHeight - 1, (int) (posNew.y / (_arenaHeight / _movementGridHeight)));
            GridPos gridPos = new GridPos(row, col);
            return gridPos;
        }
    }
}
