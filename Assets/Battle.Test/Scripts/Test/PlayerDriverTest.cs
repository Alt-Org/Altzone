using Altzone.Scripts.Config;
using Battle.Scripts.Battle.interfaces;
using Battle.Test.Scripts.Battle.Players;
using Prg.Scripts.Common.Unity.Attributes;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Test.Scripts.Test
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

        [Header("Live Data"), ReadOnly] public bool _isLocal;
        [ReadOnly] public string _nickname;

        private IPlayerDriver _playerDriver;

        private void Awake()
        {
            _playerDriver = GetComponent<IPlayerDriver>();
            Assert.IsNotNull(_playerDriver, "_playerDriver != null");
            Debug.Log($"playerDriver {_playerDriver}");
            if (_stunDuration == 0)
            {
                var variables = RuntimeGameConfig.Get().Variables;
                _stunDuration = variables._playerHeadHitStunDuration;
            }
            _nickname = _playerDriver.NickName ?? "noname";
            _isLocal = _playerDriver.IsLocal;
        }

        private void Update()
        {
            if (_moveTo)
            {
                _moveTo = false;
                // Toggle between test target position and current player position 
                var position = _moveToPosition;
                _moveToPosition = _playerDriver.Position;
                _playerDriver.MoveTo(position);
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