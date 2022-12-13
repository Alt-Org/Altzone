using System.Collections;
using System.Collections.Generic;
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

        private bool _isLocal;

        private void Awake()
        {
            _battlePlayArea = Context.GetBattlePlayArea();
            _photonView = PhotonView.Get(this);
            _gridManager = Context.GetGridManager();
            _playerActor = Instantiate(_playerPrefab).GetComponent<PlayerActor>();
        }

        private void OnEnable()
        {
            var player = _photonView.Owner;
            _isLocal = player.IsLocal;
            if (!_isLocal)
            {
                return;
            }
            var playerInputHandler = Context.GetPlayerInputHandler();
            playerInputHandler.SetPlayerDriver(this);
            _playerPos = PhotonNetwork.LocalPlayer.ActorNumber;
            var startingPos = _battlePlayArea.GetPlayerStartPosition(_playerPos);
            ((IPlayerDriver)this).MoveTo(startingPos);
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
    }
}
