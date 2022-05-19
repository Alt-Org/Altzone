using Battle.Test.Scripts.Battle.Players;
using UnityEngine;

namespace Battle.Test.Scripts.Test
{
    public class MovePlayerTest : MonoBehaviour
    {
        [Header("Debug Only")] public Vector2 _playerPosition;
        public bool _startMoving;

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
            }
        }
    }
}