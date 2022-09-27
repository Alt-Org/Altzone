using Altzone.Scripts.Config;
using Battle.Scripts.Battle;
using Battle.Scripts.Battle.Players;
using Prg.Scripts.Common.Unity.Attributes;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Test
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
        public double _gridMovementDelay = 0.1;

        [Header("Live Data"), ReadOnly] public bool _isLocal;
        [ReadOnly] public string _nickname;

        [Header("Player Driver"), SerializeField] private PlayerDriver _playerDriverInstance;
        private PlayerDriverState _playerDriverState;
        private IPlayerDriver _playerDriver;
        private IGridManager _gridManager;

        private void Awake()
        {
            _playerDriver = _playerDriverInstance as IPlayerDriver;
            Assert.IsNotNull(_playerDriver, "_playerDriver != null");
            Debug.Log($"playerDriver {_playerDriver}");
            if (_stunDuration == 0)
            {
                var variables = RuntimeGameConfig.Get().Variables;
                _stunDuration = variables._playerShieldHitStunDuration;
            }
            _nickname = _playerDriver.NickName ?? "noname";
            _isLocal = _playerDriver.IsLocal;
            _gridManager = Context.GetGridManager;
        }

        private void Update()
        {
            if (_moveTo)
            {
                _moveTo = false;
                // Toggle between test target position and current player position 
                var position = _moveToPosition;
                _moveToPosition = _playerDriver.Position;
                if (_isGridMovement)
                {
                    _playerDriverState = _playerDriverInstance.GetComponent<PlayerDriverState>();
                    var row = (int)Mathf.Clamp(position.y, 0, _gridManager.RowCount - 1);
                    var col = (int)Mathf.Clamp(position.x, 0, _gridManager.ColCount - 1);
                    GridPos gridPos = new GridPos(row, col);
                    _playerDriverState.DelayedMove(gridPos, _gridMovementDelay);
                }
                else
                {
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
