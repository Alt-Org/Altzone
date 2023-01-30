using System;
using UnityEngine;

namespace Battle.Scripts.Battle.Game
{
    /// <summary>
    /// <c>GridManager</c> translates between world and grid coordinates, also keeps track which coordinates player can move to.
    /// </summary>
    internal class GridManager : MonoBehaviour, IGridManager
    {
        private int _movementGridWidth;
        private int _movementGridHeight;
        private int _shieldGridWidth;
        private int _shieldGridHeight;
        private IBattlePlayArea _battlePlayArea;
        private float _arenaWidth;
        private float _arenaHeight;
        private bool[,] _gridEmptySpacesBlue;
        private bool[,] _gridEmptySpacesRed;
        private Rect _startAreaBlue;
        private Rect _startAreaRed;

        private void Start()
        {
            _battlePlayArea = Context.GetBattlePlayArea;
            _arenaWidth = _battlePlayArea.ArenaWidth;
            _arenaHeight = _battlePlayArea.ArenaHeight;

            _movementGridWidth = _battlePlayArea.MovementGridWidth;
            _movementGridHeight = _battlePlayArea.MovementGridHeight;
            _shieldGridWidth = _battlePlayArea.ShieldGridWidth;
            _shieldGridHeight = _battlePlayArea.ShieldGridHeight;

            InitializeGridArrays();
        }

        private void InitializeGridArrays()
        {
            _startAreaBlue = _battlePlayArea.GetPlayerPlayArea(PhotonBattle.TeamBlueValue);
            _startAreaRed = _battlePlayArea.GetPlayerPlayArea(PhotonBattle.TeamRedValue);

            var smallOffset = 0.001f;
            var blueAreaStart = ((IGridManager)this).WorldPointToGridPosition(new Vector2(_startAreaBlue.xMin, _startAreaBlue.yMin + smallOffset));
            var redAreaStart = ((IGridManager)this).WorldPointToGridPosition(new Vector2(_startAreaRed.xMin, _startAreaRed.yMin + smallOffset));
            var blueAreaEnd = ((IGridManager)this).WorldPointToGridPosition(new Vector2(_startAreaBlue.xMax, _startAreaBlue.yMax - smallOffset));
            var redAreaEnd = ((IGridManager)this).WorldPointToGridPosition(new Vector2(_startAreaRed.xMax, _startAreaRed.yMax - smallOffset));

            var blueRowMin = blueAreaStart.Row;
            var redRowMin = redAreaStart.Row;
            var blueRowMax = blueAreaEnd.Row;
            var redRowMax = redAreaEnd.Row;

            _gridEmptySpacesBlue = new bool[_movementGridHeight, _movementGridWidth];
            _gridEmptySpacesRed = new bool[_movementGridHeight, _movementGridWidth];

            for (int row = 0; row < _movementGridHeight; row++)
            {
                for (int col = 0; col < _movementGridWidth; col++)
                {
                    if (row >= blueRowMin && row <= blueRowMax)
                    {
                        _gridEmptySpacesBlue[row, col] = true;
                    }
                    else
                    {
                        _gridEmptySpacesBlue[row, col] = false;
                    }
                }
            }

            for (int row = 0; row < _movementGridHeight; row++)
            {
                for (int col = 0; col < _movementGridWidth; col++)
                {
                    if (row >= redRowMin && row <= redRowMax)
                    {
                        _gridEmptySpacesRed[row, col] = true;
                    }
                    else
                    {
                        _gridEmptySpacesRed[row, col] = false;
                    }
                }
            }
        }

        #region IGridManager

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
            var col = Math.Min(_movementGridWidth - 1, (int)(posNew.x / (_arenaWidth / _movementGridWidth)));
            var row = Math.Min(_movementGridHeight - 1, (int)(posNew.y / (_arenaHeight / _movementGridHeight)));
            GridPos gridPos = new GridPos(row, col);
            return gridPos;
        }

        GridPos IGridManager.ShieldGridPosition(Vector2 targetPosition)
        {
            var posNew = new Vector2(targetPosition.x + _arenaWidth / 2, targetPosition.y + _arenaHeight / 2);
            var col = Math.Min(_shieldGridWidth - 1, (int)(posNew.x / (_arenaWidth / _shieldGridWidth)));
            var row = Math.Min(_shieldGridHeight - 1, (int)(posNew.y / (_arenaHeight / _shieldGridHeight)));
            GridPos gridPos = new GridPos(row, col);
            return gridPos;
        }

        Vector2 IGridManager.ShieldSquareCorner(GridPos shieldGridPos, bool turnRight, int teamNumber)
        {
            var row = 0;
            var col = 0;
            if (teamNumber == PhotonBattle.TeamBlueValue)
            {
                row = shieldGridPos.Row;
            }
            if (teamNumber == PhotonBattle.TeamRedValue)
            {
                row = shieldGridPos.Row + 1;
            }
            if (turnRight)
            {
                col = shieldGridPos.Col + 1;
            }
            if (!turnRight)
            {
                col = shieldGridPos.Col;
            }
            var xPos = col * _arenaWidth / _shieldGridWidth;
            var yPos = row * _arenaHeight / _shieldGridHeight;
            var corner = new Vector2(xPos - _arenaWidth / 2, yPos - _arenaHeight / 2);
            return corner;
        }

        bool IGridManager.IsMovementGridSpaceFree(GridPos gridPos, int teamNumber)
        {
            switch (teamNumber)
            {
                case PhotonBattle.TeamBlueValue:
                    return _gridEmptySpacesBlue[gridPos.Row, gridPos.Col];
                case PhotonBattle.TeamRedValue:
                    return _gridEmptySpacesRed[gridPos.Row, gridPos.Col];
                default:
                    throw new UnityException($"Invalid Team Number {teamNumber}");
            }
        }

        #endregion
    }
}
