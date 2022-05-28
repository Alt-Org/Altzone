using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using Prg.Scripts.Common.Unity.Attributes;
using UnityEngine;

namespace Battle.Test.Scripts.Battle.Players
{
    internal class PlayerDriverState : MonoBehaviour
    {
        [SerializeField, ReadOnly] private int _currentPoseIndex;
        [SerializeField, ReadOnly] private int _currentShieldResistance;

        private IPlayerDriver _playerDriver;
        private CharacterModel _characterModel;
        private float _stunDuration;
        private bool _isDisableShieldStateChanges;

        public void ResetState(IPlayerDriver playerDriver, CharacterModel characterModel)
        {
            _playerDriver = playerDriver;
            _characterModel = characterModel;

            var runtimeGameConfig = RuntimeGameConfig.Get();
            var variables = runtimeGameConfig.Variables;
            _stunDuration = variables._playerShieldHitStunDuration;
            var features = runtimeGameConfig.Features;
            _isDisableShieldStateChanges = features._isDisableShieldStateChanges;

            _currentPoseIndex = 0;
            _currentShieldResistance = characterModel.Resistance;
        }

        public string OnShieldCollision()
        {
            if (_isDisableShieldStateChanges)
            {
                return string.Empty;
            }
            if (_currentShieldResistance > 0)
            {
                _currentShieldResistance -= 1;
                Debug.Log($"hit pose {_currentPoseIndex} shield {_currentShieldResistance}");
                _playerDriver.SetShieldResistance(_currentShieldResistance);
                return "HIT";
            }
            if (_currentPoseIndex == _playerDriver.MaxPoseIndex)
            {
                Debug.Log($"stun pose {_currentPoseIndex} shield {_currentShieldResistance}");
                _playerDriver.SetStunned(_stunDuration);
                return "STUN";
            }
            _currentShieldResistance = _characterModel.Resistance;
            _currentPoseIndex += 1;
            Debug.Log($"bend pose {_currentPoseIndex} shield {_currentShieldResistance}");
            _playerDriver.SetCharacterPose(_currentPoseIndex);
            _playerDriver.SetShieldResistance(_currentShieldResistance);
            return "BEND";
        }

        public void OnHeadCollision()
        {
        }
    }
}