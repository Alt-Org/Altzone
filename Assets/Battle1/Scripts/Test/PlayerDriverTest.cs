using System;
using Battle1.Scripts.Battle;
using Battle1.Scripts.Battle.Game;
using Battle1.Scripts.Battle.Players;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle1.Scripts.Test
{
    public class PlayerDriverTest : MonoBehaviour
    {
        [Header("Player Test Actions")] public bool _moveTo;
        [SerializeField] private GridPos _movePosition;
        [SerializeField] private float _moveExecuteDelay;

        [Header("Player Driver"), SerializeField] private PlayerDriverStatic _playerDriverInstance;
        private PlayerDriverState _playerDriverState;
        private GridManager _gridManager;

        private void OnEnable()
        {
            _gridManager = Context.GetGridManager;
            Assert.IsNotNull(_playerDriverInstance, "_playerDriver != null");
            Debug.Log($"playerDriver {_playerDriverInstance}");
        }

        void Update()
        {
            if (_moveTo)
            {
                _moveTo = false;
                var targetPosition = _gridManager.GridPositionToWorldPoint(_movePosition);
                throw new NotSupportedException($"can not test MoveTo {targetPosition}");
            }
        }
    }
}
