using System.Collections;
using System.Collections.Generic;
using Battle.Scripts.Battle;
using Battle.Scripts.Battle.Game;
using Battle.Scripts.Battle.Players;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Test
{
    public class PlayerDriverTest : MonoBehaviour
    {
        [Header("Player Test Actions")] public bool _moveTo;
        [SerializeField] private GridPos _movePosition;
        [SerializeField] private float _moveExecuteDelay;

        [Header("Player Driver"), SerializeField] private PlayerDriver _playerDriverInstance;
        private IPlayerDriver _playerDriver;
        private PlayerDriverState _playerDriverState;
        private IPlayerInputTarget _inputTarget;
        private GridManager _gridManager;

        private void OnEnable()
        {
            _playerDriver = _playerDriverInstance as IPlayerDriver;
            _gridManager = Context.GetGridManager;
            Assert.IsNotNull(_playerDriver, "_playerDriver != null");
            Debug.Log($"playerDriver {_playerDriver}");
        }

        void Update()
        {
            if (_moveTo)
            {
                _moveTo = false;
                _inputTarget = _playerDriverInstance as IPlayerDriver;
                var targetPosition = _gridManager.GridPositionToWorldPoint(_movePosition);
                _inputTarget.MoveTo(targetPosition);
            }
        }
    }
}
