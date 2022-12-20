using System;
using UnityEngine;

namespace Battle.Scripts.Battle
{
    [Serializable]
    public class GridPos : Tuple<int, int>
    {
        [SerializeField] private int _row;
        [SerializeField] private int _col;
        public int Row => _row;
        public int Col => _col;

        public GridPos(int row, int col) : base(row, col)
        {
            _row = row;
            _col = col;
        }

        public override string ToString()
        {
            return $"GridPos {nameof(Row)},{nameof(Col)}: {Row},{Col}";
        }
    }

    internal interface IGridManager
    {
        Vector2 GridPositionToWorldPoint(GridPos gridPos);

        GridPos WorldPointToGridPosition(Vector2 targetPosition);
    }
}
