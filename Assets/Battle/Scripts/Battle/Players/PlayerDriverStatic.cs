using System.Collections;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    public class PlayerDriverStatic : MonoBehaviour, IPlayerDriver
    {
        [SerializeField] private int _playerPos = PhotonBattle.PlayerPosition1;
        [SerializeField] private int _teamNumber = PhotonBattle.TeamBlueValue;
        [SerializeField] private GameObject _playerPrefab;

        private IPlayerActor _playerActor;
        private IGridManager _gridManager;
        private IBattlePlayArea _battlePlayArea;

        private void Awake()
        {
            _battlePlayArea = Context.GetBattlePlayArea;
            _gridManager = Context.GetGridManager;
            _playerActor = Instantiate(_playerPrefab).GetComponent<PlayerActor>();
        }

        private IEnumerator Start()
        {
            yield return null;
            var playerInputHandler = Context.GetPlayerInputHandler;
            playerInputHandler.SetPlayerDriver(this);
            if (_teamNumber == 1)
            {
                ((IPlayerDriver)this).Rotate(180f);
            }
        }

        int IPlayerDriver.PlayerPos => _playerPos;

        void IPlayerDriver.Rotate(float angle)
        {
            _playerActor.Rotate(angle);
        }

        void IPlayerDriver.MoveTo(Vector2 targetPosition)
        {
            var gridPos = _gridManager.WorldPointToGridPosition(targetPosition);
            targetPosition = _gridManager.GridPositionToWorldPoint(gridPos);
            _playerActor.MoveTo(targetPosition);
        }
    }
}
