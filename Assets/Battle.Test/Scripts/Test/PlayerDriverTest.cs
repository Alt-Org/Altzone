using Altzone.Scripts.Config;
using Battle.Scripts.Battle.interfaces;
using Battle.Test.Scripts.Battle.Players;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Test.Scripts.Test
{
    internal class PlayerDriverTest : MonoBehaviour
    {
        [Header("Test Settings")] public Vector2 _moveToPosition;
        public int _poseIndex;
        public BattlePlayMode _playMode;
        public bool _isShieldVisible;
        public float _stunDuration;

        [Header("Live Data")] public string _nickname;

        [Header("Debug Actions")] public bool _moveTo;
        public bool _setPose;
        public bool _setPlayMode;
        public bool _setShieldVisibility;
        public bool _setStunned;

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
            if (_setPlayMode)
            {
                _setPlayMode = false;
                _playerDriver.SetPlayMode(_playMode);
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
        }
    }
}