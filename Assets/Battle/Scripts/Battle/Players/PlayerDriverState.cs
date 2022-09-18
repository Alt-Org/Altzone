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
        [SerializeField, ReadOnly] private int _currentPoseIndex;
        [SerializeField, ReadOnly] private int _currentShieldResistance;
        [SerializeField, ReadOnly] private double _lastBallHitTime;

        private IPlayerDriver _playerDriver;
        private CharacterModel _characterModel;
        private IBallManager _ballManager;
        private IPlayerActor _playerActor;
        private Transform _transform;
        private IGridManager _gridManager;
        private IEnumerator _delayedMoveCoroutine;
        private float _playerAttackMultiplier;
        private float _stunDuration;
        private bool _isDisableShieldStateChanges;
        private bool _isDisableBallSpeedChanges;
        private int[] _savedGridPosition;
        public double LastBallHitTime => _lastBallHitTime;

        public void ResetState(IPlayerDriver playerDriver, IPlayerActor playerActor, CharacterModel characterModel)
        {
            _playerDriver = playerDriver;
            _playerActor = playerActor;
            _characterModel = characterModel;
            _ballManager = Context.BallManager;
            _gridManager = Context.GetGridManager;
            _transform = _playerActor.Transform;
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

            Vector2 playerCurrentPosition = _transform.position;
            var gridPos = _gridManager.CalcRowAndColumn(playerCurrentPosition, Context.GetBattleCamera.IsRotated);
            _savedGridPosition = new int[2] { gridPos[0], gridPos[1] };
            _playerDriver.SetSpaceTaken(gridPos[0], gridPos[1]);
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

        public void DelayedMove(int row, int col, double movementStartTime)
        {
            _delayedMoveCoroutine = DelayTime(row, col, movementStartTime);
            StartCoroutine(_delayedMoveCoroutine);
        }

        private IEnumerator DelayTime(int row, int col, double waitTime)
        {
            yield return new WaitForSeconds((float)waitTime);
            _playerDriver.SetSpaceFree(_savedGridPosition[0], _savedGridPosition[1]);
            _savedGridPosition = new int[2] { row, col };
            var targetPosition = _gridManager.GridPositionToWorldpoint(row, col, Context.GetBattleCamera.IsRotated);
            _playerDriver.MoveTo(targetPosition);
        }
    }
}
