using System;
using System.Collections;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using Photon.Pun;
using Prg.Scripts.Common.Unity.Attributes;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    internal class PlayerDriverState : MonoBehaviour, IPlayerDriverState
    {
        [Header("Live Data"), SerializeField, ReadOnly] private int _currentPoseIndex;
        [SerializeField, ReadOnly] private int _currentShieldResistance;
        [SerializeField, ReadOnly] private double _lastBallHitTime;
        
        [Header("Debug"), SerializeField, ReadOnly] private int _currentRow;
        [SerializeField, ReadOnly] private int _currentCol;

        private PhotonView _photonView;
        private IPlayerDriver _playerDriver;
        private CharacterModel _characterModel;
        private IBallManager _ballManager;
        private IPlayerActor _playerActor;
        private IGridManager _gridManager;
        private float _playerAttackMultiplier;
        private float _stunDuration;
        private bool _isDisableShieldStateChanges;
        private bool _isDisableBallSpeedChanges;
        private GridPos _savedGridPosition;
        private bool _isWaitingForAnswer;
        private double _movementDelay;
        public double LastBallHitTime => _lastBallHitTime;

        public bool CanRequestMove => !_isWaitingForAnswer && !_playerActor.IsBusy;

        public Vector2 ResetState(IPlayerDriver playerDriver, IPlayerActor playerActor, CharacterModel characterModel, Vector2 playerWorldPosition)
        {
            _photonView = PhotonView.Get(this);
            _playerDriver = playerDriver;
            _playerActor = playerActor;
            _characterModel = characterModel;
            _ballManager = Context.BallManager;
            var runtimeGameConfig = RuntimeGameConfig.Get();
            var variables = runtimeGameConfig.Variables;
            _playerAttackMultiplier = variables._playerAttackMultiplier;
            _stunDuration = variables._playerShieldHitStunDuration;
            var features = runtimeGameConfig.Features;
            _isDisableShieldStateChanges = features._isDisableShieldStateChanges;
            _isDisableBallSpeedChanges = features._isDisableBallSpeedChanges;
            _movementDelay = variables._playerMovementNetworkDelay;

            _currentPoseIndex = 0;
            _currentShieldResistance = characterModel.Resistance;
            _lastBallHitTime = PhotonNetwork.Time;

            if (features._isDisableBattleGridMovement)
            {
                _currentRow = -1;
                _currentCol = -1;
                return playerWorldPosition;
            }
            var isRotated = Context.GetBattleCamera.IsRotated;
            _gridManager = Context.GetGridManager;
            var gridPos = _gridManager.WorldPointToGridPosition(playerWorldPosition, isRotated);
            _currentRow = gridPos.Row;
            _currentCol = gridPos.Col;
            _savedGridPosition = gridPos;
            _gridManager.SetSpaceTaken(gridPos);
            var currentPosition = _gridManager.GridPositionToWorldPoint(gridPos, isRotated);
            _playerDriver.MoveTo(currentPosition);
            return currentPosition;
        }

        public void CheckRotation(Vector2 position)
        {
            var doRotate = position.y > 0;
            Debug.Log($"{name} playerPos {_playerDriver.PlayerPos} position {position} rotate {doRotate}");
            if (doRotate)
            {
                _playerDriver.Rotate(true);
            }
        }

        public void OnShieldCollision(out string debugString)
        {
            if (!_isDisableBallSpeedChanges)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    _ballManager.SetBallSpeed(_characterModel.Attack * _playerAttackMultiplier);
                }
            }
            if (_isDisableShieldStateChanges)
            {
                debugString = string.Empty;
                return;
            }
            if (_currentPoseIndex == _playerDriver.MaxPoseIndex)
            {
                Debug.Log($"stun pose {_currentPoseIndex} shield {_currentShieldResistance}");
                _playerDriver.SetPlayMode(BattlePlayMode.Ghosted);
                _playerDriver.SetStunned(_stunDuration);
                debugString = "STUN";
                return;
            }
            if (_currentShieldResistance > 0)
            {
                _currentShieldResistance -= 1;
                Debug.Log($"hit pose {_currentPoseIndex} shield {_currentShieldResistance}");
                _playerDriver.SetPlayMode(BattlePlayMode.Ghosted);
                _playerDriver.SetShieldResistance(_currentShieldResistance);
                debugString = "HIT";
                return;
            }
            _currentShieldResistance = _characterModel.Resistance;
            _currentPoseIndex += 1;
            Debug.Log($"bend pose {_currentPoseIndex} shield {_currentShieldResistance}");
            _playerDriver.SetPlayMode(BattlePlayMode.Ghosted);
            _playerDriver.SetStunned(_stunDuration);
            _playerDriver.SetCharacterPose(_currentPoseIndex);
            if (_currentPoseIndex < _playerDriver.MaxPoseIndex)
            {
                _playerDriver.SetShieldResistance(_currentShieldResistance);
            }
            debugString = "BEND";
        }

        public void OnHeadCollision()
        {
            Debug.Log($"pose {_currentPoseIndex} shield {_currentShieldResistance}");
            _lastBallHitTime = PhotonNetwork.Time;
            _playerDriver.SetPlayMode(BattlePlayMode.Ghosted);
        }

        public override string ToString()
        {
            return $"pose={_currentPoseIndex} res={_currentShieldResistance}";
        }

        public void DelayedMove(int row, int col, float moveExecuteDelay)
        {
            GridPos gridPos = new GridPos(row, col);
            DelayedMove(gridPos, moveExecuteDelay);
        }

        public void DelayedMove(GridPos gridPos, float moveExecuteDelay)
        {
            StartCoroutine(DelayTime(gridPos, moveExecuteDelay));
        }

        private IEnumerator DelayTime(GridPos gridPos, float waitTime)
        {
            yield return new WaitForSeconds((float)waitTime);
            _gridManager.SetSpaceFree(_savedGridPosition);
            _currentRow = gridPos.Row;
            _currentCol = gridPos.Col;
            _savedGridPosition = gridPos;
            var targetPosition = _gridManager.GridPositionToWorldPoint(gridPos, Context.GetBattleCamera.IsRotated);
            _playerActor.MoveTo(targetPosition);
            SetIsWaitingForAnswer(false);
        }

        public void SetIsWaitingForAnswer(bool isWaitingForAnswer)
        {
            _isWaitingForAnswer = isWaitingForAnswer;
        }

        void IPlayerDriverState.ProcessMoveRequest(GridPos gridPos)
        {
            var row = gridPos.Row;
            var col = gridPos.Col;
            if (_photonView == null)
            {
                if (!_gridManager.GridFreeState(row, col))
                {
                    Debug.Log($"Grid check failed. row: {row}, col: {col}");
                    SetIsWaitingForAnswer(false);
                    return;
                }
                _gridManager.SetSpaceTaken(gridPos);
                DelayedMove(gridPos, (float)_movementDelay);
                return;
            }
            _photonView.RPC(nameof(ProcessMoveRequestRpc), RpcTarget.MasterClient, row, col);
        }

        [PunRPC]
        private void ProcessMoveRequestRpc(int row, int col, PhotonMessageInfo info)
        {
            if (!_gridManager.GridFreeState(row, col))
            {
                Debug.Log($"Grid check failed. row: {row}, col: {col}");
                _photonView.RPC(nameof(SetWaitingStateRpc), info.Sender, false);
                return;
            }
            var movementStartTime = info.SentServerTime + _movementDelay;
            Debug.Log($"Grid Request approved: row: {row}, col: {col}, player: {info.Sender}, time: {movementStartTime}");
            _gridManager.SetSpaceTaken(new GridPos(row, col));
            _photonView.RPC(nameof(MoveDelayedRpc), RpcTarget.All, row, col, movementStartTime);
        }

        [PunRPC]
        private void SetWaitingStateRpc(bool isWaitingForAnswer)
        {
            SetIsWaitingForAnswer(isWaitingForAnswer);
        }

        [PunRPC]
        private void MoveDelayedRpc(int row, int col, double movementStartTime)
        {
            var moveExecuteDelay = Math.Max(0, movementStartTime - PhotonNetwork.Time);
            DelayedMove(row, col, (float)moveExecuteDelay);
        }
    }
}
