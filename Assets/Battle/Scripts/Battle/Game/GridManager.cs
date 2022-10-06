using System;
using Altzone.Scripts.Config;
using System.Collections;
using System.Collections.Generic;
using Battle.Scripts.Battle.Game;
using UnityEngine;
using UnityEngine.Assertions;
using Photon.Pun;

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

        void SetSpaceFree(GridPos gridPos);

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
        private PhotonView _photonView;
        private Camera _camera;
        private int _gridWidth;
        private int _gridHeight;
        private IGridManager _gridManager;

        private bool[,] _gridEmptySpaces;

        private void Awake()
        {
            _photonView = PhotonView.Get(this);
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
        void IGridManager.SetSpaceFree(GridPos gridPos)
        {
            _photonView.RPC(nameof(SetSpaceFreeRpc), RpcTarget.MasterClient, gridPos.Row, gridPos.Col);
        }

        void IGridManager.SetSpaceTaken(GridPos gridPos)
        {
            var row = gridPos.Row;
            var col = gridPos.Col;
            ((IGridManager)this).SetSpaceTaken(row, col);
        }

        void IGridManager.SetSpaceTaken(int row, int col)
        {
            _photonView.RPC(nameof(SetSpaceTakenRpc), RpcTarget.MasterClient, row, col);
        }
        bool IGridManager.GridFreeState(int row, int col)
        {
            return _gridEmptySpaces[row, col];
        }

        private void SetGridState(int row, int col, bool state)
        {
            _gridEmptySpaces[row, col] = state;
        }

        [PunRPC]
        private void SetSpaceTakenRpc(int row, int col)
        {
            SetGridState(row, col, false);
            Debug.Log($"Grid space taken: row: {row}, col: {col}");
        }

        [PunRPC]
        private void SetSpaceFreeRpc(int row, int col)
        {
            SetGridState(row, col, true);
            Debug.Log($"Grid space free: row: {row}, col: {col}");
        }
    }
}
