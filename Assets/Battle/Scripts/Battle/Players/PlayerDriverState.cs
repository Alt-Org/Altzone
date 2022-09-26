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

        private IPlayerDriver _playerDriver;
        private CharacterModel _characterModel;
        private IBallManager _ballManager;
        private IPlayerActor _playerActor;
        private IGridManager _gridManager;
        private IEnumerator _delayedMoveCoroutine;
        private float _playerAttackMultiplier;
        private float _stunDuration;
        private bool _isDisableShieldStateChanges;
        private bool _isDisableBallSpeedChanges;
        private GridPos _savedGridPosition;
        public double LastBallHitTime => _lastBallHitTime;

        public Vector2 ResetState(IPlayerDriver playerDriver, IPlayerActor playerActor, CharacterModel characterModel, Vector2 playerWorldPosition)
        {
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
            _playerDriver.SetSpaceTaken(gridPos);
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

        public void DelayedMove(GridPos gridPos, double moveExecuteDelay)
        {
            _delayedMoveCoroutine = DelayTime(gridPos, moveExecuteDelay);
            StartCoroutine(_delayedMoveCoroutine);
        }

        private IEnumerator DelayTime(GridPos gridPos, double waitTime)
        {
            yield return new WaitForSeconds((float)waitTime);
            _playerDriver.SetSpaceFree(_savedGridPosition);
            _currentRow = gridPos.Row;
            _currentCol = gridPos.Col;
            _savedGridPosition = gridPos;
            var targetPosition = _gridManager.GridPositionToWorldPoint(gridPos, Context.GetBattleCamera.IsRotated);
            _playerActor.MoveTo(targetPosition);
        }
    }
}
