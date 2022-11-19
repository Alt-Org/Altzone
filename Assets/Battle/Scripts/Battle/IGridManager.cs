using System;
using UnityEngine;

namespace Battle.Scripts.Battle
{
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

    public interface IGridManager
    {
        Vector2 GridPositionToWorldPoint(GridPos gridPos, bool isRotated);

        GridPos WorldPointToGridPosition(Vector2 targetPosition, bool isRotated);
    }
}