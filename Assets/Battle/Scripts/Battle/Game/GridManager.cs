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
        private bool[,] _gridEmptySpacesAlpha;
        private bool[,] _gridEmptySpacesBeta;
        private Rect _startAreaAlpha;
        private Rect _startAreaBeta;

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
            _startAreaAlpha = _battlePlayArea.GetPlayerPlayArea(PhotonBattle.TeamAlphaValue);
            _startAreaBeta = _battlePlayArea.GetPlayerPlayArea(PhotonBattle.TeamBetaValue);

            var smallOffset = 0.001f;
            var alphaAreaStart = ((IGridManager)this).WorldPointToGridPosition(new Vector2(_startAreaAlpha.xMin, _startAreaAlpha.yMin + smallOffset));
            var betaAreaStart = ((IGridManager)this).WorldPointToGridPosition(new Vector2(_startAreaBeta.xMin, _startAreaBeta.yMin + smallOffset));
            var alphaAreaEnd = ((IGridManager)this).WorldPointToGridPosition(new Vector2(_startAreaAlpha.xMax, _startAreaAlpha.yMax - smallOffset));
            var betaAreaEnd = ((IGridManager)this).WorldPointToGridPosition(new Vector2(_startAreaBeta.xMax, _startAreaBeta.yMax - smallOffset));

            var alphaRowMin = alphaAreaStart.Row;
            var betaRowMin = betaAreaStart.Row;
            var alphaRowMax = alphaAreaEnd.Row;
            var betaRowMax = betaAreaEnd.Row;

            _gridEmptySpacesAlpha = new bool[_movementGridHeight, _movementGridWidth];
            _gridEmptySpacesBeta = new bool[_movementGridHeight, _movementGridWidth];

            for (int row = 0; row < _movementGridHeight; row++)
            {
                for (int col = 0; col < _movementGridWidth; col++)
                {
                    if (row >= alphaRowMin && row <= alphaRowMax)
                    {
                        _gridEmptySpacesAlpha[row, col] = true;
                    }
                    else
                    {
                        _gridEmptySpacesAlpha[row, col] = false;
                    }
                }
            }

            for (int row = 0; row < _movementGridHeight; row++)
            {
                for (int col = 0; col < _movementGridWidth; col++)
                {
                    if (row >= betaRowMin && row <= betaRowMax)
                    {
                        _gridEmptySpacesBeta[row, col] = true;
                    }
                    else
                    {
                        _gridEmptySpacesBeta[row, col] = false;
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
            if (teamNumber == PhotonBattle.TeamAlphaValue)
            {
                row = shieldGridPos.Row;
            }
            if (teamNumber == PhotonBattle.TeamBetaValue)
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
                case PhotonBattle.TeamAlphaValue:
                    return _gridEmptySpacesAlpha[gridPos.Row, gridPos.Col];
                case PhotonBattle.TeamBetaValue:
                    return _gridEmptySpacesBeta[gridPos.Row, gridPos.Col];
                default:
                    throw new UnityException($"Invalid Team Number {teamNumber}");
            }
        }

        #endregion
    }
}
