using Altzone.Scripts.Battle;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Test.Scripts.Battle.Players
{
    /// <summary>
    /// Photon <c>PlayerDriver</c> implementation.
    /// </summary>
    internal class PlayerDriver : MonoBehaviourPunCallbacks
    {
        [SerializeField] private PlayerActor _playerActor;

        public Player Player => photonView.Owner;

        private void Awake()
        {
            var player = photonView.Owner;
            Debug.Log($"{player.GetDebugLabel()} {photonView}");
            var playerPos = PhotonBattle.GetPlayerPos(player);
            var playerTag = $"{playerPos}:{player.NickName}";
            name = name.Replace("Clone", playerTag);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            var player = photonView.Owner;
            Debug.Log($"{player.GetDebugLabel()} {photonView}");
            var photonPlayerInstantiate = FindObjectOfType<PhotonPlayerInstantiate>();
            Assert.IsNotNull(photonPlayerInstantiate, "photonPlayerInstantiate != null");
            _playerActor = photonPlayerInstantiate.OnPhotonPlayerInstantiated(player, this);
        }
    }
}