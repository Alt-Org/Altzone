using System;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Model;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Test.Scripts.Battle.Players
{
    /// <summary>
    /// Photon <c>PlayerDriver</c> implementation.
    /// </summary>
    internal class PhotonPlayerDriver : MonoBehaviourPunCallbacks, IPlayerDriver
    {
        [Serializable]
        internal class DebugSettings
        {
            public PlayerActor _playerPrefab;
        }

        [Header("Live Data"), SerializeField] private PlayerActor _playerActorInstance;

        [Header("Debug Settings"), SerializeField] private DebugSettings _debug;

        private CharacterModel _characterModel;
        private IPlayerActor _playerActor;
        
        public static void InstantiateLocalPlayer(Player player, string networkPrefabName)
        {
            Assert.IsTrue(player.IsLocal, "player.IsLocal");
            Debug.Log($"{player.GetDebugLabel()} prefab {networkPrefabName}");
            PhotonNetwork.Instantiate(networkPrefabName, Vector3.zero, Quaternion.identity);
        }

        private void Awake()
        {
            print("+");
            var player = photonView.Owner;
            Debug.Log($"{player.GetDebugLabel()} {photonView}");
            var playerPos = ((IPlayerDriver)this).PlayerPos;
            var playerTag = $"{playerPos}:{((IPlayerDriver)this).NickName}";
            name = name.Replace("Clone", playerTag);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            var player = photonView.Owner;
            Debug.Log($"{player.GetDebugLabel()} {photonView}");
            if (!PhotonBattle.IsRealPlayer(player))
            {
                return;
            }
            if (_playerActor != null)
            {
                return;
            }
            _characterModel = PhotonBattle.GetCharacterModelForRoom(player);
            _playerActorInstance = PlayerActor.Instantiate(this, _debug._playerPrefab);
            _playerActor = _playerActorInstance;
            _playerActor.Speed = _characterModel.Speed;
        }

        #region IPlayerDriver

        string IPlayerDriver.NickName => photonView.Owner.NickName;

        int IPlayerDriver.ActorNumber => photonView.Owner.ActorNumber;

        int IPlayerDriver.PlayerPos => PhotonBattle.GetPlayerPos(photonView.Owner);

        CharacterModel IPlayerDriver.CharacterModel => _characterModel;

        void IPlayerDriver.MoveTo(Vector2 targetPosition)
        {
            photonView.RPC(nameof(MoveToRpc), RpcTarget.All, targetPosition);
        }

        #endregion

        #region Photon RPC

        [PunRPC]
        private void MoveToRpc(Vector2 targetPosition)
        {
            _playerActor.MoveTo(targetPosition);
        }

        #endregion
    }
}