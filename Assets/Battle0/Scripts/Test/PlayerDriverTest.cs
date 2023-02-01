using System.Collections;
using Altzone.Scripts.Config;
using Battle0.Scripts.Battle;
using Battle0.Scripts.Battle.Game;
using Battle0.Scripts.Battle.Players;
using Prg.Scripts.Common.Unity.Attributes;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle0.Scripts.Test
{
    internal class PlayerDriverTest : MonoBehaviour
    {
        [Header("Player Test Actions")] public bool _moveTo;
        public bool _setPlayModeNormal;
        public bool _setPlayModeFrozen;
        public bool _setPlayModeGhosted;
        public bool _setPose;
        public bool _setShieldVisibility;
        public bool _setStunned;
        public bool _setRotation;

        [Header("Test Settings")] public Vector2 _moveToPosition;
        public int _poseIndex;
        public bool _isShieldVisible;
        public float _stunDuration;
        public bool _isPlayerUpsideDown;
        public bool _isGridMovement;
        public float _gridMovementDelay = 0.1f;

        [Header("Live Data"), ReadOnly] public bool _isLocal;
        [ReadOnly] public string _nickname;

        [Header("Player Driver"), SerializeField] private PlayerDriver _playerDriverInstance;
        private IPlayerDriver _playerDriver;
        private PlayerDriverState _playerDriverState;
        private IGridManager _gridManager;

        private void Awake()
        {
            if (_playerDriverInstance == null)
            {
                Debug.Log($"{nameof(PlayerDriverTest)} is disabled, no PlayerDriver set");
                enabled = false;
            }
        }

        private void OnEnable()
        {
            _playerDriver = _playerDriverInstance as IPlayerDriver;
            Assert.IsNotNull(_playerDriver, "_playerDriver != null");
            Debug.Log($"playerDriver {_playerDriver}");
            if (_stunDuration == 0)
            {
                var variables = Battle0GameConfig.Get().Variables;
                _stunDuration = variables._playerShieldHitStunDuration;
            }
            _nickname = _playerDriver.NickName ?? "noname";
            _isLocal = _playerDriver.IsLocal;
            _gridManager = Context.GetGridManager;
        }

        private IEnumerator Start()
        {
            // Wait for PlayerDriverState instantiation.
            yield return new WaitUntil(() => (_playerDriverState ??= _playerDriverInstance.GetComponent<PlayerDriverState>()) != null);
        }

        private void Update()
        {
            if (_moveTo)
            {
                _moveTo = false;
                if (_isGridMovement)
                {
                    var row = (int)Mathf.Clamp(_moveToPosition.y, 0, _gridManager.RowCount - 1);
                    var col = (int)Mathf.Clamp(_moveToPosition.x, 0, _gridManager.ColCount - 1);
                    var gridPos = new GridPos(row, col);
                    _playerDriverState.DelayedMove(gridPos, _gridMovementDelay);
                }
                else
                {
                    // Toggle between test target position and current player position 
                    var position = _moveToPosition;
                    _moveToPosition = _playerDriver.Position;
                    _playerDriver.MoveTo(position);
                }
                return;
            }
            if (_setPose)
            {
                _setPose = false;
                _playerDriver.SetCharacterPose(_poseIndex);
                if (_poseIndex < _playerDriver.MaxPoseIndex)
                {
                    _poseIndex += 1;
                }
                else
                {
                    _poseIndex = 0;
                }
                return;
            }
            if (_setPlayModeNormal)
            {
                _setPlayModeNormal = false;
                _playerDriver.SetPlayMode(BattlePlayMode.Normal);
                return;
            }
            if (_setPlayModeFrozen)
            {
                _setPlayModeFrozen = false;
                _playerDriver.SetPlayMode(BattlePlayMode.Frozen);
                return;
            }
            if (_setPlayModeGhosted)
            {
                _setPlayModeGhosted = false;
                _playerDriver.SetPlayMode(BattlePlayMode.Ghosted);
                return;
            }
            if (_setShieldVisibility)
            {
                _setShieldVisibility = false;
                _playerDriver.SetShieldVisibility(_isShieldVisible);
                _isShieldVisible = !_isShieldVisible;
                return;
            }
            if (_setStunned)
            {
                _setStunned = false;
                _playerDriver.SetStunned(_stunDuration);
            }
            if (_setRotation)
            {
                _setRotation = false;
                _playerDriver.Rotate(_isPlayerUpsideDown);
                _isPlayerUpsideDown = !_isPlayerUpsideDown;
            }
        }
    }
}
