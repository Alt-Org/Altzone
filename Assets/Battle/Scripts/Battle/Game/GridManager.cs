using System;
using Altzone.Scripts.Config;
using System.Collections;
using System.Collections.Generic;
using Battle.Scripts.Battle.Game;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Battle
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

        bool GridState(int row, int col);

        void SetGridState(int row, int col, bool state);
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
            _camera = Context.GetBattleCamera.Camera;
            var runtimeGameConfig = RuntimeGameConfig.Get();
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
            var col = (int)(viewportPosition.x * _gridWidth);
            var row = (int)(viewportPosition.y * _gridHeight);
            GridPos gridPos = new GridPos(row, col);
            return gridPos;
        }

        bool IGridManager.GridState(int row, int col)
        {
            return _gridEmptySpaces[row, col];
        }

        void IGridManager.SetGridState(int row, int col, bool state)
        {
            _gridEmptySpaces[row, col] = state;
        }
    }
}
