using System;
using Photon.Pun;
using UnityEngine;

namespace Battle.Test.Scripts.Battle.Player
{
    internal class PlayerDriver : MonoBehaviourPunCallbacks
    {
        [SerializeField] private PhotonPlayerInstantiate _photonPlayerInstantiate;

        public void SetPhotonPlayerInstantiate(PhotonPlayerInstantiate photonPlayerInstantiate)
        {
            _photonPlayerInstantiate = photonPlayerInstantiate;
            enabled = true;
        }

        private void Awake()
        {
            enabled = false;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            var player = PhotonNetwork.LocalPlayer;
            Debug.Log($"{player.GetDebugLabel()}");
            _photonPlayerInstantiate.OnPhotonPlayerInstantiated(player);
        }
    }
}