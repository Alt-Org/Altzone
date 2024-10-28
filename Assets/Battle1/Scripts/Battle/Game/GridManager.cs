using System;
using UnityEngine;

namespace Battle1.Scripts.Battle.Game
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

    /// <summary>
    /// <c>GridManager</c> translates between world and grid coordinates, also keeps track which coordinates player can move to.
    /// </summary>
    internal class GridManager : MonoBehaviour
    {
        #region Public Methods

        public Vector2 GridPositionToWorldPoint(GridPos gridPos)
        {
            float xPosition = gridPos.Col * _arenaWidth / _gridWidth + _arenaWidth / _gridWidth * 0.5f;
            float yPosition = gridPos.Row * _arenaHeight / _gridHeight + _arenaHeight / _gridHeight * 0.5f;
            Vector2 worldPosition = new(xPosition - _arenaWidth / 2, yPosition - _arenaHeight / 2);
            return worldPosition;
        }

        public GridPos WorldPointToGridPosition(Vector2 targetPosition)
        {
            Vector2 posNew = new(targetPosition.x + _arenaWidth / 2, targetPosition.y + _arenaHeight / 2);
            int col = Math.Min(_gridWidth - 1, (int)(posNew.x / (_arenaWidth / _gridWidth)));
            int row = Math.Min(_gridHeight - 1, (int)(posNew.y / (_arenaHeight / _gridHeight)));
            GridPos gridPos = new(row, col);
            return gridPos;
        }

        public GridPos ClampGridPosition(GridPos gridPos)
        {
            return new GridPos(Math.Clamp(gridPos.Row, 0, _gridHeight), Math.Clamp(gridPos.Col, 0, _gridWidth));
        }

        public bool IsMovementGridSpaceFree(GridPos gridPos, BattleTeamNumber teamNumber)
        {
            return teamNumber switch
            {
                BattleTeamNumber.TeamAlpha => _gridEmptySpacesAlpha[gridPos.Row, gridPos.Col],
                BattleTeamNumber.TeamBeta => _gridEmptySpacesBeta[gridPos.Row, gridPos.Col],
                _ => throw new UnityException($"Invalid Team Number {teamNumber}"),
            };
        }

        #endregion Public Methods

        #region Private

        #region Private - Fields
        private int _gridWidth;
        private int _gridHeight;
        private PlayerPlayArea _battlePlayArea;
        private float _arenaWidth;
        private float _arenaHeight;
        private bool[,] _gridEmptySpacesAlpha;
        private bool[,] _gridEmptySpacesBeta;
        private Rect _startAreaAlpha;
        private Rect _startAreaBeta;
        #endregion Private - Fields

        #region Private - Methods

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
            _startAreaAlpha = _battlePlayArea.GetPlayerPlayArea(BattleTeamNumber.TeamAlpha);
            _startAreaBeta = _battlePlayArea.GetPlayerPlayArea(BattleTeamNumber.TeamBeta);

            float smallOffset = 0.001f;
            GridPos alphaAreaStart = WorldPointToGridPosition(new Vector2(_startAreaAlpha.xMin, _startAreaAlpha.yMin + smallOffset));
            GridPos betaAreaStart = WorldPointToGridPosition(new Vector2(_startAreaBeta.xMin, _startAreaBeta.yMin + smallOffset));
            GridPos alphaAreaEnd = WorldPointToGridPosition(new Vector2(_startAreaAlpha.xMax, _startAreaAlpha.yMax - smallOffset));
            GridPos betaAreaEnd = WorldPointToGridPosition(new Vector2(_startAreaBeta.xMax, _startAreaBeta.yMax - smallOffset));

            int alphaRowMin = alphaAreaStart.Row;
            int betaRowMin = betaAreaStart.Row;
            int alphaRowMax = alphaAreaEnd.Row;
            int betaRowMax = betaAreaEnd.Row;

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

        #endregion Private - Methods

        #endregion Private
    }
}
