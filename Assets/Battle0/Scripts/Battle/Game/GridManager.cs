using System;
using Altzone.Scripts.Config;
using UnityEngine;

namespace Battle0.Scripts.Battle.Game
{
    public interface IGridManager
    {
        /// <summary>
        /// Gets grid width aka row count.
        /// </summary>
        int RowCount { get; }

        /// <summary>
        /// Gets grid height aka column count.
        /// </summary>
        int ColCount { get; }

        Vector2 GridPositionToWorldPoint(GridPos gridPos, bool isRotated);

        GridPos WorldPointToGridPosition(Vector2 targetPosition, bool isRotated);

        void SetSpaceFree(GridPos gridPos);

        void SetSpaceFree(int row, int col);

        void SetSpaceTaken(GridPos gridPos);

        void SetSpaceTaken(int row, int col);

        bool GridFreeState(int row, int col);
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
        private Camera _camera;
        private int _gridWidth;
        private int _gridHeight;

        private bool[,] _gridEmptySpaces;

        private void Awake()
        {
            _camera = null;//Context.GetBattleCamera;
            var runtimeGameConfig = Battle0GameConfig.Get();
            var variables = runtimeGameConfig.Variables;
            var battleUi = runtimeGameConfig.BattleUi;

            _gridWidth = variables._battleUiGridWidth;
            _gridHeight = variables._battleUiGridHeight;

            _gridEmptySpaces = new bool[_gridHeight, _gridWidth];
            for (int i = 0; i < _gridHeight; i++)
            {
                for (int j = 0; j < _gridWidth; j++)
                {
                    _gridEmptySpaces[i, j] = true;
                }
            }
        }

        public int RowCount => _gridHeight;

        public int ColCount => _gridWidth;

        Vector2 IGridManager.GridPositionToWorldPoint(GridPos gridPos, bool isRotated)
        {
            var viewportPosition = new Vector2();
            viewportPosition.x = (float)gridPos.Col / _gridWidth + 0.5f / _gridWidth;
            viewportPosition.y = (float)gridPos.Row / _gridHeight + 0.5f / _gridHeight;
            Vector2 worldPosition = _camera.ViewportToWorldPoint(viewportPosition);
            if (isRotated)
            {
                worldPosition.x = -worldPosition.x;
                worldPosition.y = -worldPosition.y;
            }
            return worldPosition;
        }

        GridPos IGridManager.WorldPointToGridPosition(Vector2 targetPosition, bool isRotated)
        {
            if (isRotated)
            {
                targetPosition.x = -targetPosition.x;
                targetPosition.y = -targetPosition.y;
            }
            var viewportPosition = _camera.WorldToViewportPoint(targetPosition);
            var col = Math.Min(_gridWidth - 1, (int)(viewportPosition.x * _gridWidth));
            var row = Math.Min(_gridHeight - 1, (int)(viewportPosition.y * _gridHeight));
            GridPos gridPos = new GridPos(row, col);
            return gridPos;
        }
        void IGridManager.SetSpaceFree(GridPos gridPos)
        {
            SetGridState(gridPos.Row, gridPos.Col, true);
        }

        void IGridManager.SetSpaceFree(int row, int col)
        {
            SetGridState(row, col, true);
        }

        void IGridManager.SetSpaceTaken(GridPos gridPos)
        {
            SetGridState(gridPos.Row, gridPos.Col, false);
        }
        void IGridManager.SetSpaceTaken(int row, int col)
        {
            SetGridState(row, col, false);
        }

        bool IGridManager.GridFreeState(int row, int col)
        {
            return _gridEmptySpaces[row, col];
        }

        private void SetGridState(int row, int col, bool state)
        {
            _gridEmptySpaces[row, col] = state;
            Debug.Log($"Grid space set: row: {row}, col: {col}, state: {state}");
        }
    }
}
