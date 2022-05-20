using Battle.Scripts.Battle.interfaces;
using Battle.Test.Scripts.Battle.Players;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Test.Scripts.Test
{
    internal class PlayerDriverTest : MonoBehaviour
    {
        [Header("Test Settings")] public Vector2 _playerPosition;
        public int _poseIndex;
        public BattlePlayMode _playMode;
        public float _stunDuration = 3f;

        [Header("Live Data")] public string _nickname;
        
        [Header("Debug Actions")] public bool _startMoving;
        public bool _setPose;
        public bool _setPlayMode;
        public bool _setStunned;

        private IPlayerDriver _playerDriver;

        private void Awake()
        {
            _playerDriver = GetComponent<IPlayerDriver>();
            Assert.IsNotNull(_playerDriver, "_playerDriver != null");
            Debug.Log($"playerDriver {_playerDriver}");
            _nickname = _playerDriver.NickName ?? "noname";
        }

        private void Update()
        {
            if (_startMoving)
            {
                _startMoving = false;
                _playerDriver.MoveTo(_playerPosition);
                _playerPosition = -_playerPosition;
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
            if (_setStunned)
            {
                _setStunned = false;
                _playerDriver.SetStunned(_stunDuration);
            }
        }
    }
}