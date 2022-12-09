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
        private PhotonView _photonView;

        private bool _isLocal;

        private void Awake()
        {
            _photonView = PhotonView.Get(this);
            _gridManager = Context.GetGridManager();
            _playerActor = Instantiate(_playerPrefab).GetComponent<PlayerActor>();
            var player = _photonView.Owner;
            _isLocal = player.IsLocal;
        }

        private void OnEnable()
        {
            if (!_isLocal)
            {
                return;
            }
            var playerInputHandler = Context.GetPlayerInputHandler();
            playerInputHandler.SetPlayerDriver(this);
        }

        void IPlayerDriver.MoveTo(Vector2 targetPosition)
        {
            _photonView.RPC(nameof(MovePlayerToRpc), RpcTarget.All, targetPosition);
        }

        [PunRPC]
        private void MovePlayerToRpc(Vector2 targetPosition)
        {
            _playerActor.MoveTo(targetPosition);
        }
    }
}
