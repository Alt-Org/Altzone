using Photon.Pun;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    public class PlayerDriverPhoton : MonoBehaviour, IPlayerDriver
    {
        [SerializeField] private GameObject _playerPrefab;
        private IPlayerActor _playerActor;
        private IGridManager _gridManager;
        private IBattlePlayArea _battlePlayArea;
        private PhotonView _photonView;
        private int _playerPos;
        private int _teamNumber;

        private bool _isLocal;

        private void Awake()
        {
            _battlePlayArea = Context.GetBattlePlayArea;
            _photonView = PhotonView.Get(this);
            _gridManager = Context.GetGridManager;
            _playerActor = Instantiate(_playerPrefab).GetComponent<PlayerActor>();
            _playerPos = PhotonBattle.GetPlayerPos(_photonView.Owner);
            _teamNumber = PhotonBattle.GetTeamNumber(_playerPos);
        }

        private void OnEnable()
        {
            var player = _photonView.Owner;
            _isLocal = player.IsLocal;
            if (!_isLocal)
            {
                return;
            }
            var playerInputHandler = Context.GetPlayerInputHandler;
            playerInputHandler.SetPlayerDriver(this);
            var startingPos = _battlePlayArea.GetPlayerStartPosition(_playerPos);
            ((IPlayerDriver)this).MoveTo(startingPos);
            if (_teamNumber == 1)
            {
                ((IPlayerDriver)this).Rotate(180f);
            }
        }
        void IPlayerDriver.Rotate(float angle)
        {
            _photonView.RPC(nameof(RotatePlayerRpc), RpcTarget.All, angle);
        }

        void IPlayerDriver.MoveTo(Vector2 targetPosition)
        {
            _photonView.RPC(nameof(MovePlayerToRpc), RpcTarget.All, targetPosition);
        }
        void IPlayerDriver.MoveTo(GridPos gridPos)
        {
            var targetPosition = _gridManager.GridPositionToWorldPoint(gridPos);
            _photonView.RPC(nameof(MovePlayerToRpc), RpcTarget.All, targetPosition);
        }

        [PunRPC]
        private void MovePlayerToRpc(Vector2 targetPosition)
        {
            var gridPos = _gridManager.WorldPointToGridPosition(targetPosition);
            targetPosition = _gridManager.GridPositionToWorldPoint(gridPos);
            _playerActor.MoveTo(targetPosition);
        }

        [PunRPC]
        private void RotatePlayerRpc(float angle)
        {
            _playerActor.Rotate(angle);
        }
    }
}
