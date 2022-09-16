using Altzone.Scripts.Config;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Scripts.Battle
{
    internal interface IGridManager
    {
        bool[,] _gridEmptySpaces { get; set; }

        Vector2 GridPositionToWorldpoint(int row, int col, bool isRotated);

        int[] CalcRowAndColumn(Vector2 targetPosition, bool isRotated);
    }

    internal class GridManager : MonoBehaviour, IGridManager
    {
        private int _gridWidth;
        private int _gridHeight;

        public bool[,] _gridEmptySpaces { get; set; }

        private void Awake()
        {
            var runtimeGameConfig = RuntimeGameConfig.Get();
            var variables = runtimeGameConfig.Variables;
            var battleUi = runtimeGameConfig.BattleUi;

            _gridWidth = variables._battleUiGridWidth;
            _gridHeight = variables._battleUiGridHeight;

            _gridEmptySpaces = new bool[_gridWidth, _gridHeight];
            for (int i = 0; i < _gridWidth; i++)
            {
                for (int j = 0; j < _gridHeight; j++)
                {
                    _gridEmptySpaces[i, j] = true;
                }
            }
        }

        public Vector2 GridPositionToWorldpoint(int row, int col, bool isRotated)
        {
            var viewportPosition = new Vector2();
            viewportPosition.x = (float)row / _gridWidth + 0.5f / _gridWidth;
            viewportPosition.y = (float)col / _gridHeight + 0.5f / _gridHeight;
            Vector2 worldPosition = Camera.main.ViewportToWorldPoint(viewportPosition);
            if (isRotated)
            {
                worldPosition.x = -worldPosition.x;
                worldPosition.y = -worldPosition.y;
            }
            return worldPosition;
        }

        public int[] CalcRowAndColumn(Vector2 worldPosition, bool isRotated)
        {
            if (isRotated)
            {
                worldPosition.x = -worldPosition.x;
                worldPosition.y = -worldPosition.y;
            }
            var viewportPosition = Camera.main.WorldToViewportPoint(worldPosition);
            var row = (int)(viewportPosition.x * _gridWidth);
            var col = (int)(viewportPosition.y * _gridHeight);
            return new int[] { row, col };
        }
    }
}
