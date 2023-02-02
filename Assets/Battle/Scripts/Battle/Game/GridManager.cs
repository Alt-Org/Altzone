using System;
using UnityEngine;

namespace Battle.Scripts.Battle.Game
{
    /// <summary>
    /// <c>GridManager</c> translates between world and grid coordinates, also keeps track which coordinates player can move to.
    /// </summary>
    internal class GridManager : MonoBehaviour, IGridManager
    {
        private int _gridWidth;
        private int _gridHeight;
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
            _gridWidth = _battlePlayArea.GridWidth;
            _gridHeight = _battlePlayArea.GridHeight;

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

            _gridEmptySpacesAlpha = new bool[_gridHeight, _gridWidth];
            _gridEmptySpacesBeta = new bool[_gridHeight, _gridWidth];

            for (int row = 0; row < _gridHeight; row++)
            {
                for (int col = 0; col < _gridWidth; col++)
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

            for (int row = 0; row < _gridHeight; row++)
            {
                for (int col = 0; col < _gridWidth; col++)
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
            var xPosition = gridPos.Col * _arenaWidth / _gridWidth + _arenaWidth / _gridWidth * 0.5f;
            var yPosition = gridPos.Row * _arenaHeight / _gridHeight + _arenaHeight / _gridHeight * 0.5f;
            Vector2 worldPosition = new Vector2(xPosition - _arenaWidth / 2, yPosition - _arenaHeight / 2);
            return worldPosition;
        }

        GridPos IGridManager.WorldPointToGridPosition(Vector2 targetPosition)
        {
            var posNew = new Vector2(targetPosition.x + _arenaWidth / 2, targetPosition.y + _arenaHeight / 2);
            var col = Math.Min(_gridWidth - 1, (int)(posNew.x / (_arenaWidth / _gridWidth)));
            var row = Math.Min(_gridHeight - 1, (int)(posNew.y / (_arenaHeight / _gridHeight)));
            GridPos gridPos = new GridPos(row, col);
            return gridPos;
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
