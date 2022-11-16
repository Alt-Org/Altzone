using System;
using UnityEngine;

namespace Battle.Scripts.Battle.Game
{
    public interface IGridManager
    {
        Vector2 GridPositionToWorldPoint(GridPos gridPos, bool isRotated);

        GridPos WorldPointToGridPosition(Vector2 targetPosition, bool isRotated);
    }

    public class GridPos : Tuple<int, int>
    {
        public int Row => Item1;
        public int Col => Item2;

        public GridPos(int row, int col) : base(row, col)
        {
        }

        public override string ToString()
        {
            return $"GridPos {nameof(Row)},{nameof(Col)}: {Row},{Col}";
        }
    }

    internal class GridManager : MonoBehaviour, IGridManager
    {
        [SerializeField] private Vector2 _testPosition;
        [SerializeField] private GridPos _testGridPos;

        private int _movementGridWidth;
        private int _movementGridHeight;
        private IBattlePlayArea _battlePlayArea;
        private Vector2 _arenaSize;

        private void Awake()
        {
            _battlePlayArea = FindObjectOfType<PlayerPlayArea>();
            _arenaSize = _battlePlayArea.ArenaSize;

            _movementGridWidth = _battlePlayArea.MovementGridWidth;
            _movementGridHeight = _battlePlayArea.MovementGridHeight;
        }

        Vector2 IGridManager.GridPositionToWorldPoint(GridPos gridPos, bool isRotated)
        {
            var arenaWidth = _arenaSize.x;
            var arenaHeight = _arenaSize.y;
            var xPosition = gridPos.Col * arenaWidth / _movementGridWidth + arenaWidth / _movementGridWidth * 0.5f;
            var yPosition = gridPos.Row * arenaHeight / _movementGridHeight + arenaHeight / _movementGridHeight * 0.5f;
            Vector2 worldPosition = new Vector2(xPosition - arenaWidth / 2, yPosition - arenaHeight / 2);
            if (isRotated)
            {
                worldPosition.x = -worldPosition.x;
                worldPosition.y = -worldPosition.y;
            }
            return worldPosition;
        }

        GridPos IGridManager.WorldPointToGridPosition(Vector2 targetPosition, bool isRotated)
        {
            var arenaWidth = _arenaSize.x;
            var arenaHeight = _arenaSize.y;
            if (isRotated)
            {
                targetPosition.x = -targetPosition.x;
                targetPosition.y = -targetPosition.y;
            }
            var posNew = new Vector2(targetPosition.x + arenaWidth / 2, targetPosition.y + arenaHeight / 2);
            var col = Math.Min(_movementGridWidth - 1, (int) (posNew.x / (arenaWidth / _movementGridWidth)));
            var row = Math.Min(_movementGridHeight - 1, (int) (posNew.y / (arenaHeight / _movementGridHeight)));
            GridPos gridPos = new GridPos(row, col);
            return gridPos;
        }
    }
}
