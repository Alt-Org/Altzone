using Battle.Scripts.Battle.interfaces;
using Battle.Test.Scripts.Battle.Players;
using UnityEngine;

namespace Battle.Test.Scripts.Test
{
    internal class MovePlayerTest : MonoBehaviour
    {
        [Header("Test Settings")] public Vector2 _playerPosition;
        public int _poseIndex;
        public BattlePlayMode _playMode;

        [Header("Debug Actions")] public bool _startMoving;
        public bool _setPose;
        public bool _setPlayMode;

        private IPlayerDriver _playerDriver;

        private void Awake()
        {
            _playerDriver = GetComponent<IPlayerDriver>();
            Debug.Log($"playerDriver {_playerDriver}");
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
            }
        }
    }
}